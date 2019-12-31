using Elements.Geometry.Interfaces;
using System;
using Newtonsoft.Json;
using System.Collections.Generic;

namespace Elements.Geometry
{
    /// <summary>
    /// An arc defined as a CCW rotation from the +X axis around a center between a start angle and an end angle.
    /// </summary>
    public partial class Arc : ICurve, IEquatable<Arc>
    {
        /// <summary>
        /// Calculate the length of the arc.
        /// </summary>
        public override double Length()
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
        /// Return the point at parameter u on the arc.
        /// </summary>
        /// <param name="u">A parameter between 0.0 and 1.0.</param>
        /// <returns>A Vector3 representing the point along the arc.</returns>
        public override Vector3 PointAt(double u)
        {
            if (u > 1.0 || u < 0.0)
            {
                throw new ArgumentOutOfRangeException($"The value provided for parameter u, {u}, must be between 0.0 and 1.0.");
            }

            var angle = this.StartAngle + (this.EndAngle - this.StartAngle) * u;
            var theta = DegToRad(angle);
            var x = this.Center.X + this.Radius * Math.Cos(theta);
            var y = this.Center.Y + this.Radius * Math.Sin(theta);
            return new Vector3(x,y);
        }

        /// <summary>
        /// Return transform on the arc at parameter u.
        /// </summary>
        /// <param name="u">A parameter between 0.0 and 1.0 on the arc.</param>
        /// <returns>A transform with its origin at u along the curve and its Z axis tangent to the curve.</returns>
        public override Transform TransformAt(double u)
        {
            var p = PointAt(u);
            var x = (p-this.Center).Normalized();
            var y = Vector3.ZAxis;
            return new Transform(p, x, x.Cross(y));
        }

        /// <summary>
        /// Get a collection of transforms which represent frames along the arc.
        /// </summary>
        /// <param name="startSetback">The parameter offset from the start of the arc. Between 0 and 1.</param>
        /// <param name="endSetback">The parameter offset from the end of the arc. Between 0 and 1.</param>
        /// <returns>A collection of transforms.</returns>
        public override Transform[] Frames(double startSetback, double endSetback)
        {
            var div = 10;
            var l = this.Length();
            var step = (1.0 - startSetback - endSetback)  / div;
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
                    t = TransformAt(startSetback + i * step);
                }
                result[i] = t;
            }
            return result;
        }

        /// <summary>
        /// Get an arc which is the reverse of this Arc.
        /// </summary>
        public Arc Reversed()
        {
            return new Arc(this.Center, this.Radius, this.EndAngle, this.StartAngle);
        }

        private double DegToRad(double degrees)
        {
            return degrees * Math.PI / 180.0;
        }

        /// <summary>
        /// Get a bounding box for this arc.
        /// </summary>
        /// <returns>A bounding box for this arc.</returns>
        public override BBox3 Bounds()
        {
            var delta = new Vector3(this.Radius, this.Radius, this.Radius);
            var min = new Vector3(this.Center - delta);
            var max = new Vector3(this.Center + delta);
            return new BBox3(min, max);
        }

        /// <summary>
        /// Compute the plane of the arc.
        /// </summary>
        /// <returns>The plane in which the arc lies.</returns>
        public Plane Plane()
        {
            return new Plane(this.PointAt(0.0), this.PointAt(1.0), this.Center);
        }

        /// <summary>
        /// A list of vertices describing the arc for rendering.
        /// </summary>
        internal override IList<Vector3> RenderVertices()
        {
            var div = 10;
            var vertices = new Vector3[div];
            for (var i = 0; i < div; i++)
            {
                var t = i * (1.0/div);
                vertices[i] = PointAt(t);
            }
            return vertices;
        }

        /// <summary>
        /// Is this arc equal to the provided arc?
        /// </summary>
        /// <param name="other">The arc to test.</param>
        /// <returns>Returns true if the two arcs are equal, otherwise false.</returns>
        public bool Equals(Arc other)
        {
            if(other == null)
            {
                return false;
            }
            return this.Center.Equals(other.Center) && this.StartAngle == other.StartAngle && this.EndAngle == other.EndAngle;
        }
    }
}