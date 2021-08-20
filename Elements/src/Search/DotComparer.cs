using System.Collections.Generic;
using Elements.Geometry;

namespace Elements.Search
{
    /// <summary>
    /// A comparer used to order collections of vectors
    /// according to their "sameness" with the provided vector.
    /// Often used to order points along a vector.
    /// </summary>
    internal class DotComparer : IComparer<Vector3>
    {
        private Vector3 _v;

        /// <summary>
        /// Construct a dot comparer.
        /// </summary>
        /// <param name="v">The vector against which to compare.</param>
        public DotComparer(Vector3 v)
        {
            this._v = v;
        }

        public int Compare(Vector3 x, Vector3 y)
        {
            var a = _v.Dot(x);
            var b = _v.Dot(y);

            if (a > b)
            {
                return -1;
            }
            else if (a < b)
            {
                return 1;
            }
            return 0;
        }
    }
}