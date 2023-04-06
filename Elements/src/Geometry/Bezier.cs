using System;
using System.Collections.Generic;

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
    /// Parameterization of the curve is 0->1.
    /// </summary>
    /// <example>
    /// [!code-csharp[Main](../../Elements/test/BezierTests.cs?name=example)]
    /// </example>
    public class Bezier : BoundedCurve
    {
        private int _samples = 50;

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
            this.Domain = new Domain1d(0, 1);
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
            for (var t = 0; t <= _samples; t++)
            {
                var pt = PointAt(t * 1.0 / _samples);
                if (t == 0.0)
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

        internal override double[] GetSampleParameters(double startSetbackDistance = 0.0,
                                                       double endSetbackDistance = 0.0)
        {
            var parameters = new double[_samples + 1];

            // TODO: Use setbacks when distance along bezier is supported.
            // var startParam = ParameterAtDistanceFromParameter(startSetbackDistance, this.Domain.Min);
            // var endParam = ParameterAtDistanceFromParameter(endSetbackDistance, this.Domain.Max, true);
            var startParam = this.Domain.Min;
            var endParam = this.Domain.Max;
            
            var step = Math.Abs(endParam - startParam) / _samples;
            for (var i = 0; i <= _samples; i++)
            {
                parameters[i] = startParam + i * step;
            }
            return parameters;
        }
    }
}