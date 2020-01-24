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
using Elements.Geometry.Interfaces;
using SixLabors.ImageSharp.Processing;

[assembly: InternalsVisibleTo("Hypar.Elements.Tests")]

namespace Elements.Serialization.glTF
{
    /// <summary>
    /// Extensions for glTF serialization.
    /// </summary>
    public static class GltfExtensions
    {
        private static int _currentId = -1;

        private static int GetNextId()
        {
            _currentId++;
            return _currentId;
        }

        private const string emptyGltf = @"{
    ""asset"": {""version"": ""2.0""},
    ""nodes"": [{""name"": ""empty""}],
    ""scenes"": [{""nodes"": [0]}],
    ""scene"": 0
}";

        /// <summary>
        /// Save a model to gltf.
        /// If there is no geometry, an empty GLTF will still be produced.
        /// </summary>
        /// <param name="model">The model to serialize.</param>
        /// <param name="path">The output path.</param>
        /// <param name="useBinarySerialization">Should binary serialization be used?</param>
        public static void ToGlTF(this Model model, string path, bool useBinarySerialization = true)
        {
            if (model.Elements.Count > 0)
            {
                if (useBinarySerialization)
                {
                    if (SaveGlb(model, path))
                    {
                        return;
                    }
                    // Else fall through to produce an empty GLTF.
                }
                else
                {
                    if (SaveGltf(model, path))
                    {
                        return;
                    }
                    // Else fall through to produce an empty GLTF.
                }
            }

            // There are no elements that produced geometry. Write an empty GLTF.
            File.WriteAllText(path, emptyGltf);
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
            if (gltf == null)
            {
                return "";
            }
            gltf.SaveBinaryModel(buffer.ToArray(), tmp);
            var bytes = File.ReadAllBytes(tmp);
            return Convert.ToBase64String(bytes);
        }

        internal static Dictionary<string, int> AddMaterials(this Gltf gltf, IList<Material> materials, List<byte> buffer, List<BufferView> bufferViews)
        {
            var materialDict = new Dictionary<string, int>();
            var newMaterials = new List<glTFLoader.Schema.Material>();

            var textureDict = new Dictionary<string, int>(); // the name of the texture image, the id of the texture
            var textures = new List<glTFLoader.Schema.Texture>();

            var images = new List<glTFLoader.Schema.Image>();
            var samplers = new List<glTFLoader.Schema.Sampler>();

            var matId = 0;
            var texId = 0;
            var imageId = 0;
            var samplerId = 0;

            foreach (var material in materials)
            {
                if (materialDict.ContainsKey(material.Name))
                {
                    continue;
                }

                var m = new glTFLoader.Schema.Material();
                newMaterials.Add(m);

                m.PbrMetallicRoughness = new MaterialPbrMetallicRoughness();
                m.PbrMetallicRoughness.BaseColorFactor = material.Color.ToArray();
                m.PbrMetallicRoughness.MetallicFactor = 1.0f;
                m.DoubleSided = false;
                m.Name = material.Name;

                m.Extensions = new Dictionary<string, object>{
                    {"KHR_materials_pbrSpecularGlossiness", new Dictionary<string,object>{
                        {"diffuseFactor", new[]{material.Color.Red,material.Color.Green,material.Color.Blue,material.Color.Alpha}},
                        {"specularFactor", new[]{material.SpecularFactor, material.SpecularFactor, material.SpecularFactor}},
                        {"glossinessFactor", material.GlossinessFactor}
                    }}
                };

                if (material.Texture != null)
                {
                    // Add the texture
                    var ti = new TextureInfo();
                    m.PbrMetallicRoughness.BaseColorTexture = ti;
                    ti.Index = texId;
                    ti.TexCoord = 0;
                    ((Dictionary<string, object>)m.Extensions["KHR_materials_pbrSpecularGlossiness"])["diffuseTexture"] = ti;

                    if (textureDict.ContainsKey(material.Texture))
                    {
                        ti.Index = textureDict[material.Texture];
                    }
                    else
                    {
                        var tex = new Texture();
                        textures.Add(tex);

                        var image = new glTFLoader.Schema.Image();

                        using (var ms = new MemoryStream())
                        {
                            using (var texImage = SixLabors.ImageSharp.Image.Load(material.Texture))
                            {
                                texImage.Save(ms, new SixLabors.ImageSharp.Formats.Png.PngEncoder());
                            }
                            var imageData = ms.ToArray();
                            image.BufferView = AddBufferView(bufferViews, 0, buffer.Count, imageData.Length, null, null);
                            buffer.AddRange(imageData);
                        }

                        while (buffer.Count % 4 != 0)
                        {
                            // Console.WriteLine("Padding...");
                            buffer.Add(0);
                        }

                        image.MimeType = glTFLoader.Schema.Image.MimeTypeEnum.image_png;
                        tex.Source = imageId;
                        images.Add(image);

                        var sampler = new Sampler();
                        sampler.MagFilter = Sampler.MagFilterEnum.LINEAR;
                        sampler.MinFilter = Sampler.MinFilterEnum.LINEAR;
                        sampler.WrapS = Sampler.WrapSEnum.REPEAT;
                        sampler.WrapT = Sampler.WrapTEnum.REPEAT;
                        tex.Sampler = samplerId;
                        samplers.Add(sampler);

                        textureDict.Add(material.Texture, texId);

                        texId++;
                        imageId++;
                        samplerId++;
                    }
                }

                if (material.Color.Alpha < 1.0)
                {
                    m.AlphaMode = glTFLoader.Schema.Material.AlphaModeEnum.BLEND;
                }
                else
                {
                    m.AlphaMode = glTFLoader.Schema.Material.AlphaModeEnum.OPAQUE;
                }

                materialDict.Add(m.Name, matId);
                matId++;
            }

            if (materials.Count > 0)
            {
                gltf.Materials = newMaterials.ToArray();
            }
            if (textures.Count > 0)
            {
                gltf.Textures = textures.ToArray();
            }
            if (images.Count > 0)
            {
                gltf.Images = images.ToArray();
            }
            if (samplers.Count > 0)
            {
                gltf.Samplers = samplers.ToArray();
            }

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

        private static int AddNode(this Gltf gltf, List<Node> nodes, Node n, int? parent)
        {
            nodes.Add(n);
            var id = nodes.Count - 1;

            if (parent != null)
            {
                if (nodes[(int)parent].Children == null)
                {
                    nodes[(int)parent].Children = new[] { id };
                }
                else
                {
                    // TODO: Get rid of this resizing.
                    var children = nodes[(int)parent].Children.ToList();
                    children.Add(id);
                    nodes[(int)parent].Children = children.ToArray();
                }

            }

            return id;
        }

        internal static void AddInstanceMesh(this Gltf gltf,
                                            List<glTFLoader.Schema.Node> nodes,
                                            List<int> meshIds,
                                            Transform transform)
        {
            var a = transform.XAxis;
            var b = transform.YAxis;
            var c = transform.ZAxis;

            var matrix = new[]{
                    (float)a.X, (float)a.Y, (float)a.Z, 0.0f,
                    (float)b.X, (float)b.Y, (float)b.Z, 0.0f,
                    (float)c.X, (float)c.Y, (float)c.Z, 0.0f,
                    (float)transform.Origin.X,(float)transform.Origin.Y,(float)transform.Origin.Z, 1.0f
                };

            foreach (var meshId in meshIds)
            {
                var node = new Node();
                node.Matrix = matrix;
                node.Mesh = meshId;
                gltf.AddNode(nodes, node, 0);
            }
        }

        internal static int AddTriangleMesh(this Gltf gltf,
                                            string name,
                                            List<byte> buffer,
                                            List<BufferView> bufferViews,
                                            List<Accessor> accessors,
                                            byte[] vertices,
                                            byte[] normals,
                                            byte[] indices,
                                            byte[] colors,
                                            byte[] uvs,
                                            double[] vMin,
                                            double[] vMax,
                                            double[] nMin,
                                            double[] nMax,
                                            ushort iMin,
                                            ushort iMax,
                                            double[] uvMin,
                                            double[] uvMax,
                                            int materialId,
                                            float[] cMin,
                                            float[] cMax,
                                            int? parent_index,
                                            List<glTFLoader.Schema.Mesh> meshes,
                                            List<glTFLoader.Schema.Node> nodes,
                                            Transform transform = null)
        {
            var m = new glTFLoader.Schema.Mesh();
            m.Name = name;

            var vBuff = AddBufferView(bufferViews, 0, buffer.Count, vertices.Length, null, null);
            buffer.AddRange(vertices);

            var nBuff = AddBufferView(bufferViews, 0, buffer.Count, normals.Length, null, null);
            buffer.AddRange(normals);

            var iBuff = AddBufferView(bufferViews, 0, buffer.Count, indices.Length, null, null);
            buffer.AddRange(indices);

            while (buffer.Count % 4 != 0)
            {
                // Console.WriteLine("Padding...");
                buffer.Add(0);
            }

            var vAccess = AddAccessor(accessors, vBuff, 0, Accessor.ComponentTypeEnum.FLOAT, vertices.Length / sizeof(float) / 3, new[] { (float)vMin[0], (float)vMin[1], (float)vMin[2] }, new[] { (float)vMax[0], (float)vMax[1], (float)vMax[2] }, Accessor.TypeEnum.VEC3);
            var nAccess = AddAccessor(accessors, nBuff, 0, Accessor.ComponentTypeEnum.FLOAT, normals.Length / sizeof(float) / 3, new[] { (float)nMin[0], (float)nMin[1], (float)nMin[2] }, new[] { (float)nMax[0], (float)nMax[1], (float)nMax[2] }, Accessor.TypeEnum.VEC3);
            var iAccess = AddAccessor(accessors, iBuff, 0, Accessor.ComponentTypeEnum.UNSIGNED_SHORT, indices.Length / sizeof(ushort), new[] { (float)iMin }, new[] { (float)iMax }, Accessor.TypeEnum.SCALAR);

            var prim = new MeshPrimitive();
            prim.Indices = iAccess;
            prim.Material = materialId;
            prim.Mode = MeshPrimitive.ModeEnum.TRIANGLES;
            prim.Attributes = new Dictionary<string, int>{
                {"NORMAL",nAccess},
                {"POSITION",vAccess}
            };

            if (uvs.Length > 0)
            {
                var uvBuff = AddBufferView(bufferViews, 0, buffer.Count, uvs.Length, null, null);
                buffer.AddRange(uvs);
                var uvAccess = AddAccessor(accessors, uvBuff, 0, Accessor.ComponentTypeEnum.FLOAT, uvs.Length / sizeof(float) / 2, new[] { (float)uvMin[0], (float)uvMin[1] }, new[] { (float)uvMax[0], (float)uvMax[1] }, Accessor.TypeEnum.VEC2);
                prim.Attributes.Add("TEXCOORD_0", uvAccess);
            }

            // TODO: Add to the buffer above instead of inside this block.
            // There's a chance the padding operation will put padding before
            // the color information.
            if (colors.Length > 0)
            {
                var cBuff = AddBufferView(bufferViews, 0, buffer.Count, colors.Length, null, null);
                buffer.AddRange(colors);
                var cAccess = AddAccessor(accessors, cBuff, 0, Accessor.ComponentTypeEnum.FLOAT, colors.Length / sizeof(float) / 3, cMin, cMax, Accessor.TypeEnum.VEC3);
                prim.Attributes.Add("COLOR_0", cAccess);
            }

            m.Primitives = new[] { prim };

            // Add mesh to gltf
            meshes.Add(m);

            var parentId = 0;

            if (transform != null)
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

                parentId = gltf.AddNode(nodes, transNode, 0);
            }

            // Add mesh node to gltf
            var node = new Node();
            node.Mesh = meshes.Count - 1;
            gltf.AddNode(nodes, node, parentId);

            return meshes.Count - 1;
        }

        internal static int AddLineLoop(this Gltf gltf,
                                        string name,
                                        List<byte> buffer,
                                        List<BufferView> bufferViews,
                                        List<Accessor> accessors,
                                        byte[] vertices,
                                        byte[] indices,
                                        double[] vMin,
                                        double[] vMax,
                                        ushort iMin,
                                        ushort iMax,
                                        int materialId,
                                        MeshPrimitive.ModeEnum mode,
                                        List<glTFLoader.Schema.Mesh> meshes,
                                        List<glTFLoader.Schema.Node> nodes,
                                        Transform transform = null)
        {
            var m = new glTFLoader.Schema.Mesh();
            m.Name = name;
            var vBuff = AddBufferView(bufferViews, 0, buffer.Count, vertices.Length, null, null);
            var iBuff = AddBufferView(bufferViews, 0, buffer.Count + vertices.Length, indices.Length, null, null);

            buffer.AddRange(vertices);
            buffer.AddRange(indices);

            while (buffer.Count % 4 != 0)
            {
                // Console.WriteLine("Padding...");
                buffer.Add(0);
            }

            var vAccess = AddAccessor(accessors, vBuff, 0, Accessor.ComponentTypeEnum.FLOAT, vertices.Length / sizeof(float) / 3, new[] { (float)vMin[0], (float)vMin[1], (float)vMin[2] }, new[] { (float)vMax[0], (float)vMax[1], (float)vMax[2] }, Accessor.TypeEnum.VEC3);
            var iAccess = AddAccessor(accessors, iBuff, 0, Accessor.ComponentTypeEnum.UNSIGNED_SHORT, indices.Length / sizeof(ushort), new[] { (float)iMin }, new[] { (float)iMax }, Accessor.TypeEnum.SCALAR);

            var prim = new MeshPrimitive();
            prim.Indices = iAccess;
            prim.Material = materialId;
            prim.Mode = mode;
            prim.Attributes = new Dictionary<string, int>{
                {"POSITION",vAccess}
            };

            m.Primitives = new[] { prim };

            // Add mesh to gltf
            meshes.Add(m);

            var parentId = 0;

            if (transform != null)
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

                parentId = gltf.AddNode(nodes, transNode, 0);
            }

            // Add mesh node to gltf
            var node = new Node();
            node.Mesh = meshes.Count - 1;
            gltf.AddNode(nodes, node, parentId);

            return meshes.Count - 1;
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

            var meshes = new List<glTFLoader.Schema.Mesh>();
            var nodes = new List<glTFLoader.Schema.Node> { root };

            gltf.Scene = 0;
            var scene = new Scene();
            scene.Nodes = new[] { 0 };
            gltf.Scenes = new[] { scene };

            gltf.ExtensionsUsed = new[] { "KHR_materials_pbrSpecularGlossiness" };

            var buffer = new List<byte>();
            var bufferViews = new List<BufferView>();

            var materials = gltf.AddMaterials(new[] { BuiltInMaterials.Default, BuiltInMaterials.Edges, BuiltInMaterials.EdgesHighlighted },
                                              buffer,
                                              bufferViews);

            var mesh = new Elements.Geometry.Mesh();
            solid.Tessellate(ref mesh);

            byte[] vertexBuffer;
            byte[] normalBuffer;
            byte[] indexBuffer;
            byte[] colorBuffer;
            byte[] uvBuffer;

            double[] vmin; double[] vmax;
            double[] nmin; double[] nmax;
            float[] cmin; float[] cmax;
            ushort imin; ushort imax;
            double[] uvmin; double[] uvmax;



            mesh.GetBuffers(out vertexBuffer, out indexBuffer, out normalBuffer, out colorBuffer, out uvBuffer,
                            out vmax, out vmin, out nmin, out nmax, out cmin,
                            out cmax, out imin, out imax, out uvmax, out uvmin);


            var accessors = new List<Accessor>();

            gltf.AddTriangleMesh("mesh", buffer, bufferViews, accessors, vertexBuffer, normalBuffer,
                                        indexBuffer, colorBuffer, uvBuffer, vmin, vmax, nmin, nmax,
                                        imin, imax, uvmin, uvmax, materials[BuiltInMaterials.Default.Name], cmin, cmax, null, meshes, nodes, null);

            var edgeCount = 0;
            var vertices = new List<Vector3>();
            var verticesHighlighted = new List<Vector3>();
            foreach (var e in solid.Edges.Values)
            {
                if (e.Left.Loop == null || e.Right.Loop == null)
                {
                    verticesHighlighted.AddRange(new[] { e.Left.Vertex.Point, e.Right.Vertex.Point });
                }
                else
                {
                    vertices.AddRange(new[] { e.Left.Vertex.Point, e.Right.Vertex.Point });
                }

                edgeCount++;
            }

            if (vertices.Count > 0)
            {
                // Draw standard edges
                AddLines(100000, vertices.ToArray(), gltf, materials[BuiltInMaterials.Edges.Name], buffer, bufferViews, accessors, meshes, nodes);
            }

            if (verticesHighlighted.Count > 0)
            {
                // Draw highlighted edges
                AddLines(100001, verticesHighlighted.ToArray(), gltf, materials[BuiltInMaterials.EdgesHighlighted.Name], buffer, bufferViews, accessors, meshes, nodes);
            }

            var buff = new glTFLoader.Schema.Buffer();
            buff.ByteLength = buffer.Count;
            gltf.Buffers = new[] { buff };

            gltf.BufferViews = bufferViews.ToArray();
            gltf.Accessors = accessors.ToArray();
            gltf.Nodes = nodes.ToArray();
            if (meshes.Count > 0)
            {
                gltf.Meshes = meshes.ToArray();
            }

            gltf.SaveBinaryModel(buffer.ToArray(), path);
        }

        /// <returns>Whether a Glb was successfully saved. False indicates that there was no geometry to save.</returns>
        private static bool SaveGlb(Model model, string path)
        {
            var buffer = new List<byte>();
            var gltf = InitializeGlTF(model, buffer);
            if (gltf == null)
            {
                return false;
            }

            gltf.SaveBinaryModel(buffer.ToArray(), path);
            return true;
        }

        /// <returns>Whether a Glb was successfully saved. False indicates that there was no geometry to save.</returns>
        private static bool SaveGltf(Model model, string path)
        {
            var buffer = new List<byte>();

            var gltf = InitializeGlTF(model, buffer);
            if (gltf == null)
            {
                return false;
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
                fs.Write(buffer.ToArray(), 0, buffer.Count);
            }
            gltf.SaveModel(path);
            return true;
        }

        private static Gltf InitializeGlTF(Model model, List<byte> buffer)
        {
            var gltf = new Gltf();
            var asset = new Asset();
            asset.Version = "2.0";
            asset.Generator = "hypar-gltf";

            gltf.Asset = asset;

            var root = new Node();

            var rootTransform = new Transform(model.Transform);

            // Rotate the transform for +Z up.
            rootTransform.Rotate(new Vector3(1,0,0), -90.0);
            var m = rootTransform.Matrix;
            root.Matrix = new float[]{
                (float)m.m11, (float)m.m21, (float)m.m31, 0f, 
                (float)m.m12, (float)m.m22, (float)m.m32, 0f, 
                (float)m.m31, (float)m.m32, (float)m.m33, 0f,
                (float)m.tx, (float)m.ty, (float)m.tz, 1f};

            var nodes = new List<glTFLoader.Schema.Node> { root };
            var meshes = new List<glTFLoader.Schema.Mesh>();

            gltf.Scene = 0;
            var scene = new Scene();
            scene.Nodes = new[] { 0 };
            gltf.Scenes = new[] { scene };

            gltf.ExtensionsUsed = new[] { "KHR_materials_pbrSpecularGlossiness" };

            var bufferViews = new List<BufferView>();
            var accessors = new List<Accessor>();

            var materialsToAdd = model.AllElementsOfType<Material>();
            var materials = gltf.AddMaterials(materialsToAdd.ToList(), buffer, bufferViews);

            var elements = model.Elements.Where(e =>
            {
                return e.Value is GeometricElement || e.Value is ElementInstance;
            }).Select(e => e.Value);

            var meshElementMap = new Dictionary<Guid, List<int>>();
            foreach (var e in elements)
            {
                GetRenderDataForElement(e, gltf, materials, buffer, bufferViews, accessors, meshes, nodes, meshElementMap);
            }
            if (buffer.Count == 0)
            {
                return null;
            }

            var buff = new glTFLoader.Schema.Buffer();
            buff.ByteLength = buffer.Count;
            gltf.Buffers = new[] { buff };
            gltf.BufferViews = bufferViews.ToArray();
            gltf.Accessors = accessors.ToArray();
            gltf.Nodes = nodes.ToArray();
            if (meshes.Count > 0)
            {
                gltf.Meshes = meshes.ToArray();
            }

            return gltf;
        }

        private static void GetRenderDataForElement(Element e,
                                                    Gltf gltf,
                                                    Dictionary<string, int> materials,
                                                    List<byte> buffer,
                                                    List<BufferView> bufferViews,
                                                    List<Accessor> accessors,
                                                    List<glTFLoader.Schema.Mesh> meshes,
                                                    List<glTFLoader.Schema.Node> nodes,
                                                    Dictionary<Guid, List<int>> meshElementMap)
        {
            var materialName = BuiltInMaterials.Default.Name;

            if (e is GeometricElement)
            {
                var geom = (GeometricElement)e;
                materialName = geom.Material.Name;

                geom.UpdateRepresentations();
                if (geom.Representation != null)
                {
                    foreach (var solidOp in geom.Representation.SolidOperations)
                    {
                        var solid = solidOp.GetSolid();
                        if (solid != null)
                        {
                            var meshId = ProcessSolid(solid, geom.Transform, e.Id.ToString(), materialName, ref gltf,
                                ref materials, ref buffer, bufferViews, accessors, meshes, nodes);
                            if (!meshElementMap.ContainsKey(e.Id))
                            {
                                meshElementMap.Add(e.Id, new List<int>());
                            }
                            meshElementMap[e.Id].Add(meshId);
                        }
                    }
                }
            }

            if (e is ElementInstance)
            {
                var i = (ElementInstance)e;

                // Lookup the corresponding mesh in the map.
                AddInstanceMesh(gltf, nodes, meshElementMap[i.Parent.Id], i.Transform);
            }

            if (e is ModelCurve)
            {
                var mc = (ModelCurve)e;
                AddLines(GetNextId(), mc.Curve.RenderVertices(), gltf, materials[mc.Material.Name], buffer, bufferViews, accessors, meshes, nodes, mc.Transform);
            }

            if (e is ModelPoints)
            {
                var mp = (ModelPoints)e;
                AddPoints(GetNextId(), mp.Locations, gltf, materials[mp.Material.Name], buffer, bufferViews, accessors, meshes, nodes, mp.Transform);
            }

            if (e is ITessellate)
            {
                var geo = (ITessellate)e;
                var mesh = new Elements.Geometry.Mesh();
                geo.Tessellate(ref mesh);

                byte[] vertexBuffer;
                byte[] normalBuffer;
                byte[] indexBuffer;
                byte[] colorBuffer;
                byte[] uvBuffer;

                double[] vmin; double[] vmax;
                double[] nmin; double[] nmax;
                float[] cmin; float[] cmax;
                ushort imin; ushort imax;
                double[] uvmin; double[] uvmax;

                mesh.GetBuffers(out vertexBuffer,
                                out indexBuffer,
                                out normalBuffer,
                                out colorBuffer,
                                out uvBuffer,
                                out vmax,
                                out vmin,
                                out nmin,
                                out nmax,
                                out cmin,
                                out cmax,
                                out imin,
                                out imax,
                                out uvmax,
                                out uvmin);

                // TODO(Ian): Remove this cast to GeometricElement when we
                // consolidate mesh under geometric representations.
                gltf.AddTriangleMesh(e.Id + "_mesh",
                                     buffer,
                                     bufferViews,
                                     accessors,
                                     vertexBuffer,
                                     normalBuffer,
                                     indexBuffer,
                                     colorBuffer,
                                     uvBuffer,
                                     vmin,
                                     vmax,
                                     nmin,
                                     nmax,
                                     imin,
                                     imax,
                                     uvmax,
                                     uvmin,
                                     materials[materialName],
                                     cmin,
                                     cmax,
                                     null,
                                     meshes,
                                     nodes,
                                     ((GeometricElement)e).Transform);
            }
        }

        private static int ProcessSolid(Solid solid,
                                         Transform t,
                                         string id,
                                         string materialName,
                                         ref Gltf gltf,
                                         ref Dictionary<string, int> materials,
                                         ref List<byte> buffer,
                                         List<BufferView> bufferViews,
                                         List<Accessor> accessors,
                                         List<glTFLoader.Schema.Mesh> meshes,
                                         List<glTFLoader.Schema.Node> nodes)
        {
            byte[] vertexBuffer;
            byte[] normalBuffer;
            byte[] indexBuffer;
            byte[] colorBuffer;
            byte[] uvBuffer;

            double[] vmin; double[] vmax;
            double[] nmin; double[] nmax;
            float[] cmin; float[] cmax;
            ushort imin; ushort imax;
            double[] uvmin; double[] uvmax;

            solid.Tessellate(out vertexBuffer, out indexBuffer, out normalBuffer, out colorBuffer, out uvBuffer,
                            out vmax, out vmin, out nmin, out nmax, out cmin,
                            out cmax, out imin, out imax, out uvmax, out uvmin);

            return gltf.AddTriangleMesh(id + "_mesh", buffer, bufferViews, accessors, vertexBuffer, normalBuffer,
                                indexBuffer, colorBuffer, uvBuffer, vmin, vmax, nmin, nmax,
                                imin, imax, uvmin, uvmax, materials[materialName], cmin, cmax, null, meshes, nodes, t);
        }

        private static void AddLines(long id,
                                     IList<Vector3> vertices,
                                     Gltf gltf,
                                     int material,
                                     List<byte> buffer,
                                     List<BufferView> bufferViews,
                                     List<Accessor> accessors,
                                     List<glTFLoader.Schema.Mesh> meshes,
                                     List<glTFLoader.Schema.Node> nodes,
                                     Transform t = null)
        {
            var floatSize = sizeof(float);
            var ushortSize = sizeof(ushort);
            var vBuff = new byte[vertices.Count * 3 * floatSize];
            // var indices = new byte[vertices.Count / 2 *  2 * ushortSize];
            var indices = new byte[vertices.Count * 2 * ushortSize];

            var vi = 0;
            var ii = 0;
            for (var i = 0; i < vertices.Count; i++)
            {
                var v = vertices[i];
                System.Buffer.BlockCopy(BitConverter.GetBytes((float)v.X), 0, vBuff, vi, floatSize);
                System.Buffer.BlockCopy(BitConverter.GetBytes((float)v.Y), 0, vBuff, vi + floatSize, floatSize);
                System.Buffer.BlockCopy(BitConverter.GetBytes((float)v.Z), 0, vBuff, vi + 2 * floatSize, floatSize);
                vi += 3 * floatSize;

                // On every even index, write a line segment.
                // if(i % 2 == 0 && i < vertices.Count - 1)
                if (i < vertices.Count - 1)
                {
                    System.Buffer.BlockCopy(BitConverter.GetBytes((ushort)i), 0, indices, ii, ushortSize);
                    System.Buffer.BlockCopy(BitConverter.GetBytes((ushort)(i + 1)), 0, indices, ii + ushortSize, ushortSize);
                    ii += 2 * ushortSize;
                }
            }

            var bbox = new BBox3(vertices);
            gltf.AddLineLoop($"{id}_curve", buffer, bufferViews, accessors, vBuff, indices, bbox.Min.ToArray(),
                            bbox.Max.ToArray(), 0, (ushort)(vertices.Count - 1), material, MeshPrimitive.ModeEnum.LINES, meshes, nodes, t);
        }

        private static void AddPoints(long id,
                                      IList<Vector3> vertices,
                                      Gltf gltf,
                                      int material,
                                      List<byte> buffer,
                                      List<BufferView> bufferViews,
                                      List<Accessor> accessors,
                                      List<glTFLoader.Schema.Mesh> meshes,
                                      List<glTFLoader.Schema.Node> nodes,
                                      Transform t = null)
        {
            var floatSize = sizeof(float);
            var ushortSize = sizeof(ushort);
            var vBuff = new byte[vertices.Count * 3 * floatSize];
            var indices = new byte[vertices.Count * ushortSize];

            var vi = 0;
            var ii = 0;
            for (var i = 0; i < vertices.Count; i++)
            {
                var v = vertices[i];
                System.Buffer.BlockCopy(BitConverter.GetBytes((float)v.X), 0, vBuff, vi, floatSize);
                System.Buffer.BlockCopy(BitConverter.GetBytes((float)v.Y), 0, vBuff, vi + floatSize, floatSize);
                System.Buffer.BlockCopy(BitConverter.GetBytes((float)v.Z), 0, vBuff, vi + 2 * floatSize, floatSize);
                vi += 3 * floatSize;

                System.Buffer.BlockCopy(BitConverter.GetBytes((ushort)i), 0, indices, ii, ushortSize);
                ii += ushortSize;
            }

            var bbox = new BBox3(vertices);
            gltf.AddLineLoop($"{id}_curve", buffer, bufferViews, accessors, vBuff, indices, bbox.Min.ToArray(),
                            bbox.Max.ToArray(), 0, (ushort)(vertices.Count - 1), material, MeshPrimitive.ModeEnum.POINTS, meshes, nodes, t);
        }

        // private static void AddArrow(long id,
        //                              Vector3 origin,
        //                              Vector3 direction,
        //                              Gltf gltf,
        //                              int material,
        //                              Transform t,
        //                              List<byte> buffer,
        //                              List<BufferView> bufferViews,
        //                              List<Accessor> accessors)
        // {
        //     var scale = 0.5;
        //     var end = origin + direction * scale;
        //     var up = direction.IsParallelTo(Vector3.ZAxis) ? Vector3.YAxis : Vector3.ZAxis;
        //     var tr = new Transform(Vector3.Origin, direction.Cross(up), direction);
        //     tr.Rotate(up, -45.0);
        //     var arrow1 = tr.OfPoint(Vector3.XAxis * 0.1);
        //     var pts = new[] { origin, end, end + arrow1 };
        //     var vBuff = pts.ToArray();
        //     var vCount = 3;
        //     var indices = Enumerable.Range(0, vCount).Select(i => (ushort)i).ToArray();
        //     var bbox = new BBox3(pts);
        //     gltf.AddLineLoop($"{id}_curve", buffer, bufferViews, accessors, vBuff, indices, bbox.Min.ToArray(),
        //                     bbox.Max.ToArray(), 0, (ushort)(vCount - 1), material, MeshPrimitive.ModeEnum.LINE_STRIP, t);

        // }
    }
}
