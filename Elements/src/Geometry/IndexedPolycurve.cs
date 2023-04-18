using Elements.Validators;
using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;

namespace Elements.Geometry
{
    /// <summary>
    /// A curves made up of a collection of line and arc segments.
    /// Parameterization of the curve is 0->length.
    /// </summary>
    public class IndexedPolycurve : BoundedCurve, IEnumerable<BoundedCurve>, IEquatable<IndexedPolycurve>
    {
        /// <summary>
        /// A bounding box created once during the polyline's construction.
        /// This will not be updated when a polyline's vertices are changed.
        /// </summary>
        internal BBox3 _bounds;

        /// <summary>
        /// The start of the polyline.
        /// </summary>
        [JsonIgnore]
        public override Vector3 Start
        {
            get { return this.Vertices[0]; }
        }

        /// <summary>
        /// The end of the polyline.
        /// </summary>
        [JsonIgnore]
        public override Vector3 End
        {
            get { return this.Vertices[this.Vertices.Count - 1]; }
        }

        /// <summary>
        /// An optional collection of collections of indices of polycurve segments.
        /// Line segments are represented with two indices.
        /// Arc segments are represented with three indices.
        /// </summary>
        public IList<IList<int>> CurveIndices { get; set; }

        /// <summary>The vertices of the polygon.</summary>
        [JsonProperty("Vertices", Required = Required.Always)]
        [System.ComponentModel.DataAnnotations.Required]
        [System.ComponentModel.DataAnnotations.MinLength(2)]
        public IList<Vector3> Vertices { get; set; } = new List<Vector3>();

        /// <summary>
        /// Create an indexed polycurve.
        /// </summary>
        /// <param name="vertices">A collection of vertices.</param>
        /// <param name="curveIndices">A collection of collections of indices.</param>
        /// <param name="disableValidation"></param>
        [JsonConstructor]
        public IndexedPolycurve(IList<Vector3> vertices,
                                IList<IList<int>> curveIndices = null,
                                bool disableValidation = false)
        {
            if (!Validator.DisableValidationOnConstruction && !disableValidation)
            {
                ValidateVertices();
            }

            if (curveIndices != null)
            {
                var count = 0;
                foreach (var curveIndexSet in curveIndices)
                {
                    if (curveIndexSet.Count < 2 || curveIndexSet.Count > 3)
                    {
                        throw new ArgumentException("curveIndices", $"Curve indices must reference 2 or 3 vertices. The curve index at {count} references {curveIndexSet.Count} vertices.");
                    }
                    count++;
                }
            }

            this.Vertices = vertices;
            this.CurveIndices = curveIndices;
            this.Domain = new Domain1d(0, this.Length());
            _bounds = new BBox3(Vertices);
        }

        /// <summary>
        /// Get the bounding box for this curve.
        /// </summary>
        public override BBox3 Bounds()
        {
            return new BBox3(this.Vertices);
        }

        public override double[] GetSubdivisionParameters(double startSetbackDistance = 0, double endSetbackDistance = 0)
        {
            throw new System.NotImplementedException();
        }

        /// <summary>
        /// Calculate the length of the indexed polycurve.
        /// </summary>
        public override double Length()
        {
            var length = 0.0;
            foreach (var curve in this)
            {
                length += curve.Length();
            }
            return length;
        }

        public override double ParameterAtDistanceFromParameter(double distance, double start)
        {
            throw new System.NotImplementedException();
        }

        /// <summary>
        /// Get a point on the polycurve at parameter u.
        /// </summary>
        /// <param name="u">A value between 0.0 and 1.0.</param>
        /// <returns>Returns a Vector3 indicating a point along the Polygon length from its start vertex.</returns>
        public override Vector3 PointAt(double u)
        {
            return PointAtInternal(u, out _);
        }

        /// <summary>
        /// Get a point on the polycurve at parameter u.
        /// </summary>
        /// <param name="u">A value between 0.0 and length.</param>
        /// <param name="curveIndex">The index of the segment containing parameter u.</param>
        /// <returns>Returns a Vector3 indicating a point along the Polygon length from its start vertex.</returns>
        protected virtual Vector3 PointAtInternal(double u, out int curveIndex)
        {
            throw new NotImplementedException();
        }

        public override Transform TransformAt(double u)
        {
            throw new System.NotImplementedException();
        }

        public override Curve Transformed(Transform transform)
        {
            throw new System.NotImplementedException();
        }

        /// <summary>
        /// Does this polyline equal the provided polyline?
        /// </summary>
        /// <param name="other"></param>
        /// <returns>True if the two curves are equal, otherwise false.</returns>
        public bool Equals(IndexedPolycurve other)
        {
            if (this.Vertices.Count != other.Vertices.Count)
            {
                return false;
            }
            for (var i = 0; i < Vertices.Count; i++)
            {
                if (!this.Vertices[i].Equals(other.Vertices[i]))
                {
                    return false;
                }
            }

            if (this.CurveIndices != null)
            {
                if (this.CurveIndices.Count != other.CurveIndices.Count)
                {
                    return false;
                }
                for (var i = 0; i < this.CurveIndices.Count; i++)
                {
                    if (!this.CurveIndices[i].Equals(other.CurveIndices[i]))
                    {
                        return false;
                    }
                }
            }
            return true;
        }

        /// <summary>
        /// Get a string representation of this polyline.
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return string.Join<Vector3>(",", this.Vertices);
        }

        // TODO: Investigate converting Polyline to IEnumerable<(Vector3, Vector3)>
        virtual internal IEnumerable<(Vector3 from, Vector3 to)> Edges(Transform transform = null)
        {
            for (var i = 0; i < Vertices.Count - 1; i++)
            {
                var from = Vertices[i];
                var to = Vertices[i + 1];
                yield return transform != null ? (transform.OfPoint(from), transform.OfPoint(to)) : (from, to);
            }
        }

        /// <summary>
        /// Clean up any duplicate vertices, and warn about any vertices that are too close to each other.
        /// </summary>
        protected virtual void ValidateVertices()
        {
            Vertices = Vector3.RemoveSequentialDuplicates(Vertices);
            CheckSegmentLengthAndThrow(Edges());
        }

        /// <summary>
        /// Check for coincident vertices in the supplied vertex collection.
        /// </summary>
        /// <param name="vertices"></param>
        protected void CheckCoincidenceAndThrow(IList<Vector3> vertices)
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
        /// Check if any of the polygon segments have zero length.
        /// </summary>
        internal static void CheckSegmentLengthAndThrow(IEnumerable<(Vector3 from, Vector3 to)> segments)
        {
            foreach (var (from, to) in segments)
            {
                if (from.DistanceTo(to) == 0)
                {
                    throw new ArgumentException("A segment of the polyline has zero length.");
                }
            }
        }

        /// <summary>
        /// Check for self-intersection in the supplied line segment collection.
        /// </summary>
        /// <param name="t">The transform representing the plane of the polygon.</param>
        /// <param name="segments"></param>
        internal static void CheckSelfIntersectionAndThrow(Transform t, IEnumerable<(Vector3 from, Vector3 to)> segments)
        {
            var segmentsT = new List<(Vector3 from, Vector3 to)>();
            foreach (var (from, to) in segments)
            {
                segmentsT.Add((t.OfPoint(from), t.OfPoint(to)));
            }

            for (var i = 0; i < segmentsT.Count; i++)
            {
                for (var j = 0; j < segmentsT.Count; j++)
                {
                    if (i == j)
                    {
                        // Don't check against itself.
                        continue;
                    }
                    var s1 = segmentsT[i];
                    var s2 = segmentsT[j];

                    if (Line.Intersects2d(s1.from, s1.to, s2.from, s2.to))
                    {
                        throw new ArgumentException($"The polyline could not be created. Segments {i} and {j} intersect.");
                    }
                }
            }
        }

        /// <summary>
        /// Get the enumerator for this indexed polycurve.
        /// </summary>
        /// <returns>An enumerator of bounded curves.</returns>
        public IEnumerator<BoundedCurve> GetEnumerator()
        {
            foreach (var curveIndexSet in this.CurveIndices)
            {
                // Construct a curve
                if (curveIndexSet.Count == 2)
                {
                    yield return new Line(Vertices[curveIndexSet[0]], Vertices[curveIndexSet[1]]);
                }
                else if (curveIndexSet.Count == 3)
                {
                    yield return Arc.ByThreePoints(this.Vertices[curveIndexSet[0]], this.Vertices[curveIndexSet[1]], this.Vertices[curveIndexSet[2]]);
                }
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }

    }
}