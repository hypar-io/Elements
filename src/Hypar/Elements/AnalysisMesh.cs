using Hypar.Geometry;

namespace Hypar.Elements
{
    /// <summary>
    /// AnalysisMesh represents a mesh of a fixed size and number of subivisions to which can
    /// be applied vertex colors based on a value at a vertex.
    /// </summary>
    public class AnalysisMesh : ITessellate<Mesh>
    {
        private Vector3 _origin;
        private double _xDim;
        private double _yDim;
        private int _xDiv;
        private int _yDiv;

        /// <summary>
        /// Construct an analysis mesh.
        /// </summary>
        /// <param name="origin"></param>
        /// <param name="xDim"></param>
        /// <param name="yDim"></param>
        /// <param name="xDiv"></param>
        /// <param name="yDiv"></param>
        public AnalysisMesh(Vector3 origin, double xDim, double yDim, int xDiv, int yDiv)
        {
            this._origin = origin;
            this._xDim = xDim;
            this._yDim = yDim;
            this._xDiv = xDiv;
            this._yDiv = yDiv;
        }

        /// <summary>
        /// Tessellate the analysis mesh.
        /// </summary>
        /// <returns>A tessellated representation of the analysis mesh.</returns>
        public Mesh Tessellate()
        {
            throw new System.NotImplementedException();
        }
    }
}