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
            // var viewBoxWidth = (float)(sceneBounds.Max.X - sceneBounds.Min.X);
            // var viewBoxHeight = (float)(sceneBounds.Max.Y - sceneBounds.Min.Y);
            var t = new Transform(Vector3.Origin);
            t.Rotate(rotation);
            var bounds = new BBox3(sceneBounds.Corners().Select(v => t.OfPoint(v)));
            // var wOld = viewBoxWidth;
            // var hOld = viewBoxHeight;
            var viewBoxWidth = (float)(bounds.Max.X - bounds.Min.X);
            var viewBoxHeight = (float)(bounds.Max.Y - bounds.Min.Y);

            PageHeight = viewBoxHeight;
            PageWidth = viewBoxWidth;
            // float max = Math.Max(viewBoxWidth, viewBoxHeight);
            // _matrix = SKMatrix.CreateRotationDegrees((float)rotation, (float)(viewBoxWidth * _baseDrawing.Scale / 2.0),
            //     (float)(viewBoxHeight * _baseDrawing.Scale / 2.0));
            // _matrix.TransX += (float)((wOld - viewBoxWidth) * _baseDrawing.Scale / 2.0f);
            // _matrix.TransY += (float)((viewBoxHeight - hOld) * _baseDrawing.Scale / 2.0f);
        }

        public abstract void DrawPolygon(Polygon polygon, SvgContext context);

        public abstract void DrawText(string text, Transform transform, SvgContext context);

        public abstract void DrawLine(Line line, SvgContext context);

        public abstract void DrawCircle(Vector3 center, double radius, SvgContext context);

        public abstract void Close();
    }
}