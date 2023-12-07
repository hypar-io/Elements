using System;
using System.Collections.Generic;
using System.Diagnostics;
using LibTessDotNet.Double;
using System.Text.Json.Serialization;

namespace Elements.Geometry
{
    /// <summary>
    /// A mesh triangle.
    /// </summary>
    public class Triangle
    {
        /// <summary>The triangle's vertices.</summary>
        [System.ComponentModel.DataAnnotations.Required]
        public IList<Vertex> Vertices { get; set; } = new List<Vertex>();

        /// <summary>The triangle's normal.</summary>
        public Vector3 Normal { get; set; }

        /// <summary>
        /// Construct a triangle.
        /// </summary>
        /// <param name="vertices">The vertices of the triangle.</param>
        /// <param name="normal">The normal of the triangle.</param>
        [JsonConstructor]
        public Triangle(IList<Vertex> vertices, Vector3 normal)
        {
            this.Vertices = vertices;
            foreach (var vert in vertices)
            {
                vert.Triangles.Add(this);
            }
            this.Normal = normal;
        }

        /// <summary>
        /// Create a triangle.
        /// </summary>
        /// <param name="a">The index of the first vertex of the triangle.</param>
        /// <param name="b">The index of the second vertex of the triangle.</param>
        /// <param name="c">The index of the third vertex of the triangle.</param>
        public Triangle(Vertex a, Vertex b, Vertex c)
        {
            this.Vertices = new[] { a, b, c };

            a.Triangles.Add(this);
            b.Triangles.Add(this);
            c.Triangles.Add(this);

            var ab = (b.Position - a.Position).Unitized();
            var bc = (c.Position - a.Position).Unitized();
            this.Normal = ab.Cross(bc).Unitized();

            if (Double.IsNaN(this.Normal.X) || Double.IsNaN(this.Normal.Y) || Double.IsNaN(this.Normal.Z))
            {
                Debug.WriteLine("Degenerate triangle found.");
            }
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
            var contour = new ContourVertex[this.Vertices.Count];
            for (var i = 0; i < this.Vertices.Count; i++)
            {
                var v = this.Vertices[i];
                contour[i] = new ContourVertex();
                contour[i].Position = new Vec3 { X = v.Position.X, Y = v.Position.Y, Z = v.Position.Z };
            }
            return contour;
        }

        internal bool HasDuplicatedVertices(out Vector3 duplicate)
        {
            if (this.Vertices[0].Position.IsAlmostEqualTo(this.Vertices[1].Position))
            {
                duplicate = this.Vertices[0].Position;
                return true;
            }
            if (this.Vertices[0].Position.IsAlmostEqualTo(this.Vertices[2].Position))
            {
                duplicate = this.Vertices[0].Position;
                return true;
            }
            if (this.Vertices[1].Position.IsAlmostEqualTo(this.Vertices[2].Position))
            {
                duplicate = this.Vertices[1].Position;
                return true;
            }
            duplicate = default(Vector3);
            return false;
        }
    }
}