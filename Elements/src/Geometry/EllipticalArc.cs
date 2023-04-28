using System;
using System.Linq;
using Newtonsoft.Json;

namespace Elements.Geometry
{
    /// <summary>
    /// An elliptical arc.
    /// </summary>
    public class EllipticalArc : TrimmedCurve<Ellipse>
    {
        /// <summary>
        /// The domain of the curve.
        /// </summary>
        [JsonIgnore]
        public override Domain1d Domain => new Domain1d(Units.DegreesToRadians(this.StartAngle), Units.DegreesToRadians(this.EndAngle));

        /// <summary>The angle from 0.0, in degrees, at which the arc will start with respect to the positive X axis.</summary>
        [JsonProperty("StartAngle", Required = Required.Always)]
        [System.ComponentModel.DataAnnotations.Range(0.0D, 360.0D)]
        public double StartAngle { get; protected set; }

        /// <summary>The angle from 0.0, in degrees, at which the arc will end with respect to the positive X axis.</summary>
        [JsonProperty("EndAngle", Required = Required.Always)]
        [System.ComponentModel.DataAnnotations.Range(0.0D, 360.0D)]
        public double EndAngle { get; protected set; }

        /// <summary>
        /// Create an elliptical arc.
        /// </summary>
        /// <param name="majorAxis">The major axis (X) of the ellipse.</param>
        /// <param name="minorAxis">The minor axis (Y) of the ellipse.</param>
        /// <param name="center">The center of the ellipse.</param>
        /// <param name="startAngle">The start parameter of the trim.</param>
        /// <param name="endAngle">The end parameter of the trim.</param>
        [JsonConstructor]
        public EllipticalArc(Vector3 center,
                             double majorAxis,
                             double minorAxis,
                             double startAngle,
                             double endAngle)
        {
            this.BasisCurve = new Ellipse(new Transform(center), majorAxis, minorAxis);
            this.StartAngle = startAngle;
            this.EndAngle = endAngle;
            this.Start = this.PointAt(this.Domain.Min);
            this.End = this.PointAt(this.Domain.Max);
        }

        /// <summary>
        /// Create an elliptical arc.
        /// </summary>
        /// <param name="ellipse">The ellipse on which this trim is based.</param>
        /// <param name="startAngle">The start angle of the trim in degrees.</param>
        /// <param name="endAngle">The end parameter of the trim in degrees.</param>
        public EllipticalArc(Ellipse ellipse,
                             double @startAngle,
                             double @endAngle)
        {
            this.BasisCurve = ellipse;
            this.StartAngle = startAngle;
            this.EndAngle = endAngle;
            this.Start = this.PointAt(this.Domain.Min);
            this.End = this.PointAt(this.Domain.Max);
        }

        /// <summary>
        /// The bounds of the elliptical arc.
        /// </summary>
        /// <returns></returns>
        public override BBox3 Bounds()
        {
            return new BBox3(GetSubdivisionParameters().Select(p => PointAt(p)).ToList());
        }

        /// <summary>
        /// Calculate the length of the elliptical arc.
        /// </summary>
        /// <returns>The length of the elliptical arc.</returns>
        public override double Length()
        {
            return this.BasisCurve.ArcLength(this.Domain.Min, this.Domain.Max);
        }

        /// <summary>
        /// Calculate the length of the elliptical arc between start and end parameters.
        /// </summary>
        /// <returns>The length of the elliptical arc between start and end.</returns>
        public override double Length(double start, double end)
        {
            if (!Domain.Includes(start, true))
            {
                throw new ArgumentOutOfRangeException("start", $"The start parameter {start} must be between {Domain.Min} and {Domain.Max}.");
            }
            if (!Domain.Includes(end, true))
            {
                throw new ArgumentOutOfRangeException("end", $"The end parameter {end} must be between {Domain.Min} and {Domain.Max}.");
            }

            return this.BasisCurve.ArcLength(start, end);
        }

        /// <summary>
        /// Get a point at a parameter on the elliptical arc.
        /// </summary>
        /// <param name="u">The parameter at which to find a point.</param>
        /// <returns>A point.</returns>
        public override Vector3 PointAt(double u)
        {
            if (!this.Domain.Includes(u, true))
            {
                throw new ArgumentOutOfRangeException($"The parameter {u} is not on the trimmed portion of the basis curve. The parameter must be between {Domain.Min} and {Domain.Max}.");
            }
            return this.BasisCurve.PointAt(u);
        }

        /// <summary>
        /// Get a transform at a parameter on the elliptical arc.
        /// </summary>
        /// <param name="u">The parameter at which to find a transform.</param>
        /// <returns>A transform.</returns>
        public override Transform TransformAt(double u)
        {
            if (!this.Domain.Includes(u, true))
            {
                throw new ArgumentException($"The parameter {u} is not on the trimmed portion of the basis curve. The parameter must be between {Domain.Min} and {Domain.Max}.");
            }
            return this.BasisCurve.TransformAt(u);
        }

        /// <summary>
        /// Get a curve that is the result of applying the provided transform to this curve.
        /// </summary>
        /// <param name="transform">The transform to apply.</param>
        /// <returns>A transformed curve.</returns>
        public override Curve Transformed(Transform transform)
        {
            return new EllipticalArc((Ellipse)this.BasisCurve.Transformed(transform), this.Domain.Min, this.Domain.Max);
        }

        /// <summary>
        /// Get parameters to be used to find points along the curve for visualization.
        /// </summary>
        /// <param name="startSetbackDistance">An optional setback from the start of the curve.</param>
        /// <param name="endSetbackDistance">An optional setback from the end of the curve.</param>
        /// <returns>A collection of parameter values.</returns>
        public override double[] GetSubdivisionParameters(double startSetbackDistance = 0.0,
                                                       double endSetbackDistance = 0.0)
        {
            var min = ParameterAtDistanceFromParameter(startSetbackDistance, this.Domain.Min);
            var max = ParameterAtDistanceFromParameter(this.Length() - endSetbackDistance, this.Domain.Min);

            var flip = max < min;

            if (flip)
            {
                max = this.Domain.Min;
                min = this.Domain.Max;
            }

            // TODO: Getting points at equal lengths along the curve
            // requires the use of an elliptic integral and solving with
            // newton's method to find exactly the right values. For now,
            // we'll do a very simple subdivision of the arc.
            var div = 50;
            var parameters = new double[div + 1];
            var step = (max - min) / div;
            for (var i = 0; i <= div; i++)
            {
                parameters[i] = min + i * step;
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
            if (!Domain.Includes(start, true))
            {
                throw new ArgumentException($"The parameter {start} is not on the trimmed portion of the basis curve. The parameter must be between {Domain.Min} and {Domain.Max}.");
            }

            if (distance == 0.0)
            {
                return start;
            }

            var l = this.Length();
            if (distance < 0 || distance > l)
            {
                throw new ArgumentOutOfRangeException(nameof(distance), $"Distance must be between 0 and the curve length, {l}.");
            }

            this.BasisCurve.ArcLengthUntil(start, this.Domain.Max, distance, out var end);
            return end;
        }
    }
}