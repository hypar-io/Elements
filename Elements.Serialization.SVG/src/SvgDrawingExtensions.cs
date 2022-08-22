using Elements.Geometry;
using Svg;
using System.Collections.Generic;

namespace Elements.Serialization.SVG
{
    public static class SvgDrawingExtensions
    {
        public static SvgLine ToSvgLine(this Line line, Vector3 min, float h, SvgContext context)
        {
            var svgLine = new SvgLine
            {
                StartX = line.Start.X.ToXUserUnit(min),
                StartY = line.Start.Y.ToYUserUnit(h, min),
                EndX = line.End.X.ToXUserUnit(min),
                EndY = line.End.Y.ToYUserUnit(h, min),
                Stroke = context.Stroke,
                StrokeWidth = context.StrokeWidth,
                StrokeDashArray = context.StrokeDashArray
            };

            return svgLine;
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

        public static SvgUnit ToXUserUnit(this double x, Vector3 min)
        {
            return new SvgUnit(SvgUnitType.User, (float)(x - min.X));
        }

        public static SvgUnit ToYUserUnit(this double y, float h, Vector3 min)
        {
            // invert for y down coordinates of SVG
            return new SvgUnit(SvgUnitType.User, (float)(h + min.Y - y));
        }

        public static SvgPointCollection ToSvgPointCollection(this IList<Vector3> points, Vector3 min, float h)
        {
            var ptCollection = new SvgPointCollection();
            foreach (var pt in points)
            {
                ptCollection.Add(new SvgUnit(pt.X.ToXUserUnit(min)));
                ptCollection.Add(new SvgUnit(pt.Y.ToYUserUnit(h, min)));
            }
            return ptCollection;
        }
    }
}