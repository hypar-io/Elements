using Elements.Geometry.Interfaces;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Elements.Geometry
{
    /// <summary>
    /// A trimmed curve.
    /// </summary>
    public abstract class TrimmedCurve<TBasis> : BoundedCurve, ITrimmedCurve<TBasis> where TBasis: ICurve
    {
        /// <summary>
        /// The basis curve for this bounded curve.
        /// </summary>
        [JsonIgnore]
        public TBasis BasisCurve { get; protected set; }

        public abstract bool PointOnDomain(Vector3 point);

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
                case Ellipse elliplse:
                    return Intersects(elliplse, out results);
                case IndexedPolycurve polycurve:
                    return polycurve.Intersects(this, out results);
                case Bezier bezier:
                    return Intersects(bezier, out results);
                default:
                    throw new NotImplementedException();
            }
        }

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
            return results.Any();
        }

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