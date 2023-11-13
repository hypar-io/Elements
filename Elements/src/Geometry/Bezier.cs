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
        /// Constants for Gauss quadrature points and weights (n = 24)
        /// https://pomax.github.io/bezierinfo/legendre-gauss.html
        /// </summary>
        private static readonly double[] T = new double[]
        {
        -0.0640568928626056260850430826247450385909,
        0.0640568928626056260850430826247450385909,
        -0.1911188674736163091586398207570696318404,
        0.1911188674736163091586398207570696318404,
        -0.3150426796961633743867932913198102407864,
        0.3150426796961633743867932913198102407864,
        -0.4337935076260451384870842319133497124524,
        0.4337935076260451384870842319133497124524,
        -0.5454214713888395356583756172183723700107,
        0.5454214713888395356583756172183723700107,
        -0.6480936519369755692524957869107476266696,
        0.6480936519369755692524957869107476266696,
        -0.7401241915785543642438281030999784255232,
        0.7401241915785543642438281030999784255232,
        -0.8200019859739029219539498726697452080761,
        0.8200019859739029219539498726697452080761,
        -0.8864155270044010342131543419821967550873,
        0.8864155270044010342131543419821967550873,
        -0.9382745520027327585236490017087214496548,
        0.9382745520027327585236490017087214496548,
        -0.9747285559713094981983919930081690617411,
        0.9747285559713094981983919930081690617411,
        -0.9951872199970213601799974097007368118745,
        0.9951872199970213601799974097007368118745
        };

        /// <summary>
        /// Constants for Gauss quadrature weights corresponding to the points (n = 24)
        /// https://pomax.github.io/bezierinfo/legendre-gauss.html
        /// </summary>
        private static readonly double[] C = new double[]
        {
        0.1279381953467521569740561652246953718517,
        0.1279381953467521569740561652246953718517,
        0.1258374563468282961213753825111836887264,
        0.1258374563468282961213753825111836887264,
        0.121670472927803391204463153476262425607,
        0.121670472927803391204463153476262425607,
        0.1155056680537256013533444839067835598622,
        0.1155056680537256013533444839067835598622,
        0.1074442701159656347825773424466062227946,
        0.1074442701159656347825773424466062227946,
        0.0976186521041138882698806644642471544279,
        0.0976186521041138882698806644642471544279,
        0.086190161531953275917185202983742667185,
        0.086190161531953275917185202983742667185,
        0.0733464814110803057340336152531165181193,
        0.0733464814110803057340336152531165181193,
        0.0592985849154367807463677585001085845412,
        0.0592985849154367807463677585001085845412,
        0.0442774388174198061686027482113382288593,
        0.0442774388174198061686027482113382288593,
        0.0285313886289336631813078159518782864491,
        0.0285313886289336631813078159518782864491,
        0.0123412297999871995468056670700372915759,
        0.0123412297999871995468056670700372915759
                };

        /// <summary>
        /// Computes the arc length of the Bézier curve between the given parameter values start and end.
        /// https://pomax.github.io/bezierinfo/#arclength
        /// </summary>
        /// <param name="start">The starting parameter value of the Bézier curve.</param>
        /// <param name="end">The ending parameter value of the Bézier curve.</param>
        /// <returns>The arc length between the specified parameter values.</returns>
        public override double ArcLength(double start, double end)
        {
            double z = 0.5; // Scaling factor for the Legendre-Gauss quadrature
            int len = T.Length; // Number of points in the Legendre-Gauss quadrature

            double sum = 0; // Accumulated sum for the arc length calculation

            // Iterating through the Legendre-Gauss quadrature points and weights
            for (int i = 0; i < len; i++)
            {
                double t = z * T[i] + z; // Mapping the quadrature point to the Bézier parameter range [0, 1]
                Vector3 derivative = Derivative(t); // Calculating the derivative of the Bézier curve at parameter t
                sum += C[i] * ArcFn(t, derivative); // Adding the weighted arc length contribution to the sum
            }

            // Scaling the sum by the scaling factor and the parameter interval (end - start) to get the arc length between start and end.
            return z * sum * (end - start);
        }

        /// <summary>
        /// Calculates the arc length contribution at parameter t based on the derivative of the Bézier curve.
        /// </summary>
        /// <param name="t">The parameter value of the Bézier curve.</param>
        /// <param name="d">The derivative of the Bézier curve at parameter t as a Vector3.</param>
        /// <returns>The arc length contribution at parameter t.</returns>
        private double ArcFn(double t, Vector3 d)
        {
            // Compute the Euclidean distance of the derivative vector (d) at parameter t
            return Math.Sqrt(d.X * d.X + d.Y * d.Y);
        }

        /// <summary>
        /// Computes the derivative of the Bézier curve at parameter t.
        /// https://pomax.github.io/bezierinfo/#derivatives
        /// </summary>
        /// <param name="t">The parameter value of the Bézier curve.</param>
        /// <returns>The derivative of the Bézier curve as a Vector3.</returns>
        private Vector3 Derivative(double t)
        {
            int n = ControlPoints.Count - 1; // Degree of the Bézier curve
            Vector3[] derivatives = new Vector3[n]; // Array to store the derivative control points

            // Calculating the derivative control points using the given formula
            for (int i = 0; i < n; i++)
            {
                derivatives[i] = n * (ControlPoints[i + 1] - ControlPoints[i]);
            }

            // Using the derivative control points to construct an (n-1)th degree Bézier curve at parameter t.
            return BezierCurveValue(t, derivatives);
        }

        /// <summary>
        /// Evaluates the value of an (n-1)th degree Bézier curve at parameter t using the given control points.
        /// </summary>
        /// <param name="t">The parameter value of the Bézier curve.</param>
        /// <param name="controlPoints">The control points for the Bézier curve.</param>
        /// <returns>The value of the Bézier curve at parameter t as a Vector3.</returns>
        private Vector3 BezierCurveValue(double t, Vector3[] controlPoints)
        {
            int n = controlPoints.Length - 1; // Degree of the Bézier curve
            Vector3[] points = new Vector3[n + 1];

            // Initialize the points array with the provided control points
            for (int i = 0; i <= n; i++)
            {
                points[i] = controlPoints[i];
            }

            // De Casteljau's algorithm to evaluate the value of the Bézier curve at parameter t
            for (int r = 1; r <= n; r++)
            {
                for (int i = 0; i <= n - r; i++)
                {
                    points[i] = (1 - t) * points[i] + t * points[i + 1];
                }
            }

            // The first element of the points array contains the value of the Bézier curve at parameter t.
            return points[0];
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
        /// The mid point of the curve.
        /// </summary>
        /// <returns>The length based midpoint.</returns>
        public virtual Vector3 MidPoint()
        {
            return PointAtNormalizedLength(0.5);
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

        /// <summary>
        /// Returns the point on the bezier corresponding to the specified length value.
        /// </summary>
        /// <param name="length">The length value along the bezier.</param>
        /// <returns>The point on the bezier corresponding to the specified length value.</returns>
        /// <exception cref="ArgumentException">Thrown when the specified length is out of range.</exception>
        public virtual Vector3 PointAtLength(double length)
        {
            double totalLength = ArcLength(this.Domain.Min, this.Domain.Max); // Calculate the total length of the Bezier
            if (length < 0 || length > totalLength)
            {
                throw new ArgumentException("The specified length is out of range.");
            }
            return PointAt(ParameterAtDistanceFromParameter(length, Domain.Min));
        }

        /// <summary>
        /// Returns the point on the bezier corresponding to the specified normalized length-based parameter value.
        /// </summary>
        /// <param name="parameter">The normalized length-based parameter value, ranging from 0 to 1.</param>
        /// <returns>The point on the bezier corresponding to the specified normalized length-based parameter value.</returns>
        /// <exception cref="ArgumentException">Thrown when the specified parameter is out of range.</exception>
        public virtual Vector3 PointAtNormalizedLength(double parameter)
        {
            if (parameter < 0 || parameter > 1)
            {
                throw new ArgumentException("The specified parameter is out of range.");
            }
            return PointAtLength(parameter * this.ArcLength(this.Domain.Min, this.Domain.Max));
        }

        /// <summary>
        /// Finds the parameter value on the Bezier curve that corresponds to the given 3D point within a specified threshold.
        /// </summary>
        /// <param name="point">The 3D point to find the corresponding parameter for.</param>
        /// <param name="threshold">The maximum distance threshold to consider a match between the projected point and the original point.</param>
        /// <returns>The parameter value on the Bezier curve if the distance between the projected point and the original point is within the threshold, otherwise returns null.</returns>
        public double? ParameterAt(Vector3 point, double threshold = 0.0001)
        {
            // Find the parameter corresponding to the projected point on the Bezier curve
            var parameter = ProjectedPoint(point, threshold);

            if (parameter == null)
            {
                // If the projected point does not return a relevant parameter return null
                return null;
            }
            // Find the 3D point on the Bezier curve at the obtained parameter value
            var projection = PointAt((double)parameter);

            // Check if the distance between the projected point and the original point is within
            // a tolerence of the threshold
            if (projection.DistanceTo(point) < (threshold * 10) - threshold)
            {
                // If the distance is within the threshold, return the parameter value
                return parameter;
            }
            else
            {
                // If the distance exceeds the threshold, consider the point as not on the Bezier curve and return null
                return null;
            }
        }

        /// <summary>
        /// Projects a 3D point onto the Bezier curve to find the parameter value of the closest point on the curve.
        /// </summary>
        /// <param name="point">The 3D point to project onto the Bezier curve.</param>
        /// <param name="threshold">The maximum threshold to refine the projection and find the closest point.</param>
        /// <returns>The parameter value on the Bezier curve corresponding to the projected point, or null if the projection is not within the specified threshold.</returns>
        public double? ProjectedPoint(Vector3 point, double threshold = 0.001)
        {
            // https://pomax.github.io/bezierinfo/#projections
            // Generate a lookup table (LUT) of points and their corresponding parameter values on the Bezier curve
            List<(Vector3 point, double t)> lut = GenerateLookupTable();

            // Initialize variables to store the closest distance (d) and the index (index) of the closest point in the lookup table
            double d = double.MaxValue;
            int index = 0;

            // Find the closest point to the input point in the lookup table (LUT) using Euclidean distance
            for (int i = 0; i < lut.Count; i++)
            {
                double q = Math.Sqrt(Math.Pow((point - lut[i].point).X, 2) + Math.Pow((point - lut[i].point).Y, 2) + Math.Pow((point - lut[i].point).Z, 2));
                if (q < d)
                {
                    d = q;
                    index = i;
                }
            }

            // Obtain the parameter values of the neighboring points in the LUT for further refinement
            double t1 = lut[Math.Max(index - 1, 0)].t;
            double t2 = lut[Math.Min(index + 1, lut.Count - 1)].t;
            double v = t2 - t1;

            // Refine the projection by iteratively narrowing down the parameter range to find the closest point
            while (v > threshold)
            {
                // Calculate intermediate parameter values
                double t0 = t1 + v / 4;
                double t3 = t2 - v / 4;

                // Calculate corresponding points on the Bezier curve using the intermediate parameter values
                Vector3 p0 = PointAt(t0);
                Vector3 p3 = PointAt(t3);

                // Calculate the distances between the input point and the points on the Bezier curve
                double d0 = Math.Sqrt(Math.Pow((point - p0).X, 2) + Math.Pow((point - p0).Y, 2) + Math.Pow((point - p0).Z, 2));
                double d3 = Math.Sqrt(Math.Pow((point - p3).X, 2) + Math.Pow((point - p3).Y, 2) + Math.Pow((point - p3).Z, 2));

                // Choose the sub-range that is closer to the input point and update the range
                if (d0 < d3)
                {
                    t2 = t3;
                }
                else
                {
                    t1 = t0;
                }

                // Update the range difference for the next iteration
                v = t2 - t1;
            }

            // Return the average of the refined parameter values as the projection of the input point on the Bezier curve
            return (t1 + t2) / 2;
        }

        /// <summary>
        /// Generates a lookup table of points and their corresponding parameter values on the Bezier curve.
        /// </summary>
        /// <param name="numSamples">Number of samples to take along the curve.</param>
        /// <returns>A list of tuples containing the sampled points and their corresponding parameter values on the Bezier curve.</returns>
        private List<(Vector3 point, double t)> GenerateLookupTable(int numSamples = 100)
        {
            // Initialize an empty list to store the lookup table (LUT)
            List<(Vector3 point, double t)> lut = new List<(Vector3 point, double t)>();

            // Generate lookup table by sampling points on the Bezier curve
            for (int i = 0; i <= numSamples; i++)
            {
                double t = (double)i / numSamples; // Calculate the parameter value based on the current sample index
                Vector3 point = PointAt(t); // Get the 3D point on the Bezier curve corresponding to the current parameter value
                lut.Add((point, t)); // Add the sampled point and its corresponding parameter value to the lookup table (LUT)
            }

            // Return the completed lookup table (LUT)
            return lut;
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

        /// <summary>
        /// Divides the bezier into segments of the specified length.
        /// </summary>
        /// <param name="divisionLength">The desired length of each segment.</param>
        /// <returns>A list of points representing the segment divisions.</returns>
        public Vector3[] DivideByLength(double divisionLength)
        {
            var totalLength = this.ArcLength(Domain.Min, Domain.Max);
            if (totalLength <= 0)
            {
                // Handle invalid bezier with insufficient length
                return new Vector3[0];
            }
            var parameter = ParameterAtDistanceFromParameter(divisionLength, Domain.Min);
            var segments = new List<Vector3> { this.Start };

            while (parameter < Domain.Max)
            {
                segments.Add(PointAt(parameter));
                var newParameter = ParameterAtDistanceFromParameter(divisionLength, parameter);
                parameter = newParameter != parameter ? newParameter : Domain.Max;
            }

            // Add the last vertex of the bezier as the endpoint of the last segment if it
            // is not already part of the list
            if (!segments[segments.Count - 1].IsAlmostEqualTo(this.End))
            {
                segments.Add(this.End);
            }

            return segments.ToArray();
        }

        /// <summary>
        /// Divides the bezier into segments of the specified length.
        /// </summary>
        /// <param name="divisionLength">The desired length of each segment.</param>
        /// <returns>A list of beziers representing the segments.</returns>
        public List<Bezier> SplitByLength(double divisionLength)
        {
            var totalLength = this.ArcLength(Domain.Min, Domain.Max);
            if (totalLength <= 0)
            {
                // Handle invalid bezier with insufficient length
                return null;
            }
            var currentParameter = ParameterAtDistanceFromParameter(divisionLength, Domain.Min);
            var parameters = new List<double> { this.Domain.Min };

            while (currentParameter < Domain.Max)
            {
                parameters.Add(currentParameter);
                var newParameter = ParameterAtDistanceFromParameter(divisionLength, currentParameter);
                currentParameter = newParameter != currentParameter ? newParameter : Domain.Max;
            }

            // Add the last vertex of the bezier as the endpoint of the last segment if it
            // is not already part of the list
            if (!parameters[parameters.Count - 1].ApproximatelyEquals(this.Domain.Max))
            {
                parameters.Add(this.Domain.Max);
            }

            return Split(parameters);
        }

        /// <summary>
        /// Splits the Bezier curve into segments at specified parameter values.
        /// </summary>
        /// <param name="parameters">The list of parameter values to split the curve at.</param>
        /// <param name="normalize">If true the parameters will be length normalized.</param>
        /// <returns>A list of Bezier segments obtained after splitting.</returns>
        public List<Bezier> Split(List<double> parameters, bool normalize = false)
        {
            // Calculate the total length of the Bezier curve
            var totalLength = this.ArcLength(Domain.Min, Domain.Max);

            // Check for invalid curve with insufficient length
            if (totalLength <= 0)
            {
                throw new InvalidOperationException($"Invalid bezier with insufficient length. Total Length = {totalLength}");
            }

            // Check if the list of parameters is empty or null
            if (parameters == null || parameters.Count == 0)
            {
                throw new ArgumentException("No split points provided.");
            }

            // Initialize a list to store the resulting Bezier segments
            var segments = new List<Bezier>();
            var bezier = this; // Create a reference to the original Bezier curve

            if (normalize)
            {
                parameters = parameters.Select(parameter => ParameterAtDistanceFromParameter(parameter * this.ArcLength(this.Domain.Min, this.Domain.Max), Domain.Min)).ToList();
            }
            parameters.Sort(); // Sort the parameters in ascending order

            // Iterate through each parameter to split the curve
            for (int i = 0; i < parameters.Count; i++)
            {
                double t = (Domain.Min <= parameters[i] && parameters[i] <= Domain.Max)
                    ? parameters[i] // Ensure the parameter is within the domain
                    : throw new ArgumentException($"Parameter {parameters[i]} is not within the domain ({Domain.Min}->{Domain.Max}) of the Bezier curve.");

                // Check if the parameter is within the valid range [0, 1]
                if (t >= 0 && t <= 1)
                {
                    // Split the curve at the given parameter and obtain the two resulting Bezier segments
                    var tuple = bezier.SplitAt(t);

                    // Store the first split Bezier in the list
                    segments.Add(tuple.Item1);

                    // Update bezier to the second split Bezier to continue splitting
                    bezier = tuple.Item2;

                    // Remap subsequent parameters to the new Bezier curve's parameter space
                    for (int j = i + 1; j < parameters.Count; j++)
                    {
                        parameters[j] = (parameters[j] - t) / (1 - t);
                    }
                }
            }

            segments.Add(bezier);

            // Return the list of Bezier segments obtained after splitting
            return segments;
        }

        /// <summary>
        /// Splits the bezier curve at the given parameter value.
        /// </summary>
        /// <param name="t">The parameter value at which to split the curve.</param>
        /// <returns>A tuple containing two split bezier curves.</returns>
        public Tuple<Bezier, Bezier> SplitAt(double t)
        {
            // Extract the control points from the input bezier
            var startPoint = ControlPoints[0];
            var controlPoint1 = ControlPoints[1];
            var controlPoint2 = ControlPoints[2];
            var endPoint = ControlPoints[3];

            // Compute the intermediate points using de Casteljau's algorithm
            var q0 = (1 - t) * startPoint + t * controlPoint1;
            var q1 = (1 - t) * controlPoint1 + t * controlPoint2;
            var q2 = (1 - t) * controlPoint2 + t * endPoint;

            var r0 = (1 - t) * q0 + t * q1;
            var r1 = (1 - t) * q1 + t * q2;

            // Compute the split point on the bezier curve
            var splitPoint = (1 - t) * r0 + t * r1;

            // Construct the first split bezier curve
            var subBezier1 = new Bezier(new List<Vector3>() { startPoint, q0, r0, splitPoint });

            // Construct the second split bezier curve
            var subBezier2 = new Bezier(new List<Vector3>() { splitPoint, r1, q2, endPoint });

            // Return a tuple containing the split bezier curves
            return new Tuple<Bezier, Bezier>(subBezier1, subBezier2);
        }

        /// <summary>
        /// Constructs piecewise cubic Bézier curves from a list of points using control points calculated with the specified looseness.
        /// </summary>
        /// <param name="points">The list of points defining the path.</param>
        /// <param name="looseness">The looseness factor used to calculate control points. A higher value results in smoother curves.</param>
        /// <param name="close">If true, the path will be closed, connecting the last point with the first one.</param>
        /// <returns>A list of piecewise cubic Bézier curves approximating the path defined by the input points.</returns>
        public static List<Bezier> ConstructPiecewiseCubicBezier(List<Vector3> points, double looseness = 6.0, bool close = false)
        {
            List<Bezier> beziers = new List<Bezier>();

            // Calculate the control points.
            List<Vector3[]> controlPoints = CalculateControlPoints(points, looseness);

            // Create the start Bezier curve.
            Bezier startBezier = new Bezier(
                new List<Vector3>
                {
                    points[0],
                    controlPoints[0][1],
                    points[1]
                }
            );

            // Add the start Bezier curve to the list.
            beziers.Add(startBezier);

            // Iterate through pairs of points.
            for (int i = 1; i < points.Count - 2; i++)
            {
                // Create the control points.
                List<Vector3> bezierControlPoints = new List<Vector3>
                {
                    points[i],
                    controlPoints[i - 1][0],
                    controlPoints[i][1],
                    points[i + 1]
                };

                // Create the Bezier curve.
                Bezier bezier = new Bezier(bezierControlPoints);

                // Add the Bezier curve to the list.
                beziers.Add(bezier);
            }

            // Create the end Bezier curve.
            Bezier endBezier = new Bezier(
                new List<Vector3>
                {
                    points[points.Count() - 1],
                    controlPoints[controlPoints.Count() - 1][0],
                    points[points.Count() - 2]
                }
            );

            // Add the end Bezier curve to the list.
            beziers.Add(endBezier);

            // Return the list of Bezier curves.
            return beziers;
        }

        /// <summary>
        /// Calculates the control points for constructing piecewise cubic Bézier curves from the given list of points and looseness factor.
        /// </summary>
        /// <param name="points">The list of points defining the path.</param>
        /// <param name="looseness">The looseness factor used to calculate control points. A higher value results in smoother curves.</param>
        /// <returns>A list of control points (pairs of Vector3) for the piecewise cubic Bézier curves.</returns>
        private static List<Vector3[]> CalculateControlPoints(List<Vector3> points, double looseness)
        {
            List<Vector3[]> controlPoints = new List<Vector3[]>();

            for (int i = 1; i < points.Count - 1; i++)
            {
                // Calculate the differences in x and y coordinates.
                var dx = points[i - 1].X - points[i + 1].X;
                var dy = points[i - 1].Y - points[i + 1].Y;
                var dz = points[i - 1].Z - points[i + 1].Z;

                // Calculate the control point coordinates.
                var controlPointX1 = points[i].X - dx * (1 / looseness);
                var controlPointY1 = points[i].Y - dy * (1 / looseness);
                var controlPointZ1 = points[i].Z - dz * (1 / looseness);
                var controlPoint1 = new Vector3(controlPointX1, controlPointY1, controlPointZ1);

                var controlPointX2 = points[i].X + dx * (1 / looseness);
                var controlPointY2 = points[i].Y + dy * (1 / looseness);
                var controlPointZ2 = points[i].Z + dz * (1 / looseness);
                var controlPoint2 = new Vector3(controlPointX2, controlPointY2, controlPointZ2);

                // Create an array to store the control points.
                Vector3[] controlPointArray = new Vector3[]
                {
                    controlPoint1,
                    controlPoint2
                };

                // Add the control points to the list.
                controlPoints.Add(controlPointArray);
            }

            // Return the list of control points.
            return controlPoints;
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

            var l = this.Length();
            var div = (int)Math.Round(l / DefaultMinimumChordLength);
            div = Math.Min(div, _lengthSamples);

            // Iteratively, find points on Bezier with 0 distance to the line.
            // It Bezier was limited to 4 points - more effective approach could be used.
            var roots = Equations.SolveIterative(Domain.Min, Domain.Max, div,
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

            var l = this.Length();
            var div = (int)Math.Round(l / DefaultMinimumChordLength);
            div = Math.Min(div, _lengthSamples);

            // Iteratively, find points on Bezier with radius distance to the circle.
            var invertedT = circle.Transform.Inverted();
            var roots = Equations.SolveIterative(Domain.Min, Domain.Max, div,
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

            var l = this.Length();
            var div = (int)Math.Round(l / DefaultMinimumChordLength);
            div = Math.Min(div, _lengthSamples);

            // Iteratively, find points on ellipse with distance
            // to other ellipse equal to its focal distance.
            var invertedT = ellipse.Transform.Inverted();
            var roots = Equations.SolveIterative(Domain.Min, Domain.Max, div,
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
            int leftSteps = BoxApproximationStepsCount();
            int rightSteps = other.BoxApproximationStepsCount();

            var leftCache = new Dictionary<double, Vector3>();
            var rightCache = new Dictionary<double, Vector3>();

            BBox3 box = CurveBox(leftSteps, leftCache);
            BBox3 otherBox = other.CurveBox(rightSteps, rightCache);

            Intersects(other,
                       (box, Domain),
                       (otherBox, other.Domain),
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
                                (BBox3 Box, Domain1d Domain) left,
                                (BBox3 Box, Domain1d Domain) right,
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

            var leftSplit = SplitCurveBox(left, leftCache, out var loLeft, out var hiLeft);
            var rightSplit = other.SplitCurveBox(right, rightCache, out var loRight, out var hiRight);

            // If boxes of two curves are tolerance sized -
            // average point of their centers is treated as intersection point.
            if (!leftSplit && !rightSplit)
            {
                results.Add(left.Box.Center().Average(right.Box.Center()));
                return;
            }

            // Recursively repeat process on box of subdivided curves until they are small enough.
            // Each pair, which bounding boxes are not intersecting are discarded.
            if (!leftSplit)
            {
                Intersects(other, left, loRight, leftCache, rightCache, ref results);
                Intersects(other, left, hiRight, leftCache, rightCache, ref results);
            }
            else if (!rightSplit)
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

        private bool SplitCurveBox((BBox3 Box, Domain1d Domain) def,
                                   Dictionary<double, Vector3> cache,
                                   out (BBox3 Box, Domain1d Domain) low,
                                   out (BBox3 Box, Domain1d Domain) high)
        {
            low = (default, default);
            high = (default, default);

            // If curve bounding box is tolerance size - it's considered as intersection.
            // Otherwise calculate new boxes of two halves of the curve.
            var epsilon2 = Vector3.EPSILON * Vector3.EPSILON;
            var leftConvergent = (def.Box.Max - def.Box.Min).LengthSquared() < epsilon2 * 2;
            if (leftConvergent)
            {
                return false;
            }

            // If curve bounding box is tolerance size - it's considered as intersection.
            // Otherwise calculate new boxes of two halves of the curve.
            low = CurveBoxHalf(def, cache, true);
            high = CurveBoxHalf(def, cache, false);
            return true;
        }

        /// <summary>
        /// Get bounding box of curve segment (not control points) for half of given domain.
        /// </summary>
        /// <param name="def">Curve segment definition - box, domain and number of subdivisions.</param>
        /// <param name="cache">Dictionary of precomputed points at parameter.</param>
        /// <param name="low">Take lower of higher part of curve.</param>
        /// <returns>Definition of curve segment that is half of given.</returns>
        private (BBox3 Box, Domain1d Domain) CurveBoxHalf(
            (BBox3 Box, Domain1d Domain) def,
            Dictionary<double, Vector3> cache,
            bool low)
        {
            var steps = BoxApproximationStepsCount();
            double step = (def.Domain.Length / 2) / steps;
            var min = low ? def.Domain.Min : def.Domain.Min + def.Domain.Length / 2;
            var max = low ? def.Domain.Min + def.Domain.Length / 2 : def.Domain.Max;

            List<Vector3> points = new List<Vector3>();
            for (int i = 0; i < steps; i++)
            {
                var t = min + i * step;
                points.Add(PointAtCached(t, cache));
            }
            points.Add(PointAtCached(max, cache));

            var box = new BBox3(points);
            var domain = new Domain1d(min, max);
            return (box, domain);
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

        private int BoxApproximationStepsCount()
        {
            return ControlPoints.Count * 8 - 1;
        }
    }
}