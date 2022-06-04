using Elements.Geometry;

namespace Elements.Annotations
{
    /// <summary>
    /// A dimension.
    /// </summary>
    public abstract class Annotation : Element
    {
        /// <summary>
        /// The plane in which the dimension is drawn.
        /// </summary>
        public Plane Plane { get; protected set; } = new Plane(Vector3.Origin, Vector3.ZAxis);

        /// <summary>
        /// Text to be displayed in place of the dimension's value.
        /// </summary>
        public string DisplayValue { get; set; }

        /// <summary>
        /// Text to be displayed before the dimension's value.
        /// </summary>
        public string Prefix { get; set; }

        /// <summary>
        /// Text to be displayed after the dimension's value.
        /// </summary>
        public string Suffix { get; set; }

        /// <summary>
        /// Create a dimension.
        /// </summary>
        public Annotation() { }
    }
}