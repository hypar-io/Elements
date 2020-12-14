using Elements.Geometry.Solids;
using Elements.Interfaces;
using LibTessDotNet.Double;
using System.Collections.Generic;
using System.IO;
using static Elements.Units;

namespace Elements.Geometry
{
    public partial class Mesh : IRenderable
    {
        /// <summary>
        /// Construct an empty mesh.
        /// </summary>
        public Mesh() : base(BuiltInMaterials.Default)
        {
            this.Vertices = new List<Vertex>();
            this.Triangles = new List<Triangle>();
            // An empty mesh.
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
                        mesh.AddTriangle(t);
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
        /// Add a triangle to the mesh.
        /// </summary>
        /// <param name="a">The first vertex.</param>
        /// <param name="b">The second vertex.</param>
        /// <param name="c">The third vertex.</param>
        public Triangle AddTriangle(Vertex a, Vertex b, Vertex c)
        {
            var t = new Triangle(a, b, c);
            this.Triangles.Add(t);
            return t;
        }

        /// <summary>
        /// Add a triangle to the mesh.
        /// </summary>
        /// <param name="t">The triangle to add.</param>
        public Triangle AddTriangle(Triangle t)
        {
            this.Triangles.Add(t);
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
        public Vertex AddVertex(Vector3 position, UV uv = default(UV), Vector3 normal = default(Vector3), Color color = default(Color))
        {
            var v = new Vertex(position, normal, color);
            v.UV = uv;
            this.Vertices.Add(v);
            v.Index = (this.Vertices.Count) - 1;
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

        /// <summary>
        /// Render the mesh.
        /// </summary>
        /// <param name="renderer">The renderer.</param>
        public void Render(IRenderer renderer)
        {
            renderer.Render(this);
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