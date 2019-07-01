namespace Elements.Geometry.Interfaces
{
    /// <summary>
    /// Extrudes a closed profile in a direction to create a solid.
    /// </summary>
    public interface IExtrude : IProfile, ISolid
    {
        /// <summary>
        /// The direction of the extrusion.
        /// </summary>
        Vector3 ExtrudeDirection { get; }

        /// <summary>
        /// The depth of the extrusion.
        /// </summary>
        double ExtrudeDepth { get; }

        /// <summary>
        /// Extrude to both sides?
        /// </summary>
        bool BothSides{get;}
    }
}