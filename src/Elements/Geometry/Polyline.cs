using Elements.Geometry.Interfaces;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace Elements.Geometry
{
    /// <summary>
    /// A coplanar continuous set of lines.
    /// </summary>
    public partial class Polyline : ICurve
    {
        /// <summary>
        /// Calculate the length of the polygon.
        /// </summary>
        public override double Length()
        {
            var length = 0.0;
            for(var i=0; i<this.Vertices.Count-1; i++)
            {
                length += this.Vertices[i].DistanceTo(this.Vertices[i+1]);
            }
            return length;
        }

        /// <summary>
        /// The start of the polyline.
        /// </summary>
        [JsonIgnore]
        public Vector3 Start
        {
            get { return this.Vertices[0]; }
        }

        /// <summary>
        /// The end of the polyline.
        /// </summary>
        [JsonIgnore]
        public Vector3 End
        {
            get { return this.Vertices[this.Vertices.Count - 1]; }
        }

        /// <summary>
        /// Construct a polyline from a collection of vertices.
        /// </summary>
        /// <param name="vertices">A CCW wound set of vertices.</param>
        [JsonConstructor]
        public Polyline(IList<Vector3> vertices)
        {
            CheckCoincidenceAndThrow(vertices);
            this.Vertices = vertices;
        }

        /// <summary>
        /// Construct a polyline from a collection of vertices.
        /// </summary>
        /// <param name="vertices">A CCW wound set of vertices.</param>
        public Polyline(Vector3[] vertices)
        {
            CheckCoincidenceAndThrow(vertices);
            this.Vertices = vertices;
        }

        private void CheckCoincidenceAndThrow(IList<Vector3> vertices)
        {
            for (var i = 0; i < vertices.Count; i++)
            {
                for (var j = 0; j < vertices.Count; j++)
                {
                    if (i == j)
                    {
                        continue;
                    }
                    if (vertices[i].IsAlmostEqualTo(vertices[j]))
                    {
                        throw new ArgumentException($"The polyline could not be created. Two vertices were almost equal: {i} {vertices[i]} {j} {vertices[j]}.");
                    }
                }
            }
        }

        /// <summary>
        /// Reverse the direction of a polyline.
        /// </summary>
        /// <returns>Returns a new polyline with opposite winding.</returns>
        public Polyline Reversed()
        {
            var revVerts = new List<Vector3>(this.Vertices);
            revVerts.Reverse();
            return new Polyline(revVerts);
        }

        /// <summary>
        /// Get a string representation of this polyline.
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return string.Join<Vector3>(",", this.Vertices);
        }

        /// <summary>
        /// Get a collection a lines representing each segment of this polyline.
        /// </summary>
        /// <returns>A collection of Lines.</returns>
        public virtual Line[] Segments()
        {
            var result = new Line[Vertices.Count - 1];
            for (var i = 0; i < Vertices.Count - 1; i++)
            {
                var a = Vertices[i];
                var b = Vertices[i + 1];
                result[i] = new Line(a, b);
            }
            return result;
        }

        /// <summary>
        /// Get a point on the polygon at parameter u.
        /// </summary>
        /// <param name="u">A value between 0.0 and 1.0.</param>
        /// <returns>Returns a Vector3 indicating a point along the Polygon length from its start vertex.</returns>

        public override Vector3 PointAt(double u)
        {
            var segmentIndex = 0;
            var p = PointAtInternal(u, out segmentIndex);
            return p;
        }

        /// <summary>
        /// Get a point on the polygon at parameter u.
        /// </summary>
        /// <param name="u">A value between 0.0 and 1.0.</param>
        /// <param name="segmentIndex">The index of the segment containing parameter u.</param>
        /// <returns>Returns a Vector3 indicating a point along the Polygon length from its start vertex.</returns>
        private Vector3 PointAtInternal(double u, out int segmentIndex)
        {
            if (u < 0.0 || u > 1.0)
            {
                throw new Exception($"The value of u ({u}) must be between 0.0 and 1.0.");
            }

            var d = this.Length() * u;
            var totalLength = 0.0;
            for (var i = 0; i < this.Vertices.Count - 1; i++)
            {
                var a = this.Vertices[i];
                var b = this.Vertices[i + 1];
                var currLength = a.DistanceTo(b);
                var currVec = (b - a);
                if (totalLength <= d && totalLength + currLength >= d)
                {
                    segmentIndex = i;
                    return a + currVec * ((d - totalLength) / currLength);
                }
                totalLength += currLength;
            }
            segmentIndex = this.Vertices.Count - 1;
            return this.End;
        }

        /// <summary>
        /// Get the Transform at the specified parameter along the Polygon.
        /// </summary>
        /// <param name="u">The parameter on the Polygon between 0.0 and 1.0.</param>
        /// <returns>A Transform with its Z axis aligned trangent to the Polygon.</returns>
        public override Transform TransformAt(double u)
        {
            if (u < 0.0 || u > 1.0)
            {
                throw new ArgumentOutOfRangeException($"The provided value for u ({u}) must be between 0.0 and 1.0.");
            }

            var segmentIndex = 0;
            var o = PointAtInternal(u, out segmentIndex);
            var up = Vector3.ZAxis;
            Vector3 x = null;

            // Check if the provided parameter is equal
            // to one of the vertices.
            Vector3 a = null;
            foreach(var v in this.Vertices)
            {
                if (v.Equals(o))
                {
                    a = v;
                }
            }

            if (a != null)
            {
                var idx = this.Vertices.IndexOf(a);

                if (idx == 0 || idx == this.Vertices.Count - 1)
                {
                    return CreateOthogonalTransform(idx, a);
                }
                else
                {
                    return CreateMiterTransform(idx, a);
                }
            }
            else
            {
                var d = this.Length() * u;
                var totalLength = 0.0;
                var segments = Segments();
                for (var i = 0; i < segments.Length; i++)
                {
                    var s = segments[i];
                    var currLength = s.Length();
                    if (totalLength <= d && totalLength + currLength >= d)
                    {
                        o = s.PointAt((d - totalLength) / currLength);
                        x = s.Direction().Cross(up);
                        break;
                    }
                    totalLength += currLength;
                }
            }
            return new Transform(o, x, up);
        }

        /// <summary>
        /// Get the transforms used to transform a Profile extruded along this Polyline.
        /// </summary>
        /// <param name="startSetback"></param>
        /// <param name="endSetback"></param>
        /// <returns></returns>
        public override Transform[] Frames(double startSetback, double endSetback)
        {
            return FramesInternal(startSetback, endSetback, false);
        }

        /// <summary>
        /// Get the bounding box for this curve.
        /// </summary>
        public override BBox3 Bounds()
        {
            return new BBox3(this.Vertices);
        }

        /// <summary>
        /// Compute the Plane defined by the first three vertices of the Polygon.
        /// </summary>
        /// <returns>A Plane.</returns>
        public Plane Plane()
        {
            return new Plane(this.Vertices[0], this.Vertices[1], this.Vertices[2]);
        }

        internal Transform[] FramesInternal(double startSetback, double endSetback, bool closed = false)
        {
            // Create an array of transforms with the same
            // number of items as the vertices.
            var result = new Transform[this.Vertices.Count];
            for (var i = 0; i < result.Length; i++)
            {
                var a = this.Vertices[i];
                if (closed)
                {
                    result[i] = CreateMiterTransform(i, a);
                }
                else
                {
                    result[i] = CreateOthogonalTransform(i, a);
                }
            }
            return result;
        }

        private Transform CreateMiterTransform(int i, Vector3 a)
        {
            // Create transforms at 'miter' planes.
            var b = i == 0 ? this.Vertices[this.Vertices.Count - 1] : this.Vertices[i - 1];
            var c = i == this.Vertices.Count - 1 ? this.Vertices[0] : this.Vertices[i + 1];
            var x = (b - a).Unit().Average((c - a).Unit()).Negate();
            var up = x.IsAlmostEqualTo(Vector3.ZAxis) ? Vector3.YAxis : Vector3.ZAxis;

            return new Transform(this.Vertices[i], x, up.Cross(x));
        }

        private Transform CreateOthogonalTransform(int i, Vector3 a)
        {
            Vector3 b, x, c;

            if (i == 0)
            {
                b = this.Vertices[i + 1];
                return new Transform(a, (b-a).Unit());
            }
            else if (i == this.Vertices.Count - 1)
            {
                b = this.Vertices[i - 1];
                return new Transform(a, (a-b).Unit());
            }
            else
            {
                b = this.Vertices[i - 1];
                c = this.Vertices[i + 1];
                var v1 = (b - a).Unit();
                var v2 = (c - a).Unit();
                x = v1.Average(v2).Negate();
                var up = v2.Cross(v1);
                return new Transform(this.Vertices[i], x, up.Cross(x));
            }
        }
    }
}
