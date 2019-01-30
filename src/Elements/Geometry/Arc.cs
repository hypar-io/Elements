using Elements.Geometry.Interfaces;
using System;
using Newtonsoft.Json;

namespace Elements.Geometry
{
    /// <summary>
    /// Arc represents an arc defined between a start angle and an end angle.
    /// </summary>
    public class Arc : ICurve
    {
        private Transform _transform;

        /// <summary>
        /// The type of the curve.
        /// Used during deserialization to disambiguate derived types.
        /// </summary>
        [JsonProperty("type", Order = -100)]
        public string Type
        {
            get { return this.GetType().FullName.ToLower(); }
        }
        
        /// <summary>
        /// The plane of the arc.
        /// </summary>
        /// <value></value>
        [JsonProperty("plane")]
        public Plane Plane{get;}

        /// <summary>
        /// The angle from 0.0, in degrees, at which the arc will start with respect to the positive X axis.
        /// </summary>
        [JsonProperty("start_angle")]
        public double StartAngle { get; internal set; }

        /// <summary>
        /// The angle from 0.0, in degrees, at which the arc will end with respect to the positive X axis.
        /// </summary>
        [JsonProperty("end_angle")]
        public double EndAngle { get; internal set; }

        /// <summary>
        /// Calculate the length of the arc.
        /// </summary>
        public double Length()
        {
            return 2 * Math.PI * this.Radius * (Math.Abs(this.EndAngle - this.StartAngle)) / 360.0;
        }

        /// <summary>
        /// The start point of the arc.
        /// </summary>
        [JsonIgnore]
        public Vector3 Start
        {
            get { return PointAt(0.0); }
        }

        /// <summary>
        /// The end point of the arc.
        /// </summary>
        [JsonIgnore]
        public Vector3 End
        {
            get { return PointAt(1.0); }
        }

        /// <summary>
        /// The radius of the Arc.
        /// </summary>
        [JsonProperty("radius")]
        public double Radius { get; }

        /// <summary>
        /// Create a plane.
        /// </summary>
        /// <param name="center">The center of the arc.</param>
        /// <param name="radius">The radius of the arc.</param>
        /// <param name="startAngle">The start angle of the arc in degrees.</param>
        /// <param name="endAngle">The end angle of the arc in degrees.</param>
        [JsonConstructor]
        public Arc(Vector3 center, double radius, double startAngle, double endAngle)
        {
            if (endAngle > 360.0 || startAngle > 360.00)
            {
                throw new ArgumentOutOfRangeException("The arc could not be created. The start and end angles must be greater than -360.0");
            }

            if (endAngle == startAngle)
            {
                throw new ArgumentException($"The arc could not be created. The start angle ({startAngle}) cannot be equal to the end angle ({endAngle}).");
            }

            if (radius <= 0.0)
            {
                throw new ArgumentOutOfRangeException($"The arc could not be created. The provided radius ({radius}) must be greater than 0.0.");
            }

            this.EndAngle = endAngle;
            this.StartAngle = startAngle;
            this.Radius = radius;
            this._transform = new Transform(center);
            this.Plane = this._transform.XY;
        }

        /// <summary>
        /// Create a plane.
        /// </summary>
        /// <param name="plane">The plane of the arc.</param>
        /// <param name="radius">The radius of the arc.</param>
        /// <param name="startAngle">The start angle of the arc in degrees.</param>
        /// <param name="endAngle">The end angle of the arc in degrees.</param>
        [JsonConstructor]
        public Arc(Plane plane, double radius, double startAngle, double endAngle)
        {
            if (endAngle > 360.0 || startAngle > 360.00)
            {
                throw new ArgumentOutOfRangeException("The arc could not be created. The start and end angles must be greater than -360.0");
            }

            if (endAngle == startAngle)
            {
                throw new ArgumentException($"The arc could not be created. The start angle ({startAngle}) cannot be equal to the end angle ({endAngle}).");
            }

            if (radius <= 0.0)
            {
                throw new ArgumentOutOfRangeException($"The arc could not be created. The provided radius ({radius}) must be greater than 0.0.");
            }

            this.EndAngle = endAngle;
            this.StartAngle = startAngle;
            this.Radius = radius;
            this._transform = new Transform(plane.Origin, plane.Normal.Normalized());
            this.Plane = plane;
        }

        /// <summary>
        /// Return the point at parameter u on the arc.
        /// </summary>
        /// <param name="u">A parameter between 0.0 and 1.0.</param>
        /// <returns>A Vector3 representing the point along the arc.</returns>
        public Vector3 PointAt(double u)
        {
            if (u > 1.0 || u < 0.0)
            {
                throw new ArgumentOutOfRangeException($"The value provided for parameter u, {u}, must be between 0.0 and 1.0.");
            }

            var angle = this.StartAngle + (this.EndAngle - this.StartAngle) * u;
            var theta = DegToRad(angle);
            var x = this.Plane.Origin.X + this.Radius * Math.Cos(theta);
            var y = this.Plane.Origin.Y + this.Radius * Math.Sin(theta);
            return this._transform.OfVector(new Vector3(x, y));
        }

        /// <summary>
        /// Return transform on the arc at parameter u.
        /// </summary>
        /// <param name="u">A parameter between 0.0 and 1.0 on the arc.</param>
        /// <returns>A transform with its origin at u along the curve and its Z axis tangent to the curve.</returns>
        public Transform TransformAt(double u)
        {
            var o = PointAt(u);
            var x = (o - this.Plane.Origin).Normalized();
            return new Transform(o, x, this.Plane.Normal);
        }

        /// <summary>
        /// Get a collection of Transforms which represent frames along the arc.
        /// </summary>
        /// <param name="startSetback">The offset from the start of the arc.</param>
        /// <param name="endSetback">The offset from the end of the arc.</param>
        /// <returns>A collection of Transforms.</returns>
        public Transform[] Frames(double startSetback, double endSetback)
        {
            var div = 10;
            var step = 1.0 / div;
            var result = new Transform[div + 1];
            for (var i = 0; i <= div; i++)
            {
                Transform t;
                if (i == 0)
                {
                    t = TransformAt(0.0 + startSetback);
                }
                else if (i == div)
                {
                    t = TransformAt(1.0 - endSetback);
                }
                else
                {
                    t = TransformAt(i * step);
                }
                result[i] = t;
            }
            return result;
        }

        /// <summary>
        /// Get an arc which is the reverse of this Arc.
        /// </summary>
        public ICurve Reversed()
        {
            return new Arc(this.Plane, this.Radius, this.EndAngle, this.StartAngle);
        }

        private double DegToRad(double degrees)
        {
            return degrees * Math.PI / 180.0;
        }

        /// <summary>
        /// Get a bounding box for this arc.
        /// </summary>
        /// <returns>A bounding box for this arc.</returns>
        public BBox3 Bounds()
        {
            var delta = new Vector3(this.Radius, this.Radius, this.Radius);
            var min = new Vector3(this.Plane.Origin - delta);
            var max = new Vector3(this.Plane.Origin + delta);
            return new BBox3(min, max);
        }
    }
}