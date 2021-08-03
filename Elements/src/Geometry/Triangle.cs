using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using LibTessDotNet.Double;

namespace Elements.Geometry
{
    public partial class Triangle
    {
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


        /// <summary>
        /// Is the provided triangle approximately geometrically equal to this one?
        /// </summary>
        public bool IsAlmostEqualTo(Triangle other, bool rotationDependent = false, double tolerance = Vector3.EPSILON)
        {
            if (rotationDependent)
            {
                return this.Vertices[0].Position.IsAlmostEqualTo(other.Vertices[0].Position, tolerance)
                        && this.Vertices[1].Position.IsAlmostEqualTo(other.Vertices[1].Position, tolerance)
                        && this.Vertices[2].Position.IsAlmostEqualTo(other.Vertices[2].Position, tolerance);
            }
            else
            {
                var points = new HashSet<Vector3>(this.Vertices.Select(v => v.Position), new Vector3Comparer(tolerance));
                var otherPoints = new HashSet<Vector3>(other.Vertices.Select(v => v.Position), new Vector3Comparer(tolerance));
                return points.Except(otherPoints).Count() == 0;
            }
        }
    }

    /// <summary>
    /// Triangle comparer that will compare based only on the vertices, and allows for flexible geometry comparisons.
    /// This comparer is strictly speaking an imperfect implementation of IEqualityComparer — and so should be used
    /// with caution in collections and Linq methods. It may occasionally indicate that two Triangles are distinct when
    /// they are within the specified tolerance of each other.
    /// </summary>
    public class TriangleComparer : IEqualityComparer<Triangle>
    {
        private bool _rotationDependent = false;
        private double _tolerance = Vector3.EPSILON;

        /// <summary>
        /// Construct a comparer setting whether the vertex orientation matters, and optionally the tolerance.
        /// </summary>
        public TriangleComparer(bool rotationDependent, double tolerance = Vector3.EPSILON)
        {
            _rotationDependent = rotationDependent;
            _tolerance = tolerance;
        }

        /// <summary>
        /// Are the two triangles equal according to the comparer settings?
        /// </summary>
        public bool Equals(Triangle a, Triangle b)
        {
            return a.IsAlmostEqualTo(b, _rotationDependent, _tolerance);
        }

        /// <summary>
        /// Retrieve a hashcode for this triangle that is consistent with the direction dependance and tolerance.
        /// </summary>
        public int GetHashCode(Triangle triangle)
        {
            var vertices = triangle.Vertices.ToList();
            if (!_rotationDependent)
            {
                vertices = triangle.Vertices.OrderBy(v => Math.Abs(v.Position.X) + Math.Abs(v.Position.Y) + Math.Abs(v.Position.Z)).ToList();
            }

            unchecked
            {
                var hash = 391 + vertices[0].Position.GetHashCode(_tolerance);
                hash = hash * 23 + vertices[1].Position.GetHashCode(_tolerance);
                hash = hash * 23 + vertices[2].Position.GetHashCode(_tolerance);
                return hash;
            }
        }
    }
}