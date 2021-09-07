using System.Collections.Generic;
using Elements.Geometry;

namespace Elements.Search
{
    /// <summary>
    /// A comparer used to order collections of vectors
    /// according to their "sameness" with the provided vector.
    /// </summary>
    public class DirectionComparer : IComparer<Vector3>
    {
        private Vector3 _v;

        /// <summary>
        /// Construct a direction comparer.
        /// </summary>
        /// <param name="v">The vector against which to compare.</param>
        public DirectionComparer(Vector3 v)
        {
            this._v = v;
        }

        /// <summary>
        /// Compare two vectors.
        /// </summary>
        /// <param name="x">The first vector.</param>
        /// <param name="y">The second vector.</param>
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