using Newtonsoft.Json;

namespace Elements.Geometry
{
    /// <summary>
    /// An arc with a start angle of 0 (+X) and 
    /// an end angle of 360.0.
    /// </summary>
    public class Circle : Arc
    {
        /// <summary>
        /// Construct a circle.
        /// </summary>
        /// <param name="center">The center of the circle.</param>
        /// <param name="radius">The radius of the circle.</param>
        [JsonConstructor]
        public Circle(Vector3 center, double radius = 1.0) : base(center, radius, 0.0, 360.0) { }

        /// <summary>
        /// Construct a circle.
        /// </summary>
        /// <param name="radius">The radius of the circle.</param>
        public Circle(double radius = 1.0) : base(Vector3.Origin, radius, 0.0, 360.0) { }

        /// <summary>
        /// Create a polygon through a set of points along the arc.
        /// </summary>
        /// <param name="divisions">The number of divisions of the arc.</param>
        /// <returns>A polygon.</returns>
        public Polygon ToPolygon(int divisions = 10)
        {
            var pts = new Vector3[divisions];
            for (int i = 0; i < divisions; i++)
            {
                pts[i] = this.PointAt((double)i / (double)divisions);
            }
            return new Polygon(pts, true);
        }
    }
}