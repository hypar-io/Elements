using Elements.Geometry.Interfaces;
using Elements.Validators;
using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Elements.Geometry
{
    /// <summary>
    /// A curve composed of a collection of line and arc segments.
    /// Parameterization of the curve is 0->n where n is the number of curves.
    /// </summary>
    /// [!code-csharp[Main](../../Elements/test/IndexedPolycurveTests.cs?name=example)]
    [JsonObject]
    public class IndexedPolycurve : BoundedCurve, IEnumerable<BoundedCurve>, IEquatable<IndexedPolycurve>
    {
        /// <summary>
        /// A collection of curves created from the vertices
        /// and the curve indices. If this collection is
        /// null, the curve is to be considered a collection
        /// of linear edges between vertices.
        /// </summary>
        private List<BoundedCurve> _curves;

        /// <summary>
        /// A bounding box created once during the polycurve's construction.
        /// This will not be updated when a polycurve's vertices are changed.
        /// </summary>
        internal BBox3 _bounds;

        /// <summary>
        /// The domain of the curve.
        /// </summary>
        [JsonIgnore]
        public override Domain1d Domain => new Domain1d(0, CurveIndices.Count);

        /// <summary>
        /// The start of the polycurve.
        /// </summary>
        [JsonIgnore]
        public override Vector3 Start
        {
            get { return Vertices[0]; }
        }

        /// <summary>
        /// The end of the polycurve.
        /// </summary>
        [JsonIgnore]
        public override Vector3 End
        {
            get { return Vertices[Vertices.Count - 1]; }
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
        public IndexedPolycurve()
        {

        }

        /// <summary>
        /// Create an indexed polycurve.
        /// </summary>
        /// <param name="vertices">A collection of vertices.</param>
        /// <param name="curveIndices">A collection of collections of indices.</param>
        [JsonConstructor]
        public IndexedPolycurve(IList<Vector3> vertices,
                                IList<IList<int>> curveIndices)
        {
            Vertices = vertices;

            if (!Validator.DisableValidationOnConstruction)
            {
                ValidateVertices();
            }

            CurveIndices = curveIndices;

            _bounds = Bounds();

            var count = 0;
            foreach (var curveIndexSet in CurveIndices)
            {
                if (curveIndexSet.Count < 2 || curveIndexSet.Count > 3)
                {
                    throw new ArgumentException("curveIndices", $"Curve indices must reference 2 or 3 vertices. The curve index at {count} references {curveIndexSet.Count} vertices.");
                }
                count++;
            }

            _curves = CreateCurves(CurveIndices, Vertices);
        }

        /// <summary>
        /// Create an indexed polycurve.
        /// </summary>
        /// <param name="vertices">A collection of vertices.</param>
        /// <param name="curveIndices">A collection of collections of indices.</param>
        /// <param name="transform">An optional transform to apply to each vertex.</param>
        /// <param name="disableValidation"></param>
        public IndexedPolycurve(IList<Vector3> vertices,
                                IList<IList<int>> curveIndices = null,
                                Transform transform = null,
                                bool disableValidation = false)
        {
            if (transform != null)
            {
                foreach (var v in vertices)
                {
                    Vertices.Add(transform.OfPoint(v));
                }
            }
            else
            {
                Vertices = vertices;
            }

            if (!Validator.DisableValidationOnConstruction && !disableValidation)
            {
                ValidateVertices();
            }

            if (curveIndices == null)
            {
                CurveIndices = CreateCurveIndices(Vertices);
            }
            else
            {
                CurveIndices = curveIndices;
            }

            _bounds = Bounds();

            var count = 0;
            foreach (var curveIndexSet in CurveIndices)
            {
                if (curveIndexSet.Count < 2 || curveIndexSet.Count > 3)
                {
                    throw new ArgumentException("curveIndices", $"Curve indices must reference 2 or 3 vertices. The curve index at {count} references {curveIndexSet.Count} vertices.");
                }
                count++;
            }

            _curves = CreateCurves(CurveIndices, Vertices);
        }

        /// <summary>
        /// Create an indexed polycurve.
        /// </summary>
        /// <param name="curves">A collection of bounded curves.</param>
        /// <param name="transform">An optional transform to apply to the vertices.</param>
        public IndexedPolycurve(List<BoundedCurve> curves, Transform transform = null)
        {
            if (transform != null)
            {
                var transformedCurves = new List<BoundedCurve>();
                {
                    foreach (var curve in curves)
                    {
                        transformedCurves.Add((BoundedCurve)curve.Transformed(transform));
                    }
                }
                _curves = transformedCurves;
            }
            else
            {
                _curves = curves;
            }

            var result = CreateVerticesAndCurveIndices(curves, transform);
            CurveIndices = result.Item1;
            Vertices = result.Item2;

            _bounds = Bounds();
        }

        private static (List<IList<int>>, List<Vector3>) CreateVerticesAndCurveIndices(IEnumerable<BoundedCurve> curves, Transform transform = null)
        {
            var index = 0;
            var curveIndices = new List<IList<int>>();
            var vertices = new List<Vector3>();

            Vector3 last = default;
            foreach (var curve in curves)
            {
                if (curve is Line)
                {
                    vertices.Add(curve.Start);
                    last = curve.End;
                    curveIndices.Add(new[] { index, index + 1 });
                    index += 2;
                }
                else if (curve is Arc)
                {
                    vertices.Add(curve.Start);
                    vertices.Add(curve.Mid());
                    last = curve.End;
                    curveIndices.Add(new[] { index, index + 1, index + 2 });
                    index += 3;
                }
                else
                {
                    throw new ArgumentException("curves", $"A curve of type {curve.GetType()} cannot be used to create a polycurve.");
                }
            }
            vertices.Add(last);


            if (transform != null)
            {
                var transformedVertices = new List<Vector3>();
                foreach (var v in vertices)
                {
                    transformedVertices.Add(transform.OfPoint(v));
                }
                vertices = transformedVertices;
            }
            return (curveIndices, vertices);
        }

        internal virtual List<IList<int>> CreateCurveIndices(IList<Vector3> vertices)
        {
            var curveIndices = new List<IList<int>>();
            for (var i = 0; i < vertices.Count - 1; i++)
            {
                curveIndices.Add(new[] { i, i + 1 });
            }
            return curveIndices;
        }

        private static List<BoundedCurve> CreateCurves(IList<IList<int>> curveIndices, IList<Vector3> vertices)
        {
            var curves = new List<BoundedCurve>();

            foreach (var curveIndexSet in curveIndices)
            {
                // Construct a curve
                if (curveIndexSet.Count == 2)
                {
                    curves.Add(new Line(vertices[curveIndexSet[0]], vertices[curveIndexSet[1]]));
                }
                else if (curveIndexSet.Count == 3)
                {
                    curves.Add(Arc.ByThreePoints(vertices[curveIndexSet[0]], vertices[curveIndexSet[1]], vertices[curveIndexSet[2]]));
                }
            }

            return curves;
        }

        /// <summary>
        /// Get the bounding box for this curve.
        /// </summary>
        public override BBox3 Bounds()
        {
            return new BBox3(Vertices);
        }

        /// <inheritdoc/>
        public override double[] GetSubdivisionParameters(double startSetbackDistance = 0,
                                                          double endSetbackDistance = 0)
        {
            var startParam = ParameterAtDistanceFromParameter(startSetbackDistance, Domain.Min);
            var endParam = ParameterAtDistanceFromParameter(Length() - endSetbackDistance, Domain.Min);

            var startCurveIndex = (int)Math.Floor(startParam);
            var endCurveIndex = Math.Min(_curves.Count - 1, (int)Math.Floor(endParam));

            var parameters = new List<double>() { startParam };
            for (var i = startCurveIndex; i < endCurveIndex + 1; i++)
            {
                var curve = _curves[i];
                var localParameters = curve.GetSubdivisionParameters();
                var localDomain = new Domain1d(i, i + 1);
                for (var j = 0; j < localParameters.Length; j++)
                {
                    var localParameter = localParameters[j];
                    // The curve domain may be 0->2Pi. We need to map that 
                    // into a domain that is a subsection of the larger domain.
                    var remapped = localParameter.MapBetweenDomains(curve.Domain, localDomain);

                    // Filter parameters outside of setbacks and de-duplicate the end vertices.
                    if (remapped < startParam || remapped > endParam ||
                        parameters[parameters.Count - 1].ApproximatelyEquals(remapped))
                    {
                        continue;
                    }
                    parameters.Add(remapped);
                }
            }

            if (!parameters[parameters.Count - 1].ApproximatelyEquals(endParam))
            {
                parameters.Add(endParam);
            }

            return parameters.ToArray();
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

        /// <summary>
        /// Calculate the length of the polycurve between two parameters.
        /// </summary>
        public override double ArcLength(double start, double end)
        {
            if (!Domain.Includes(start, true))
            {
                throw new Exception($"The parameter {start} must be between {Domain.Min} and {Domain.Max}.");
            }

            if (!Domain.Includes(end, true))
            {
                throw new Exception($"The parameter {end} must be between {Domain.Min} and {Domain.Max}.");
            }

            if (start.ApproximatelyEquals(end))
            {
                throw new ArgumentOutOfRangeException($"The start parameter {start} and end parameter {end} are almost equal.");
            }

            var curveIndex = (int)Math.Floor(start);
            var length = 0.0;
            for (var i = curveIndex; i < Domain.Max; i++)
            {
                var curve = _curves[i];
                var curveStartParameter = curve.Domain.Min;
                if (i == curveIndex)
                {
                    curveStartParameter = (start - curveIndex).MapToDomain(curve.Domain);
                }

                if (end <= i + 1)
                {
                    var curveEndParameter = (end - i).MapToDomain(curve.Domain);
                    return length + curve.ArcLength(curveStartParameter, curveEndParameter);
                }
                length += curve.ArcLength(curveStartParameter, curve.Domain.Max);
            }

            return 0.0;
        }

        /// <summary>
        /// Get the parameter at a distance from the start parameter along the curve.
        /// </summary>
        /// <param name="distance">The distance from the start parameter.</param>
        /// <param name="start">The parameter from which to measure the distance between domain.min and domain.max.</param>
        public override double ParameterAtDistanceFromParameter(double distance, double start)
        {
            if (!Domain.Includes(start, true))
            {
                throw new Exception($"The parameter {start} is not on the trimmed portion of the basis curve. The parameter must be between {Domain.Min} and {Domain.Max}.");
            }

            if (distance.ApproximatelyEquals(0))
            {
                return start;
            }

            var startCurveIndex = (int)Math.Floor(start);
            var trackingLength = 0.0;

            for (var i = startCurveIndex; i < _curves.Count; i++)
            {
                var curve = _curves[i];

                // The start parameter will either be the partial
                // parameter, because this is the curve on which 
                // the start parameter is located, or it will be 0.0.
                var normalizedStartParameter = i == startCurveIndex ? start - i : 0.0;
                var localStartParameter = normalizedStartParameter.MapToDomain(curve.Domain);
                var distanceToEndOfCurve = curve.ArcLength(localStartParameter, curve.Domain.Max);
                if (trackingLength + distanceToEndOfCurve >= distance)
                {
                    // Find the parameter at exactly the distance.
                    return i + curve.ParameterAtDistanceFromParameter(distance - trackingLength, localStartParameter).MapFromDomain(curve.Domain);
                }

                trackingLength += distanceToEndOfCurve;
            }
            return 1.0;
        }

        /// <summary>
        /// Get a point on the polycurve at parameter u.
        /// </summary>
        /// <param name="u">A value between domain.min and domain.max.</param>
        /// <returns>Returns a Vector3 indicating a point along the Polygon length from its start vertex.</returns>
        public override Vector3 PointAt(double u)
        {
            if (!Domain.Includes(u, true))
            {
                throw new Exception($"The parameter {u} must be between {Domain.Min} and {Domain.Max}.");
            }

            return PointAtInternal(u, out _);
        }

        /// <summary>
        /// Get a point on the polycurve at parameter u.
        /// </summary>
        /// <param name="u">A value between domain.min and domain.max.</param>
        /// <param name="curveIndex">The index of the segment containing parameter u.</param>
        /// <returns>Returns a Vector3 indicating a point along the Polygon length from its start vertex.</returns>
        protected virtual Vector3 PointAtInternal(double u, out int curveIndex)
        {
            // If we receive a value like -1e-15,
            // we'll try to find the -1 segment. Avoid
            // this by simply returning the start.
            if (u.ApproximatelyEquals(0.0))
            {
                curveIndex = 0;
                return _curves[0].PointAtNormalized(0);
            }
            else
            {
                curveIndex = (int)Math.Floor(u);
            }

            if (curveIndex == _curves.Count)
            {
                return End;
            }

            if (curveIndex == Vertices.Count)
            {
                // The polygon case.
                return Start;
            }

            var t = u - curveIndex;
            return _curves[curveIndex].PointAtNormalized(t);
        }

        /// <summary>
        /// Get the transform at the specified parameter along the polycurve.
        /// </summary>
        /// <param name="u">The parameter on the Polygon between domain.min and domain.max.</param>
        /// <returns>A transform with its Z axis aligned trangent to the polycurve.</returns>
        public override Transform TransformAt(double u)
        {
            if (u == Domain.Max)
            {
                // You're at the end!
                return _curves[_curves.Count - 1].TransformAtNormalized(1);
            }

            if (!Domain.Includes(u, true))
            {
                throw new Exception($"The parameter {u} must be between {Domain.Min} and {Domain.Max}.");
            }

            var curveIndex = (int)Math.Floor(u);
            var t = u - curveIndex;
            return _curves[curveIndex].TransformAtNormalized(t);
        }

        /// <inheritdoc/>
        public override Curve Transformed(Transform transform)
        {
            return new IndexedPolycurve(_curves, transform);
        }

        /// <summary>
        /// Create new polycurve transformed by transform.
        /// </summary>
        /// <param name="transform">The transform to apply.</param>
        public IndexedPolycurve TransformedPolycurve(Transform transform)
        {
            return new IndexedPolycurve(_curves, transform);
        }

        /// <summary>
        /// Does this indexed polycurve equal the provided indexed polycurve?
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
                    for (var j = 0; j < this.CurveIndices[i].Count; j++)
                    {
                        if (this.CurveIndices[i][j] != other.CurveIndices[i][j])
                        {
                            return false;
                        }
                    }
                }
            }
            return true;
        }

        /// <inheritdoc/>
        public override bool Intersects(ICurve curve, out List<Vector3> results)
        {
            results = new List<Vector3>();
            foreach (var segment in _curves)
            {
                if (segment.Intersects(curve, out var intersections))
                {
                    foreach (var item in intersections)
                    {
                        if (!results.Any() || 
                            !results.First().IsAlmostEqualTo(item) && !results.Last().IsAlmostEqualTo(item))
                        {
                            results.Add(item);
                        }
                    }
                }
            }
            return results.Any();
        }

        /// <summary>
        /// Get a string representation of this polycurve.
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return string.Join<Vector3>(",", this.Vertices);
        }

        /// <summary>
        /// Create a polygon from this polycurve.
        /// </summary>
        public Polygon ToPolygon()
        {
            return new Polygon(RenderVertices());
        }

        // TODO: Investigate converting polycurve to IEnumerable<(Vector3, Vector3)>
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
                        throw new ArgumentException($"The polycurve could not be created. Two vertices were almost equal: {i} {vertices[i]} {j} {vertices[j]}.");
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
                    throw new ArgumentException("A segment of the polycurve has zero length.");
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
                        throw new ArgumentException($"The polycurve could not be created. Segments {i} and {j} intersect.");
                    }
                }
            }
        }

        /// <summary>
        /// Create a polyline through curves of IndexedPolycurve, interpolating any curves that are not lines.
        /// </summary>
        /// <returns>A polyline.</returns>
        public override Polyline ToPolyline()
        {
            List<Vector3> vertices = new List<Vector3>();
            foreach (var curve in _curves)
            {
                if (curve is Line)
                {
                    vertices.Add(curve.Start);
                }
                else
                {
                    var pl = curve.ToPolyline();
                    vertices.AddRange(pl.Vertices.Take(pl.Vertices.Count - 1));
                }
            }
            vertices.Add(End);
            return new Polyline(vertices);
        }

        /// <summary>
        /// Create a polyline through a set of points along the curve.
        /// End points of curves are added first and then non straight curves are divided uniformly.
        /// If number of divisions is less than number of curves - points are uniformly distributed
        /// though whole domain, deviating heavily from original shape.
        /// </summary>
        /// <param name="divisions">The number of divisions of the curve. 
        /// This can lead to highly distorted result.</param>
        /// <returns>A polyline.</returns>
        public override Polyline ToPolyline(int divisions)
        {
            //
            if (divisions < _curves.Count)
            {
                return base.ToPolyline(divisions);
            }

            double extraSplits = divisions - _curves.Count;
            List<Vector3> vertices = new List<Vector3>(divisions + 1) { Start };
            var numArcs = _curves.Count(c => !(c is Line));
            if (numArcs > 0)
            {
                // If polycurve consists of lines and curves - excess divisions are uniformly distributed
                // among curves as dividing the lines wont add any new details in Polyline.
                foreach (var curve in _curves)
                {
                    if (!(curve is Line))
                    {
                        var splitsPerArc = Math.Ceiling(1.0d * extraSplits / numArcs);
                        var splits = Math.Min(splitsPerArc, extraSplits);
                        var step = curve.Domain.Length / (splitsPerArc + 1);
                        for (int i = 1; i <= splitsPerArc; i++)
                        {
                            var t = curve.Domain.Min + i * step;
                            vertices.Add(curve.PointAt(t));
                        }
                        extraSplits -= splitsPerArc;
                        numArcs--;
                    }
                    vertices.Add(curve.End);
                }
            }
            else
            {
                // If polycurve consists only of lines - divisions are distributed uniformly among them.
                var linesLeft = _curves.Count;
                foreach (var curve in _curves)
                {
                    var splitsPerLine = Math.Ceiling(1.0d * extraSplits / linesLeft);
                    var splits = Math.Min(splitsPerLine, extraSplits);
                    var step = curve.Domain.Length / (splitsPerLine + 1);
                    for (int i = 1; i <= splitsPerLine; i++)
                    {
                        var t = curve.Domain.Min + i * step;
                        vertices.Add(curve.PointAt(t));
                    }
                    vertices.Add(curve.End);
                    extraSplits -= splitsPerLine;
                    linesLeft--;
                }
            }
            vertices.Add(End);
            return new Polyline(vertices);
        }

        /// <summary>
        /// Get the enumerator for this indexed polycurve.
        /// </summary>
        /// <returns>An enumerator of bounded curves.</returns>
        public IEnumerator<BoundedCurve> GetEnumerator()
        {
            return _curves.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}