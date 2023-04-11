using Elements.Validators;
using System;
using Newtonsoft.Json;
using System.Collections.Generic;

namespace Elements.Geometry
{
    /// <summary>
    /// An arc defined as a CCW rotation from the +X axis around a center between a start angle and an end angle.
    /// </summary>
    /// <example>
    /// [!code-csharp[Main](../../Elements/test/ArcTests.cs?name=example)]
    /// </example>
    public partial class Arc : TrimmedCurve<Circle>, IEquatable<Arc>
    {
        /// <summary>The angle from 0.0, in degrees, at which the arc will start with respect to the positive X axis.</summary>
        [JsonProperty("StartAngle", Required = Required.Always)]
        [System.ComponentModel.DataAnnotations.Range(0.0D, 360.0D)]
        public double StartAngle { get; protected set; }

        /// <summary>The angle from 0.0, in degrees, at which the arc will end with respect to the positive X axis.</summary>
        [JsonProperty("EndAngle", Required = Required.Always)]
        [System.ComponentModel.DataAnnotations.Range(0.0D, 360.0D)]
        public double EndAngle { get; protected set; }

        /// <summary>
        /// The radius of the arc.
        /// </summary>
        public double Radius
        {
            get
            {
                return this.BasisCurve.Radius;
            }
        }

        /// <summary>
        /// The center of the arc.
        /// </summary>
        public Vector3 Center
        {
            get
            {
                return this.BasisCurve.Transform.Origin;
            }
        }

        /// <summary>
        /// The start point of the arc.
        /// </summary>
        [JsonIgnore]
        public override Vector3 Start
        {
            get { return PointAt(this.Domain.Min); }
        }

        /// <summary>
        /// The end point of the arc.
        /// </summary>
        [JsonIgnore]
        public override Vector3 End
        {
            get { return PointAt(this.Domain.Max); }
        }

        /// <summary>
        /// Create a circular arc.
        /// </summary>
        public Arc(double radius)
        {
            this.BasisCurve = new Circle();
            this.StartAngle = 0;
            this.EndAngle = 360;
            this.Domain = new Domain1d(Units.DegreesToRadians(this.StartAngle), Units.DegreesToRadians(this.EndAngle));
        }

        /// <summary>
        /// Create an arc.
        /// </summary>
        /// <param name="center">The center of the arc.</param>
        /// <param name="radius">The radius of the arc.</param>
        /// <param name="startAngle">The angle from 0.0, in degrees, at which the arc will start with respect to the positive X axis.</param>
        /// <param name="endAngle">The angle from 0.0, in degrees, at which the arc will end with respect to the positive X axis.</param>
        [JsonConstructor]
        public Arc(Vector3 @center, double @radius, double @startAngle, double @endAngle) : base()
        {
            Validate(startAngle, endAngle, radius);
            this.BasisCurve = new Circle(@center, @radius);
            this.StartAngle = @startAngle;
            this.EndAngle = @endAngle;
            this.Domain = new Domain1d(Units.DegreesToRadians(@startAngle), Units.DegreesToRadians(@endAngle));
        }

        private void Validate(double startAngle, double endAngle, double radius)
        {
            if (!Validator.DisableValidationOnConstruction)
            {
                var span = Math.Abs(endAngle - startAngle);
                if (span > 360)
                {
                    throw new ArgumentOutOfRangeException($"The total span of the arc, {span} degrees, is greater than 360.0. The arc cannot be created.");
                }

                if (endAngle == startAngle)
                {
                    throw new ArgumentException($"The arc could not be created. The start angle ({startAngle}) cannot be equal to the end angle ({endAngle}).");
                }

                if (radius <= 0.0)
                {
                    throw new ArgumentOutOfRangeException($"The arc could not be created. The provided radius ({radius}) must be greater than 0.0.");
                }
            }
        }

        /// <summary>
        /// Create an arc.
        /// Constructs a circular basis curve internally with a default transform.
        /// </summary>
        /// <param name="radius">The radius of the arc.</param>
        /// <param name="startAngle">The CCW angle from 0.0, in degrees, at which the arc will start with respect to the positive X axis.</param>
        /// <param name="endAngle">The CCW angle from 0.0, in degrees, at which the arc will end with respect to the positive X axis.</param>
        public Arc(double radius, double startAngle, double endAngle)
            : base()
        {
            Validate(startAngle, endAngle, radius);
            this.BasisCurve = new Circle(radius);
            this.StartAngle = startAngle;
            this.EndAngle = endAngle;
            this.Domain = new Domain1d(Units.DegreesToRadians(@startAngle), Units.DegreesToRadians(@endAngle));
        }

        /// <summary>
        /// Create an arc.
        /// </summary>
        /// <param name="circle">The circle on which this arc is based.</param>
        /// <param name="startParameter">The parameter, from 0.0->2PI, of the start of the arc.</param>
        /// <param name="endParameter">The parameter, from 0.0->2PI, of the end of the arc.</param>
        public Arc(Circle circle, double startParameter, double endParameter)
        {
            Validate(Units.RadiansToDegrees(startParameter), Units.RadiansToDegrees(endParameter), circle.Radius);
            this.BasisCurve = circle;
            this.Domain = new Domain1d(startParameter, endParameter);
            this.StartAngle = Units.RadiansToDegrees(this.Domain.Min);
            this.EndAngle = Units.RadiansToDegrees(this.Domain.Max);
        }

        /// <summary>
        /// Create an arc.
        /// Constructs a circular basis curve internally with the provided transform.
        /// </summary>
        /// <param name="transform">The transform for the basis curve of the arc.</param>
        /// <param name="radius">The radius of the arc.</param>
        /// <param name="startParameter">The parameter, from 0.0->2PI, of the start of the arc.</param>
        /// <param name="endParameter">The parameter, from 0.0->2PI, of the end of the arc.</param>
        public Arc(Transform transform,
                   double radius = 1.0,
                   double startParameter = 0.0,
                   double endParameter = Math.PI * 2)
        {
            Validate(Units.RadiansToDegrees(startParameter), Units.RadiansToDegrees(endParameter), radius);
            this.BasisCurve = new Circle(transform, radius);
            this.Domain = new Domain1d(startParameter, endParameter);
            this.StartAngle = Units.RadiansToDegrees(this.Domain.Min);
            this.EndAngle = Units.RadiansToDegrees(this.Domain.Max);
        }

        /// <summary>
        /// Calculate the length of the arc.
        /// </summary>
        public override double Length()
        {
            return 2 * Math.PI * this.BasisCurve.Radius * (Math.Abs(this.EndAngle - this.StartAngle)) / 360.0;
        }

        /// <summary>
        /// The mid point of the line.
        /// </summary>
        public override Vector3 Mid()
        {
            return PointAt(this.Domain.Min + this.Domain.Length / 2);
        }

        /// <summary>
        /// Get an arc which is the reverse of this Arc.
        /// </summary>
        public Arc Reversed()
        {
            return new Arc(this.BasisCurve.Transform.Origin, this.BasisCurve.Radius, this.EndAngle, this.StartAngle);
        }

        /// <summary>
        /// Get a bounding box for this arc.
        /// </summary>
        /// <returns>A bounding box for this arc.</returns>
        public override BBox3 Bounds()
        {
            var delta = new Vector3(this.BasisCurve.Radius, this.BasisCurve.Radius, this.BasisCurve.Radius);
            var min = new Vector3(this.BasisCurve.Transform.Origin - delta);
            var max = new Vector3(this.BasisCurve.Transform.Origin + delta);
            return new BBox3(min, max);
        }

        /// <summary>
        /// Compute the plane of the arc.
        /// </summary>
        /// <returns>The plane in which the arc lies.</returns>
        public Plane Plane()
        {
            return BasisCurve.Transform.XY();
        }

        internal override double[] GetSampleParameters(double startSetbackDistance = 0.0,
                                                       double endSetbackDistance = 0.0)
        {
            var min = this.Domain.Min;
            var max = this.Domain.Max;

            var flip = max < min;

            if (flip)
            {
                max = this.Domain.Min;
                min = this.Domain.Max;
            }

            var startParam = ParameterAtDistanceFromParameter(startSetbackDistance, min);
            var endParam = ParameterAtDistanceFromParameter(this.Length() - endSetbackDistance, min);

            // Parameter calculations.
            var angleSpan = endParam - startParam;

            // Angle span: t
            // d = 2 * r * sin(t/2)
            var r = this.BasisCurve.Radius;
            var two_r = 2 * r;
            var d = Math.Min(MinimumChordLength, two_r);
            var t = 2 * Math.Asin(d / two_r);
            var div = (int)Math.Ceiling(angleSpan / t);

            var parameters = new double[div + 1];
            var step = angleSpan / div;
            for (var i = 0; i <= div; i++)
            {
                parameters[i] = startParam + i * step;
            }
            return parameters;
        }

        /// <summary>
        /// Is this arc equal to the provided arc?
        /// </summary>
        /// <param name="other">The arc to test.</param>
        /// <returns>Returns true if the two arcs are equal, otherwise false.</returns>
        public bool Equals(Arc other)
        {
            if (other == null)
            {
                return false;
            }
            return this.BasisCurve.Transform.Origin.Equals(other.BasisCurve.Transform.Origin) && this.StartAngle == other.StartAngle && this.EndAngle == other.EndAngle;
        }

        /// <summary>
        /// Return the arc which is the complement of this arc.
        /// </summary>
        public Arc Complement()
        {
            var complementSpan = 360.0 - (this.EndAngle - this.StartAngle);
            var newEnd = this.StartAngle;
            var newStart = this.EndAngle;
            if (newStart > newEnd)
            {
                newStart = newStart - 360.0;
            }
            return new Arc(this.BasisCurve.Transform.Origin, this.BasisCurve.Radius, newStart, newEnd);
        }

        /// <summary>
        /// Construct a transformed copy of this Curve.
        /// </summary>
        /// <param name="transform">The transform to apply.</param>
        public override Curve Transformed(Transform transform)
        {
            return TransformedArc(transform);
        }

        /// <summary>
        /// Construct a transformed copy of this Arc.
        /// </summary>
        /// <param name="transform">The transform to apply.</param>
        public Arc TransformedArc(Transform transform)
        {
            return new Arc(transform.OfPoint(this.BasisCurve.Transform.Origin), this.BasisCurve.Radius, StartAngle, EndAngle);
        }

        /// <summary>
        /// Get the point at parameter u.
        /// </summary>
        /// <returns>The point at parameter u if us is within the trim, otherwise an exception is thrown.</returns>
        public override Vector3 PointAt(double u)
        {
            if (!this.Domain.Includes(u, true))
            {
                throw new Exception($"The parameter {u} is not on the trimmed portion of the basis curve. The parameter must be between {Domain.Min} and {Domain.Max}.");
            }
            return this.BasisCurve.PointAt(u);
        }

        /// <summary>
        /// Get the transform at parameter u.
        /// </summary>
        /// <returns>The transform at parameter u if us is within the trim, otherwise an exception is thrown.</returns>
        public override Transform TransformAt(double u)
        {
            if (!this.Domain.Includes(u, true))
            {
                throw new Exception($"The parameter {u} is not on the trimmed portion of the basis curve. The parameter must be between {Domain.Min} and {Domain.Max}.");
            }
            return this.BasisCurve.TransformAt(u);
        }

        /// <summary>
        /// Create a polyline through a set of points along the curve.
        /// </summary>
        /// <param name="divisions">The number of divisions of the curve.</param>
        /// <returns>A polyline.</returns>
        public override Polyline ToPolyline(int divisions = 10)
        {
            var pts = new List<Vector3>(divisions + 1);
            var step = this.Domain.Length / divisions;
            for (var t = this.Domain.Min; t < this.Domain.Max; t += step)
            {
                pts.Add(PointAt(t));
            }

            // We don't go all the way to the end parameter, and 
            // add it here explicitly because rounding errors can
            // cause small imprecision which accumulates to make
            // the final parameter slightly more/less than the actual
            // end parameter.
            pts.Add(PointAt(this.Domain.Max));
            return new Polyline(pts);
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
                throw new Exception($"The parameter {start} is not on the trimmed portion of the basis curve. The parameter must be between {Domain.Min} and {Domain.Max}.");
            }

            if (distance == 0.0)
            {
                return start;
            }

            return this.BasisCurve.ParameterAtDistanceFromParameter(distance, start);
        }
    }
}