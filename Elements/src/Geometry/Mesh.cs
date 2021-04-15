using Elements.Serialization.JSON;
using LibTessDotNet.Double;
using Newtonsoft.Json;
using Octree;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using static Elements.Units;

namespace Elements.Geometry
{
    [JsonConverter(typeof(MeshConverter))]
    public partial class Mesh
    {
        private PointOctree<Vertex> _octree = new PointOctree<Vertex>(100000, new Octree.Point(0f, 0f, 0f), (float)Vector3.EPSILON);

        /// <summary>
        /// Construct an empty mesh.
        /// </summary>
        public Mesh()
        {
            // An empty mesh.
            this.Vertices = new List<Vertex>();
            this.Triangles = new List<Triangle>();
        }

        /// <summary>
        /// Construct a mesh from an STL file.
        /// </summary>
        /// <param name="stlPath">The path to the STL file.</param>
        /// <param name="unit">The length unit used in the file.</param>
        /// <returns></returns>
        public static Mesh FromSTL(string stlPath, LengthUnit unit = LengthUnit.Millimeter)
        {
            List<Vertex> vertexCache = new List<Vertex>();
            var mesh = new Mesh();

            var conversion = Units.GetConversionToMeters(unit);

            using (var reader = new StreamReader(stlPath))
            {
                string line;
                while ((line = reader.ReadLine()) != null)
                {
                    line = line.TrimStart();

                    if (line.StartsWith("facet"))
                    {
                        vertexCache.Clear();
                    }

                    if (line.StartsWith("vertex"))
                    {
                        var splits = line.Split(' ');
                        var x = double.Parse(splits[1]) * conversion;
                        var y = double.Parse(splits[2]) * conversion;
                        var z = double.Parse(splits[3]) * conversion;
                        var v = new Vertex(new Vector3(x, y, z));
                        mesh.AddVertex(v);
                        vertexCache.Add(v);
                    }

                    if (line.StartsWith("endfacet"))
                    {
                        var t = new Triangle(vertexCache[0], vertexCache[1], vertexCache[2]);
                        if (!HasDuplicatedVertices(t, out _))
                        {
                            mesh.AddTriangle(t);
                        }
                    }
                }
            }
            mesh.ComputeNormals();
            return mesh;
        }

        /// <summary>
        /// Get a string representation of the mesh.
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return $@"
Vertices:{Vertices.Count},
Triangles:{Triangles.Count}";
        }

        /// <summary>
        /// Compute the vertex normals by averaging
        /// the normals of the incident faces.
        /// </summary>
        public void ComputeNormals()
        {
            foreach (var v in this.Vertices)
            {
                if (v.Triangles.Count == 0)
                {
                    v.Normal = default(Vector3);
                    continue;
                }
                var avg = new Vector3();
                foreach (var t in v.Triangles)
                {
                    avg += t.Normal;
                }
                v.Normal = (avg / v.Triangles.Count).Unitized();
            }
        }

        /// <summary>
        /// Get all buffers required for rendering.
        /// </summary>
        public void GetBuffers(out byte[] vertexBuffer, out byte[] indexBuffer,
                                out byte[] normalBuffer, out byte[] colorBuffer, out byte[] uvBuffer,
                                out double[] v_max, out double[] v_min, out double[] n_min, out double[] n_max,
                                out float[] c_min, out float[] c_max, out ushort index_min, out ushort index_max,
                                out double[] uv_min, out double[] uv_max)
        {
            var floatSize = sizeof(float);
            var ushortSize = sizeof(ushort);

            vertexBuffer = new byte[this.Vertices.Count * floatSize * 3];
            normalBuffer = new byte[this.Vertices.Count * floatSize * 3];
            indexBuffer = new byte[this.Triangles.Count * ushortSize * 3];
            uvBuffer = new byte[this.Vertices.Count * floatSize * 2];

            if (!this.Vertices[0].Color.Equals(default(Color)))
            {
                colorBuffer = new byte[this.Vertices.Count * floatSize * 3];
                c_min = new float[] { float.MaxValue, float.MaxValue, float.MaxValue };
                c_max = new float[] { float.MinValue, float.MinValue, float.MinValue };
            }
            else
            {
                colorBuffer = new byte[0];
                c_min = new float[0];
                c_max = new float[0];
            }

            v_max = new double[3] { double.MinValue, double.MinValue, double.MinValue };
            v_min = new double[3] { double.MaxValue, double.MaxValue, double.MaxValue };
            n_min = new double[3] { double.MaxValue, double.MaxValue, double.MaxValue };
            n_max = new double[3] { double.MinValue, double.MinValue, double.MinValue };
            uv_max = new double[2] { double.MinValue, double.MinValue };
            uv_min = new double[2] { double.MaxValue, double.MaxValue };

            index_max = ushort.MinValue;
            index_min = ushort.MaxValue;

            var vi = 0;
            var ii = 0;
            var ci = 0;
            var uvi = 0;

            for (var i = 0; i < this.Vertices.Count; i++)
            {
                var v = this.Vertices[i];

                System.Buffer.BlockCopy(BitConverter.GetBytes((float)v.Position.X), 0, vertexBuffer, vi, floatSize);
                System.Buffer.BlockCopy(BitConverter.GetBytes((float)v.Position.Y), 0, vertexBuffer, vi + floatSize, floatSize);
                System.Buffer.BlockCopy(BitConverter.GetBytes((float)v.Position.Z), 0, vertexBuffer, vi + 2 * floatSize, floatSize);

                System.Buffer.BlockCopy(BitConverter.GetBytes((float)v.Normal.X), 0, normalBuffer, vi, floatSize);
                System.Buffer.BlockCopy(BitConverter.GetBytes((float)v.Normal.Y), 0, normalBuffer, vi + floatSize, floatSize);
                System.Buffer.BlockCopy(BitConverter.GetBytes((float)v.Normal.Z), 0, normalBuffer, vi + 2 * floatSize, floatSize);

                System.Buffer.BlockCopy(BitConverter.GetBytes((float)v.UV.U), 0, uvBuffer, uvi, floatSize);
                System.Buffer.BlockCopy(BitConverter.GetBytes((float)v.UV.V), 0, uvBuffer, uvi + floatSize, floatSize);

                uvi += 2 * floatSize;
                vi += 3 * floatSize;

                v_max[0] = Math.Max(v_max[0], v.Position.X);
                v_max[1] = Math.Max(v_max[1], v.Position.Y);
                v_max[2] = Math.Max(v_max[2], v.Position.Z);
                v_min[0] = Math.Min(v_min[0], v.Position.X);
                v_min[1] = Math.Min(v_min[1], v.Position.Y);
                v_min[2] = Math.Min(v_min[2], v.Position.Z);

                n_max[0] = Math.Max(n_max[0], v.Normal.X);
                n_max[1] = Math.Max(n_max[1], v.Normal.Y);
                n_max[2] = Math.Max(n_max[2], v.Normal.Z);
                n_min[0] = Math.Min(n_min[0], v.Normal.X);
                n_min[1] = Math.Min(n_min[1], v.Normal.Y);
                n_min[2] = Math.Min(n_min[2], v.Normal.Z);

                uv_max[0] = Math.Max(uv_max[0], v.UV.U);
                uv_max[1] = Math.Max(uv_max[1], v.UV.V);
                uv_min[0] = Math.Min(uv_min[0], v.UV.U);
                uv_min[1] = Math.Min(uv_min[1], v.UV.V);

                index_max = Math.Max(index_max, (ushort)v.Index);
                index_min = Math.Min(index_min, (ushort)v.Index);

                if (!v.Color.Equals(default(Color)))
                {
                    c_max[0] = Math.Max(c_max[0], (float)v.Color.Red);
                    c_max[1] = Math.Max(c_max[1], (float)v.Color.Green);
                    c_max[2] = Math.Max(c_max[2], (float)v.Color.Blue);
                    c_min[0] = Math.Min(c_min[0], (float)v.Color.Red);
                    c_min[1] = Math.Min(c_min[1], (float)v.Color.Green);
                    c_min[2] = Math.Min(c_min[2], (float)v.Color.Blue);

                    System.Buffer.BlockCopy(BitConverter.GetBytes((float)v.Color.Red), 0, colorBuffer, ci, floatSize);
                    System.Buffer.BlockCopy(BitConverter.GetBytes((float)v.Color.Green), 0, colorBuffer, ci + floatSize, floatSize);
                    System.Buffer.BlockCopy(BitConverter.GetBytes((float)v.Color.Blue), 0, colorBuffer, ci + 2 * floatSize, floatSize);
                    ci += 3 * floatSize;
                }
            }

            for (var i = 0; i < this.Triangles.Count; i++)
            {
                var t = this.Triangles[i];

                System.Buffer.BlockCopy(BitConverter.GetBytes((ushort)t.Vertices[0].Index), 0, indexBuffer, ii, ushortSize);
                System.Buffer.BlockCopy(BitConverter.GetBytes((ushort)t.Vertices[1].Index), 0, indexBuffer, ii + ushortSize, ushortSize);
                System.Buffer.BlockCopy(BitConverter.GetBytes((ushort)t.Vertices[2].Index), 0, indexBuffer, ii + 2 * ushortSize, ushortSize);
                ii += 3 * ushortSize;
            }
        }

        /// <summary>
        /// Add a triangle to the mesh.
        /// </summary>
        /// <param name="a">The first vertex.</param>
        /// <param name="b">The second vertex.</param>
        /// <param name="c">The third vertex.</param>
        public Triangle AddTriangle(Vertex a, Vertex b, Vertex c)
        {
            var t = new Triangle(a, b, c);
            if (HasDuplicatedVertices(t, out Vector3 duplicate))
            {
                return null;
                // throw new ArgumentException($"Not a valid Triangle.  Duplicate vertex at {duplicate}.");
            }
            this.Triangles.Add(t);
            return t;
        }

        /// <summary>
        /// Add a triangle to the mesh.
        /// </summary>
        /// <param name="t">The triangle to add.</param>
        public Triangle AddTriangle(Triangle t)
        {
            if (HasDuplicatedVertices(t, out Vector3 duplicate))
            {
                throw new ArgumentException($"Not a valid Triangle.  Duplicate vertex at {duplicate}.");
            }
            this.Triangles.Add(t);
            return t;
        }

        /// <summary>
        /// Add a vertex to the mesh.
        /// </summary>
        /// <param name="position">The position of the vertex.</param>
        /// <param name="normal">The vertex's normal.</param>
        /// <param name="color">The vertex's color.</param>
        /// <param name="merge">If true, and a vertex already exists with a 
        /// position within Vector3.EPSILON, that vertex will be returned.</param>
        /// <param name="edgeAngle">If merge is true, vertices will be 
        /// merged if the angle between them is less than edgeAngle.</param>
        /// <param name="uv">The texture coordinate of the vertex.</param>
        /// <returns>The newly created vertex.</returns>
        public Vertex AddVertex(Vector3 position,
                                UV uv = default(UV),
                                Vector3 normal = default(Vector3),
                                Color color = default(Color),
                                bool merge = false,
                                double edgeAngle = 45.0)
        {
            var p = new Octree.Point((float)position.X, (float)position.Y, (float)position.Z);

            if (merge)
            {
                var search = this._octree.GetNearby(p, (float)Vector3.EPSILON);
                if (search.Length > 0)
                {
                    var angle = search[0].Normal.AngleTo(normal);
                    if (angle < edgeAngle)
                    {
                        return search[0];
                    }
                }
            }

            var v = new Vertex(position, normal, color);
            v.UV = uv;
            this.Vertices.Add(v);
            v.Index = (this.Vertices.Count) - 1;
            this._octree.Add(v, p);
            return v;
        }

        /// <summary>
        /// Add a vertex to the mesh.
        /// </summary>
        /// <param name="v">The vertex to add.</param>
        public Vertex AddVertex(Vertex v)
        {
            this.Vertices.Add(v);
            v.Index = (this.Vertices.Count) - 1;
            return v;
        }

        /// <summary>
        /// Calculate the volume of the mesh.
        /// This value will be inexact for open meshes.
        /// </summary>
        /// <returns>The volume of the mesh.</returns>
        public double Volume()
        {
            return Math.Abs(this.Triangles.Sum(t => SignedVolumeOfTriangle(t)));
        }

        internal void AddMesh(Mesh mesh)
        {
            for (var i = 0; i < mesh.Vertices.Count; i++)
            {
                AddVertex(mesh.Vertices[i]);
            }

            for (var i = 0; i < mesh.Triangles.Count; i++)
            {
                AddTriangle(mesh.Triangles[i]);
            }
        }

        private bool IsInvalid(Vertex v)
        {
            if (v.Position.IsNaN())
            {
                return true;
            }

            if (v.Normal.IsNaN())
            {
                return true;
            }

            return false;
        }

        private static bool HasDuplicatedVertices(Triangle t, out Vector3 duplicate)
        {
            if (t.Vertices[0].Position.IsAlmostEqualTo(t.Vertices[1].Position))
            {
                duplicate = t.Vertices[0].Position;
                return true;
            }
            if (t.Vertices[0].Position.IsAlmostEqualTo(t.Vertices[2].Position))
            {
                duplicate = t.Vertices[0].Position;
                return true;
            }
            if (t.Vertices[1].Position.IsAlmostEqualTo(t.Vertices[2].Position))
            {
                duplicate = t.Vertices[1].Position;
                return true;
            }
            duplicate = default(Vector3);
            return false;
        }

        private double SignedVolumeOfTriangle(Triangle t)
        {
            var p1 = t.Vertices[0].Position;
            var p2 = t.Vertices[1].Position;
            var p3 = t.Vertices[2].Position;

            var v321 = p3.X * p2.Y * p1.Z;
            var v231 = p2.X * p3.Y * p1.Z;
            var v312 = p3.X * p1.Y * p2.Z;
            var v132 = p1.X * p3.Y * p2.Z;
            var v213 = p2.X * p1.Y * p3.Z;
            var v123 = p1.X * p2.Y * p3.Z;
            return (1.0 / 6.0) * (-v321 + v231 + v312 - v132 - v213 + v123);
        }
    }

    internal static class TessExtensions
    {
        internal static Vector3 ToVector3(this Vec3 v)
        {
            return new Vector3(v.X, v.Y, v.Z);
        }
    }
}