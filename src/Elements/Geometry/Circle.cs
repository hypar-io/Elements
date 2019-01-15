using Elements.Geometry.Interfaces;
using Newtonsoft.Json;
using System;

namespace Elements.Geometry
{
    /// <summary>
    /// A Circle defined by its center and radius.
    /// </summary>
    public class Circle : ICurve
    {
        [JsonProperty("radius")]
        double Radius{get;}

        [JsonProperty("center")]
        Vector3 Center{get;}

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
        /// The start of the Circle.
        /// </summary>
        [JsonIgnore]
        public Vector3 Start
        {
            get
            {
                return new Vector3(this.Center.X + this.Radius, this.Center.Y);
            }
        }

        /// <summary>
        /// The end of the Circle.
        /// </summary>
        [JsonIgnore]
        public Vector3 End
        {
            get
            {
                return new Vector3(this.Center.X + this.Radius, this.Center.Y);
            }
        }

        /// <summary>
        /// An array of vertices which tesselate the Circle.
        /// </summary>
        [JsonIgnore]
        public Vector3[] Vertices
        {
            get
            {
                return Polygon.Circle(this.Radius, 10).Vertices;
            }
        }

        /// <summary>
        /// Construct a Circle.
        /// </summary>
        /// <param name="center">The center of the Circle.</param>
        /// <param name="radius">The radius of the Circle.</param>
        public Circle(Vector3 center, double radius)
        {
            if (radius <= 0)
            {
                throw new ArgumentOutOfRangeException("Could not construct a Circle. The radius must be greater than 0.0.");
            }
            this.Center = center;
            this.Radius = radius;
        }
        
        /// <summary>
        /// The circumference of the Circle.
        /// </summary>
        public double Length()
        {
            return 2 * Math.PI * this.Radius;
        }

        /// <summary>
        /// Get a point along the Circle.
        /// </summary>
        /// <param name="u">The parameter along the Circle at which to find the point.</param>
        public Vector3 PointAt(double u)
        {
            var theta = Math.PI * 2 * u;
            return new Vector3(this.Radius * Math.Cos(theta), this.Radius * Math.Sin(theta));
        }

        /// <summary>
        /// Get a Transform along the Circle.
        /// </summary>
        /// <param name="u">The parameter along the circle at which to get the Transform.</param>
        /// <param name="up"></param>
        /// <returns></returns>
        public Transform TransformAt(double u, Vector3 up = null)
        {
            var o = PointAt(u);
            var x = (this.Center - o).Normalized();
            up = up != null ? up : Vector3.ZAxis;
            var y = up.Cross(x).Normalized();
            var z = x.Cross(y).Normalized();
            return new Transform(o , x, z);
        }

        public Transform[] Frames(double startSetback, double endSetback)
        {
            throw new System.NotImplementedException();
        }

        public ICurve Reversed()
        {
            throw new System.NotImplementedException();
        }
    }
}