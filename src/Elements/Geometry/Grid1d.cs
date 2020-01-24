using System;
using System.Collections.Generic;

namespace Elements.Geometry
{
    public class Grid1d : Line, IEquatable<Grid1d>
    {
        private List<Line> _segments = new List<Line>();
        public Grid1d(Vector3 start, Vector3 end) : base(start,end)
        {

        }

        public bool Equals(Grid1d other)
        {
            throw new NotImplementedException();
        }
    }
}
