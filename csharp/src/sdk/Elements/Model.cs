using glTFLoader;
using glTFLoader.Schema;
using Hypar.Geometry;
using Hypar.GeoJSON;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json;
using System.Collections;

namespace Hypar.Elements
{
    /// <summary>
    /// A model is a map of elements, keyed by their unique identifier. 
    /// </summary>
    public class Model : IDictionary<string, Element>
    {
        private List<byte> _buffer = new List<byte>();
        private Dictionary<string, Material> _materials = new Dictionary<string, Material>();
        private Dictionary<string, Element> _elements = new Dictionary<string, Element>();

        /// <summary>
        /// The origin of the model.
        /// </summary>
        /// <returns></returns>
        [JsonProperty("origin")]
        public Position Origin {get;set;}

        /// <summary>
        /// The identifiers of all elements in the model.
        /// </summary>
        /// <value></value>
        public ICollection<string> Keys
        {
            get{return _elements.Keys;}
        }

        /// <summary>
        /// The elements in the model.
        /// </summary>
        /// <value></value>
        public ICollection<Element> Values
        {
            get{return _elements.Values;}
        }

        /// <summary>
        /// The number of elements in the model.
        /// </summary>
        /// <value></value>
        public int Count
        {
            get{return _elements.Count;}
        }

        /// <summary>
        /// Is the model read only?
        /// </summary>
        /// <value></value>
        public bool IsReadOnly
        {
            get{return false;}
        }

        /// <summary>
        /// Return an element by its identifier.
        /// </summary>
        /// <value></value>
        public Element this[string key]
        {
            get{return this._elements[key];}
            set{this._elements[key] = value;}
        }

        /// <summary>
        /// Construct an empty model.
        /// </summary>
        public Model()
        {
            this.Origin = new Position(0,0);
        }

        /// <summary>
        /// Add an element to the model.
        /// </summary>
        /// <param name="element">The element to add to the model.</param>
        public void AddElement(Element element)
        {
            if(!this._elements.ContainsKey(element.Id.ToString()))
            {
                this._elements.Add(element.Id.ToString(), element);
                if(element is IMaterialize)
                {
                    AddMaterial(((IMaterialize)element).Material);
                }
            }
            else
            {
                throw new Exception("An Element with the same Id already exists in the Model.");
            }
        }

        /// <summary>
        /// Add a collection of elements to the model.
        /// </summary>
        /// <param name="elements">The elements to add to the model.</param>
        public void AddElements(IEnumerable<Element> elements)
        {
            foreach(var e in elements)
            {
                AddElement(e);
            }
        }

        /// <summary>
        /// Add a material to the model.
        /// </summary>
        /// <param name="material">The material to add to the model.</param>
        private void AddMaterial(Material material)
        {
            if(!this._materials.ContainsKey(material.Name))
            {
                this._materials.Add(material.Name, material);
            }
            else
            {
                this._materials[material.Name] = material;
            }
        }

        /// <summary>
        /// Save the Model to a binary glTF file.
        /// </summary>
        /// <param name="path"></param>
        public void SaveGlb(string path)
        {
            var gltf = InitializeGlTF();

            if(File.Exists(path))
            {
                File.Delete(path);
            }

            gltf.SaveBinaryModel(_buffer.ToArray(), path);
        }

        /// <summary>
        /// Save the Model to a glTF file.
        /// </summary>
        /// <param name="path"></param>
        public void SaveGltf(string path)
        {
            var gltf = InitializeGlTF();

            if(File.Exists(path))
            {
                File.Delete(path);
            }

            var uri = Path.GetFileNameWithoutExtension(path) + ".bin";
            if(File.Exists(uri))
            {
                File.Delete(uri);
            }

            gltf.Buffers[0].Uri = uri;

            using (var fs = new FileStream(uri, FileMode.Create, FileAccess.Write))
            {
                fs.Write(_buffer.ToArray(), 0, _buffer.Count());
            }
            gltf.SaveModel(path);
        }

        private string SaveBase64()
        {
            var tmp = Path.GetTempFileName();
            var gltf = InitializeGlTF();
            gltf.SaveBinaryModel(_buffer.ToArray(), tmp);
            var bytes = File.ReadAllBytes(tmp);
            return Convert.ToBase64String(bytes);
        }

        /// <summary>
        /// Serialize the model to JSON.
        /// </summary>
        /// <returns></returns>
        public string ToJson()
        {   
            return JsonConvert.SerializeObject(this, Formatting.Indented, new JsonSerializerSettings
            {
                TypeNameHandling = TypeNameHandling.Auto
            });
        }

        /// <summary>
        /// Construct a representation of the model for sending to Hypar.
        /// </summary>
        /// <returns></returns>
        public Dictionary<string,object> ToHypar()
        {
            var model = SaveBase64();

            var result = new Dictionary<string, object>();
            result["model"] = model;
            result["elements"] = JsonConvert.SerializeObject(this._elements);
            result["origin"] = this.Origin;
            
            return result;
        }

        private Gltf InitializeGlTF()
        {
            var gltf = new Gltf();
            var asset = new Asset();
            asset.Version = "2.0";
            asset.Generator = "hypar-gltf";

            gltf.Asset = asset;

            var root = new Node();
            
            // root.Matrix = new float[]{1.0f,0.0f,0.0f,0.0f,
            //                 0.0f,0.0f,-1.0f,0.0f,
            //                 0.0f,1.0f,0.0f,0.0f,
            //                 0.0f,0.0f,0.0f,1.0f};

            root.Translation = new[]{0.0f,0.0f,0.0f};
            root.Scale = new[]{1.0f,1.0f,1.0f};

            // Set Z up by rotating -90d around the X Axis
            var q = new Quaternion(new Vector3(1,0,0), -Math.PI/2);
            root.Rotation = new[]{
                (float)q.X, (float)q.Y, (float)q.Z, (float)q.W
            };

            gltf.Nodes = new[]{root};

            gltf.Scene = 0;
            var scene = new Scene();
            scene.Nodes = new[]{0};
            gltf.Scenes = new[]{scene};

            gltf.ExtensionsUsed = new[]{"KHR_materials_pbrSpecularGlossiness"};
            
            var materials = new Dictionary<string, int>();
            foreach(var kvp in this._materials)
            {
                var m = kvp.Value;
                var mId = gltf.AddMaterial(m.Name, m.Color.Red, m.Color.Green, m.Color.Blue, m.Color.Alpha, m.SpecularFactor, m.GlossinessFactor);
                materials.Add(m.Name, mId);
            }

            foreach(var kvp in this._elements)
            {
                var e = kvp.Value;
                if(e is ITessellate<Hypar.Geometry.Mesh>)
                {
                    var mp = e as ITessellate<Hypar.Geometry.Mesh>;
                    var mesh = mp.Tessellate();
                    Transform transform = null;
                    if(e is ITransformable)
                    {
                        transform = ((ITransformable)e).Transform;
                    }
                    gltf.AddTriangleMesh(e.Id + "_mesh", _buffer, mesh.Vertices.ToArray(), mesh.Normals.ToArray(), 
                                        mesh.Indices.ToArray(), mesh.VertexColors.ToArray(), 
                                        mesh.VMin, mesh.VMax, mesh.NMin, mesh.NMax, mesh.CMin, mesh.CMax, 
                                        mesh.IMin, mesh.IMax, materials[((IMaterialize)e).Material.Name], null, transform);

                }
            }

            var buff = new glTFLoader.Schema.Buffer();
            buff.ByteLength = _buffer.Count();
            gltf.Buffers = new[]{buff};

            return gltf;
        }

        /// <summary>
        /// Add an element to the model.
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        public void Add(string key, Element value)
        {
            if(!this._elements.ContainsKey(key))
            {
                this._elements.Add(key, value);
                if(value is IMaterialize)
                {
                    AddMaterial(((IMaterialize)value).Material);
                }
            }
            else
            {
                throw new Exception("An Element with the same Id already exists in the Model.");
            }
        }

        /// <summary>
        /// Does an element with specified key exist in the model?
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public bool ContainsKey(string key)
        {
            return this.ContainsKey(key);
        }
        
        /// <summary>
        /// Remove an element from the model.
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public bool Remove(string key)
        {
            if(this.ContainsKey(key))
            {
                this.Remove(key);
                return true;
            } else 
            {
                return false;
            }
        }

        /// <summary>
        /// Try and get an element from the model given an identifier.
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public bool TryGetValue(string key, out Element value)
        {
            return this._elements.TryGetValue(key, out value);
        }

        /// <summary>
        /// And an element to the model.
        /// </summary>
        /// <param name="item"></param>
        public void Add(KeyValuePair<string, Element> item)
        {
            this._elements.Add(item.Key, item.Value);
        }

        /// <summary>
        /// Remove all elements from the model.
        /// </summary>
        public void Clear()
        {
            this._elements.Clear();
        }

        /// <summary>
        /// Does the model contain the specified element?
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        public bool Contains(KeyValuePair<string, Element> item)
        {
            return this._elements.Contains(item);
        }

        /// <summary>
        /// Copy the elements in the model to the specified array.
        /// </summary>
        /// <param name="array"></param>
        /// <param name="arrayIndex"></param>
        public void CopyTo(KeyValuePair<string, Element>[] array, int arrayIndex)
        {
            throw new NotImplementedException();
        }
        
        /// <summary>
        /// Remove an element from the model.
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        public bool Remove(KeyValuePair<string, Element> item)
        {
            if(this._elements.ContainsKey(item.Key))
            {
                this._elements.Remove(item.Key);
                return true;
            }

            return false;
        }

        /// <summary>
        /// Get the enumerator for elements in the model.
        /// </summary>
        /// <returns></returns>
        public IEnumerator<KeyValuePair<string, Element>> GetEnumerator()
        {
            return this._elements.GetEnumerator();
        }

        /// <summary>
        /// Get the enumerator for elements in the model.
        /// </summary>
        /// <returns></returns>
        IEnumerator IEnumerable.GetEnumerator()
        {
            return this._elements.GetEnumerator();
        }
    }
}