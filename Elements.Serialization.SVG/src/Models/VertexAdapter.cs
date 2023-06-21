using Elements.Geometry;

namespace Elements.Serialization.SVG
{
    internal class VertexAdapter
    {
        public VertexAdapter(Vector3 point, DimensionLine dimensionLine, Vector3 origin, LineAdapter line)
        {
            DimensionLine = dimensionLine;
            Point = point;
            Line = line;
            Origin = origin;
            Projection = GetProjection(0, null);
        }

        public Vector3 Point { get; }
        public DimensionLine DimensionLine { get; }
        public LineAdapter Line { get; }
        public Vector3 Origin { get; }
        public Vector3 Projection { get; }

        public Vector3 GetProjection(double offset, Vector3? origin)
        {
            var max = origin ?? Origin;
            var plane = new Plane(max + DimensionLine.Normal * offset, DimensionLine.Normal);
            return Point.Project(plane);
        }
    }
}