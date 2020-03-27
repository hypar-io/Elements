using System;
namespace Elements.Geometry
{
    /// <summary>
    /// Represents the state of containment of a point relative to an enclosing polygon, profile, or volume
    /// </summary>
    public enum Containment
    {
        /// <summary>
        /// The point lies entirely outside.
        /// </summary>
        Outside,
        /// <summary>
        /// The point lies entirely inside.
        /// </summary>
        Inside,
        /// <summary>
        /// The point lies exactly or nearly at an edge.
        /// </summary>
        CoincidesAtEdge,
        /// <summary>
        /// The point lies exactly or nearly at a vertex.
        /// </summary>
        CoincidesAtVertex,
        /// <summary>
        /// The point lies exactly or nearly at a face.
        /// </summary>
        CoincidesAtFace // currently not in use
    }
}
