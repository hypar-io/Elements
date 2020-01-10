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
        public Circle(Vector3 center, double radius = 1.0) : base(center, radius, 0.0, 360.0){}
    }
}