using System;
using System.Linq;
using Elements.Geometry;

namespace Elements.Serialization.SVG
{
    public abstract class BaseDrawingTool
    {
        protected BBox3 _sceneBounds;
        protected readonly SvgBaseDrawing _baseDrawing;

        public double PageHeight;
        public double PageWidth;

        public BaseDrawingTool(SvgBaseDrawing baseDrawing)
        {
            _baseDrawing = baseDrawing;
        }

        public virtual void SetBounds(BBox3 sceneBounds, double rotation)
        {
            _sceneBounds = sceneBounds;
            var transform = new Transform(Vector3.Origin);
            transform.Rotate(rotation);
            var bounds = new BBox3(sceneBounds.Corners().Select(v => transform.OfPoint(v)));
            var viewBoxWidth = (float)(bounds.Max.X - bounds.Min.X);
            var viewBoxHeight = (float)(bounds.Max.Y - bounds.Min.Y);

            PageHeight = viewBoxHeight;
            PageWidth = viewBoxWidth;
        }

        public abstract void DrawPolygon(Polygon polygon, SvgContext context);

        public abstract void DrawText(string text, Transform transform, SvgContext context);

        public abstract void DrawLine(Line line, SvgContext context);

        public abstract void DrawCircle(Vector3 center, double radius, SvgContext context);

        public abstract void Close();
    }
}