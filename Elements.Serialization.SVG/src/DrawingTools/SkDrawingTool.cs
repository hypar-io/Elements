using System.Collections.Generic;
using Elements.Geometry;
using SkiaSharp;
using Svg;
using System.Linq;
using System;
using System.IO;

namespace Elements.Serialization.SVG
{
    public class SkDrawingTool : BaseDrawingTool
    {
        private SKCanvas _canvas;
        private SKMatrix _matrix = new SKMatrix();

        public SkDrawingTool(SKCanvas canvas, SvgBaseDrawing baseDrawing) : base(baseDrawing)
        {
            _canvas = canvas;
        }

        public SkDrawingTool(Stream stream, double viewBoxHeight, double viewBoxWidth, SvgBaseDrawing baseDrawing)
        : base(baseDrawing)
        {
            _canvas = SKSvgCanvas.Create(SKRect.Create((float)(viewBoxWidth * baseDrawing.Scale),
             (float)(viewBoxHeight * baseDrawing.Scale)), stream);
        }

        public override void SetBounds(BBox3 sceneBounds, double rotation)
        {
            var wOld = (float)(sceneBounds.Max.X - sceneBounds.Min.X);
            var hOld = (float)(sceneBounds.Max.Y - sceneBounds.Min.Y);

            base.SetBounds(sceneBounds, rotation);
            double max = Math.Max(PageWidth, PageHeight);
            _matrix = SKMatrix.CreateRotationDegrees((float)rotation, (float)(PageWidth * _baseDrawing.Scale / 2.0),
                (float)(PageHeight * _baseDrawing.Scale / 2.0));
            _matrix.TransX += (float)((wOld - PageWidth) * _baseDrawing.Scale / 2.0f);
            _matrix.TransY += (float)((PageHeight - hOld) * _baseDrawing.Scale / 2.0f);
        }

        public override void DrawPolygon(Polygon polygon, SvgContext context)
        {
            var path = new SKPath();
            var points = new List<SKPoint>();
            foreach (var point in polygon.Vertices)
            {
                points.Add(ToSkPoint(point));
            }

            path.AddPoly(points.ToArray(), true);
            var paint = new SKPaint() { Style = SKPaintStyle.Fill, StrokeWidth = (float)(context.ElementStroke.Width * _baseDrawing.Scale) };
            if (context.Color.HasValue)
            {
                paint.Color = ToSkColor(context.Color.Value);
                _canvas.DrawPath(path, paint);
            }

            paint.Color = ToSkColor(context.ElementStroke.Color);
            paint.Style = SKPaintStyle.Stroke;
            _canvas.DrawPath(path, paint);
        }

        public override void DrawText(string text, Transform transform, SvgContext context)
        {
            var r = default(SKRect);
            var t2 = SKTypeface.FromFamilyName(context.Font.FamilyName, SKFontStyleWeight.Normal, SKFontStyleWidth.Normal, SKFontStyleSlant.Upright);
            var font2 = new SKFont(t2, (float)(context.Font.Size * _baseDrawing.Scale));
            var p2 = new SKPaint(font2);

            double textLength = p2.MeasureText(text, ref r);

            var heightVectot = transform.OfVector(Vector3.YAxis);
            var top = ToSkPoint(transform.Origin + heightVectot * textLength / 2.0);
            var bottom = ToSkPoint(transform.Origin - heightVectot * textLength / 2.0);

            var path = new SKPath();
            path.AddPoly(new SKPoint[] { top, bottom });

            _canvas.DrawTextOnPath(text, path, (float)(textLength * _baseDrawing.Scale / 2.0 - r.MidX) / 2f, r.Height / 2.0f, p2);
        }

        public override void DrawLine(Line line, SvgContext context)
        {
            var paint = new SKPaint() { Style = SKPaintStyle.Fill, StrokeWidth = (float)(context.ElementStroke.Width * _baseDrawing.Scale) };
            if (context.Color.HasValue)
            {
                paint.Color = ToSkColor(context.Color.Value);
            }

            paint.Color = ToSkColor(context.ElementStroke.Color);
            paint.Style = SKPaintStyle.Stroke;
            if (context.ElementStroke.HasDash)
            {
                paint.PathEffect = SKPathEffect.CreateDash(context.ElementStroke.DashIntervals.Select(i => (float)(i * _baseDrawing.Scale)).ToArray(), (float)context.ElementStroke.DashPhase);
            }

            _canvas.DrawLine(ToSkPoint(line.Start), ToSkPoint(line.End), paint);
        }

        public override void DrawCircle(Vector3 center, double radius, SvgContext context)
        {
            var location = ToSkPoint(center);
            var paint = new SKPaint() { Style = SKPaintStyle.Fill, StrokeWidth = (float)(context.ElementStroke.Width * _baseDrawing.Scale) };
            if (context.Color.HasValue)
            {
                paint.Color = ToSkColor(context.Color.Value);
                _canvas.DrawCircle(location, (float)(radius * _baseDrawing.Scale), paint);
            }

            paint.Color = ToSkColor(context.ElementStroke.Color);
            paint.Style = SKPaintStyle.Stroke;

            _canvas.DrawCircle(location, (float)(radius * _baseDrawing.Scale), paint);
        }

        public override void Close()
        {
            _canvas.Dispose();
        }

        private SKPoint ToSkPoint(Vector3 point)
        {
            var viewBoxHeight = (float)(_sceneBounds.Max.Y - _sceneBounds.Min.Y);
            var skPoint = new SKPoint((float)((point.X - _sceneBounds.Min.X) * _baseDrawing.Scale),
                 (float)((viewBoxHeight + _sceneBounds.Min.Y - point.Y) * _baseDrawing.Scale));
            return _matrix.MapPoint(skPoint);
        }

        private static SKColor ToSkColor(Color color)
        {
            return new SKColor((byte)(color.Red * 255), (byte)(color.Green * 255), (byte)(color.Blue * 255), (byte)(color.Alpha * 255));
        }
    }
}