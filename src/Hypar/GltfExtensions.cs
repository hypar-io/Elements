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

        internal static int AddTriangleMesh(this Gltf gltf, List<byte> buffer, double[] vertices, double[] normals, int[] indices, int materialId, int? parent_index, Transform transform = null)
        {
            var m = new glTFLoader.Schema.Mesh();
            
            var v_max_x = double.MinValue;
            var v_max_y = double.MinValue;
            var v_max_z = double.MinValue;
            var v_min_x = double.MaxValue;
            var v_min_y = double.MaxValue;
            var v_min_z = double.MaxValue;

            for(var i=0; i<vertices.Length; i+=3)
            {
                v_max_x = Math.Max(v_max_x, vertices[i]);
                v_min_x = Math.Min(v_min_x, vertices[i]);
                v_max_y = Math.Max(v_max_y, vertices[i+1]);
                v_min_y = Math.Min(v_min_y, vertices[i+1]);
                v_max_z = Math.Max(v_max_z, vertices[i+2]);
                v_min_z = Math.Min(v_min_z, vertices[i+2]);
            }

            var n_max_x = double.MinValue;
            var n_max_y = double.MinValue;
            var n_max_z = double.MinValue;
            var n_min_x = double.MaxValue;
            var n_min_y = double.MaxValue;
            var n_min_z = double.MaxValue;

            for(var i=0; i<normals.Length; i+=3)
            {
                n_max_x = Math.Max(n_max_x, normals[i]);
                n_min_x = Math.Min(n_min_x, normals[i]);
                n_max_y = Math.Max(n_max_y, normals[i+1]);
                n_min_y = Math.Min(n_min_y, normals[i+1]);
                n_max_z = Math.Max(n_max_z, normals[i+2]);
                n_min_z = Math.Min(n_min_z, normals[i+2]);
            }

            var i_max = indices.Max();
            var i_min = indices.Min();

            var vBuff = gltf.AddBufferView(0, buffer.Count(), vertices.Length * sizeof(float), null, null);
            var nBuff = gltf.AddBufferView(0, buffer.Count() + vertices.Length * sizeof(float), normals.Length * sizeof(float), null, null);
            var iBuff = gltf.AddBufferView(0, buffer.Count() + vertices.Length * sizeof(float) + normals.Length * sizeof(float), indices.Length * sizeof(int), null, null);

            buffer.AddRange(vertices.SelectMany(v=>BitConverter.GetBytes((float)v)));
            buffer.AddRange(normals.SelectMany(v=>BitConverter.GetBytes((float)v)));
            buffer.AddRange(indices.SelectMany(v=>BitConverter.GetBytes(v)));
            
            var vAccess = gltf.AddAccessor(vBuff, 0, Accessor.ComponentTypeEnum.FLOAT, vertices.Length/3, new[]{(float)v_min_x, (float)v_min_y, (float)v_min_z}, new[]{(float)v_max_x,(float)v_max_y,(float)v_max_z}, Accessor.TypeEnum.VEC3);
            var nAccess = gltf.AddAccessor(nBuff, 0, Accessor.ComponentTypeEnum.FLOAT, normals.Length/3, new[]{(float)n_min_x, (float)n_min_y, (float)n_min_z}, new[]{(float)n_max_x, (float)n_max_y, (float)n_max_z}, Accessor.TypeEnum.VEC3);
            var iAccess = gltf.AddAccessor(iBuff, 0, Accessor.ComponentTypeEnum.UNSIGNED_INT, indices.Length, new[]{(float)i_min}, new[]{(float)i_max}, Accessor.TypeEnum.SCALAR);
            
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