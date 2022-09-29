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

            var aLeft = a.Start.X <= a.End.X ? a.Start : a.End;
            var bLeft = b.Start.X <= b.End.X ? b.Start : b.End;

            if (aLeft == bLeft)
            {
                // The left-most points of the lines are equal, but the lines
                // themselves are not neccessarily equal. Use the lines' 
                // bounding boxes to get the max points.
                var bb1 = new BBox3();
                bb1.Extend(a.Start);
                bb1.Extend(a.End);
                var bb2 = new BBox3();
                bb2.Extend(b.Start);
                bb2.Extend(b.End);
                if (bb1.Max.Y > bb2.Max.Y)
                {
                    return -1;
                }
                else if (bb1.Max.Y < bb2.Max.Y)
                {
                    return 1;
                }
                return 0;
            }
            else
            {
                if (aLeft.Y.ApproximatelyEquals(bLeft.Y))
                {
                    return 0;
                }
                if (aLeft.Y > bLeft.Y)
                {
                    return -1;
                }
                return 1;
            }
        }
    }
}