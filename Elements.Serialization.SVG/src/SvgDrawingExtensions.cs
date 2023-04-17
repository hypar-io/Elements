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
        /// <param name="sceneBoundsMin">The minimum</param>
        /// <param name="viewBoxHeight"></param>
        /// <param name="context"></param>
        /// <returns>An SVG line.</returns>
        public static SvgLine ToSvgLine(this Line line, Vector3 sceneBoundsMin, float viewBoxHeight, SvgContext context)
        {
            var svgLine = new SvgLine
            {
                StartX = line.Start.X.ToXUserUnit(sceneBoundsMin),
                StartY = line.Start.Y.ToYUserUnit(viewBoxHeight, sceneBoundsMin),
                EndX = line.End.X.ToXUserUnit(sceneBoundsMin),
                EndY = line.End.Y.ToYUserUnit(viewBoxHeight, sceneBoundsMin),
                StrokeWidth = context.StrokeWidth,
                StrokeDashArray = context.StrokeDashArray,
            };

            // If use properties to set color, the generated SVG has color names like this: fill:'Black',
            // but expected value is fill:black
            string style = string.Empty;
            if (context.Fill != null)
            {
                style = $"fill:{SvgColourServerToString(context.Fill)}";
            }

            if (context.Stroke != null)
            {
                if (!string.IsNullOrEmpty(style))
                {
                    style += "; ";
                }
                style += $"stroke:{SvgColourServerToString(context.Stroke)}";
            }

            if (!string.IsNullOrEmpty(style))
            {
                svgLine.CustomAttributes.Add("style", style);
            }
            return svgLine;
        }

        private static string SvgColourServerToString(SvgColourServer colorServer)
        {
            return $"rgb({colorServer.Colour.R}, {colorServer.Colour.G}, {colorServer.Colour.B})";
        }
	/// <summary>
        /// Convert a geometric line to an SVG using a section and a context.
        /// </summary>
        /// <param name="line">The line to be converted.</param>
        /// <param name="drawingPlan">The SvgSection containing the scene boundaries.</param>
        /// <param name="context">The SVG context.</param>
        /// <returns>An SVG line.</returns>
        public static SvgLine ToSvgLine(this Line line, SvgSection drawingPlan, SvgContext context)
        {
            return ToSvgLine(line, drawingPlan.GetSceneBounds().Min, drawingPlan.ViewBoxHeight, context);
        }

        /// <summary>
        /// Convert a polygon to an SVG polygon.
        /// </summary>
        /// <param name="polygon">The polygon to convert.</param>
        /// <param name="sceneBoundsMin">The scene bounds minimum.</param>
        /// <param name="viewBoxHeight">The height of the view box.</param>
        /// <param name="context">The SVG context.</param>
        /// <returns>An SVG polygon.</returns>
	public static SvgPolygon ToSvgPolygon(this Polygon polygon, Vector3 min, float h, SvgContext context)
        {
            var svgPolygon = new SvgPolygon()
            {
                StrokeWidth = context.StrokeWidth,
                StrokeDashArray = context.StrokeDashArray,
                Points = polygon.Vertices.ToSvgPointCollection(min, h)
            };

            string style = string.Empty;
            if (context.Fill != null)
            {
                style += $"fill:{SvgColourServerToString(context.Fill)}";
            }

            if (context.Stroke != null)
            {
                if (!string.IsNullOrEmpty(style))
                {
                    style += "; ";
                }
                style += $"stroke:{SvgColourServerToString(context.Stroke)}";
            }

            if (!string.IsNullOrEmpty(style))
            {
                svgPolygon.CustomAttributes.Add("style", style);
            }

            return svgPolygon;
        }

        /// <summary>
        /// Convert a geometric polygon to an SVG polygon.
        /// </summary>
        /// <param name="polygon">The polygon to be converted.</param>
        /// <param name="drawingPlan">The section relative to which conversion will occur.</param>
        /// <param name="context">The SVG context.</param>
        /// <returns>An SVG polygon.</returns>
        public static SvgPolygon ToSvgPolygon(this Polygon polygon, SvgSection drawingPlan, SvgContext context)
        {
            return ToSvgPolygon(polygon, drawingPlan.GetSceneBounds().Min, drawingPlan.ViewBoxHeight, context);
        }

        /// <summary>
        /// Convert a double value to an SVG user unit type.
        /// </summary>
        /// <param name="x">The value to convert.</param>
        /// <param name="sceneBoundsMin">The minimum of the scene bounds.</param>
        /// <returns>An SVG unit.</returns>
        public static SvgUnit ToXUserUnit(this double x, Vector3 sceneBoundsMin)
        {
            return new SvgUnit(SvgUnitType.User, (float)(x - sceneBoundsMin.X));
        }

        /// <summary>
        /// Convert a double value to an SVG user unit type.
        /// </summary>
        /// <param name="x">The value to convert.</param>
        /// <param name="drawingPlan">The section relative to which conversion will occur.</param>
        /// <returns>An SVG unit.</returns>
        public static SvgUnit ToXUserUnit(this double x, SvgSection drawingPlan)
        {
            return ToXUserUnit(x, drawingPlan.GetSceneBounds().Min);
        }

        /// <summary>
        /// Convert a double value to an SVG user unit type.
        /// </summary>
        /// <param name="y">The value to convert.</param>
        /// <param name="viewBoxHeight">The height of the view box.</param>
        /// <param name="sceneBoundsMin">The minimum of the scene bounds.</param>
        /// <returns>An SVG unit.</returns>
        public static SvgUnit ToYUserUnit(this double y, float viewBoxHeight, Vector3 sceneBoundsMin)
        {
            // invert for y down coordinates of SVG
            return new SvgUnit(SvgUnitType.User, (float)(viewBoxHeight + sceneBoundsMin.Y - y));
        }

        /// <summary>
        /// Convert a double value to an SVG user unit type.
        /// </summary>
        /// <param name="y">The unit to convert.</param>
        /// <param name="drawingPlan">The section relative to which conversion will occur.</param>
        /// <returns>An SVG unit.</returns>
        public static SvgUnit ToYUserUnit(this double y, SvgSection drawingPlan)
        {
            return ToYUserUnit(y, drawingPlan.ViewBoxHeight, drawingPlan.GetSceneBounds().Min);
        }

        /// <summary>
        /// Convert a collection of points into a collection of SVG points.
        /// </summary>
        /// <param name="points">The points to convert.</param>
        /// <param name="sceneBoundsMin">The minimum of the scene bounds.</param>
        /// <param name="viewBoxHeight">The height of the view box.</param>
        /// <returns>An collection of SVG points.</returns>
        public static SvgPointCollection ToSvgPointCollection(this IList<Vector3> points, Vector3 sceneBoundsMin, float viewBoxHeight)
        {
            var ptCollection = new SvgPointCollection();
            foreach (var pt in points)
            {
                ptCollection.Add(new SvgUnit(pt.X.ToXUserUnit(sceneBoundsMin).Value));
                ptCollection.Add(new SvgUnit(pt.Y.ToYUserUnit(viewBoxHeight, sceneBoundsMin).Value));
            }
            return ptCollection;
        }

        /// <summary>
        /// Convert a collection of points into a collection of SVG points.
        /// </summary>
        /// <param name="points">The points to convert.</param>
        /// <param name="drawingPlan">The section relative to which conversion will occur.</param>
        /// <returns>An collection of SVG points.</returns>
        public static SvgPointCollection ToSvgPointCollection(this IList<Vector3> points, SvgSection drawingPlan)
        {
            return ToSvgPointCollection(points, drawingPlan.GetSceneBounds().Min, drawingPlan.ViewBoxHeight);
        }
    }
}
