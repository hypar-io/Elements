using Hypar.Geometry;
using System;
using System.Collections.Generic;
using System.Linq;
using glTFLoader.Schema;

namespace Hypar
{
    internal static class GltfExtensions
    {
        internal static int AddMaterial(this Gltf gltf, string name, float red, float green, float blue, float alpha, float specularFactor, float glossinessFactor)
        {
            var m = new glTFLoader.Schema.Material();

            m.PbrMetallicRoughness = new MaterialPbrMetallicRoughness();
            m.PbrMetallicRoughness.BaseColorFactor = new[]{red,green,blue,alpha};
            m.PbrMetallicRoughness.MetallicFactor = 1.0f;

            m.Name = name;

            if(alpha < 1.0)
            {
                m.AlphaMode = glTFLoader.Schema.Material.AlphaModeEnum.BLEND;
            }
            else
            {
                m.AlphaMode = glTFLoader.Schema.Material.AlphaModeEnum.OPAQUE;
            }

            m.Extensions = new Dictionary<string, object>{
                {"KHR_materials_pbrSpecularGlossiness", new Dictionary<string,object>{
                    {"diffuseFactor", new[]{red,green,blue,alpha}},
                    {"specularFactor", new[]{specularFactor, specularFactor, specularFactor}},
                    {"glossinessFactor", glossinessFactor}
                }}
            };

            if(gltf.Materials != null)
            {
                var materials = gltf.Materials.ToList();
                materials.Add(m);
                gltf.Materials = materials.ToArray();
            }
            else
            {
                gltf.Materials = new []{m};
            }

            return gltf.Materials.Length - 1;
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
                    var children = gltf.Nodes[(int)parent].Children.ToList();
                    children.Add(id);
                    gltf.Nodes[(int)parent].Children = children.ToArray();
                }
                
            }

            return id;
        }

        internal static int AddTriangleMesh(this Gltf gltf, string name, List<byte> buffer, double[] vertices, double[] normals, int[] indices, 
        double[] colors, double[] vMin, double[] vMax, double[] nMin, double[] nMax, double[] cMin, double[] cMax, int iMin, int iMax, int materialId, int? parent_index, Transform transform = null)
        {
            var m = new glTFLoader.Schema.Mesh();
            m.Name = name;

            var vBuff = gltf.AddBufferView(0, buffer.Count(), vertices.Length * sizeof(float), null, null);
            var nBuff = gltf.AddBufferView(0, buffer.Count() + vertices.Length * sizeof(float), normals.Length * sizeof(float), null, null);
            var iBuff = gltf.AddBufferView(0, buffer.Count() + vertices.Length * sizeof(float) + normals.Length * sizeof(float), indices.Length * sizeof(int), null, null);

            buffer.AddRange(vertices.SelectMany(v=>BitConverter.GetBytes((float)v)));
            buffer.AddRange(normals.SelectMany(v=>BitConverter.GetBytes((float)v)));
            buffer.AddRange(indices.SelectMany(v=>BitConverter.GetBytes(v)));
            
            while(buffer.Count() % 4 != 0)
            {
                Console.WriteLine("Padding...");
                buffer.Add(0);
            }

            var vAccess = gltf.AddAccessor(vBuff, 0, Accessor.ComponentTypeEnum.FLOAT, vertices.Length/3, new[]{(float)vMin[0], (float)vMin[1], (float)vMin[2]}, new[]{(float)vMax[0],(float)vMax[1],(float)vMax[2]}, Accessor.TypeEnum.VEC3);
            var nAccess = gltf.AddAccessor(nBuff, 0, Accessor.ComponentTypeEnum.FLOAT, normals.Length/3, new[]{(float)nMin[0], (float)nMin[1], (float)nMin[2]}, new[]{(float)nMax[0], (float)nMax[1], (float)nMax[2]}, Accessor.TypeEnum.VEC3);
            var iAccess = gltf.AddAccessor(iBuff, 0, Accessor.ComponentTypeEnum.UNSIGNED_INT, indices.Length, new[]{(float)iMin}, new[]{(float)iMax}, Accessor.TypeEnum.SCALAR);
            
            var prim = new MeshPrimitive();
            prim.Indices = iAccess;
            prim.Material = materialId;
            prim.Mode = MeshPrimitive.ModeEnum.TRIANGLE_FAN;
            prim.Attributes = new Dictionary<string,int>{
                {"NORMAL",nAccess},
                {"POSITION",vAccess}
            };

            m.Primitives = new[]{prim};

            // Add mesh to gltf
            if(gltf.Meshes != null) 
            {
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
    }
}