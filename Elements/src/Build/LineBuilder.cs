using System.Collections.Generic;
using Elements.Geometry;

namespace Elements
{
    public class LineBuilder : BuilderBase
    {
        private Vector3 _start = Vector3.Origin;
        private Vector3 _end = new Vector3(5, 0, 0);

        public LineBuilder(List<string> errors) : base(errors) { }

        public Line Build()
        {
            var line = new Line(_start, _end);
            return line;
        }

        public LineBuilder From(Vector3 start)
        {
            _start = start;
            return this;
        }

        public LineBuilder To(Vector3 end)
        {
            _end = end;
            return this;
        }

    }
}