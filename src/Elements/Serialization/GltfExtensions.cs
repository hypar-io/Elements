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

[assembly:InternalsVisibleTo("Hypar.Elements.Tests")]

namespace Elements.Serialization
{
    internal static class GltfExtensions
    {
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

        private static int AddAccessor(this Gltf gltf, int bufferView, int byteOffset, Accessor.ComponentTypeEnum componentType, int count, float[] min, float[] max, Accessor.TypeEnum accessorType)
        {
            var a = new Accessor();
            a.BufferView = bufferView;
            a.ByteOffset = byteOffset;
            a.ComponentType = componentType;
            a.Min = min;
            a.Max = max;
            a.Type = accessorType;
            a.Count = count;

            if(gltf.Accessors != null)
            {
                var accessors = gltf.Accessors.ToList();
                accessors.Add(a);
                gltf.Accessors = accessors.ToArray();
            }
            else
            {
                gltf.Accessors = new []{a};
            }

            return gltf.Accessors.Length - 1;
        }

        private static int AddBufferView(this Gltf gltf, int buffer, int byteOffset, int byteLength, BufferView.TargetEnum? target, int? byteStride)
        {
            var b = new BufferView();
            b.Buffer = buffer;
            b.ByteLength = byteLength;
            b.ByteOffset = byteOffset;
            b.Target = target;
            b.ByteStride = byteStride;

            if(gltf.BufferViews != null)
            {
                var bufferViews = gltf.BufferViews.ToList();
                bufferViews.Add(b);
                gltf.BufferViews = bufferViews.ToArray();
            }
            else
            {
                gltf.BufferViews = new []{b};
            }

            return gltf.BufferViews.Length - 1;
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

        internal static int AddTriangleMesh(this Gltf gltf, string name, List<byte> buffer, double[] vertices, double[] normals, ushort[] indices, 
        double[] vMin, double[] vMax, double[] nMin, double[] nMax, ushort iMin, ushort iMax, int materialId, int? parent_index, Transform transform = null)
        {
            var m = new glTFLoader.Schema.Mesh();
            m.Name = name;

            var vBuff = gltf.AddBufferView(0, buffer.Count, vertices.Length * sizeof(float), null, null);
            var nBuff = gltf.AddBufferView(0, buffer.Count + vertices.Length * sizeof(float), normals.Length * sizeof(float), null, null);
            var iBuff = gltf.AddBufferView(0, buffer.Count + vertices.Length * sizeof(float) + normals.Length * sizeof(float), indices.Length * sizeof(ushort), null, null);

            foreach(var v in vertices)
            {
                buffer.AddRange(BitConverter.GetBytes((float)v));
            }
            foreach(var n in normals)
            {
                buffer.AddRange(BitConverter.GetBytes((float)n));
            }
            foreach(var i in indices)
            {
                buffer.AddRange(BitConverter.GetBytes(i));
            }
            
            while(buffer.Count % 4 != 0)
            {
                // Console.WriteLine("Padding...");
                buffer.Add(0);
            }

            var vAccess = gltf.AddAccessor(vBuff, 0, Accessor.ComponentTypeEnum.FLOAT, vertices.Length/3, new[]{(float)vMin[0], (float)vMin[1], (float)vMin[2]}, new[]{(float)vMax[0],(float)vMax[1],(float)vMax[2]}, Accessor.TypeEnum.VEC3);
            var nAccess = gltf.AddAccessor(nBuff, 0, Accessor.ComponentTypeEnum.FLOAT, normals.Length/3, new[]{(float)nMin[0], (float)nMin[1], (float)nMin[2]}, new[]{(float)nMax[0], (float)nMax[1], (float)nMax[2]}, Accessor.TypeEnum.VEC3);
            var iAccess = gltf.AddAccessor(iBuff, 0, Accessor.ComponentTypeEnum.UNSIGNED_SHORT, indices.Length, new[]{(float)iMin}, new[]{(float)iMax}, Accessor.TypeEnum.SCALAR);
            
            var prim = new MeshPrimitive();
            prim.Indices = iAccess;
            prim.Material = materialId;
            prim.Mode = MeshPrimitive.ModeEnum.TRIANGLES;
            prim.Attributes = new Dictionary<string,int>{
                {"NORMAL",nAccess},
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
    
        internal static int AddLineLoop(this Gltf gltf, string name, List<byte> buffer, double[] vertices, ushort[] indices, double[] vMin, double[] vMax, ushort iMin, ushort iMax, int materialId, MeshPrimitive.ModeEnum mode, Transform transform = null)
        {
            var m = new glTFLoader.Schema.Mesh();
            m.Name = name;
            var vBuff = gltf.AddBufferView(0, buffer.Count, vertices.Length * sizeof(float), null, null);
            var iBuff = gltf.AddBufferView(0, buffer.Count + vertices.Length * sizeof(float), indices.Length * sizeof(ushort), null, null);

            foreach(var v in vertices)
            {
                buffer.AddRange(BitConverter.GetBytes((float)v));
            }
            foreach(var i in indices)
            {
                buffer.AddRange(BitConverter.GetBytes(i));
            }

            while(buffer.Count % 4 != 0)
            {
                // Console.WriteLine("Padding...");
                buffer.Add(0);
            }
            
            var vAccess = gltf.AddAccessor(vBuff, 0, Accessor.ComponentTypeEnum.FLOAT, vertices.Length/3, new[]{(float)vMin[0], (float)vMin[1], (float)vMin[2]}, new[]{(float)vMax[0],(float)vMax[1],(float)vMax[2]}, Accessor.TypeEnum.VEC3);
            var iAccess = gltf.AddAccessor(iBuff, 0, Accessor.ComponentTypeEnum.UNSIGNED_SHORT, indices.Length, new[]{(float)iMin}, new[]{(float)iMax}, Accessor.TypeEnum.SCALAR);

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

            gltf.AddTriangleMesh("mesh", buffer, mesh.Vertices.ToArray(), mesh.Normals.ToArray(),
                                        mesh.Indices.ToArray(), mesh.VMin, mesh.VMax, mesh.NMin, mesh.NMax,
                                        mesh.IMin, mesh.IMax, materials[BuiltInMaterials.Default.Name], null, null);

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
                gltf.AddLineLoop($"edge_{edgeCount}", buffer, vBuff, indices.ToArray(), bbox.Min.ToArray(), bbox.Max.ToArray(), 0, (ushort)(vCount - 1), materials[BuiltInMaterials.Edges.Name], MeshPrimitive.ModeEnum.LINES, null);
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
                gltf.AddLineLoop($"edge_{edgeCount}", buffer, vBuff, indices.ToArray(), bbox.Min.ToArray(), bbox.Max.ToArray(), 0, (ushort)(vCount - 1), materials[BuiltInMaterials.EdgesHighlighted.Name], MeshPrimitive.ModeEnum.LINES, null);
            }

            var buff = new glTFLoader.Schema.Buffer();
            buff.ByteLength = buffer.Count;
            gltf.Buffers = new[] { buff };

            if (File.Exists(path))
            {
                File.Delete(path);
            }

            gltf.SaveBinaryModel(buffer.ToArray(), path);
        }
    }
}