using Elements.Geometry;

namespace Elements.Serialization.SVG
{
    internal class LineAdapter
    {
        private Line _line;

        public LineAdapter(Line line, bool isOpening = false)
        {
            _line = line;
            IsOpening = isOpening;
        }

        public Line GetLine()
        {
            return _line;
        }

        public bool IsOpening { get; set; }
        public Vector3 Start => _line.Start;
        public Vector3 End => _line.End;
        public Vector3 GetDirection()
        {
            if (IsOpening)
            {
                return _line.Direction().Negate();
            }

            return _line.Direction();
        }
    }
}