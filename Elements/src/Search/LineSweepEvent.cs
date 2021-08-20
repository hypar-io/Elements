using System;
using System.Collections.Generic;
using Elements.Geometry;

namespace Elements.Search
{
    internal class LineSweepEvent : IComparable<LineSweepEvent>
    {
        public Vector3 Point;

        public List<(int segmentId, bool isLeftMostPoint)> Segments;

        public LineSweepEvent(Vector3 point, List<(int segmentId, bool isLeftMostPoint)> segments)
        {
            this.Point = point;
            this.Segments = segments;
        }

        public int CompareTo(LineSweepEvent other)
        {
            if (this.Point.X < other.Point.X)
            {
                return -1;
            }
            else if (this.Point.X > other.Point.X)
            {
                return 1;
            }
            return 0;
        }

        public override string ToString()
        {
            return Point.ToString();
        }
    }
}