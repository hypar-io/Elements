using System.Collections.Generic;
using Elements.Geometry;
using SkiaSharp;

namespace Elements.Serialization.SVG
{
    public class DrawText : DrawingAction
    {
        public DrawText(string text, Transform transform, SvgContext context)
        {
            _text = text;
            _transform = transform;
            _context = context;
        }

        internal override void Draw(BaseDrawingTool drawingTool)
        {
            drawingTool.DrawText(_text, _transform, _context);
            // var r = default(SKRect);
            // var t2 = SKTypeface.FromFamilyName(_context.Font.FamilyName, SKFontStyleWeight.Normal, SKFontStyleWidth.Normal, SKFontStyleSlant.Upright);
            // var font2 = new SKFont(t2, (float)(_context.Font.Size * baseDrawing.Scale));
            // var p2 = new SKPaint(font2);

            // double textLength = p2.MeasureText(_text, ref r);

            // // double textLength = 6;

            // var heightVectot = _transform.OfVector(Vector3.YAxis);
            // var top = (_transform.Origin + heightVectot * textLength / 2.0).ToSkPoint(baseDrawing);
            // var bottom = (_transform.Origin - heightVectot * textLength / 2.0).ToSkPoint(baseDrawing);

            // var path = new SKPath();
            // path.AddPoly(new SKPoint[] { top, bottom });

            // baseDrawing.Canvas.DrawTextOnPath(_text, path, (float)(textLength * baseDrawing.Scale / 2.0 - r.MidX) / 2f, r.Height / 2.0f, p2);
        }

        private string _text;
        private Transform _transform;
        private SvgContext _context;
    }
}