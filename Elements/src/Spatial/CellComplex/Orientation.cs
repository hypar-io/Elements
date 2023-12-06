using System.Text.Json.Serialization;
using Elements.Geometry;

namespace Elements.Spatial.CellComplex
{
    /// <summary>
    /// A unique orientation direction in a CellComplex.
    /// </summary>
    public class Orientation : VertexBase<Orientation>
    {
        /// <summary>
        /// Create an orientation.
        /// </summary>
        /// <param name="id"></param>
        /// <param name="value"></param>
        /// <param name="name"></param>
        /// <returns></returns>
        [JsonConstructor]
        public Orientation(ulong id, Vector3 value, string name = null) : base(id, value, name) { }

        /// <summary>
        /// Represents a unique orientation direction within a CellComplex.
        /// Is not intended to be created or modified outside of the CellComplex class code.
        /// </summary>
        /// <param name="cellComplex">CellComplex that this belongs to</param>
        /// <param name="id"></param>
        /// <param name="orientation">The orientation direction</param>
        /// <param name="name">Optional name</param>
        public Orientation(CellComplex cellComplex, ulong id, Vector3 orientation, string name = null) : base(cellComplex, id, orientation, name) { }

        /// <summary>
        /// Do not use this method: it just throws an exception.
        /// Orientations are relative directions and do not exist at any point in absolute space. A distance cannot be calculated for orientations.
        /// </summary>
        /// <param name="point"></param>
        /// <returns></returns>
        public override double DistanceTo(Vector3 point)
        {
            throw new System.Exception("Orientations are relative directions and do not exist at any point in absolute space. A distance cannot be calculated for orientations.");
        }
    }
}