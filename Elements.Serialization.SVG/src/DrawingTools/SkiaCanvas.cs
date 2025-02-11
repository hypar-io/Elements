using System.Collections.Generic;
using Elements.Geometry;
using SkiaSharp;
using System.Linq;
using System;
using System.IO;

namespace Elements.Serialization.SVG
{
    /// <summary>
    /// The adapter for Skia library.
    /// </summary>
    public class SkiaCanvas : BaseSvgCanvas
    {
        private SKCanvas _canvas;
        private SKMatrix _matrix = new SKMatrix();

        /// <summary>
        /// Initializes a new instance of SkDrawingTool
        /// </summary>
        /// <param name="canvas">The canvas.</param>
        /// <param name="document">The SVG document.</param>
        /// <returns></returns>
        public SkiaCanvas(SKCanvas canvas, SvgBaseDrawing document) : base(document)
        {
            _canvas = canvas;
        }

        /// <summary>
        /// Initializes a new instance of SkDrawingTool.
        /// </summary>
        /// <param name="stream">The document stream.</param>
        /// <param name="viewBoxHeight">The height of the model bounding box.</param>
        /// <param name="viewBoxWidth">The width of the model bounding box.</param>
        /// <param name="document">The SVG document</param>
        public SkiaCanvas(Stream stream, double viewBoxHeight, double viewBoxWidth, SvgBaseDrawing document)
        : base(document)
        {
            _canvas = SKSvgCanvas.Create(SKRect.Create((float)(viewBoxWidth * document.Scale),
             (float)(viewBoxHeight * document.Scale)), stream);
        }

        /// <summary>
        /// Sets the scene bounds
        /// </summary>
        /// <param name="sceneBounds">The scene bounding box.</param>
        /// <param name="rotation">The plan rotation angle (in degrees).</param>
        public override void SetBounds(BBox3 sceneBounds, double rotation)
        {
            var wOld = (float)(sceneBounds.Max.X - sceneBounds.Min.X);
            var hOld = (float)(sceneBounds.Max.Y - sceneBounds.Min.Y);

            base.SetBounds(sceneBounds, rotation);
            double max = Math.Max(PageWidth, PageHeight);
            _matrix = SKMatrix.CreateRotationDegrees((float)rotation, (float)(PageWidth * Document.Scale / 2.0),
                (float)(PageHeight * Document.Scale / 2.0));
            _matrix.TransX += (float)((wOld - PageWidth) * Document.Scale / 2.0f);
            _matrix.TransY += (float)((PageHeight - hOld) * Document.Scale / 2.0f);
        }

        /// <summary>
        /// Draws polygon on canvas.
        /// </summary>
        /// <param name="polygon">The polygon.</param>
        /// <param name="context">The svg context that can be used to draw the polygon (color, line thickness, etc.)</param>
        public override void DrawPolygon(Polygon polygon, SvgContext context)
        {
            var path = new SKPath();
            var points = new List<SKPoint>();
            foreach (var point in polygon.Vertices)
            {
                points.Add(ToSkPoint(point));
            }

            path.AddPoly(points.ToArray(), true);
            var paint = new SKPaint() { Style = SKPaintStyle.Fill, StrokeWidth = (float)(context.ElementStroke.Width * Document.Scale) };
            if (context.Color.HasValue)
            {
                paint.Color = ToSkColor(context.Color.Value);
                _canvas.DrawPath(path, paint);
            }

            paint.Color = ToSkColor(context.ElementStroke.Color);
            paint.Style = SKPaintStyle.Stroke;
            _canvas.DrawPath(path, paint);
        }

        /// <summary>
        /// Draws text on canvas.
        /// </summary>
        /// <param name="text">The text.</param>
        /// <param name="transform">The text transformation matrix.</param>
        /// <param name="context">The svg context that can be used to draw the text (color, line thickness, etc.)</param>
        public override void DrawText(string text, Transform transform, SvgContext context)
        {
            var r = default(SKRect);
            var t2 = SKTypeface.FromFamilyName(context.Font.FamilyName, SKFontStyleWeight.Normal, SKFontStyleWidth.Normal, SKFontStyleSlant.Upright);
            var font2 = new SKFont(t2, (float)(context.Font.Size * Document.Scale));
            var p2 = new SKPaint(font2);

            double textLength = p2.MeasureText(text, ref r);

            var heightVectot = transform.OfVector(Vector3.YAxis);
            var top = ToSkPoint(transform.Origin + heightVectot * textLength / 2.0);
            var bottom = ToSkPoint(transform.Origin - heightVectot * textLength / 2.0);

            var path = new SKPath();
            path.AddPoly(new SKPoint[] { top, bottom });

            _canvas.DrawTextOnPath(text, path, (float)(textLength * Document.Scale / 2.0 - r.MidX) / 2f, r.Height / 2.0f, p2);
        }

        /// <summary>
        /// Draws line on canvas.
        /// </summary>
        /// <param name="line">The line.</param>
        /// <param name="context"></param>
        public override void DrawLine(Line line, SvgContext context)
        {
            var paint = new SKPaint() { Style = SKPaintStyle.Fill, StrokeWidth = (float)(context.ElementStroke.Width * Document.Scale) };
            if (context.Color.HasValue)
            {
                paint.Color = ToSkColor(context.Color.Value);
            }

            paint.Color = ToSkColor(context.ElementStroke.Color);
            paint.Style = SKPaintStyle.Stroke;
            if (context.ElementStroke.HasDash)
            {
                paint.PathEffect = SKPathEffect.CreateDash(context.ElementStroke.DashIntervals.Select(i => (float)(i * Document.Scale)).ToArray(), (float)context.ElementStroke.DashPhase);
            }

            _canvas.DrawLine(ToSkPoint(line.Start), ToSkPoint(line.End), paint);
        }

        /// <summary>
        /// Draws circle on canvas.
        /// </summary>
        /// <param name="center">The circle center.</param>
        /// <param name="radius">The circle radius.</param>
        /// <param name="context">The svg context that can be used to draw the circle (color, line thickness, etc.)</param>
        public override void DrawCircle(Vector3 center, double radius, SvgContext context)
        {
            var location = ToSkPoint(center);
            var paint = new SKPaint() { Style = SKPaintStyle.Fill, StrokeWidth = (float)(context.ElementStroke.Width * Document.Scale) };
            if (context.Color.HasValue)
            {
                paint.Color = ToSkColor(context.Color.Value);
                _canvas.DrawCircle(location, (float)(radius * Document.Scale), paint);
            }

            paint.Color = ToSkColor(context.ElementStroke.Color);
            paint.Style = SKPaintStyle.Stroke;

            _canvas.DrawCircle(location, (float)(radius * Document.Scale), paint);
        }

        /// <summary>
        /// Disposes the canvas
        /// </summary>
        public override void Close()
        {
            _canvas.Dispose();
        }

        private SKPoint ToSkPoint(Vector3 point)
        {
            var viewBoxHeight = (float)(SceneBounds.Max.Y - SceneBounds.Min.Y);
            var skPoint = new SKPoint((float)((point.X - SceneBounds.Min.X) * Document.Scale),
                 (float)((viewBoxHeight + SceneBounds.Min.Y - point.Y) * Document.Scale));
            return _matrix.MapPoint(skPoint);
        }

        private static SKColor ToSkColor(Color color)
        {
            return new SKColor((byte)(color.Red * 255), (byte)(color.Green * 255), (byte)(color.Blue * 255), (byte)(color.Alpha * 255));
        }
    }
}