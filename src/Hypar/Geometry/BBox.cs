using System.Collections.Generic;

namespace Hypar.Geometry
{
    public class BBox
    {
        private Vector3 _min;
        private Vector3 _max;

        public Vector3 Max => _max;

        public Vector3 Min => _min;

        public static BBox FromPoints(IEnumerable<Vector3> points)
        {
            var min = Vector3.Origin();
            var max = Vector3.Origin();
            foreach(var p in points)
            {
                if(p < min) min = p;
                if(p > max) max = p;
            }
            return new BBox(min, max);
        }
        internal BBox(Vector3 min, Vector3 max)
        {
            this._min = min;
            this._max = max;
        }
    }
}