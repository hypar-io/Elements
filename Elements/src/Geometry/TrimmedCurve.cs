using Elements.Geometry.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;

namespace Elements.Geometry
{
    /// <summary>
    /// A trimmed curve.
    /// </summary>
    public abstract class TrimmedCurve<TBasis> : BoundedCurve, ITrimmedCurve<TBasis> where TBasis : ICurve
    {
        /// <summary>
        /// The basis curve for this bounded curve.
        /// </summary>
        [JsonIgnore]
        public TBasis BasisCurve { get; protected set; }

        /// <summary>
        /// Check if point is on the domain of the curve.
        /// </summary>
        /// <param name="point">Point to check.</param>
        /// <returns>True if point is on trimmed domain of the curve.</returns>
        public abstract bool PointOnDomain(Vector3 point);

        /// <inheritdoc/>
        public override bool Intersects(ICurve curve, out List<Vector3> results)
        {
            switch (curve)
            {
                case Line line:
                    return Intersects(line, out results);
                case Arc arc:
                    return Intersects(arc, out results);
                case EllipticalArc ellipticArc:
                    return Intersects(ellipticArc, out results);
                case InfiniteLine line:
                    return Intersects(line, out results);
                case Circle circle:
                    return Intersects(circle, out results);
                case Ellipse ellipse:
                    return Intersects(ellipse, out results);
                case IndexedPolycurve polycurve:
                    return polycurve.Intersects(this, out results);
                case Bezier bezier:
                    return Intersects(bezier, out results);
                default:
                    throw new NotImplementedException();
            }
        }

        /// <summary>
        /// Does this trimmed curve intersects with an infinite line?
        /// If they coincides, they are not considered as intersecting,
        /// unless their domain only touches each other at end points.
        /// </summary>
        /// <param name="line">Infinite line to intersect.</param>
        /// <param name="results">List containing up to twp intersection points.</param>
        /// <returns>True if intersection exists, otherwise false.</returns>
        public bool Intersects(InfiniteLine line, out List<Vector3> results)
        {
            results = new List<Vector3>();
            if (BasisCurve.Intersects(line, out var candidates))
            {
                foreach (var item in candidates)
                {
                    if (this.PointOnDomain(item))
                    {
                        results.Add(item);
                    }
                }
            }
            return results.Any();
        }

        /// <summary>
        /// Does this trimmed curve intersects with a circle?
        /// If they coincides, they are not considered as intersecting,
        /// unless their domain only touches each other at end points.
        /// </summary>
        /// <param name="circle">Circle to intersect.</param>
        /// <param name="results">List containing up to four intersection points.</param>
        /// <returns>True if any intersections exist, otherwise false.</returns>
        public bool Intersects(Circle circle, out List<Vector3> results)
        {
            results = new List<Vector3>();
            if (BasisCurve.Intersects(circle, out var candidates))
            {
                foreach (var item in candidates)
                {
                    if (this.PointOnDomain(item))
                    {
                        results.Add(item);
                    }
                }
            }
            return results.Any();
        }

        /// <summary>
        /// Does this trimmed curve intersects with an ellipse?
        /// If they coincides, they are not considered as intersecting,
        /// unless their domain only touches each other at end points.
        /// </summary>
        /// <param name="ellipse">Ellipse to intersect.</param>
        /// <param name="results">List containing up to four intersection points.</param>
        /// <returns>True if any intersections exist, otherwise false.</returns>
        public bool Intersects(Ellipse ellipse, out List<Vector3> results)
        {
            results = new List<Vector3>();
            if (BasisCurve.Intersects(ellipse, out var candidates))
            {
                foreach (var item in candidates)
                {
                    if (PointOnDomain(item))
                    {
                        results.Add(item);
                    }
                }
            }
            return results.Any();
        }

        /// <summary>
        /// Does this trimmed curve intersects with other trimmed curve?
        /// If they coincides, they are not considered as intersecting,
        /// unless their domain only touches each other at end points.
        /// </summary>
        /// <param name="curve">Trimmed curve to intersect.</param>
        /// <param name="results">List containing intersection points.</param>
        /// <returns>True if any intersections exist, otherwise false.</returns>
        public bool Intersects<T>(TrimmedCurve<T> curve, out List<Vector3> results) where T : ICurve
        {
            results = new List<Vector3>();
            if (BasisCurve.Intersects(curve.BasisCurve, out var candidates))
            {
                foreach (var item in candidates)
                {
                    if (this.PointOnDomain(item) && curve.PointOnDomain(item))
                    {
                        results.Add(item);
                    }
                }
            }
            else
            {
                // Overlapping curves are not considered intersecting.
                // However, if their domains are not overlap,
                // end points that belongs to both curves are recorded as intersections.
                bool startOnStart = Start.IsAlmostEqualTo(curve.Start);
                bool startOnEnd = Start.IsAlmostEqualTo(curve.End);
                bool endOnStart = End.IsAlmostEqualTo(curve.Start);
                bool endOnEnd = End.IsAlmostEqualTo(curve.End);
                if (startOnStart || startOnEnd)
                {
                    if (endOnStart || endOnEnd)
                    {
                        if (!curve.PointOnDomain(Mid()))
                        {
                            results.Add(Start);
                            results.Add(End);
                        }
                    }
                    else
                    {
                        var other = startOnStart ? curve.End : curve.Start;
                        if (!curve.PointOnDomain(End) && !PointOnDomain(other))
                        {
                            results.Add(Start);
                        }
                    }
                }
                else if (endOnStart || endOnEnd)
                {
                    var other = endOnStart ? curve.End : curve.Start;
                    if (!curve.PointOnDomain(Start) && !PointOnDomain(other))
                    {
                        results.Add(End);
                    }
                }
            }

            return results.Any();
        }

        /// <summary>
        /// Does this trimmed curve intersects with a Bezier curve?
        /// </summary>
        /// <param name="bezier">Bezier curve to intersect.</param>
        /// <param name="results">List containing intersection points.</param>
        /// <returns>True if any intersections exist, otherwise false.</returns>
        public bool Intersects(Bezier bezier, out List<Vector3> results)
        {
            results = new List<Vector3>();
            if (bezier.Intersects(BasisCurve, out var candidates))
            {
                foreach (var item in candidates)
                {
                    if (PointOnDomain(item))
                    {
                        results.Add(item);
                    }
                }
            }
            return results.Any();
        }
    }
}