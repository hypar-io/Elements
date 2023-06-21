using System;
using System.Collections.Generic;
using Elements.Geometry;

namespace Elements.Search
{
    /// <summary>
    /// Comparer used during line sweeps to sort segments by the Y value of
    /// their left-most point.
    /// </summary>
    internal class LeftMostPointComparer<T> : IComparer<T>
    {
        private readonly Func<T, Line> _getSegment;

        /// <summary>
        /// Construct a left most point comparer.
        /// </summary>
        /// <param name="getSegment">A method which returns the segment of the 
        /// object for comparison to other objects.</param>
        public LeftMostPointComparer(Func<T, Line> getSegment)
        {
            this._getSegment = getSegment;
        }

        /// <summary>
        /// Compare two objects of type T.
        /// </summary>
        /// <param name="t1">The first object.</param>
        /// <param name="t2">The second object.</param>
        public int Compare(T t1, T t2)
        {
            var a = _getSegment(t1);
            var b = _getSegment(t2);

            const double small = 0.001;
            var al = a.Domain.Length;
            var bl = b.Domain.Length;
            var aLeft = a.Start.X <= a.End.X ? a.PointAt(al * small) : a.PointAt(al * (1 - small));
            var bLeft = b.Start.X <= b.End.X ? b.PointAt(bl * small) : b.PointAt(bl * (1 - small));

            if (aLeft.Y.ApproximatelyEquals(bLeft.Y))
                {
                    if (aLeft.X.ApproximatelyEquals(bLeft.X))
                    {
                        return 0;
                    }
                    return aLeft.X.CompareTo(bLeft.X);
                }
                if (aLeft.Y > bLeft.Y)
                {
                    return -1;
                }
                return 1;
        }
    }
}
