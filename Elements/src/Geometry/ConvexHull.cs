using System;
using System.Collections.Generic;
using System.Linq;

namespace Elements.Geometry
{
    /// <summary>
    /// A utility class for calculating Convex Hulls from inputs
    /// </summary>
    public static class ConvexHull
    {
        /// <summary>
        /// Calculate a polygon from the 2d convex hull of a collection of points.
        /// Adapted from https://rosettacode.org/wiki/Convex_hull#C.23
        /// </summary>
        /// <param name="points">A collection of points</param>
        /// <returns>A polygon representing the convex hull of the provided points.</returns>
        public static Polygon FromPoints(IEnumerable<Vector3> points)
        {
            if (points.Count() == 0)
            {
                return null;
            }
            var pointsSorted = points.Select(p => new Vector3(p.X, p.Y)).OrderBy(p => p.X).ThenBy(p => p.Y).ToArray();
            List<Vector3> hullPoints = new List<Vector3>();

            Func<Vector3, Vector3, Vector3, bool> Ccw = (Vector3 a, Vector3 b, Vector3 c) => ((b.X - a.X) * (c.Y - a.Y)) > ((b.Y - a.Y) * (c.X - a.X));

            // lower hull
            foreach (var pt in pointsSorted)
            {
                while (hullPoints.Count >= 2 && !Ccw(hullPoints[hullPoints.Count - 2], hullPoints[hullPoints.Count - 1], pt))
                {
                    hullPoints.RemoveAt(hullPoints.Count - 1);
                }
                hullPoints.Add(pt);
            }

            // upper hull
            int t = hullPoints.Count + 1;
            for (int i = pointsSorted.Length - 1; i >= 0; i--)
            {
                Vector3 pt = pointsSorted[i];
                while (hullPoints.Count >= t && !Ccw(hullPoints[hullPoints.Count - 2], hullPoints[hullPoints.Count - 1], pt))
                {
                    hullPoints.RemoveAt(hullPoints.Count - 1);
                }
                hullPoints.Add(pt);
            }

            hullPoints.RemoveAt(hullPoints.Count - 1);
            return new Polygon(hullPoints);
        }

        /// <summary>
        /// Compute the 2D convex hull of a set of 3D points in a plane.
        /// </summary>
        /// <param name="points">A collection of points</param>
        /// <param name="planeNormal">The normal direction of the plane in which to compute the hull.</param>
        /// <returns>A polygon representing the convex hull, projected along the normal vector to the average depth of the provided points.</returns>
        public static Polygon FromPointsInPlane(IEnumerable<Vector3> points, Vector3 planeNormal)
        {
            if (planeNormal.Length().ApproximatelyEquals(0))
            {
                throw new ArgumentException("The current normal vector cannot be of length 0");
            }
            if (planeNormal.Unitized() == Vector3.ZAxis
                 || planeNormal.Unitized().Negate() == Vector3.ZAxis)
            {
                return FromPoints(points);
            }
            else
            {
                var center3D = points.Average();
                var toOrientation = new Transform(center3D, planeNormal);
                var fromOrientation = toOrientation.Inverted();
                var tPoints = points.Select(p => fromOrientation.OfPoint(p)).Select(p => new Vector3(p.X, p.Y));
                var twoDHull = FromPoints(tPoints);
                return twoDHull.TransformedPolygon(toOrientation);
            }
        }

        /// <summary>
        /// Calculate a polygon from the 2d convex hull of a polyline or polygon's vertices.
        /// </summary>
        /// <param name="p">A polygon</param>
        /// <returns>A polygon representing the convex hull of the provided shape.</returns>
        public static Polygon FromPolyline(Polyline p)
        {
            return FromPoints(p.Vertices);
        }

        /// <summary>
        /// Calculate a polygon from the 2d convex hull of the vertices of a collection of polylines or polygons.
        /// </summary>
        /// <param name="polylines">A collection of polygons</param>
        /// <returns>A polygon representing the convex hull of the provided shapes.</returns>
        public static Polygon FromPolylines(IEnumerable<Polyline> polylines)
        {
            return FromPoints(polylines.SelectMany(p => p.Vertices));
        }

        /// <summary>
        /// Calculate a polygon from the 2d convex hull of a profile.
        /// </summary>
        /// <param name="p">A profile</param>
        /// <returns>A polygon representing the convex hull of the provided shape.</returns>
        public static Polygon FromProfile(Profile p)
        {
            // it's safe to consider only the perimeter because the voids must be within it
            return FromPolyline(p.Perimeter);
        }

    }
}