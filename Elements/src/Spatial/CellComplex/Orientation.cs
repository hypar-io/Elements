using Elements.Geometry;

namespace Elements.Spatial.CellComplex
{
    /// <summary>
    /// A uniqueorientation direction in a cell complex
    /// </summary>
    public class Orientation : Vertex
    {
        /// <summary>
        /// Represents a uniqueorientation direction within a CellComplex.
        /// Is not intended to be created or modified outside of the CellComplex class code.
        /// </summary>
        /// <param name="cellComplex">CellComplex that this belongs to</param>
        /// <param name="id"></param>
        /// <param name="point">The orientation direction</param>
        /// <param name="name">Optional name</param>
        /// <returns></returns>
        internal Orientation(CellComplex cellComplex, ulong id, Vector3 orientation, string name = null) : base(cellComplex, id, orientation, name) { }
    }
}