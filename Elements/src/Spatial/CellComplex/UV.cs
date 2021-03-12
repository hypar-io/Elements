using Elements.Geometry;

namespace Elements.Spatial.CellComplex
{
    /// <summary>
    /// A unique U or V direction in a cell complex
    /// </summary>
    public class UV : Vertex
    {
        /// <summary>
        /// Represents a unique U or V direction within a CellComplex.
        /// Is not intended to be created or modified outside of the CellComplex class code.
        /// </summary>
        /// <param name="cellComplex">CellComplex that this belongs to</param>
        /// <param name="id"></param>
        /// <param name="point">The U or V direction</param>
        /// <param name="name">Optional name</param>
        /// <returns></returns>
        internal UV(CellComplex cellComplex, long id, Vector3 point, string name = null) : base(cellComplex, id, point, name) { }
    }
}