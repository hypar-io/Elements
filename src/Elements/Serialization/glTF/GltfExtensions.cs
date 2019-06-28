using Elements.Geometry;
using System;
using System.Collections.Generic;
// TODO: Get rid of System.Linq
using System.Linq;
using glTFLoader;
using glTFLoader.Schema;
using System.IO;
using System.Runtime.CompilerServices;
using Elements.Geometry.Solids;
using Elements.Interfaces;
using Elements.Geometry.Interfaces;
using System.Diagnostics;

[assembly: InternalsVisibleTo("Hypar.Elements.Tests")]

namespace Elements.Serialization.glTF
{
    /// <summary>
    /// Extensions for glTF serialization.
    /// </summary>
    public static class GltfExtensions
    {
        /// <summary>
        /// Save a model to gltf.
        /// </summary>
        /// <param name="model">The model to serialize.</param>
        /// <param name="path">The output path.</param>
        /// <param name="useBinarySerialization">Should binary serialization be used?</param>
        public static void ToGlTF(this Model model, string path, bool useBinarySerialization = true)
        {
            if(useBinarySerialization)
            {
                SaveGlb(model, path);
            }
            else
            {
                SaveGltf(model, path);
            }
        }

        /// <summary>
        /// Convert the Model to a base64 encoded string.
        /// </summary>
        /// <returns>A Base64 string representing the Model.</returns>
        public static string ToBase64String(this Model model)
        {
            var buffer = new List<byte>();
            var tmp = Path.GetTempFileName();
            var gltf = InitializeGlTF(model, buffer);
            gltf.SaveBinaryModel(buffer.ToArray(), tmp);
            var bytes = File.ReadAllBytes(tmp);
            return Convert.ToBase64String(bytes);
        }

        internal static Dictionary<string,int> AddMaterials(this Gltf gltf, IList<Material> materials)
        {
            var materialDict = new Dictionary<string, int>();
            var newMaterials = new List<glTFLoader.Schema.Material>();
            var matId = 0;

            foreach(var material in materials)
            {
                if(materialDict.ContainsKey(material.Name))
                {
                    continue;
                }

                var m = new glTFLoader.Schema.Material();
                newMaterials.Add(m);

                m.PbrMetallicRoughness = new MaterialPbrMetallicRoughness();
                m.PbrMetallicRoughness.BaseColorFactor = new[]{material.Color.Red,material.Color.Green,material.Color.Blue,material.Color.Alpha};
                m.PbrMetallicRoughness.MetallicFactor = 1.0f;
                m.DoubleSided = material.DoubleSided;
                m.Name = material.Name;

                if(material.Color.Alpha < 1.0)
                {
                    m.AlphaMode = glTFLoader.Schema.Material.AlphaModeEnum.BLEND;
                }
                else
                {
                    m.AlphaMode = glTFLoader.Schema.Material.AlphaModeEnum.OPAQUE;
                }

                m.Extensions = new Dictionary<string, object>{
                    {"KHR_materials_pbrSpecularGlossiness", new Dictionary<string,object>{
                        {"diffuseFactor", new[]{material.Color.Red,material.Color.Green,material.Color.Blue,material.Color.Alpha}},
                        {"specularFactor", new[]{material.SpecularFactor, material.SpecularFactor, material.SpecularFactor}},
                        {"glossinessFactor", material.GlossinessFactor}
                    }}
                };

                materialDict.Add(m.Name, matId);
                matId++;
            }

            gltf.Materials = newMaterials.ToArray();

            return materialDict;
        }

        private static int AddAccessor(List<Accessor> accessors, int bufferView, int byteOffset, Accessor.ComponentTypeEnum componentType, int count, float[] min, float[] max, Accessor.TypeEnum accessorType)
        {
            var a = new Accessor();
            a.BufferView = bufferView;
            a.ByteOffset = byteOffset;
            a.ComponentType = componentType;
            a.Min = min;
            a.Max = max;
            a.Type = accessorType;
            a.Count = count;

            accessors.Add(a);

            return accessors.Count - 1;
        }

        private static int AddBufferView(List<BufferView> bufferViews, int buffer, int byteOffset, int byteLength, BufferView.TargetEnum? target, int? byteStride)
        {
            var b = new BufferView();
            b.Buffer = buffer;
            b.ByteLength = byteLength;
            b.ByteOffset = byteOffset;
            b.Target = target;
            b.ByteStride = byteStride;

            bufferViews.Add(b);

            return bufferViews.Count - 1;
        }
        
        private static int AddNode(this Gltf gltf, Node n, int? parent)
        {
            // TODO: Get rid of this resizing.
            var nodes = gltf.Nodes.ToList();
            nodes.Add(n);
            gltf.Nodes = nodes.ToArray();
            var id = gltf.Nodes.Length - 1;

            if(parent != null)
            {
                if(gltf.Nodes[(int)parent].Children == null)
                {
                    gltf.Nodes[(int)parent].Children = new[]{id};
                }
                else
                {
                    // TODO: Get rid of this resizing.
                    var children = gltf.Nodes[(int)parent].Children.ToList();
                    children.Add(id);
                    gltf.Nodes[(int)parent].Children = children.ToArray();
                }
                
            }

            return id;
        }

        internal static int AddTriangleMesh(this Gltf gltf, string name, List<byte> buffer, List<BufferView> bufferViews, List<Accessor> accessors, byte[] vertices, byte[] normals, byte[] indices, byte[] colors,
        double[] vMin, double[] vMax, double[] nMin, double[] nMax, ushort iMin, ushort iMax, int materialId, float[] cMin, float[] cMax, int? parent_index, Transform transform = null)
        {
            var m = new glTFLoader.Schema.Mesh();
            m.Name = name;
            
            var sw = new Stopwatch();
            sw.Start();
            var vBuff = AddBufferView(bufferViews, 0, buffer.Count, vertices.Length, null, null);
            var nBuff = AddBufferView(bufferViews, 0, buffer.Count + vertices.Length, normals.Length, null, null);
            var iBuff = AddBufferView(bufferViews, 0, buffer.Count + vertices.Length + normals.Length, indices.Length, null, null);
            sw.Stop();
            Console.WriteLine($"glTF:\t\t\t{sw.Elapsed} for adding buffer views.");
            sw.Reset();

            sw.Start();
            buffer.AddRange(vertices);
            buffer.AddRange(normals);
            buffer.AddRange(indices);
            sw.Stop();
            Console.WriteLine($"glTF:\t\t\t{sw.Elapsed} for extending the buffer.");
            sw.Reset();
            
            while(buffer.Count % 4 != 0)
            {
                Console.WriteLine("Padding...");
                buffer.Add(0);
            }

            var vAccess = AddAccessor(accessors, vBuff, 0, Accessor.ComponentTypeEnum.FLOAT, vertices.Length/sizeof(float)/3, new[]{(float)vMin[0], (float)vMin[1], (float)vMin[2]}, new[]{(float)vMax[0],(float)vMax[1],(float)vMax[2]}, Accessor.TypeEnum.VEC3);
            var nAccess = AddAccessor(accessors, nBuff, 0, Accessor.ComponentTypeEnum.FLOAT, normals.Length/sizeof(float)/3, new[]{(float)nMin[0], (float)nMin[1], (float)nMin[2]}, new[]{(float)nMax[0], (float)nMax[1], (float)nMax[2]}, Accessor.TypeEnum.VEC3);
            var iAccess = AddAccessor(accessors, iBuff, 0, Accessor.ComponentTypeEnum.UNSIGNED_SHORT, indices.Length/sizeof(ushort), new[]{(float)iMin}, new[]{(float)iMax}, Accessor.TypeEnum.SCALAR);
            
            var prim = new MeshPrimitive();
            prim.Indices = iAccess;
            prim.Material = materialId;
            prim.Mode = MeshPrimitive.ModeEnum.TRIANGLES;
            prim.Attributes = new Dictionary<string,int>{
                {"NORMAL",nAccess},
                {"POSITION",vAccess}
            };

            // TODO: Add to the buffer above instead of inside this block.
            // There's a chance the padding operation will put padding before
            // the color information.
            if(colors.Length > 0)
            {
                var cBuff = AddBufferView(bufferViews, 0, buffer.Count, colors.Length, null, null);
                buffer.AddRange(colors);

                var cAccess = AddAccessor(accessors, cBuff, 0, Accessor.ComponentTypeEnum.FLOAT, colors.Length/3, cMin, cMax, Accessor.TypeEnum.VEC3);
                prim.Attributes.Add("COLOR_0", cAccess);
            }

            m.Primitives = new[]{prim};

            // Add mesh to gltf
            if(gltf.Meshes != null) 
            {
                // TODO: Get rid of this resizing.
                var meshes = gltf.Meshes.ToList();
                meshes.Add(m);
                gltf.Meshes = meshes.ToArray();
            }
            else
            {
                gltf.Meshes = new[]{m};
            }

            var parentId = 0;
            
            if(transform != null)
            {
                var a = transform.XAxis;
                var b = transform.YAxis;
                var c = transform.ZAxis;

                var transNode = new Node();

                transNode.Matrix = new[]{
                    (float)a.X, (float)a.Y, (float)a.Z, 0.0f,
                    (float)b.X, (float)b.Y, (float)b.Z, 0.0f,
                    (float)c.X, (float)c.Y, (float)c.Z, 0.0f,
                    (float)transform.Origin.X,(float)transform.Origin.Y,(float)transform.Origin.Z, 1.0f
                };

                parentId = gltf.AddNode(transNode, 0);
            }

            // Add mesh node to gltf
            var node = new Node();
            node.Mesh = gltf.Meshes.Length - 1;
            gltf.AddNode(node, parentId);
            
            return gltf.Meshes.Length - 1;
        }
    
        internal static int AddLineLoop(this Gltf gltf, string name, List<byte> buffer, List<BufferView> bufferViews, List<Accessor> accessors, double[] vertices, ushort[] indices, double[] vMin, double[] vMax, ushort iMin, ushort iMax, int materialId, MeshPrimitive.ModeEnum mode, Transform transform = null)
        {
            var m = new glTFLoader.Schema.Mesh();
            m.Name = name;
            var vBuff = AddBufferView(bufferViews, 0, buffer.Count, vertices.Length * sizeof(float), null, null);
            var iBuff = AddBufferView(bufferViews, 0, buffer.Count + vertices.Length * sizeof(float), indices.Length * sizeof(ushort), null, null);

            for(var i=0;i<vertices.Length; i++)
            {
                buffer.AddRange(BitConverter.GetBytes((float)vertices[i]));
            }
            for(var i=0;i<indices.Length; i++)
            {
                buffer.AddRange(BitConverter.GetBytes(indices[i]));
            }

            while(buffer.Count % 4 != 0)
            {
                // Console.WriteLine("Padding...");
                buffer.Add(0);
            }
            
            var vAccess = AddAccessor(accessors, vBuff, 0, Accessor.ComponentTypeEnum.FLOAT, vertices.Length/3, new[]{(float)vMin[0], (float)vMin[1], (float)vMin[2]}, new[]{(float)vMax[0],(float)vMax[1],(float)vMax[2]}, Accessor.TypeEnum.VEC3);
            var iAccess = AddAccessor(accessors, iBuff, 0, Accessor.ComponentTypeEnum.UNSIGNED_SHORT, indices.Length, new[]{(float)iMin}, new[]{(float)iMax}, Accessor.TypeEnum.SCALAR);

            var prim = new MeshPrimitive();
            prim.Indices = iAccess;
            prim.Material = materialId;
            prim.Mode = mode;
            prim.Attributes = new Dictionary<string,int>{
                {"POSITION",vAccess}
            };

            m.Primitives = new[]{prim};
            
            // Add mesh to gltf
            if(gltf.Meshes != null) 
            {
                // TODO: Get rid of this resizing.
                var meshes = gltf.Meshes.ToList();
                meshes.Add(m);
                gltf.Meshes = meshes.ToArray();
            }
            else
            {
                gltf.Meshes = new[]{m};
            }

            var parentId = 0;

            if(transform != null)
            {
                var a = transform.XAxis;
                var b = transform.YAxis;
                var c = transform.ZAxis;

                var transNode = new Node();

                transNode.Matrix = new[]{
                    (float)a.X, (float)a.Y, (float)a.Z, 0.0f,
                    (float)b.X, (float)b.Y, (float)b.Z, 0.0f,
                    (float)c.X, (float)c.Y, (float)c.Z, 0.0f,
                    (float)transform.Origin.X,(float)transform.Origin.Y,(float)transform.Origin.Z, 1.0f
                };

                parentId = gltf.AddNode(transNode, 0);
            }

            // Add mesh node to gltf
            var node = new Node();
            node.Mesh = gltf.Meshes.Length - 1;
            gltf.AddNode(node, parentId);
            
            return gltf.Meshes.Length - 1;
        }

        internal static void ToGlb(this Solid solid, string path)
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

            var materials = gltf.AddMaterials(new[]{BuiltInMaterials.Default, BuiltInMaterials.Edges, BuiltInMaterials.EdgesHighlighted});
            
            var buffer = new List<byte>();
            var mesh = new Elements.Geometry.Mesh();
            solid.Tessellate(ref mesh);

            byte[] vertexBuffer;
            byte[] normalBuffer;
            byte[] indexBuffer;
            byte[] colorBuffer;
            double[] vmin; double[] vmax;
            double[] nmin; double[] nmax;
            float[] cmin; float[] cmax;
            ushort imin; ushort imax;

            mesh.GetBuffers(out vertexBuffer, out indexBuffer, out normalBuffer, out colorBuffer,
                            out vmax, out vmin, out nmin, out nmax, out cmin, 
                            out cmax, out imin, out imax);

            var bufferViews = new List<BufferView>();
            var accessors = new List<Accessor>();

            gltf.AddTriangleMesh("mesh", buffer, bufferViews, accessors, vertexBuffer, normalBuffer,
                                        indexBuffer, colorBuffer, vmin, vmax, nmin, nmax,
                                        imin, imax, materials[BuiltInMaterials.Default.Name], cmin, cmax, null, null);

            var edgeCount = 0;
            var vertices = new List<Vector3>();
            var verticesHighlighted = new List<Vector3>();
            foreach(var e in solid.Edges.Values)
            {
                if(e.Left.Loop == null || e.Right.Loop == null)
                {
                    verticesHighlighted.AddRange(new[]{e.Left.Vertex.Point, e.Right.Vertex.Point});
                }
                else
                {
                    vertices.AddRange(new[]{e.Left.Vertex.Point, e.Right.Vertex.Point});
                }

                edgeCount++;
            }

            if(vertices.Count > 0)
            {
                // Draw standard edges
                var vBuff = vertices.ToArray().ToArray();
                var vCount = vertices.Count;
                // var indices = Enumerable.Range(0, vCount).Select(i => (ushort)i).ToArray();
                var indices = new List<ushort>();
                for(var i=0; i<vertices.Count; i+=2)
                {
                    indices.Add((ushort)i);
                    indices.Add((ushort)(i+1));
                }
                var bbox = new BBox3(vertices.ToArray());
                gltf.AddLineLoop($"edge_{edgeCount}", buffer, bufferViews, accessors, vBuff, indices.ToArray(), bbox.Min.ToArray(), bbox.Max.ToArray(), 0, (ushort)(vCount - 1), materials[BuiltInMaterials.Edges.Name], MeshPrimitive.ModeEnum.LINES, null);
            }

            if(verticesHighlighted.Count > 0)
            {
                // Draw highlighted edges
                var vBuff = vertices.ToArray().ToArray();
                var vCount = vertices.Count;
                var indices = new List<ushort>();
                for(var i=0; i<vertices.Count; i+=2)
                {
                    indices.Add((ushort)i);
                    indices.Add((ushort)(i+1));
                }
                var bbox = new BBox3(vertices.ToArray());
                gltf.AddLineLoop($"edge_{edgeCount}", buffer, bufferViews, accessors, vBuff, indices.ToArray(), bbox.Min.ToArray(), bbox.Max.ToArray(), 0, (ushort)(vCount - 1), materials[BuiltInMaterials.EdgesHighlighted.Name], MeshPrimitive.ModeEnum.LINES, null);
            }

            var buff = new glTFLoader.Schema.Buffer();
            buff.ByteLength = buffer.Count;
            gltf.Buffers = new[] { buff };

            gltf.BufferViews = bufferViews.ToArray();
            gltf.Accessors = accessors.ToArray();

            if (File.Exists(path))
            {
                File.Delete(path);
            }

            gltf.SaveBinaryModel(buffer.ToArray(), path);
        }

        private static void SaveGlb(Model model, string path)
        {
            var buffer = new List<byte>();
            var gltf = InitializeGlTF(model, buffer);
            if (File.Exists(path))
            {
                File.Delete(path);
            }

            gltf.SaveBinaryModel(buffer.ToArray(), path);
        }

        private static void SaveGltf(Model model, string path)
        {
            var buffer = new List<byte>();

            var gltf = InitializeGlTF(model, buffer);

            if (File.Exists(path))
            {
                File.Delete(path);
            }

            var uri = Path.GetFileNameWithoutExtension(path) + ".bin";
            gltf.Buffers[0].Uri = uri;

            var binSaveDir = Path.GetDirectoryName(path);
            var binSaveName = Path.GetFileNameWithoutExtension(path) + ".bin";
            var binSavePath = Path.Combine(binSaveDir, binSaveName);
            if (File.Exists(binSavePath))
            {
                File.Delete(binSavePath);
            }
            using (var fs = new FileStream(binSavePath, FileMode.Create, FileAccess.Write))
            {
                fs.Write(buffer.ToArray(), 0, buffer.Count());
            }
            gltf.SaveModel(path);
        }

        private static Gltf InitializeGlTF(Model model, List<byte> buffer)
        {
            var sw = new Stopwatch();
            sw.Start();

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

            sw.Stop();
            Console.WriteLine($"glTF: {sw.Elapsed} elapsed for writing root node.");
            sw.Reset();

            sw.Start();
            var materialsToAdd = model.Materials.Values.ToList();
            materialsToAdd.Add(BuiltInMaterials.XAxis);
            materialsToAdd.Add(BuiltInMaterials.YAxis);
            materialsToAdd.Add(BuiltInMaterials.ZAxis);
            materialsToAdd.Add(BuiltInMaterials.Edges);
            materialsToAdd.Add(BuiltInMaterials.EdgesHighlighted);
            
            var materials = gltf.AddMaterials(materialsToAdd);

            sw.Stop();
            Console.WriteLine($"glTF: {sw.Elapsed} elapsed for writing materials.");
            sw.Reset();

            sw.Start();
            // Lines are stored in a list of lists
            // according to the max available index size of ushort.
            var lines = new List<List<Vector3>>(){new List<Vector3>()};
            
            var bufferViews = new List<BufferView>();
            var accessors = new List<Accessor>();

            var elements = model.Elements.Values.ToArray();
            for(var i=0;i<elements.Length;i++)
            {
                var e = elements[i];
                GetRenderDataForElement(e, gltf, materials, lines, buffer, bufferViews, accessors);
            }
            sw.Stop();
            Console.WriteLine($"glTF: {sw.Elapsed} elapsed for getting render data for nodes.");
            sw.Reset();

            sw.Start();
            var lineArr = lines.ToArray();
            var lineCounter = 0;
            for(var i=0; i<lineArr.Length; i++)
            {
                lineCounter++;
                if(lineCounter > 20000)
                {
                    lineCounter = 0;
                }
            }
            if (lines.Count() > 0)
            {
                foreach(var lineSet in lines)
                {
                    AddLines(100000, lineSet.ToArray(), gltf, materials[BuiltInMaterials.Edges.Name], buffer, bufferViews, accessors, null);
                }
            }
            sw.Stop();
            Console.WriteLine($"glTF: {sw.Elapsed} elapsed for writing edges.");
            sw.Reset();

            sw.Start();
            var buff = new glTFLoader.Schema.Buffer();
            buff.ByteLength = buffer.Count();
            gltf.Buffers = new[] { buff };
            sw.Stop();
            Console.WriteLine($"glTF: {sw.Elapsed} elapsed for assigning buffer.");
            sw.Reset();

            gltf.BufferViews = bufferViews.ToArray();
            gltf.Accessors = accessors.ToArray();

            return gltf;
        }

        private static void GetRenderDataForElement(IElement e, Gltf gltf, 
            Dictionary<string, int> materials, List<List<Vector3>> lines, List<byte> buffer, List<BufferView> bufferViews , List<Accessor> accessors)
        {
            var sw = new Stopwatch();
            sw.Start();
            if (e is IAggregateElements)
            {
                var ae = (IAggregateElements)e;

                if (ae.Elements.Count > 0)
                {
                    for(var i=0; i< ae.Elements.Count; i++)
                    {
                        var esub = ae.Elements[i];
                        GetRenderDataForElement(esub, gltf, materials, lines, buffer, bufferViews, accessors);
                    }
                }
            }
            sw.Stop();
            // Console.WriteLine($"glTF:\t{sw.Elapsed} for processing aggregates");
            sw.Reset();

            sw.Start();
            var materialName = BuiltInMaterials.Default.Name;

            if(e is IElementType<StructuralFramingType>)
            {
                // Get the material from the framing type's material.
                materialName = ((IElementType<StructuralFramingType>)e).ElementType.Material.Name;
            }

            if(e is IElementType<WallType>)
            {
                // Get the material from the first layer of the wall.
                materialName = ((IElementType<WallType>)e).ElementType.MaterialLayers[0].Material.Name;
            }

            if(e is IElementType<FloorType>)
            {
                // Get the material from the first layer of the floor.
                materialName = ((IElementType<FloorType>)e).ElementType.MaterialLayers[0].Material.Name;
            }

            if (e is IMaterial)
            {
                // Get the material from the material property.
                materialName = ((IMaterial)e).Material.Name;
            }
            sw.Stop();
            // Console.WriteLine($"glTF:\t{sw.Elapsed} for getting the material name.");
            sw.Reset();

            sw.Start();
            Solid solid = null;

            if (e is ISolid)
            {
                // Element already has a solid representation.
                var geo = e as ISolid;
                solid = geo.GetUpdatedSolid();
            }
            sw.Stop();
            Console.WriteLine($"glTF:\t{sw.Elapsed} for updating element solids.");
            sw.Reset();

            sw.Start();
            if(solid != null)
            {
                ProcessSolid(solid, e.Transform, e.Id.ToString(), materialName, ref gltf, 
                            ref materials, ref lines, ref buffer, bufferViews, accessors);
            }
            sw.Stop();
            Console.WriteLine($"glTF:\t{sw.Elapsed} for processing element solids.");
            sw.Reset();

            if (e is ITessellate)
            {
                var geo = (ITessellate)e;
                var mesh = new Elements.Geometry.Mesh();
                geo.Tessellate(ref mesh);

                byte[] vertexBuffer;
                byte[] normalBuffer;
                byte[] indexBuffer;
                byte[] colorBuffer;
                double[] vmin; double[] vmax;
                double[] nmin; double[] nmax;
                float[] cmin; float[] cmax;
                ushort imin; ushort imax;

                mesh.GetBuffers(out vertexBuffer, out indexBuffer, out normalBuffer, out colorBuffer,
                                out vmax, out vmin, out nmin, out nmax, out cmin,
                                out cmax, out imin, out imax);

                gltf.AddTriangleMesh(e.Id + "_mesh", buffer, bufferViews, accessors, vertexBuffer, normalBuffer,
                                        indexBuffer, colorBuffer, vmin, vmax, nmin, nmax,
                                        imin, imax, materials[materialName], cmin, cmax, null, e.Transform);
            }
        }

        private static void ProcessSolid(Solid solid, Transform t, string id, string materialName, ref Gltf gltf, 
            ref Dictionary<string, int> materials, ref List<List<Vector3>> lines, ref List<byte> buffer, 
            List<BufferView> bufferViews, List<Accessor> accessors)
        {
            Elements.Geometry.Mesh mesh = null;
            var currLines = lines.Last();
            
            // Check if we'll overrun the index size
            // for the current line array. If so,
            // create a new line array.
            if(currLines.Count * 2 > ushort.MaxValue)
            {
                currLines = new List<Vector3>();
                lines.Add(currLines);
            }

            foreach (var edge in solid.Edges.Values)
            {
                if (t != null)
                {
                    currLines.AddRange(new[] { t.OfPoint(edge.Left.Vertex.Point), t.OfPoint(edge.Right.Vertex.Point) });
                }
                else
                {
                    currLines.AddRange(new[] { edge.Left.Vertex.Point, edge.Right.Vertex.Point });
                }
            }

            var sw = new Stopwatch();
            sw.Start();

            mesh = new Elements.Geometry.Mesh();
            solid.Tessellate(ref mesh);
            sw.Stop();
            Console.WriteLine($"glTF:\t\t{sw.Elapsed} for tessellating the solid.");
            sw.Reset();

            sw.Start();
            byte[] vertexBuffer;
            byte[] normalBuffer;
            byte[] indexBuffer;
            byte[] colorBuffer;
            double[] vmin; double[] vmax;
            double[] nmin; double[] nmax;
            float[] cmin; float[] cmax;
            ushort imin; ushort imax;

            mesh.GetBuffers(out vertexBuffer, out indexBuffer, out normalBuffer, out colorBuffer,
                            out vmax, out vmin, out nmin, out nmax, out cmin,
                            out cmax, out imin, out imax);
            sw.Stop();
            Console.WriteLine($"glTF:\t\t{sw.Elapsed} for getting the mesh buffers.");
            sw.Reset();

            sw.Start();
            gltf.AddTriangleMesh(id + "_mesh", buffer, bufferViews, accessors, vertexBuffer, normalBuffer,
                                indexBuffer, colorBuffer, vmin, vmax, nmin, nmax,
                                imin, imax, materials[materialName], cmin, cmax, null, t);
            sw.Stop();
            Console.WriteLine($"glTF:\t\t{sw.Elapsed} for adding a triangle mesh.");
            sw.Reset();
        }

        private static void AddLines(long id, Vector3[] vertices, Gltf gltf, 
            int material, List<byte> buffer, List<BufferView> bufferViews, List<Accessor> accessors, Transform t = null)
        {
            var vBuff = vertices.ToArray();
            var vCount = vertices.Length;
            var indices = new ushort[vertices.Length * 2];
        
            var idx = 0;
            for (var i = 0; i < vertices.Length; i += 2)
            {
                indices[i] = (ushort)i;
                indices[i+1] = (ushort)(i+1);
                idx++;
            }

            var bbox = new BBox3(vertices);
            gltf.AddLineLoop($"{id}_curve", buffer, bufferViews, accessors, vBuff, indices, bbox.Min.ToArray(),
                            bbox.Max.ToArray(), 0, (ushort)(vCount - 1), material, MeshPrimitive.ModeEnum.LINES, t);
        }

        private static void AddArrow(long id, Vector3 origin, Vector3 direction, 
            Gltf gltf, int material, Transform t, List<byte> buffer, 
            List<BufferView> bufferViews, List<Accessor> accessors)
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
            gltf.AddLineLoop($"{id}_curve", buffer, bufferViews, accessors, vBuff, indices, bbox.Min.ToArray(),
                            bbox.Max.ToArray(), 0, (ushort)(vCount - 1), material, MeshPrimitive.ModeEnum.LINE_STRIP, t);

        }
    }
}
