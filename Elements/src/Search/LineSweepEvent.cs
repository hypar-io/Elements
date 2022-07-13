using System;
using System.Collections.Generic;
using Elements.Geometry;

namespace Elements.Search
{
    internal class LineSweepEvent<T>
    {
        public Vector3 Point;

        public IEnumerable<(int segmentId, bool isLeftMostPoint, T data)> Segments;

        public LineSweepEvent(Vector3 point, IEnumerable<(int segmentId, bool isLeftMostPoint, T data)> segments)
        {
            this.Point = point;
            this.Segments = segments;
        }

        public override string ToString()
        {
            return Point.ToString();
        }
    }
}