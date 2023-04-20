namespace Elements.Geometry.Interfaces
{
    /// <summary>
    /// A conic section.
    /// </summary>
    public interface IConic
    {
        /// <summary>
        /// The coordinate system that defines the orientation of the conic section.
        /// </summary>
        Transform Transform { get; }
    }
}