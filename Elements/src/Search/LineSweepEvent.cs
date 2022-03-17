using System;
using System.Collections.Generic;
using Elements.Geometry;

namespace Elements.Search
{
    internal class LineSweepEvent<T> : IComparable<LineSweepEvent<T>>
    {
        public Vector3 Point;

        public IEnumerable<(int segmentId, bool isLeftMostPoint, T data)> Segments;

        public LineSweepEvent(Vector3 point, IEnumerable<(int segmentId, bool isLeftMostPoint, T data)> segments)
        {
            this.Point = point;
            this.Segments = segments;
        }

        public int CompareTo(LineSweepEvent<T> other)
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