using MathNet.Numerics.Integration;
using System;
using System.Linq;

namespace Elements.Geometry
{
    /// <summary>
    /// An elliptical arc.
    /// </summary>
    public class EllipticalArc : TrimmedCurve<Ellipse>
    {
        /// <summary>
        /// Create an elliptical arc.
        /// </summary>
        /// <param name="majorAxis">The major axis (X) of the ellipse.</param>
        /// <param name="minorAxis">The minor axis (Y) of the ellipse.</param>
        /// <param name="center">The center of the ellipse.</param>
        /// <param name="startAngle">The start parameter of the trim.</param>
        /// <param name="endAngle">The end parameter of the trim.</param>
        public EllipticalArc(Vector3 center, double majorAxis, double minorAxis, double startAngle, double endAngle)
        {
            this.BasisCurve = new Ellipse(new Transform(center), majorAxis, minorAxis);
            this.Domain = new Domain1d(Units.DegreesToRadians(startAngle), Units.DegreesToRadians(endAngle));
            this.Start = this.PointAt(this.Domain.Min);
            this.End = this.PointAt(this.Domain.Max);
        }

        /// <summary>
        /// Create an elliptical arc.
        /// </summary>
        /// <param name="ellipse">The ellipse on which this trim is based.</param>
        /// <param name="startAngle">The start angle of the trim in degrees.</param>
        /// <param name="endAngle">The end parameter of the trim in degrees.</param>
        public EllipticalArc(Ellipse ellipse, double @startAngle, double @endAngle)
        {
            this.BasisCurve = ellipse;
            this.Domain = new Domain1d(Units.DegreesToRadians(startAngle), Units.DegreesToRadians(endAngle));
            this.Start = this.PointAt(this.Domain.Min);
            this.End = this.PointAt(this.Domain.Max);
        }

        /// <summary>
        /// The bounds of the elliptical arc.
        /// </summary>
        /// <returns></returns>
        public override BBox3 Bounds()
        {
            return new BBox3(GetSampleParameters().Select(p=>PointAt(p)).ToList());
        }

        /// <summary>
        /// Calculate the length of the elliptical arc.
        /// </summary>
        /// <returns>The length of the elliptical arc.</returns>
        public override double Length()
        {
            // sqrt(dx^2 + dy^2) <- tiny unit of arc length
            // y = sqrt[b^2(1 - x^2/a^2)] <- from formula for an ellipse
            // dy/dx = +/- (b/a^2) * sqrt[a^2 - x^2]

            var a = this.BasisCurve.MajorAxis;
            var b = this.BasisCurve.MinorAxis;
            var length = NewtonCotesTrapeziumRule.IntegrateAdaptive(x => (b/Math.Pow(a,2)) * Math.Sqrt(Math.Pow(a,2) - Math.Pow(x,2)), this.Domain.Min, this.Domain.Max, 1e-5);
            return length;
        }

        /// <summary>
        /// Get a point at a parameter on the elliptical arc.
        /// </summary>
        /// <param name="u">The parameter at which to find a point.</param>
        /// <returns>A point.</returns>
        public override Vector3 PointAt(double u)
        {
            if(!this.Domain.Includes(u, true))
            {
                throw new Exception($"The parameter {u} is not on the trimmed portion of the basis curve. The parameter must be between {Domain.Min} and {Domain.Max}.");
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
            if(!this.Domain.Includes(u, true))
            {
                throw new Exception($"The parameter {u} is not on the trimmed portion of the basis curve. The parameter must be between {Domain.Min} and {Domain.Max}.");
            }
            return this.BasisCurve.TransformAt(u);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="transform"></param>
        /// <returns></returns>
        public override Curve Transformed(Transform transform)
        {
            return new EllipticalArc((Ellipse)this.BasisCurve.Transformed(transform), this.Domain.Min, this.Domain.Max);
        }

        internal override double[] GetSampleParameters(double startSetbackDistance = 0.0,
                                                       double endSetbackDistance = 0.0)
        {
            var min = this.Domain.Min;
            var max = this.Domain.Max;

            var flip = max < min;

            if(flip)
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
    }
}