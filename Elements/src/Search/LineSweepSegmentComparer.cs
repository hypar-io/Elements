using System.Collections.Generic;
using Elements.Geometry;

namespace Elements.Search
{
    /// <summary>
    /// Comparer used during line sweeps to sort segments by the Y value of
    /// their left-most point.
    /// </summary>
    internal class LineSweepSegmentComparer : IComparer<int>
    {
        private IList<Line> _segments;

        /// <summary>
        /// Create a line sweep segment comparer.
        /// </summary>
        /// <param name="segments">A collection of segments which will
        /// be referenced by their indices internally.</param>
        public LineSweepSegmentComparer(IList<Line> segments)
        {
            _segments = segments;
        }

        /// <summary>
        /// Compare two lines.
        /// </summary>
        /// <param name="a">The index of the first line.</param>
        /// <param name="b">The index of the second line.</param>
        public int Compare(int a, int b)
        {
            var x = _segments[a];
            var y = _segments[b];

            var xLeft = x.Start.X <= x.End.X ? x.Start : x.End;
            var yLeft = y.Start.X <= y.End.X ? y.Start : y.End;

            if (xLeft == yLeft)
            {
                // The left-most points of the lines are equal, but the lines
                // themselves are not neccessarily equal. Use the lines' 
                // bounding boxes to get the max points.
                var bb1 = new BBox3();
                bb1.Extend(x.Start);
                bb1.Extend(x.End);
                var bb2 = new BBox3();
                bb2.Extend(y.Start);
                bb2.Extend(y.End);
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
                if (xLeft.Y > yLeft.Y)
                {
                    return -1;
                }
                else if (xLeft.Y < yLeft.Y)
                {
                    return 1;
                }
                return 0;
            }
        }
    }
}