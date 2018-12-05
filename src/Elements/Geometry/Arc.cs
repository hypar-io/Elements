using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;

namespace Elements.Geometry
{
    /// <summary>
    /// Arc represents an arc defined between a start angle and an end angle.
    /// </summary>
    public class Arc : ICurve
    {
        /// <summary>
        /// The center of the Arc.
        /// </summary>
        /// <value></value>
        [JsonProperty("center")]
        public Vector3 Center { get; }

        /// <summary>
        /// The angle from 0.0, in degrees, at which the Arc will start.
        /// </summary>
        /// <value></value>
        [JsonProperty("start_angle")]
        public double StartAngle { get; }

        /// <summary>
        /// The angle from 0.0, in degrees, at which the Arc will end.
        /// </summary>
        /// <value></value>
        [JsonProperty("end_angle")]
        public double EndAngle { get; }

        /// <summary>
        /// Calculate the length of the Arc.
        /// </summary>
        public double Length()
        {
            return 2*Math.PI*this.Radius * (Math.Abs(this.EndAngle-this.StartAngle))/360.0;
        }

        /// <summary>
        /// The start point of the Arc.
        /// </summary>
        [JsonIgnore]
        public Vector3 Start
        {
            get{return PointAt(0.0);}
        }

        /// <summary>
        /// The end point of the Arc.
        /// </summary>
        [JsonIgnore]
        public Vector3 End
        {
            get{return PointAt(1.0);}
        }

        /// <summary>
        /// The vertices of the Arc.
        /// </summary>
        [JsonIgnore]
        public IList<Vector3> Vertices
        {
            get
            {
                var verts = new Vector3[11];
                var count = 0;
                for(var u=0.0; u<1.0; u+=1.0/10.0)
                {
                    verts[count] = PointAt(u);
                    count++;
                }
                return verts;
            }
        }

        /// <summary>
        /// The radius of the Arc.
        /// </summary>
        [JsonProperty("radius")]
        public double Radius{get;}

        /// <summary>
        /// An Arc.
        /// </summary>
        /// <param name="center">The center of the Arc.</param>
        /// <param name="radius">The radius of the Arc.</param>
        /// <param name="startAngle">The start angle of the Arc in degrees.</param>
        /// <param name="endAngle">The end angle of the Arc in degrees.</param>
        public Arc(Vector3 center, double radius, double startAngle, double endAngle)
        {
            if(endAngle > 360.0 || startAngle > 360.00)
            {
                throw new ArgumentOutOfRangeException("The start and end angles must be greater than -360.0");
            }

            if(endAngle == startAngle)
            {
                throw new ArgumentException($"The start angle ({startAngle}) cannot be equal to the end angle ({endAngle}).");
            }

            if(radius <= 0.0)
            {
                throw new ArgumentOutOfRangeException($"The provided radius ({radius}) must be greater than 0.0.");
            }
        
            this.EndAngle = endAngle;
            this.StartAngle = startAngle;
            this.Center = center;
            this.Radius = radius;
        }

        /// <summary>
        /// Return the point at parameter u on the Arc.
        /// </summary>
        /// <param name="u">A parameter between 0.0 and 1.0.</param>
        /// <returns>A Vector3 representing the point along the Arc.</returns>
        public Vector3 PointAt(double u)
        {
            if(u > 1.0 || u < 0.0)
            {
                throw new ArgumentOutOfRangeException($"The value provided for parameter u ({u}) must be between 0.0 and 1.0.");
            }

            var angle = (this.EndAngle - this.StartAngle) * u;
            return new Vector3(this.Center.X + this.Radius * Math.Cos(angle * Math.PI/180), this.Center.Y + this.Radius * Math.Sin(angle * Math.PI/180));
        }

        /// <summary>
        /// Return transform on the Arc at parameter u.
        /// </summary>
        /// <param name="u">A parameter between 0.0 and 1.0 on the Arc.</param>
        /// <param name="up">An optional up parameter.</param>
        /// <returns>A Transform with its origin at u along the curve and its Z axis tangent to the curve.</returns>
        public Transform TransformAt(double u, Vector3 up = null)
        {
            var o = PointAt(u);
            var x = (o-this.Center).Normalized().Negated();
            var z = up != null ? up : Vector3.ZAxis;
            return new Transform(o, x, x.Cross(z));
        }

        /// <summary>
        /// Get a collection of Transforms which represent frames along this ICurve.
        /// </summary>
        /// <param name="startSetback">The offset from the start of the ICurve.</param>
        /// <param name="endSetback">The offset from the end of the ICurve.</param>
        /// <returns>A collection of Transforms.</returns>
        public Transform[] Frames(double startSetback, double endSetback)
        {
            var div = 10;
            var step = 1.0/div;
            var result = new Transform[div+1];
            for(var i = 0; i <= div ; i++)
            {
                Transform t;
                if(i == 0)
                {
                    t = TransformAt(0.0 + startSetback);
                }
                else if(i == div)
                {
                    t = TransformAt(1.0 - endSetback);
                }
                else
                {
                    t = TransformAt(i*step);
                }
                result[i] = t;
            }
            return result;
        }
    }
}