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
        public Plane Plane { get; protected set; } = new Plane(Vector3.Origin, Vector3.ZAxis);

        /// <summary>
        /// If this property is not null, the display value will 
        /// show instead of the computed value.
        /// </summary>
        public string DisplayValue { get; set; }

        /// <summary>
        /// Create a dimension.
        /// </summary>
        public Dimension() { }
    }
}