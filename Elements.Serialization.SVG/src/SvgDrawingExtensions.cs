using Elements.Geometry;
using Svg;
using System.Collections.Generic;

namespace Elements.Serialization.SVG
{
    /// <summary>
    /// Extension methods for drawing elements to SVG.
    /// </summary>
    public static class SvgDrawingExtensions
    {
        /// <summary>
        /// Convert a geometric line to an SVG line.
        /// </summary>
        /// <param name="line">The line to be drawn.</param>
        /// <param name="min">The minimum</param>
        /// <param name="h"></param>
        /// <param name="context"></param>
        /// <returns></returns>
        public static SvgLine ToSvgLine(this Line line, Vector3 min, float h, SvgContext context)
        {
            var svgLine = new SvgLine
            {
                StartX = line.Start.X.ToXUserUnit(min),
                StartY = line.Start.Y.ToYUserUnit(h, min),
                EndX = line.End.X.ToXUserUnit(min),
                EndY = line.End.Y.ToYUserUnit(h, min),
                StrokeWidth = context.StrokeWidth,
                StrokeDashArray = context.StrokeDashArray,
            };

            // If use properties to set color, the generated SVG has color names like this: fill:'Black',
            // but expected value is fill:black
            string style = string.Empty;
            if (context.Fill != null)
            {
                style += $"fill:{context.Fill.Colour.Name.ToLower()}";
            }

            if (context.Stroke != null)
            {
                if (!string.IsNullOrEmpty(style))
                {
                    style += "; ";
                }
                style += $"stroke:{context.Stroke.Colour.Name.ToLower()}";
            }

            if (!string.IsNullOrEmpty(style))
            {
                svgLine.CustomAttributes.Add("style", style);
            }
            return svgLine;
        }

        /// <summary>
        /// Convert a geometric line to an SVG using a section and a context.
        /// </summary>
        /// <param name="line">The line to be converted.</param>
        /// <param name="drawingPlan">The SvgSection containing the scene boundaries.</param>
        /// <param name="context">The SVG context.</param>
        /// <returns></returns>
        public static SvgLine ToSvgLine(this Line line, SvgSection drawingPlan, SvgContext context)
        {
            return ToSvgLine(line, drawingPlan.GetSceneBounds().Min, drawingPlan.ViewBoxHeight, context);
        }

        public static SvgPolygon ToSvgPolygon(this Polygon polygon, Vector3 min, float h, SvgContext context)
        {
            return new SvgPolygon()
            {
                Fill = context.Fill,
                Stroke = context.Stroke,
                StrokeWidth = context.StrokeWidth,
                StrokeDashArray = context.StrokeDashArray,
                Points = polygon.Vertices.ToSvgPointCollection(min, h)
            };
        }

        /// <summary>
        /// Convert a geometric polygon to an SVG polygon.
        /// </summary>
        /// <param name="polygon">The polygon to be converted.</param>
        /// <param name="drawingPlan">The SvgSection containing the scene boundaries.</param>
        /// <param name="context">The SVG context.</param>
        /// <returns></returns>
        public static SvgPolygon ToSvgPolygon(this Polygon polygon, SvgSection drawingPlan, SvgContext context)
        {
            return ToSvgPolygon(polygon, drawingPlan.GetSceneBounds().Min, drawingPlan.ViewBoxHeight, context);
        }

        public static SvgUnit ToXUserUnit(this double x, Vector3 min)
        {
            return new SvgUnit(SvgUnitType.User, (float)(x - min.X));
        }

        public static SvgUnit ToXUserUnit(this double x, SvgSection drawingPlan)
        {
            return ToXUserUnit(x, drawingPlan.GetSceneBounds().Min);
        }

        public static SvgUnit ToYUserUnit(this double y, float h, Vector3 min)
        {
            // invert for y down coordinates of SVG
            return new SvgUnit(SvgUnitType.User, (float)(h + min.Y - y));
        }

        public static SvgUnit ToYUserUnit(this double y, SvgSection drawingPlan)
        {
            return ToYUserUnit(y, drawingPlan.ViewBoxHeight, drawingPlan.GetSceneBounds().Min);
        }

        public static SvgPointCollection ToSvgPointCollection(this IList<Vector3> points, Vector3 min, float h)
        {
            var ptCollection = new SvgPointCollection();
            foreach (var pt in points)
            {
                ptCollection.Add(new SvgUnit(pt.X.ToXUserUnit(min).Value));
                ptCollection.Add(new SvgUnit(pt.Y.ToYUserUnit(h, min).Value));
            }
            return ptCollection;
        }

        public static SvgPointCollection ToSvgPointCollection(this IList<Vector3> points, SvgSection drawingPlan)
        {
            return ToSvgPointCollection(points, drawingPlan.GetSceneBounds().Min, drawingPlan.ViewBoxHeight);
        }
    }
}