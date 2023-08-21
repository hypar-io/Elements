using System;
using System.Collections.Generic;
using System.Linq;
using Elements.Geometry.Interfaces;
using Newtonsoft.Json;

namespace Elements.Geometry
{
    // Some resources:
    // https://pomax.github.io/bezierinfo/#curveintersection
    // http://webhome.cs.uvic.ca/~blob/courses/305/notes/pdf/ref-frames.pdf

    /// <summary>
    /// The frame type to be used for operations requiring 
    /// a moving frame around the curve.
    /// </summary>
    public enum FrameType
    {
        /// <summary>
        /// A Frenet frame.
        /// </summary>
        Frenet,
        /// <summary>
        /// A frame with the up axis aligned with +Z.
        /// </summary>
        RoadLike
    }

    /// <summary>
    /// A Bezier curve.
    /// Parameterization of the curve is 0 -> 1.
    /// </summary>
    /// <example>
    /// [!code-csharp[Main](../../Elements/test/BezierTests.cs?name=example)]
    /// </example>
    public class Bezier : BoundedCurve
    {
        private readonly int _lengthSamples = 500;

        /// <summary>
        /// The domain of the curve.
        /// </summary>
        [JsonIgnore]
        public override Domain1d Domain => new Domain1d(0, 1);

        /// <summary>
        /// A collection of points describing the bezier's frame.
        /// https://en.wikipedia.org/wiki/B%C3%A9zier_curve
        /// </summary>
        public List<Vector3> ControlPoints { get; set; }

        /// <summary>
        /// The frame type to use when calculating transforms along the curve.
        /// </summary>
        public FrameType FrameType { get; set; }

        /// <summary>
        /// Construct a bezier.
        /// </summary>
        /// <param name="controlPoints">The control points of the curve.</param>
        /// <param name="frameType">The frame type to use when calculating frames.</param>
        public Bezier(List<Vector3> controlPoints, FrameType frameType = FrameType.Frenet)
        {
            if (controlPoints.Count < 3)
            {
                throw new ArgumentOutOfRangeException("The controlPoints collection must have at least 3 points.");
            }

            this.ControlPoints = controlPoints;
            this.FrameType = frameType;
            this.Start = PointAt(0);
            this.End = PointAt(1);
        }

        /// <summary>
        /// Get the bounding box of the curve's control points.
        /// </summary>
        public override BBox3 Bounds()
        {
            return new BBox3(this.RenderVertices());
        }

        /// <summary>
        /// Get a piecewise linear approximation of the length of the curve.
        /// https://en.wikipedia.org/wiki/Arc_length
        /// </summary>
        public override double Length()
        {
            Vector3 last = new Vector3();
            double length = 0.0;
            var step = 1.0 / _lengthSamples;
            for (var i = 0; i <= _lengthSamples; i++)
            {
                var t = i * step;
                var pt = PointAt(t);
                if (i == 0)
                {
                    last = pt;
                    continue;
                }
                length += pt.DistanceTo(last);
                last = pt;
            }
            return length;
        }

        /// <summary>
        /// Calculate the length of the bezier between start and end parameters.
        /// </summary>
        /// <returns>The length of the bezier between start and end.</returns>
        public override double ArcLength(double start, double end)
        {
            if (!Domain.Includes(start, true))
            {
                throw new ArgumentOutOfRangeException("start", $"The start parameter {start} must be between {Domain.Min} and {Domain.Max}.");
            }
            if (!Domain.Includes(end, true))
            {
                throw new ArgumentOutOfRangeException("end", $"The end parameter {end} must be between {Domain.Min} and {Domain.Max}.");
            }

            // TODO: We use max value here so that the calculation will continue
            // until at least the end of the curve. This is not a nice solution.
            return ArcLengthUntil(start, double.MaxValue, out end);
        }

        private double ArcLengthUntil(double start, double distance, out double end)
        {
            Vector3 last = new Vector3();
            double length = 0.0;
            var step = (this.Domain.Max - start) / _lengthSamples;
            end = start;
            for (var i = 0; i <= _lengthSamples; i++)
            {
                var t = start + i * step;
                var pt = PointAt(t);
                if (i == 0)
                {
                    last = pt;
                    continue;
                }
                var d = pt.DistanceTo(last);
                if (length + d > distance)
                {
                    end = t;
                    return length;
                }
                length += d;
                last = pt;
            }
            return length;
        }

        /// <summary>
        /// Get the point on the curve at parameter u.
        /// </summary>
        /// <param name="u">The parameter between 0.0 and 1.0.</param>
        public override Vector3 PointAt(double u)
        {
            var t = u;
            var n = ControlPoints.Count - 1;

            // https://en.wikipedia.org/wiki/B%C3%A9zier_curve
            // B(t) = SUM(i=0..n)(n i)(1-t)^n-i * t^i * Pi
            Vector3 p = new Vector3();
            for (var i = 0; i <= n; i++)
            {
                p += BerensteinBasisPolynomial(n, i, t) * ControlPoints[i];
            }
            return p;
        }

        private double BinomialCoefficient(int n, int i)
        {
            return Factorial(n) / (Factorial(i) * Factorial(n - i));
        }

        /// <summary>
        /// https://en.wikipedia.org/wiki/Factorial
        /// </summary>
        /// <param name="n"></param>
        private int Factorial(int n)
        {
            if (n == 0)
            {
                return 1;
            }

            var fact = n;
            for (var i = n - 1; i >= 1; i--)
            {
                fact *= i;
            }
            return fact;
        }

        private double BerensteinBasisPolynomial(int n, int i, double t)
        {
            // Berenstein basis polynomial
            // bi,n(t) = (n i)t^i(1-t)^n-i
            return BinomialCoefficient(n, i) * Math.Pow(t, i) * Math.Pow(1 - t, n - i);
        }

        /// <summary>
        /// Get the transform on the curve at parameter u.
        /// </summary>
        /// <param name="u">The parameter along the curve between 0.0 and 1.0.</param>
        public override Transform TransformAt(double u)
        {
            switch (this.FrameType)
            {
                case FrameType.Frenet:
                    return new Transform(PointAt(u), NormalAt(u), TangentAt(u).Negate());
                case FrameType.RoadLike:
                    var z = TangentAt(u).Negate();
                    // If Z is parallel to the Z axis, the other vectors will
                    // have zero length. We use the -Y axis in that case.
                    var up = z.IsParallelTo(Vector3.ZAxis) ? Vector3.YAxis.Negate() : Vector3.ZAxis;
                    var x = up.Cross(z);
                    return new Transform(PointAt(u), x, z);
            }

            throw new Exception("Curve.FrameType must specify which frame type to use.");
        }

        /// <summary>
        /// Get the velocity to the curve at parameter u.
        /// </summary>
        /// <param name="u">A parameter between 0.0 and 1.0.</param>
        public Vector3 VelocityAt(double u)
        {
            // First derivative
            // B'(t) = n * SUM(i-0..n-1)b i,n-1(t)(Pi+1 - Pi)
            var n = this.ControlPoints.Count - 1;
            var V = new Vector3();
            var t = u;
            for (var i = 0; i <= n - 1; i++)
            {
                V += BerensteinBasisPolynomial(n - 1, i, t) * (this.ControlPoints[i + 1] - this.ControlPoints[i]);
            }
            return V;
        }

        /// <summary>
        /// Get the acceleration of the curve at parameter u.
        /// </summary>
        /// <param name="u">A parameter between 0.0 and 1.0.</param>
        public Vector3 AccelerationAt(double u)
        {
            // Second derivative
            // https://pages.mtu.edu/~shene/COURSES/cs3621/NOTES/spline/Bezier/bezier-der.html
            // B''(t) = SUM(i=0..n-2) b n-2,i (t)(n(n-1)(Pi+2 - 2Pi+1  + Pi)
            var n = this.ControlPoints.Count - 1;
            var Q = new Vector3();
            var t = u;
            for (var i = 0; i <= n - 2; i++)
            {
                Q += BerensteinBasisPolynomial(n - 2, i, t) * (n * (n - 1) * (this.ControlPoints[i + 2] - 2 * this.ControlPoints[i + 1] + this.ControlPoints[i]));
            }
            return Q;
        }

        /// <summary>
        /// Get the tangent to the curve at parameter u.
        /// </summary>
        /// <param name="u">A parameter between 0.0 and 1.0.</param>
        public Vector3 TangentAt(double u)
        {
            return VelocityAt(u).Unitized();
        }

        /// <summary>
        /// Get the normal of the curve at parameter u.
        /// </summary>
        /// <param name="u">A parameter between 0.0 and 1.0.</param>
        public Vector3 NormalAt(double u)
        {
            var V = VelocityAt(u);
            var Q = AccelerationAt(u);
            return V.Cross(Q).Cross(V).Unitized();
        }

        /// <summary>
        /// Get the binormal to the curve at parameter u.
        /// </summary>
        /// <param name="u">A parameter between 0.0 and 1.0.</param>
        public Vector3 BinormalAt(double u)
        {
            var T = TangentAt(u);
            var N = NormalAt(u);
            return T.Cross(N);
        }

        /// <summary>
        /// Construct a transformed copy of this Bezier.
        /// </summary>
        /// <param name="transform">The transform to apply.</param>
        public Bezier TransformedBezier(Transform transform)
        {
            var newCtrlPoints = new List<Vector3>();
            foreach (var vp in ControlPoints)
            {
                newCtrlPoints.Add(transform.OfPoint(vp));
            }
            return new Bezier(newCtrlPoints, this.FrameType);
        }

        /// <summary>
        /// Construct a transformed copy of this Curve.
        /// </summary>
        /// <param name="transform">The transform to apply.</param>
        public override Curve Transformed(Transform transform)
        {
            return TransformedBezier(transform);
        }

        /// <inheritdoc/>
        public override double[] GetSubdivisionParameters(double startSetbackDistance = 0.0,
                                                          double endSetbackDistance = 0.0)
        {
            var l = this.Length();
            var div = (int)Math.Round(l / DefaultMinimumChordLength);

            var parameters = new double[div + 1];
            var startParam = startSetbackDistance == 0.0 ? this.Domain.Min : ParameterAtDistanceFromParameter(startSetbackDistance, this.Domain.Min);
            var endParam = startSetbackDistance == 0.0 ? this.Domain.Max : ParameterAtDistanceFromParameter(l - endSetbackDistance, this.Domain.Min);

            var step = Math.Abs(endParam - startParam) / div;
            for (var i = 0; i <= div; i++)
            {
                parameters[i] = startParam + i * step;
            }
            return parameters;
        }

        /// <summary>
        /// Get the parameter at a distance from the start parameter along the curve.
        /// </summary>
        /// <param name="distance">The distance from the start parameter.</param>
        /// <param name="start">The parameter from which to measure the distance.</param>
        public override double ParameterAtDistanceFromParameter(double distance, double start)
        {
            ArcLengthUntil(start, distance, out var end);
            return end;
        }

        /// <inheritdoc/>
        public override bool Intersects(ICurve curve, out List<Vector3> results)
        {
            switch (curve)
            {
                case Line line:
                    return line.Intersects(this, out results);
                case Arc arc:
                    return arc.Intersects(this, out results);
                case EllipticalArc ellipticArc:
                    return ellipticArc.Intersects(this, out results);
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

        /// <summary>
        /// Does this bezier curve intersects with an infinite line?
        /// Iterative approximation is used to find intersections.
        /// </summary>
        /// <param name="line">Infinite line to intersect.</param>
        /// <param name="results">List containing intersection points.</param>
        /// <returns>True if intersection exists, otherwise false.</returns>
        public bool Intersects(InfiniteLine line, out List<Vector3> results)
        {
            BBox3 box = new BBox3(ControlPoints);
            // Bezier curve always inside it's bounding box.
            // Rough check if line intersects curve box.
            if (!new Line(line.Origin, line.Origin + line.Direction).Intersects(
                box, out _, true))
            {
                results = new List<Vector3>();
                return false;
            }

            // Iteratively, find points on Bezier with 0 distance to the line.
            // It Bezier was limited to 4 points - more effective approach could be used.
            var roots = Equations.SolveIterative(Domain.Min, Domain.Max, 45,
                    new Func<double, double>((t) =>
                    {
                        var p = PointAt(t);
                        return (p - p.ClosestPointOn(line)).LengthSquared();
                    }), Vector3.EPSILON * Vector3.EPSILON);

            results = roots.Select(r => PointAt(r)).UniqueAverageWithinTolerance(
                Vector3.EPSILON * 2).ToList();
            return results.Any();
        }

        /// <summary>
        /// Does this Bezier curve intersects with a circle?
        /// Iterative approximation is used to find intersections.
        /// </summary>
        /// <param name="circle">Circle to intersect.</param>
        /// <param name="results">List containing intersection points.</param>
        /// <returns>True if any intersections exist, otherwise false.</returns>
        public bool Intersects(Circle circle, out List<Vector3> results)
        {
            BBox3 box = new BBox3(ControlPoints);
            // Bezier curve always inside it's bounding box.
            // Rough check if curve is too far away.
            var boxCenter = box.Center();
            if (circle.Center.DistanceTo(boxCenter) >
                circle.Radius + (box.Max - boxCenter).Length())
            {
                results = new List<Vector3>();
                return false;
            }

            // Iteratively, find points on Bezier with radius distance to the circle.
            var invertedT = circle.Transform.Inverted();
            var roots = Equations.SolveIterative(Domain.Min, Domain.Max, 45,
                    new Func<double, double>((t) =>
                    {
                        var p = PointAt(t);
                        var local = invertedT.OfPoint(p);
                        return local.LengthSquared() - circle.Radius * circle.Radius;
                    }), Vector3.EPSILON * Vector3.EPSILON);

            results = roots.Select(r => PointAt(r)).UniqueAverageWithinTolerance(
                Vector3.EPSILON * 2).ToList();
            return results.Any();
        }

        /// <summary>
        /// Does this Bezier curve intersects with an ellipse?
        /// Iterative approximation is used to find intersections.
        /// </summary>
        /// <param name="ellipse">Ellipse to intersect.</param>
        /// <param name="results">List containing intersection points.</param>
        /// <returns>True if any intersections exist, otherwise false.</returns>
        public bool Intersects(Ellipse ellipse, out List<Vector3> results)
        {
            BBox3 box = new BBox3(ControlPoints);
            // Bezier curve always inside it's bounding box.
            // Rough check if curve is too far away.
            var boxCenter = box.Center();
            if (ellipse.Center.DistanceTo(boxCenter) > 
                Math.Max(ellipse.MajorAxis, ellipse.MinorAxis) + (box.Max - boxCenter).Length())
            {
                results = new List<Vector3>();
                return false;
            }

            // Iteratively, find points on ellipse with distance
            // to other ellipse equal to its focal distance.
            var invertedT = ellipse.Transform.Inverted();
            var roots = Equations.SolveIterative(Domain.Min, Domain.Max, 45,
                new Func<double, double>((t) =>
                {
                    var p = PointAt(t);
                    var local = invertedT.OfPoint(p);
                    var dx = Math.Pow(local.X / ellipse.MajorAxis, 2);
                    var dy = Math.Pow(local.Y / ellipse.MinorAxis, 2);
                    return dx + dy + local.Z * local.Z - 1;
                }), Vector3.EPSILON * Vector3.EPSILON);

            results = roots.Select(r => PointAt(r)).UniqueAverageWithinTolerance(
                Vector3.EPSILON * 2).ToList();
            return results.Any();
        }

        /// <summary>
        /// Does this Bezier curve intersects with other Bezier curve?
        /// Iterative approximation is used to find intersections.
        /// </summary>
        /// <param name="other">Other Bezier curve to intersect.</param>
        /// <param name="results">List containing intersection points.</param>
        /// <returns>True if any intersections exist, otherwise false.</returns>
        public bool Intersects(Bezier other, out List<Vector3> results)
        {
            results = new List<Vector3>();
            int leftSteps = ControlPoints.Count * 8 - 1;
            int rightSteps = other.ControlPoints.Count * 8 - 1;

            var leftCache = new Dictionary<double, Vector3>();
            var rightCache = new Dictionary<double, Vector3>();

            BBox3 box = CurveBox(leftSteps, leftCache);
            BBox3 otherBox = other.CurveBox(rightSteps, rightCache);

            Intersects(other,
                       (box, Domain, leftSteps),
                       (otherBox, other.Domain, rightSteps), 
                       leftCache,
                       rightCache,
                       ref results);

            // Subdivision algorithm produces duplicates, all tolerance away from real answer.
            // Grouping and averaging them improves output as we as eliminates duplications.
            results = results.UniqueAverageWithinTolerance().ToList();
            return results.Any();
        }

        private BBox3 CurveBox(int numSteps, Dictionary<double, Vector3> cache)
        {
            List<Vector3> points = new List<Vector3>();
            double step = Domain.Length / numSteps;
            for (int i = 0; i < numSteps; i++)
            {
                var t = Domain.Min + i * step;
                points.Add(PointAtCached(t, cache));
            }
            points.Add(PointAtCached(Domain.Max, cache));
            BBox3 box = new BBox3(points);
            return box;
        }

        private void Intersects(Bezier other,
                                (BBox3 Box, Domain1d Domain, double Steps) left,
                                (BBox3 Box, Domain1d Domain, double Steps) right,
                                Dictionary<double, Vector3> leftCache,
                                Dictionary<double, Vector3> rightCache,
                                ref List<Vector3> results)
        {
            // If bounding boxes of two curves (not control points) are not intersect
            // curves not intersect.
            if (!left.Box.Intersects(right.Box))
            {
                return;
            }

            var epsilon2 = Vector3.EPSILON * Vector3.EPSILON;
            (BBox3 Box, Domain1d Domain, double Resolution) loLeft = (default, default, 0);
            (BBox3 Box, Domain1d Domain, double Resolution) hiLeft = (default, default, 0);
            (BBox3 Box, Domain1d Domain, double Resolution) loRight = (default, default, 0);
            (BBox3 Box, Domain1d Domain, double Resolution) hiRight = (default, default, 0);

            // If curve bounding box is tolerance size - it's considered as intersection.
            // Otherwise calculate new boxes of two halves of the curve. 
            var leftConvergent = (left.Box.Max - left.Box.Min).LengthSquared() < epsilon2 * 2;
            if (!leftConvergent)
            {
                loLeft = SplitCurveBox(left, leftCache, true);
                hiLeft = SplitCurveBox(left, leftCache, false);
            }

            // Same as above but for the other curve.
            bool rightConvergent = (right.Box.Max - right.Box.Min).LengthSquared() < epsilon2 * 2;
            if (!rightConvergent)
            {
                loRight = other.SplitCurveBox(right, rightCache, true);
                hiRight = other.SplitCurveBox(right, rightCache, false);
            }

            // If boxes of two curves are tolerance sized -
            // average point of their centers is treated as intersection point.
            if (leftConvergent && rightConvergent)
            {
                results.Add(left.Box.Center().Average(right.Box.Center()));
                return;
            }

            // Recursively repeat process on box of subdivided curves until they are small enough.
            // Each pair, which bounding boxes are not intersecting are discarded.
            if (leftConvergent)
            {
                Intersects(other, left, loRight, leftCache, rightCache, ref results);
                Intersects(other, left, hiRight, leftCache, rightCache, ref results);
            }
            else if (rightConvergent) 
            {
                Intersects(other, loLeft, right, leftCache, rightCache, ref results);
                Intersects(other, hiLeft, right, leftCache, rightCache, ref results);
            }
            else
            {
                Intersects(other, loLeft, loRight, leftCache, rightCache, ref results);
                Intersects(other, hiLeft, loRight, leftCache, rightCache, ref results);
                Intersects(other, loLeft, hiRight, leftCache, rightCache, ref results);
                Intersects(other, hiLeft, hiRight, leftCache, rightCache, ref results);
            }
        }


        /// <summary>
        /// Get bounding box of curve segment (not control points) for half of given domain.
        /// </summary>
        /// <param name="def">Curve segment definition - box, domain and number of subdivisions.</param>
        /// <param name="cache">Dictionary of precomputed points at parameter.</param>
        /// <param name="low">Take lower of higher part of curve.</param>
        /// <returns>Definition of curve segment that is half of given.</returns>
        private (BBox3 Box, Domain1d Domain, double Steps) SplitCurveBox(
            (BBox3 Box, Domain1d Domain, double Steps) def,
            Dictionary<double, Vector3> cache,
            bool low)
        {
            double step = (def.Domain.Length / 2) / def.Steps;
            var min = low ? def.Domain.Min : def.Domain.Min + def.Domain.Length / 2;
            var max = low ? def.Domain.Min + def.Domain.Length / 2 : def.Domain.Max;

            List<Vector3> points = new List<Vector3>();
            for (int i = 0; i < def.Steps; i++)
            {
                var t = min + i * step;
                points.Add(PointAtCached(t, cache));
            }
            points.Add(PointAtCached(max, cache));

            var box = new BBox3(points);
            var domain = new Domain1d(min, max);
            return (box, domain, def.Steps);
        }

        private Vector3 PointAtCached(double t, Dictionary<double, Vector3> cache)
        {
            if (cache.TryGetValue(t, out var p))
            {
                return p;
            }
            else
            {
                p = PointAt(t);
                cache.Add(t, p);
                return p;
            }
        }
    }
}