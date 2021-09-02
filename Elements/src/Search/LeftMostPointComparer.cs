using System.Collections.Generic;
using Elements.Geometry;

namespace Elements.Search
{
    /// <summary>
    /// Comparer used during line sweeps to sort segments by the Y value of
    /// their left-most point.
    /// </summary>
    internal class LeftMostPointComparer : IComparer<Line>
    {
        /// <summary>
        /// Compare two lines.
        /// </summary>
        /// <param name="a">The index of the first line.</param>
        /// <param name="b">The index of the second line.</param>
        public int Compare(Line a, Line b)
        {
            var xLeft = a.Start.X <= a.End.X ? a.Start : a.End;
            var yLeft = b.Start.X <= b.End.X ? b.Start : b.End;

            if (xLeft == yLeft)
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