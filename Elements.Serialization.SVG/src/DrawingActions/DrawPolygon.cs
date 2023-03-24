using System.Collections.Generic;
using Elements.Geometry;
using SkiaSharp;

namespace Elements.Serialization.SVG
{
    public class DrawPolygon : DrawingAction
    {
        public DrawPolygon(Polygon polygon, SvgContext context, string text = "")
        {
            _polygon = polygon;
            _text = text;
            _context = context;
        }

        internal override void Draw(BaseDrawingTool drawinTool)
        {
            drawinTool.DrawPolygon(_polygon, _context);
            // var path = new SKPath();
            // var points = new List<SKPoint>();
            // foreach (var point in _polygon.Vertices)
            // {
            //     points.Add(point.ToSkPoint(baseDrawing));
            // }

            // path.AddPoly(points.ToArray(), true);
            // var paint = new SKPaint() { Style = SKPaintStyle.Fill, StrokeWidth = (float)_context.ElementStroke.Width };
            // if (_context.Color.HasValue)
            // {
            //     paint.Color = _context.Color.Value.ToSkColor();
            //     baseDrawing.Canvas.DrawPath(path, paint);
            // }

            // paint.Color = _context.ElementStroke.Color.ToSkColor();
            // paint.Style = SKPaintStyle.Stroke;
            // baseDrawing.Canvas.DrawPath(path, paint);
        }

        private Polygon _polygon;
        private string _text;
        private SvgContext _context;
    }
}