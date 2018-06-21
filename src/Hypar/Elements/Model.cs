using glTFLoader;
using glTFLoader.Schema;
using Hypar.Geometry;
using Hypar.GeoJSON;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json;

namespace Hypar.Elements
{
    /// <summary>
    /// A container for Meshes and Materials
    /// </summary>
    public class Model
    {
        private List<byte> _buffer = new List<byte>();
        private Dictionary<string, Material> _materials = new Dictionary<string, Material>();
        private Dictionary<string, Element> _elements = new Dictionary<string, Element>();

        private Dictionary<string, IDataExtractor> _dataExtractors = new Dictionary<string,IDataExtractor>();

        /// <summary>
        /// All materials in the model.
        /// </summary>
        /// <returns></returns>
        public Dictionary<string, Material> Materials
        {
            get{return _materials;}
        }

        /// <summary>
        /// All elements in the model.
        /// </summary>
        /// <returns></returns>
        public Dictionary<string,Element> Elements
        {
            get{return _elements;}
        }

        /// <summary>
        /// The origin of the model.
        /// </summary>
        /// <returns></returns>
        public Position Origin {get;set;}

        /// <summary>
        /// Construct an empty model.
        /// </summary>
        public Model()
        {
            this.Origin = new Position(0,0);
        }

        /// <summary>
        /// Add a data extractor to the model.
        /// </summary>
        /// <param name="name"></param>
        /// <param name="extractor"></param>
        public void AddDataExtractor(string name, IDataExtractor extractor)
        {
            if(this._dataExtractors.ContainsKey(name))
            {
                throw new Exception($"A data extractor with the name {name} already exists. Data extractors must have a unique name.");
            }
            this._dataExtractors.Add(name, extractor);
        }

        /// <summary>
        /// Add a Mesh to the Model.
        /// </summary>
        /// <param name="element">The element to add to the model.</param>
        public void AddElement(Element element)
        {
            if(!this._elements.ContainsKey(element.Id.ToString()))
            {
                this._elements.Add(element.Id.ToString(), element);
                AddMaterial(element.Material);
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
        public void AddMaterial(Material material)
        {
            if(!this._materials.ContainsKey(material.Id))
            {
                this._materials.Add(material.Id, material);
            }
            else
            {
                this._materials[material.Id] = material;
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
        public string ToJSON()
        {
            return JsonConvert.SerializeObject(this);
        }

        /// <summary>
        /// Construct a representation of the model for sending to Hypar.
        /// </summary>
        /// <returns></returns>
        public Dictionary<string,object> ToHypar()
        {
            var model = SaveBase64();

            var computed = new Dictionary<string,object>();
            foreach(var kvp in this._dataExtractors)
            {
                computed.Add(kvp.Key, kvp.Value.ExtractData(this));
            }

            var result = new Dictionary<string, object>();
            result["model"] = model;
            if(computed.Any())
            {
                result["computed"] = computed;
            }

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
                var mId = gltf.AddMaterial(m.Id, m.Red, m.Green, m.Blue, m.Alpha, m.SpecularFactor, m.GlossinessFactor);
                materials.Add(m.Id, mId);
            }

            foreach(var kvp in this._elements)
            {
                var e = kvp.Value;
                if(e is ITessellate<Hypar.Geometry.Mesh>)
                {
                    var mp = e as ITessellate<Hypar.Geometry.Mesh>;
                    var mesh = mp.Tessellate();
                    gltf.AddTriangleMesh(e.Id + "_mesh", _buffer, mesh.Vertices.ToArray(), mesh.Normals.ToArray(), 
                                        mesh.Indices.ToArray(), mesh.VertexColors.ToArray(), 
                                        mesh.VMin, mesh.VMax, mesh.NMin, mesh.NMax, mesh.CMin, mesh.CMax, 
                                        mesh.IMin, mesh.IMax, materials[e.Material.Id], null, e.Transform);

                }
            }

            var buff = new glTFLoader.Schema.Buffer();
            buff.ByteLength = _buffer.Count();
            gltf.Buffers = new[]{buff};

            return gltf;
        }
    }
}