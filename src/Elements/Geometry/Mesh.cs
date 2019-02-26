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
        [JsonProperty("vertices")]
        public Vertex[] Vertices { get; }

        /// <summary>
        /// The triangle's normal.
        /// </summary>
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

            // Bend the normals for the associated vertices.
            var p1 = new Plane(a.Position, b.Position, c.Position);
            a.Normal = ((a.Normal + p1.Normal) / 2.0).Normalized();
            b.Normal = ((b.Normal + p1.Normal) / 2.0).Normalized();
            c.Normal = ((c.Normal + p1.Normal) / 2.0).Normalized();

            this.Normal = p1.Normal;
        }

        [JsonConstructor]
        internal Triangle(Vertex[] vertices)
        {
            this.Vertices = vertices;

            var a = this.Vertices[0];
            var b = this.Vertices[1];
            var c = this.Vertices[2];

            // Bend the normals for the associated vertices.
            var p1 = new Plane(a.Position, b.Position, c.Position);
            a.Normal = ((a.Normal + p1.Normal) / 2.0).Normalized();
            b.Normal = ((b.Normal + p1.Normal) / 2.0).Normalized();
            c.Normal = ((c.Normal + p1.Normal) / 2.0).Normalized();

            this.Normal = p1.Normal;
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
    /// A mesh vertex.
    /// </summary>
    public class Vertex
    {
        /// <summary>
        /// The position of the vertex.
        /// </summary>
        [JsonProperty("position")]
        public Vector3 Position { get; }

        /// <summary>
        /// The vertex's normal.
        /// </summary>
        [JsonIgnore]
        public Vector3 Normal { get; internal set; }

        /// <summary>
        /// The vertex's color.
        /// </summary>
        [JsonProperty("color")]
        public Color Color { get; internal set;}

        /// <summary>
        /// The index of the vertex within a mesh.
        /// </summary>
        [JsonProperty("index")]
        public int Index { get; internal set; }

        /// <summary>
        /// Create a vertex.
        /// </summary>
        /// <param name="position">The position of the vertex.</param>
        /// <param name="normal">The vertex's normal.</param>
        /// <param name="color">The vertex's color.</param>
        public Vertex(Vector3 position, Vector3 normal = null, Color color = null)
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
        [JsonProperty("vertices")]
        public List<Vertex> Vertices => _vertices;

        /// <summary>
        /// The mesh's triangles.
        /// </summary>
        [JsonProperty("triangles")]
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
        /// Get all buffers required for rendering.
        /// </summary>
        public void GetBuffers(out double[] vertexBuffer, out ushort[] indexBuffer,
                                out double[] normalBuffer, out float[] colorBuffer,
                                out double[] v_max, out double[] v_min, out double[] n_min, out double[] n_max,
                                out float[] c_min, out float[] c_max, out ushort index_min, out ushort index_max)
        {
            vertexBuffer = new double[this._vertices.Count * 3];
            normalBuffer = new double[this._vertices.Count * 3];
            indexBuffer = new ushort[this._triangles.Count * 3];

            if(this._vertices[0].Color != null)
            {
                colorBuffer = new float[this._vertices.Count * 3];
                c_min = new float[] { float.MaxValue, float.MaxValue, float.MaxValue };
                c_max = new float[] { float.MinValue, float.MinValue, float.MinValue };
            }
            else
            {
                colorBuffer = new float[0];
                c_min = new float[0];
                c_max= new float[0];
            }

            v_max = new double[3] { double.MinValue, double.MinValue, double.MinValue };
            v_min = new double[3] { double.MaxValue, double.MaxValue, double.MaxValue };
            n_min = new double[3] { double.MaxValue, double.MaxValue, double.MaxValue };
            n_max = new double[3] { double.MinValue, double.MinValue, double.MinValue };

            index_max = ushort.MinValue;
            index_min = ushort.MaxValue;

            var vi = 0;
            var ci = 0;

            for (var i = 0; i < this._vertices.Count; i++)
            {
                var v = this._vertices[i];
                vertexBuffer[vi] = v.Position.X;
                vertexBuffer[vi + 1] = v.Position.Y;
                vertexBuffer[vi + 2] = v.Position.Z;

                normalBuffer[vi] = v.Normal.X;
                normalBuffer[vi + 1] = v.Normal.Y;
                normalBuffer[vi + 2] = v.Normal.Z;

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

                index_max = Math.Max(index_max, (ushort)v.Index);
                index_min = Math.Min(index_min, (ushort)v.Index);

                vi += 3;

                if (v.Color != null)
                {
                    c_max[0] = Math.Max(c_max[0], v.Color.Red);
                    c_max[1] = Math.Max(c_max[1], v.Color.Green);
                    c_max[2] = Math.Max(c_max[2], v.Color.Blue);
                    c_min[0] = Math.Min(c_min[0], v.Color.Red);
                    c_min[1] = Math.Min(c_min[1], v.Color.Green);
                    c_min[2] = Math.Min(c_min[2], v.Color.Blue);

                    colorBuffer[ci] = v.Color.Red;
                    colorBuffer[ci + 1] = v.Color.Green;
                    colorBuffer[ci + 2] = v.Color.Blue;

                    ci += 3;
                }
            }

            var ti = 0;
            for (var i = 0; i < this._triangles.Count; i++)
            {
                var t = this._triangles[i];
                indexBuffer[ti] = (ushort)t.Vertices[0].Index;
                indexBuffer[ti + 1] = (ushort)t.Vertices[1].Index;
                indexBuffer[ti + 2] = (ushort)t.Vertices[2].Index;
                ti += 3;
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
            a.Normal = a.Normal == null ? n : a.Normal;
            b.Normal = b.Normal == null ? n : b.Normal;
            c.Normal = c.Normal == null ? n : c.Normal;

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
        /// <returns>The newly created vertex.</returns>
        internal Vertex AddVertex(Vector3 position, Vector3 normal = null, Color color = null)
        {
            var v = new Vertex(position, normal, color);
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