using System.Collections.Generic;
using Elements.Geometry;

namespace Elements.Search
{
    /// <summary>
    /// A comparer used to order collections of points based on
    /// their distance from a provided point.
    /// </summary>
    public class DistanceComparer : IComparer<(Vector3, int)>
    {
        private Vector3 _v;

        /// <summary>
        /// Construct an event comparer.
        /// </summary>
        /// <param name="v">The vector against which to compare.</param>
        public DistanceComparer(Vector3 v)
        {
            this._v = v;
        }

        /// <summary>
        /// Compare two points.
        /// </summary>
        /// <param name="x">The first point.</param>
        /// <param name="y">The second point.</param>
        public int Compare((Vector3, int) x, (Vector3, int) y)
        {
            var a = _v.DistanceTo(x.Item1);
            var b = _v.DistanceTo(y.Item1);

            if (a > b)
            {
                return 1;
            }
            else if (a < b)
            {
                return -1;
            }
            return 0;
        }
    }
}