using System;
using System.Collections.Generic;
using ClipperLib;
using LibTessDotNet.Double;
using System.Linq;

namespace Elements.Geometry
{
    /// <summary>
    /// Polygon extension methods.
    /// </summary>
    internal static class PolygonExtensions
    {
        /// <summary>
        /// Construct a clipper path from a Polygon.
        /// </summary>
        /// <param name="p"></param>
        /// <param name="tolerance">Optional tolerance value. If converting back to a polygon after the operation, be sure to use the same tolerance value.</param>
        /// <returns></returns>
        internal static List<IntPoint> ToClipperPath(this Polygon p, double tolerance = Vector3.EPSILON)
        {
            var scale = Math.Round(1.0 / tolerance);
            var path = new List<IntPoint>();
            foreach (var v in p.Vertices)
            {
                path.Add(new IntPoint(Math.Round(v.X * scale), Math.Round(v.Y * scale)));
            }
            return path;
        }

        /// <summary>
        /// Construct a Polygon from a clipper path
        /// </summary>
        /// <param name="p"></param>
        /// <param name="tolerance">Optional tolerance value. Be sure to use the same tolerance value as you used when converting to Clipper path.</param>
        /// <returns></returns>
        internal static Polygon ToPolygon(this List<IntPoint> p, double tolerance = Vector3.EPSILON)
        {
            var scale = Math.Round(1.0 / tolerance);
            var converted = new Vector3[p.Count];
            for (var i = 0; i < converted.Length; i++)
            {
                var v = p[i];
                converted[i] = new Vector3(v.X / scale, v.Y / scale);
            }
            try
            {
                return new Polygon(converted);
            }
            catch
            {
                // Often, the polygons coming back from clipper will have self-intersections, in the form of lines that go out and back.
                // here we make a last-ditch attempt to fix this and construct a new polygon.
                var cleanedVertices = Vector3.AttemptPostClipperCleanup(converted);
                if (cleanedVertices.Count < 3)
                {
                    return null;
                }
                try
                {
                    return new Polygon(cleanedVertices);
                }
                catch
                {
                    throw new Exception("Unable to clean up bad polygon resulting from a polygon boolean operation.");
                }
            }
        }

        public static IList<Polygon> Reversed(this IList<Polygon> polygons)
        {
            return polygons.Select(p => p.Reversed()).ToArray();
        }

        internal static ContourVertex[] ToContourVertexArray(this Polyline poly)
        {
            var contour = new ContourVertex[poly.Vertices.Count];
            for (var i = 0; i < poly.Vertices.Count; i++)
            {
                var vert = poly.Vertices[i];
                var cv = new ContourVertex();
                cv.Position = new Vec3 { X = vert.X, Y = vert.Y, Z = vert.Z };
                contour[i] = cv;
            }
            return contour;
        }
    }
}