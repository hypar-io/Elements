using LibTessDotNet.Double;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace Elements.Geometry
{
    /// <summary>
    /// A mesh triangle.
    /// </summary>
    public class Triangle
    {
        /// <summary>
        /// The triangle's vertices.
        /// </summary>
        public Vertex[] Vertices { get; }

        /// <summary>
        /// The triangle's normal.
        /// </summary>
        /// <value></value>
        public Vector3 Normal { get; }

        /// <summary>
        /// Create a triangle.
        /// </summary>
        /// <param name="a">The index of the first vertex of the triangle.</param>
        /// <param name="b">The index of the second vertex of the triangle.</param>
        /// <param name="c">The index of the third vertex of the triangle.</param>
        public Triangle(Vertex a, Vertex b, Vertex c)
        {
            this.Vertices = new[] { a, b, c };

            if (!a.Triangles.Contains(this))
            {
                a.Triangles.Add(this);
            }

            if (!b.Triangles.Contains(this))
            {
                b.Triangles.Add(this);
            }

            if (!c.Triangles.Contains(this))
            {
                c.Triangles.Add(this);
            }

            var ab = (b.Position - a.Position).Normalized();
            var bc = (c.Position - a.Position).Normalized();
            this.Normal = ab.Cross(bc).Normalized();
        }

        [JsonConstructor]
        internal Triangle(Vertex[] vertices)
        {
            if (vertices.Length != 3)
            {
                throw new ArgumentException("Triangles can only be created with three vertices.");
            }

            this.Vertices = vertices;
            foreach (var v in vertices)
            {
                if (!v.Triangles.Contains(this))
                {
                    v.Triangles.Add(this);
                }
            }
            var a = vertices[0];
            var b = vertices[1];
            var c = vertices[2];

            var ab = (b.Position - a.Position).Normalized();
            var bc = (c.Position - a.Position).Normalized();
            this.Normal = ab.Cross(bc).Normalized();
        }

        /// <summary>
        /// The area of the triangle.
        /// </summary>
        public double Area()
        {
            var a = this.Vertices[0].Position;
            var b = this.Vertices[1].Position;
            var c = this.Vertices[2].Position;

            // Heron's formula
            var l1 = a.DistanceTo(b);
            var l2 = b.DistanceTo(c);
            var l3 = c.DistanceTo(a);

            var s = (l1 + l2 + l3) / 2;
            return Math.Sqrt(s * (s - l1) * (s - l2) * (s - l3));
        }

        internal Polygon ToPolygon()
        {
            return new Polygon(new[] { this.Vertices[0].Position, this.Vertices[1].Position, this.Vertices[2].Position });
        }

        internal ContourVertex[] ToContourVertexArray()
        {
            var contour = new ContourVertex[this.Vertices.Length];
            for (var i = 0; i < this.Vertices.Length; i++)
            {
                var v = this.Vertices[i];
                contour[i] = new ContourVertex();
                contour[i].Position = new Vec3 { X = v.Position.X, Y = v.Position.Y, Z = v.Position.Z };
            }
            return contour;
        }
    }

    /// <summary>
    /// A UV texture coordinate.
    /// </summary>
    public struct UV
    {
        /// <summary>
        /// The U coordinate.
        /// </summary>
        public double U { get; set; }

        /// <summary>
        /// The v coordinate.
        /// </summary>
        public double V { get; set; }

        /// <summary>
        /// Construct a UV.
        /// </summary>
        /// <param name="u">The u parameter.</param>
        /// <param name="v">The v parameter.</param>
        public UV(double u, double v)
        {
            this.U = u;
            this.V = v;
        }
    }

    /// <summary>
    /// A mesh vertex.
    /// </summary>
    public class Vertex
    {
        /// <summary>
        /// The position of the vertex.
        /// </summary>
        public Vector3 Position { get; set; }

        /// <summary>
        /// The vertex's normal.
        /// </summary>
        [JsonIgnore]
        public Vector3 Normal { get; set; }

        /// <summary>
        /// The vertex's color.
        /// </summary>
        public Color Color { get; set; }

        /// <summary>
        /// The index of the vertex within a mesh.
        /// </summary>
        public int Index { get; internal set; }

        /// <summary>
        /// The texture coordinate of the vertex.
        /// </summary>
        public UV UV { get; set; }

        /// <summary>
        /// The triangles which contain this vertex.
        /// </summary>
        public List<Triangle> Triangles { get; } = new List<Triangle>();

        /// <summary>
        /// Create a vertex.
        /// </summary>
        /// <param name="position">The position of the vertex.</param>
        /// <param name="normal">The vertex's normal.</param>
        /// <param name="color">The vertex's color.</param>
        public Vertex(Vector3 position, Vector3? normal = null, Color color = default(Color))
        {
            this.Position = position;
            this.Normal = Vector3.Origin;
            this.Color = color;
        }
    }

    /// <summary>
    /// An indexed mesh.
    /// </summary>
    public class Mesh
    {
        private List<Vertex> _vertices = new List<Vertex>();
        private List<Triangle> _triangles = new List<Triangle>();

        /// <summary>
        /// The mesh's vertices.
        /// </summary>
        public List<Vertex> Vertices => _vertices;

        /// <summary>
        /// The mesh's triangles.
        /// </summary>
        public List<Triangle> Triangles => _triangles;

        /// <summary>
        /// Construct an empty mesh.
        /// </summary>
        public Mesh()
        {
            // An empty mesh.
        }

        /// <summary>
        /// Get a string representation of the mesh.
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return $@"
Vertices:{_vertices.Count},
Triangles:{_triangles.Count}";
        }

        /// <summary>
        /// Compute the vertex normals by averaging
        /// the normals of the incident faces.
        /// </summary>
        public void ComputeNormals()
        {
            foreach (var v in this.Vertices)
            {
                var avg = new Vector3();
                foreach (var t in v.Triangles)
                {
                    avg += t.Normal;
                }
                v.Normal = avg / v.Triangles.Count;
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

            vertexBuffer = new byte[this._vertices.Count * floatSize * 3];
            normalBuffer = new byte[this._vertices.Count * floatSize * 3];
            indexBuffer = new byte[this._triangles.Count * ushortSize * 3];
            uvBuffer = new byte[this._vertices.Count * floatSize * 2];

            if (!this._vertices[0].Color.Equals(default(Color)))
            {
                colorBuffer = new byte[this._vertices.Count * floatSize * 3];
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

            for (var i = 0; i < this._vertices.Count; i++)
            {
                var v = this._vertices[i];

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

            for (var i = 0; i < this._triangles.Count; i++)
            {
                var t = this._triangles[i];

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
        internal Triangle AddTriangle(Vertex a, Vertex b, Vertex c)
        {
            // Calculate the face normal
            var v1 = b.Position - a.Position;
            var v2 = c.Position - a.Position;
            var n = v1.Cross(v2).Normalized();
            if (Double.IsNaN(n.X) || Double.IsNaN(n.Y) || Double.IsNaN(n.Z))
            {
                Debug.WriteLine("Degenerate triangle found.");
                return null;
            }

            // If the vertices normals are null, set them to the face normal.
            a.Normal = a.Normal.Equals(default(Vector3)) ? n : a.Normal;
            b.Normal = b.Normal.Equals(default(Vector3)) ? n : b.Normal;
            c.Normal = c.Normal.Equals(default(Vector3)) ? n : c.Normal;

            var t = new Triangle(a, b, c);
            this._triangles.Add(t);
            return t;
        }

        internal Triangle AddTriangle(Triangle t)
        {
            this._triangles.Add(t);
            return t;
        }

        /// <summary>
        /// Add a vertex to the mesh.
        /// </summary>
        /// <param name="position">The position of the vertex.</param>
        /// <param name="normal">The vertex's normal.</param>
        /// <param name="color">The vertex's color.</param>
        /// <param name="uv">The texture coordinate of the vertex.</param>
        /// <returns>The newly created vertex.</returns>
        internal Vertex AddVertex(Vector3 position, UV uv, Vector3 normal = default(Vector3), Color color = default(Color))
        {
            var v = new Vertex(position, normal, color);
            v.UV = uv;
            this._vertices.Add(v);
            v.Index = (this._vertices.Count) - 1;
            return v;
        }

        internal Vertex AddVertex(Vertex v)
        {
            this._vertices.Add(v);
            v.Index = (this._vertices.Count) - 1;
            return v;
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

        private bool IsInvalid(Elements.Geometry.Vertex v)
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
    }

    internal static class TessExtensions
    {
        internal static Vector3 ToVector3(this Vec3 v)
        {
            return new Vector3(v.X, v.Y, v.Z);
        }
    }
}