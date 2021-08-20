using System.Collections.Generic;
using Elements.Geometry;

namespace Elements.Search
{
    /// <summary>
    /// Comparer used during line sweeps to sort segments by the Y value of
    /// their left-most point.
    /// </summary>
    public class LineSweepSegmentComparer : IComparer<Line>
    {
        /// <summary>
        /// Compare two lines.
        /// </summary>
        /// <param name="x">The first line.</param>
        /// <param name="y">The second line.</param>
        public int Compare(Line x, Line y)
        {
            var xLeft = x.Start.X <= x.End.X ? x.Start : x.End;
            var yLeft = y.Start.X <= y.End.X ? y.Start : y.End;

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