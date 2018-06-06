using Hypar.Geometry;

namespace Hypar.Elements
{
    public class AnalysisMesh : IMeshProvider
    {
        private Vector3 _origin;
        private double _xDim;
        private double _yDim;
        private int _xDiv;
        private int _yDiv;
        public AnalysisMesh(Vector3 origin, double xDim, double yDim, int xDiv, int yDiv)
        {
            this._origin = origin;
            this._xDim = xDim;
            this._yDim = yDim;
            this._xDiv = xDiv;
            this._yDiv = yDiv;
        }

        public Mesh Tessellate()
        {
            throw new System.NotImplementedException();
        }
    }
}