using Elements.Geometry;

namespace Elements.Dimensions
{
    /// <summary>
    /// A dimension.
    /// </summary>
    public abstract class Dimension : Element
    {
        /// <summary>
        /// The plane in which the dimension is drawn.
        /// </summary>
        public Plane Plane { get; set; }

        /// <summary>
        /// Create a dimension.
        /// </summary>
        public Dimension() { }
    }
}