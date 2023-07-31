using System;
using System.Collections.Generic;
using ClipperLib;

namespace Elements.Geometry
{
    /// <summary>
    /// Polyline extension methods.
    /// </summary>
    internal static class PolylineExtensions
    {
        /// <summary>
        /// Construct a clipper path from a Polygon.
        /// </summary>
        /// <param name="p"></param>
        /// <param name="tolerance">An optional tolerance. If converting back to a Polyline, be sure to use the same tolerance.</param>
        /// <returns></returns>
        internal static List<IntPoint> ToClipperPath(this Polyline p, double tolerance = Vector3.EPSILON)
        {
            var clipperScale = Math.Round(1.0 / tolerance);
            var path = new List<IntPoint>();
            foreach (var v in p.Vertices)
            {
                path.Add(new IntPoint(Math.Round(v.X * clipperScale), Math.Round(v.Y * clipperScale)));
            }
            return path;
        }

        /// <summary>
        /// Convert a line to a polyline
        /// </summary>
        /// <param name="l">The line to convert.</param>
        public static Polyline ToPolyline(this Line l) => new Polyline(new[] { l.Start, l.End });

    }
}