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
using STEP;
using IFC.Storage;
using IFC;
using Hypar.Elements.Interfaces;

namespace Elements
{
    /// <summary>
    /// A container for Elements, Element Types, Materials, and Profiles.
    /// </summary>
    public class Model
    {
        private List<byte> _buffer = new List<byte>();
        private Dictionary<long, Material> _materials = new Dictionary<long, Material>();
        private Dictionary<long, Element> _elements = new Dictionary<long, Element>();

        private Dictionary<long, ElementType> _elementTypes = new Dictionary<long, ElementType>();

        private Dictionary<long, Profile> _profiles = new Dictionary<long, Profile>();

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
        public Dictionary<long, Profile> Profiles
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
        /// Get the first Element with the specified name.
        /// </summary>
        /// <param name="name"></param>
        /// <returns>An Element or null if no Element can be found with the provided name.</returns>
        public Element GetElementByName(string name)
        {
            var found = this.Elements.FirstOrDefault(e => e.Value.Name == name);
            if (found.Equals(new KeyValuePair<long, Element>()))
            {
                return null;
            }
            return found.Value;
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
        public Profile GetProfileByName(string name)
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
        /// <returns>A Base64 string representing the Model.</returns>
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
        /// <returns>A JSON string representing the Model.</returns>
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
        /// <param name="json">The JSON to deserialize to a Model.</param>
        /// <returns>A Model.</returns>
        public static Model FromJson(string json)
        {
            return JsonConvert.DeserializeObject<Model>(json, new JsonSerializerSettings
            {
                Converters = new[] { new ModelConverter() },
                NullValueHandling = NullValueHandling.Ignore
            });
        }

        /// <summary>
        /// Construct a Model from an IFC STEP file.
        /// </summary>
        /// <param name="ifcPath">The path to the IFC file on disk.</param>
        public static Model FromIFC(string ifcPath)
        {
            IList<STEPError> errors;
            var ifcModel = new IFC.Model(ifcPath, new LocalStorageProvider(), out errors);

            // var materials = ifcModel.AllInstancesOfType<IfcMaterial>();

            var floorType = new FloorType("IFC Floor", 0.1);
            var ifcSlabs = ifcModel.AllInstancesOfType<IfcSlab>();
            var ifcSpaces = ifcModel.AllInstancesOfType<IfcSpace>();
            var ifcWalls = ifcModel.AllInstancesOfType<IfcWallStandardCase>();
            var ifcBeams = ifcModel.AllInstancesOfType<IfcBeam>();
            var ifcColumns = ifcModel.AllInstancesOfType<IfcColumn>();
            var ifcVoids = ifcModel.AllInstancesOfType<IfcRelVoidsElement>();
            // var stories = ifcModel.AllInstancesOfType<IfcBuildingStorey>();
            // var relContains = ifcModel.AllInstancesOfType<IfcRelContainedInSpatialStructure>();

            var slabs = ifcSlabs.Select(s => s.ToFloor());
            var spaces = ifcSpaces.Select(sp => sp.ToSpace());
            var openings = new List<Opening>();
            foreach (var v in ifcVoids)
            {
                var element = v.RelatingBuildingElement;
                // var elementTransform = element.ObjectPlacement.ToTransform();
                var o = ((IfcOpeningElement)v.RelatedOpeningElement).ToOpening();
                openings.Add(o);
            }
            var walls = ifcWalls.Select(w => w.ToWall());
            var beams = ifcBeams.Select(b => b.ToBeam());
            var columns = ifcColumns.Select(c => c.ToColumn());

            var model = new Model();
            model.AddElements(slabs);
            model.AddElements(spaces);
            model.AddElements(walls);
            model.AddElements(beams);
            model.AddElements(columns);
            // if (openings.Any())
            // {
            //     model.AddElements(openings);
            // }

            return model;
        }

        internal Model(Dictionary<long, Element> elements, Dictionary<long, Material> materials, Dictionary<long, ElementType> elementTypes,
                        Dictionary<long, Profile> profiles)
        {
            this._elements = elements;
            this._materials = materials;
            this._elementTypes = elementTypes;
            this._profiles = profiles;
            AddMaterial(BuiltInMaterials.Edges);
            AddMaterial(BuiltInMaterials.Void);
        }

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

        private void GetRootLevelElementData(IElement element)
        {
            if (element is IGeometry3D)
            {
                var geo = (IGeometry3D)element;
                foreach (var solid in geo.Geometry)
                {
                    AddMaterial(solid.Material);
                }
            }

            if (element is IProfileProvider)
            {
                var ipp = (IProfileProvider)element;
                if (ipp.Profile != null)
                {
                    AddProfile((Profile)ipp.Profile);
                }
            }

            if (element is IElementTypeProvider<WallType>)
            {
                var wtp = (IElementTypeProvider<WallType>)element;
                if (wtp.ElementType != null)
                {
                    AddElementType(wtp.ElementType);
                }
            }

            if (element is IElementTypeProvider<FloorType>)
            {
                var ftp = (IElementTypeProvider<FloorType>)element;
                if (ftp.ElementType != null)
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

        private Gltf InitializeGlTF()
        {
            var gltf = new Gltf();
            var asset = new Asset();
            asset.Version = "2.0";
            asset.Generator = "hypar-gltf";

            gltf.Asset = asset;

            var root = new Node();

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

            var materialsToAdd = this._materials.Values.ToList();
            materialsToAdd.Add(BuiltInMaterials.XAxis);
            materialsToAdd.Add(BuiltInMaterials.YAxis);
            materialsToAdd.Add(BuiltInMaterials.ZAxis);
            materialsToAdd.Add(BuiltInMaterials.Edges);
            materialsToAdd.Add(BuiltInMaterials.EdgesHighlighted);

            var materials = gltf.AddMaterials(materialsToAdd);

            var lines = new List<Vector3>();

            foreach (var kvp in this._elements)
            {
                var e = kvp.Value;
                GetRenderDataForElement(e, gltf, materials, lines);
            }

            AddLines(100000, lines.ToArray(), gltf, materials[BuiltInMaterials.Edges.Name], null);

            var buff = new glTFLoader.Schema.Buffer();
            buff.ByteLength = _buffer.Count();
            gltf.Buffers = new[] { buff };

            return gltf;
        }

        private void GetRenderDataForElement(IElement e, Gltf gltf, Dictionary<string, int> materials, List<Vector3> lines)
        {
            if (e is IGeometry3D)
            {
                var geo = e as IGeometry3D;

                Elements.Geometry.Mesh mesh = null;

                foreach (var solid in geo.Geometry)
                {
                    foreach (var edge in solid.Edges.Values)
                    {
                        if(e.Transform != null)
                        {
                            lines.AddRange(new[] { e.Transform.OfVector(edge.Left.Vertex.Point), e.Transform.OfVector(edge.Right.Vertex.Point) });
                        }
                        else
                        {
                            lines.AddRange(new[] { edge.Left.Vertex.Point, edge.Right.Vertex.Point });
                        }
                    }

                    mesh = new Elements.Geometry.Mesh();
                    solid.Tessellate(ref mesh);
                    gltf.AddTriangleMesh(e.Id + "_mesh", _buffer, mesh.Vertices.ToArray(), mesh.Normals.ToArray(),
                                        mesh.Indices.ToArray(), mesh.VMin, mesh.VMax, mesh.NMin, mesh.NMax,
                                        mesh.IMin, mesh.IMax, materials[solid.Material.Name], null, e.Transform);
                }
            }

            if (e is IAggregateElement)
            {
                var ae = (IAggregateElement)e;

                if (ae.Elements.Count > 0)
                {
                    foreach (var esub in ae.Elements)
                    {
                        GetRenderDataForElement(esub, gltf, materials, lines);
                    }
                }
            }
        }

        private void AddLines(long id, Vector3[] vertices, Gltf gltf, int material, Transform t = null)
        {
            var vBuff = vertices.ToArray();
            var vCount = vertices.Length;
            var indices = new List<ushort>();
            for (ushort i = 0; i < vertices.Length; i += 2)
            {
                indices.Add(i);
                indices.Add((ushort)(i + 1));
            }
            // var indices = Enumerable.Range(0, vCount).Select(i => (ushort)i).ToArray();
            var bbox = new BBox3(vertices);
            gltf.AddLineLoop($"{id}_curve", _buffer, vBuff, indices.ToArray(), bbox.Min.ToArray(),
                            bbox.Max.ToArray(), 0, (ushort)(vCount - 1), material, MeshPrimitive.ModeEnum.LINES, t);
        }

        private void AddArrow(long id, Vector3 origin, Vector3 direction, Gltf gltf, int material, Transform t)
        {
            var scale = 0.5;
            var end = origin + direction * scale;
            var up = direction.IsParallelTo(Vector3.ZAxis) ? Vector3.YAxis : Vector3.ZAxis;
            var tr = new Transform(Vector3.Origin, direction.Cross(up), direction);
            tr.Rotate(up, -45.0);
            var arrow1 = tr.OfVector(Vector3.XAxis * 0.1);
            var pts = new[] { origin, end, end + arrow1 };
            var vBuff = pts.ToArray();
            var vCount = 3;
            var indices = Enumerable.Range(0, vCount).Select(i => (ushort)i).ToArray();
            var bbox = new BBox3(pts);
            gltf.AddLineLoop($"{id}_curve", _buffer, vBuff, indices, bbox.Min.ToArray(),
                            bbox.Max.ToArray(), 0, (ushort)(vCount - 1), material, MeshPrimitive.ModeEnum.LINE_STRIP, t);

        }
    }
}