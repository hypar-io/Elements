using glTFLoader;
using glTFLoader.Schema;
using Elements.Serialization;
using Elements.Geometry;
using Elements.GeoJSON;
using Elements.Geometry.Interfaces;
using Elements.Interfaces;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json;
using System.Reflection;

namespace Elements
{
    /// <summary>
    /// A model is a map of elements, keyed by their unique identifier. 
    /// </summary>
    public class Model
    {
        private List<byte> _buffer = new List<byte>();
        private Dictionary<long, Material> _materials = new Dictionary<long, Material>();
        private Dictionary<long, Element> _elements = new Dictionary<long, Element>();
        private Dictionary<long, ElementType> _elementTypes = new Dictionary<long, ElementType>();

        private Dictionary<long, IProfile> _profiles = new Dictionary<long, IProfile>();

        /// <summary>
        /// The version of the assembly.
        /// </summary>
        [JsonProperty("version")]
        public string Version => Assembly.GetExecutingAssembly().GetName().Version.ToString();

        /// <summary>
        /// The origin of the model.
        /// </summary>
        [JsonProperty("origin")]
        public Position Origin { get; set; }

        /// <summary>
        /// All Elements in the Model.
        /// </summary>
        [JsonProperty("elements")]
        public Dictionary<long, Element> Elements
        {
            get { return this._elements; }
        }

        /// <summary>
        /// All Materials in the Model.
        /// </summary>
        [JsonProperty("materials")]
        public Dictionary<long, Material> Materials
        {
            get { return this._materials; }
        }

        /// <summary>
        /// All ElementTypes in the Model.
        /// </summary>
        [JsonProperty("element_types")]
        public Dictionary<long, ElementType> ElementTypes
        {
            get { return this._elementTypes; }
        }

        /// <summary>
        /// All Profiles in the model.
        /// </summary>
        [JsonProperty("profiles")]
        public Dictionary<long, IProfile> Profiles
        {
            get { return this._profiles; }
        }

        /// <summary>
        /// Construct an empty model.
        /// </summary>
        public Model()
        {
            this.Origin = new Position(0, 0);
            AddMaterial(BuiltInMaterials.Edges);
        }

        internal Model(Dictionary<long, Element> elements, Dictionary<long, Material> materials, Dictionary<long, ElementType> elementTypes,
                        Dictionary<long, IProfile> profiles)
        {
            this._elements = elements;
            this._materials = materials;
            this._elementTypes = elementTypes;
            this._profiles = profiles;
            AddMaterial(BuiltInMaterials.Edges);
        }

        /// <summary>
        /// Add an element to the model.
        /// </summary>
        /// <param name="element">The element to add to the model.</param>
        /// <exception cref="System.ArgumentException">Thrown when an element with the same Id already exists in the model.</exception>
        public void AddElement(Element element)
        {
            if (element == null)
            {
                return;
            }

            if (!this._elements.ContainsKey(element.Id))
            {
                this._elements.Add(element.Id, element);
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
        private void GetRootLevelElementData(IElement element)
        {
            if (element is IGeometry3D)
            {
                var geo = (IGeometry3D)element;
                foreach(var brep in geo.Geometry)
                {
                    AddMaterial(brep.Material);

                    if(brep is IProfileProvider)
                    {
                        var ipp = (IProfileProvider)brep;
                        if(ipp.Profile != null)
                        {
                            AddProfile((Profile)ipp.Profile);
                        }
                    }
                }
            }

            if (element is IElementTypeProvider<WallType>)
            {
                var wtp = (IElementTypeProvider<WallType>)element;
                if(wtp.ElementType != null)
                {
                    AddElementType(wtp.ElementType);
                }
            }

            if (element is IElementTypeProvider<FloorType>)
            {
                var ftp = (IElementTypeProvider<FloorType>)element;
                if(ftp.ElementType != null)
                {
                    AddElementType(ftp.ElementType);
                }
            }

            if (element is IAggregateElement)
            {
                var ae = (IAggregateElement)element;
                if (ae.Elements.Count > 0)
                {
                    foreach (var esub in ae.Elements)
                    {
                        GetRootLevelElementData(esub);
                    }
                }
            }
        }

        /// <summary>
        /// Add a collection of elements to the model.
        /// </summary>
        /// <param name="elements">The elements to add to the model.</param>
        public void AddElements(IEnumerable<Element> elements)
        {
            foreach (var e in elements)
            {
                AddElement(e);
            }
        }

        private void AddElementType(ElementType elementType)
        {
            if (!this._elementTypes.ContainsKey(elementType.Id))
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
            if (!this._profiles.ContainsKey(profile.Id))
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
        public Element GetElementById(int id)
        {
            if (this._elements.ContainsKey(id))
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
            if (!this._materials.ContainsKey(material.Id))
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
            return this._materials.Values.FirstOrDefault(m => m.Name == name);
        }

        /// <summary>
        /// Get an ElementType by name.
        /// </summary>
        /// <param name="name">The name of the ElementType.</param>
        /// <returns>An ElementType or null if no ElementType with the specified name can be found.</returns>
        public ElementType GetElementTypeByName(string name)
        {
            return this._elementTypes.Values.FirstOrDefault(et => et.Name == name);
        }

        /// <summary>
        /// Get a Profile by name.
        /// </summary>
        /// <param name="name">The name of the Profile.</param>
        /// <returns>A Profile or null if no Profile with the specified name can be found.</returns>
        public IProfile GetProfileByName(string name)
        {
            return this._profiles.Values.FirstOrDefault(p => p.Name != null && p.Name == name);
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
            if (File.Exists(path))
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

            if (File.Exists(path))
            {
                File.Delete(path);
            }

            var uri = Path.GetFileNameWithoutExtension(path) + ".bin";
            if (File.Exists(uri))
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

        /// <summary>
        /// Convert the Model to a base64 encoded string.
        /// </summary>
        /// <returns></returns>
        public string ToBase64String()
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
        public string ToJson(bool indented = false)
        {
            var result = JsonConvert.SerializeObject(this, Formatting.Indented, new JsonSerializerSettings
            {
                Converters = new[] { new ModelConverter() },
                NullValueHandling = NullValueHandling.Ignore
            });
            if (indented)
            {
                return result.Replace("\n", "").Replace("\r\n", "").Replace("\t", "").Replace("  ", "");
            }
            else
            {
                return result;
            }
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
                Converters = new[] { new ModelConverter() },
                NullValueHandling = NullValueHandling.Ignore
            });
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

            root.Translation = new[] { 0.0f, 0.0f, 0.0f };
            root.Scale = new[] { 1.0f, 1.0f, 1.0f };

            // Set Z up by rotating -90d around the X Axis
            var q = new Quaternion(new Vector3(1, 0, 0), -Math.PI / 2);
            root.Rotation = new[]{
                (float)q.X, (float)q.Y, (float)q.Z, (float)q.W
            };

            gltf.Nodes = new[] { root };

            gltf.Scene = 0;
            var scene = new Scene();
            scene.Nodes = new[] { 0 };
            gltf.Scenes = new[] { scene };

            gltf.ExtensionsUsed = new[] { "KHR_materials_pbrSpecularGlossiness" };

            var materials = new Dictionary<string, int>();
            foreach (var kvp in this._materials)
            {
                var m = kvp.Value;
                if(materials.ContainsKey(m.Name))
                {
                    continue;
                }
                
                var mId = gltf.AddMaterial(m.Name, m.Color.Red, m.Color.Green, m.Color.Blue, m.Color.Alpha, m.SpecularFactor, m.GlossinessFactor);
                materials.Add(m.Name, mId);
            }

            var tmId = gltf.AddMaterial("x_axis", 1.0f, 0.0f, 0.0f, 1.0f, 0.0f, 0.0f);
            materials.Add("x_axis", tmId);
            tmId = gltf.AddMaterial("y_axis", 0.0f, 1.0f, 0.0f, 1.0f, 0.0f, 0.0f);
            materials.Add("y_axis", tmId);
            tmId = gltf.AddMaterial("z_axis", 0.0f, 0.0f, 1.0f, 1.0f, 0.0f, 0.0f);
            materials.Add("z_axis", tmId);

            foreach (var kvp in this._elements)
            {
                var e = kvp.Value;
                GetRenderDataForElement(e, gltf, materials);
            }

            var buff = new glTFLoader.Schema.Buffer();
            buff.ByteLength = _buffer.Count();
            gltf.Buffers = new[] { buff };

            return gltf;
        }

        private void GetRenderDataForElement(IElement e, Gltf gltf, Dictionary<string, int> materials)
        {
            AddTransformVisualization(IdProvider.Instance.GetNextId(), e.Transform, gltf, materials);

            if (e is IGeometry3D)
            {
                var geo = e as IGeometry3D;

                Elements.Geometry.Mesh mesh = null;

                foreach(var brep in geo.Geometry)
                {
                    if (brep is IExtrudeAlongCurve)
                    {
                        var eac = (IExtrudeAlongCurve)brep;

                        // var frames = eac.Curve.Frames(0, 0);
                        // foreach (var f in frames)
                        // {
                        //     var x = new Elements.Geometry.Line(f.Origin, f.Origin + f.XAxis * 2);
                        //     AddCurve(e.Id + 10000, x, gltf, materials["x_axis"], e.Transform);
                        //     var y = new Elements.Geometry.Line(f.Origin, f.Origin + f.YAxis * 2);
                        //     AddCurve(e.Id + 10001, y, gltf, materials["y_axis"], e.Transform);
                        //     var z = new Elements.Geometry.Line(f.Origin, f.Origin + f.ZAxis * 2);
                        //     AddCurve(e.Id + 10002, z, gltf, materials["z_axis"], e.Transform);
                        // }
                        // Add the center curve
                        AddCurve(e.Id, eac.Curve, gltf, materials[BuiltInMaterials.Edges.Name], e.Transform);
                    }

                    mesh = new Elements.Geometry.Mesh();
                    foreach (var f in brep.Faces)
                    {
                        // Draw a winding indicator
                        // AddArrow(e.Id + 10003, f.Vertices[0], (f.Vertices[1] - f.Vertices[0]).Normalized(), gltf, materials["z_axis"], e.Transform);
                        f.Tessellate(mesh);
                        // foreach(var edge in f.Edges)
                        // {
                        //     AddCurve(e.Id, edge, gltf, materials[BuiltInMaterials.Edges.Name], e.Transform);
                        // }
                    }

                    gltf.AddTriangleMesh(e.Id + "_mesh", _buffer, mesh.Vertices.ToArray(), mesh.Normals.ToArray(),
                                        mesh.Indices.ToArray(), mesh.VertexColors.ToArray(),
                                        mesh.VMin, mesh.VMax, mesh.NMin, mesh.NMax, mesh.CMin, mesh.CMax,
                                        mesh.IMin, mesh.IMax, materials[brep.Material.Name], null, e.Transform);
                }
            }

            if (e is IAggregateElement)
            {
                var ae = (IAggregateElement)e;

                if (ae.Elements.Count > 0)
                {
                    foreach (var esub in ae.Elements)
                    {
                        GetRenderDataForElement(esub, gltf, materials);
                    }
                }
            }

        }

        private void AddTransformVisualization(long id, Transform f, Gltf gltf, Dictionary<string, int> materials)
        {
            if (f == null)
            {
                return;
            }
            
            var x = new Elements.Geometry.Line(f.Origin, f.Origin + f.XAxis * 2);
            AddCurve(id + 10000, x, gltf, materials["x_axis"]);
            var y = new Elements.Geometry.Line(f.Origin, f.Origin + f.YAxis * 2);
            AddCurve(id + 10001, y, gltf, materials["y_axis"]);
            var z = new Elements.Geometry.Line(f.Origin, f.Origin + f.ZAxis * 2);
            AddCurve(id + 10002, z, gltf, materials["z_axis"]);
        }

        private void AddCurve(long id, ICurve curve, Gltf gltf, int material, Transform t = null)
        {
            var vBuff = curve.Vertices.ToArray();
            var vCount = curve.Vertices.Count();
            var indices = Enumerable.Range(0, vCount).Select(i => (ushort)i).ToArray();
            var bbox = new BBox3(curve.Vertices);
            gltf.AddLineLoop($"{id}_curve", _buffer, vBuff, indices, bbox.Min.ToArray(),
                            bbox.Max.ToArray(), 0, (ushort)(vCount - 1), material, t);

        }

        private void AddArrow(long id, Vector3 origin, Vector3 direction, Gltf gltf, int material, Transform t)
        {
            var scale = 0.5;
            var end = origin + direction * scale;
            var up = direction.IsParallelTo(Vector3.ZAxis) ? Vector3.YAxis : Vector3.ZAxis;
            var tr = new Transform(Vector3.Origin, direction.Cross(up), direction);
            tr.Rotate(up, -45.0);
            var arrow1 = tr.OfPoint(Vector3.XAxis * 0.1);
            var pts = new[] { origin, end, end + arrow1 };
            var vBuff = pts.ToArray();
            var vCount = 3;
            var indices = Enumerable.Range(0, vCount).Select(i => (ushort)i).ToArray();
            var bbox = new BBox3(pts);
            gltf.AddLineLoop($"{id}_curve", _buffer, vBuff, indices, bbox.Min.ToArray(),
                            bbox.Max.ToArray(), 0, (ushort)(vCount - 1), material, t);

        }
    }
}