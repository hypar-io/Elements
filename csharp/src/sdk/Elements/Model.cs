using glTFLoader;
using glTFLoader.Schema;
using Hypar.Elements.Serialization;
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
    public class Model
    {
        private List<byte> _buffer = new List<byte>();
        private Dictionary<string, Material> _materials = new Dictionary<string, Material>();
        private Dictionary<string, Element> _elements = new Dictionary<string, Element>();
        private Dictionary<string, ElementType> _elementTypes = new Dictionary<string, ElementType>();

        private Dictionary<string, Profile> _profiles = new Dictionary<string, Profile>();

        /// <summary>
        /// The origin of the model.
        /// </summary>
        /// <returns></returns>
        [JsonProperty("origin")]
        public Position Origin {get;set;}

        /// <summary>
        /// All Elements in the Model.
        /// </summary>
        public Dictionary<string,Element> Elements
        {
            get{return this._elements;}
        }

        /// <summary>
        /// All Materials in the Model.
        /// </summary>
        public Dictionary<string,Material> Materials
        {
            get{return this._materials;}
        }

        /// <summary>
        /// All ElementTypes in the Model.
        /// </summary>
        public Dictionary<string, ElementType> ElementTypes
        {
            get{return this._elementTypes;}
        }

        /// <summary>
        /// All Profiles in the model.
        /// </summary>
        public Dictionary<string, Profile> Profiles
        {
            get{return this._profiles;}
        }

        /// <summary>
        /// Construct an empty model.
        /// </summary>
        public Model()
        {
            this.Origin = new Position(0,0);
            AddMaterial(BuiltInMaterials.Black);
        }

        internal Model(Dictionary<string, Element> elements, Dictionary<string, Material> materials, Dictionary<string,ElementType> elementTypes,
                        Dictionary<string, Profile> profiles)
        {
            this._elements = elements;
            this._materials = materials;
            this._elementTypes = elementTypes;
            this._profiles = profiles;
            AddMaterial(BuiltInMaterials.Black);
        }

        /// <summary>
        /// Add an element to the model.
        /// </summary>
        /// <param name="element">The element to add to the model.</param>
        /// <exception cref="System.ArgumentException">Thrown when an element with the same Id already exists in the model.</exception>
        public void AddElement(Element element)
        {
            if(!this._elements.ContainsKey(element.Id.ToString()))
            {
                this._elements.Add(element.Id.ToString(), element);
                GetRootLevelElementData(element);
            }
            else
            {
                throw new ArgumentException("An Element with the same Id already exists in the Model.");
            }
        }

        /// <summary>
        /// Recursively gather element data to be referenced at the root.
        /// This includes things like profiles, materials, and element types.
        /// </summary>
        /// <param name="element">The Element from which to gather data.</param>
        private void GetRootLevelElementData(Element element)
        {
            if(element.Material != null)
            {
                AddMaterial(element.Material);
            }

            if(element is IElementTypeProvider<WallType>)
            {
                var wtp = (IElementTypeProvider<WallType>)element;
                AddElementType(wtp.ElementType);
            }

            if(element is IElementTypeProvider<FloorType>)
            {
                var ftp = (IElementTypeProvider<FloorType>)element;
                AddElementType(ftp.ElementType);
            }

            if(element is IProfileProvider)
            {
                var ipp = (IProfileProvider)element;
                AddProfile(ipp.Profile);
            }

            if(element.SubElements.Count > 0)
            {
                foreach(var esub in element.SubElements)
                {
                    GetRootLevelElementData(esub);
                }
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

        private void AddElementType(ElementType elementType)
        {
            if(!this._elementTypes.ContainsKey(elementType.Id))
            {
                this._elementTypes.Add(elementType.Id, elementType);
            }
            else
            {
                this._elementTypes[elementType.Id] = elementType;
            }
        }

        private void AddProfile(Profile profile)
        {
            if(!this._profiles.ContainsKey(profile.Id))
            {
                this._profiles.Add(profile.Id, profile);
            }
            else
            {
                this._profiles[profile.Id] = profile;
            }
        }

        /// <summary>
        /// Get an Element by id from the Model.
        /// </summary>
        /// <param name="id">The identifier of the Element.</param>
        /// <returns>An Element or null if no Element can be found with the provided id.</returns>
        public Element GetElementById(string id)
        {
            if(this._elements.ContainsKey(id))
            {
                return this._elements[id];
            }
            return null;
        }

        /// <summary>
        /// Add a material to the model.
        /// </summary>
        /// <param name="material">The material to add to the model.</param>
        private void AddMaterial(Material material)
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
        /// Get a Material by name.
        /// </summary>
        /// <param name="name">The name of the Material.</param>
        /// <returns>A Material or null if no Material with the specified id can be found.</returns>
        public Material GetMaterialByName(string name)
        {
            return this._materials.Values.FirstOrDefault(m=>m.Name == name);
        }

        /// <summary>
        /// Get an ElementType by name.
        /// </summary>
        /// <param name="name">The name of the ElementType.</param>
        /// <returns>An ElementType or null if no ElementType with the specified name can be found.</returns>
        public ElementType GetElementTypeByName(string name)
        {
            return this._elementTypes.Values.FirstOrDefault(et=>et.Name == name);
        }

        /// <summary>
        /// Get a Profile by name.
        /// </summary>
        /// <param name="name">The name of the Profile.</param>
        /// <returns>A Profile or null if no Profile with the specified name can be found.</returns>
        public Profile GetProfileByName(string name)
        {
            return this._profiles.Values.FirstOrDefault(p=> p.Name != null && p.Name == name);
        }

        /// <summary>
        /// Get all Elements of the specified Type.
        /// </summary>
        /// <typeparam name="T">The Type of element to return.</typeparam>
        /// <returns>A collection of Elements of the specified type.</returns>
        public IEnumerable<T> ElementsOfType<T>()
        {
            return this._elements.Values.OfType<T>();
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
                Converters = new []{new ModelConverter()},
                NullValueHandling = NullValueHandling.Ignore
            });
        }
        
        /// <summary>
        /// Deserialize a model from JSON.
        /// </summary>
        /// <param name="json"></param>
        /// <returns></returns>
        public static Model FromJson(string json)
        {
            return JsonConvert.DeserializeObject<Model>(json, new JsonSerializerSettings
            {
                Converters = new []{new ModelConverter()},
                NullValueHandling = NullValueHandling.Ignore
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
                GetRenderDataForElement(e, gltf, materials);
            }

            var buff = new glTFLoader.Schema.Buffer();
            buff.ByteLength = _buffer.Count();
            gltf.Buffers = new[]{buff};

            return gltf;
        }

        private void GetRenderDataForElement(Element e, Gltf gltf, Dictionary<string, int> materials)
        {
            if(e is ITessellateMesh)
            {
                var mp = e as ITessellateMesh;
                var mesh = mp.Mesh();
                Transform transform = null;
                if(e.Transform != null)
                {
                    transform = e.Transform;
                }
                gltf.AddTriangleMesh(e.Id + "_mesh", _buffer, mesh.Vertices.ToArray(), mesh.Normals.ToArray(), 
                                    mesh.Indices.ToArray(), mesh.VertexColors.ToArray(), 
                                    mesh.VMin, mesh.VMax, mesh.NMin, mesh.NMax, mesh.CMin, mesh.CMax, 
                                    mesh.IMin, mesh.IMax, materials[e.Material.Name], null, transform);

            }

            if(e is ITessellateCurves)
            {
                var cp = e as ITessellateCurves;
                var curves = cp.Curves();
                var counter = 0;
                foreach(var c in curves)
                {
                    var vBuff = c.ToArray();
                    var indices = Enumerable.Range(0, c.Count).Select(i=>(ushort)i).ToArray();
                    var bbox = new BBox3(c);
                    gltf.AddLineLoop($"{e.Id}_curve_{counter}", _buffer, vBuff, indices, bbox.Min.ToArray(), 
                                    bbox.Max.ToArray(), 0, (ushort)(c.Count - 1), materials[BuiltInMaterials.Black.Name], e.Transform);
                }
            }
            
            if(e.SubElements.Count > 0)
            {
                foreach(var esub in e.SubElements)
                {
                    GetRenderDataForElement(esub, gltf, materials);
                }
            }
        }
    }
}