using System;
using System.Collections.Generic;
using System.Linq;

namespace Elements.Geometry
{
    /// <summary>
    /// Extension methods for Vector3.
    /// </summary>
    public static class Vector3Extensions
    {
        /// <summary>
        /// Are the provided points on the same plane?
        /// </summary>
        /// <param name="points"></param>
        public static bool AreCoplanar(this IList<Vector3> points)
        {
            if (points.Count < 4) return true; // all sets of less than four points are coplanar

            // Choose the first three non-collinear points
            var p0 = points[0];
            int p1Index = -1;
            int p2Index = -1;
            for (int i = 1; i < points.Count; i++)
            {
                if (p1Index == -1 && (points[i] - p0).Length() > Vector3.EPSILON)
                {
                    p1Index = i;
                }
                else if (p1Index != -1 && (points[p1Index] - p0).Cross(points[i] - p0).Length() > Vector3.EPSILON)
                {
                    p2Index = i;
                    break;
                }
            }

            if (p1Index == -1) return true; // All points are coincident
            if (p2Index == -1) return true; // All points are collinear

            if (p2Index == points.Count - 1) // p2 is the last point, which means all the other points are collinear.
            {
                return true;
            }
            var normal = (points[p1Index] - p0).Cross(points[p2Index] - p0).Unitized();

            for (int i = p2Index + 1; i < points.Count; i++)
            {
                var pi = points[i];
                var dot = normal.Dot(pi - p0);
                if (Math.Abs(dot) > Vector3.EPSILON)
                {
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// Are the provided points along the same line?
        /// </summary>
        /// <param name="points"></param>
        [Obsolete("Use AreCollinearByDistance instead")]
        public static bool AreCollinear(this IList<Vector3> points)
        {
            return AreCollinearByDistance(points);
        }

        /// <summary>
        /// Check whether three points are on the same line withing certain distance.
        /// </summary>
        /// <param name="points">List of points to check. Order is not important.</param>
        /// <param name="tolerance">Distance tolerance.</param>
        public static bool AreCollinearByDistance(this IList<Vector3> points, double tolerance = Vector3.EPSILON)
        {
            if (points == null || points.Count == 0)
            {
                throw new ArgumentException("Cannot test collinearity of an empty list");
            }

            var fitLine = points.FitLine(out var directions);
            if (fitLine == null)
            {
                // this will happen if all points are within tolerance of their average — if they're all basically coincident.
                return true;
            }
            var fitDir = fitLine.Direction();
            var toleranceSquared = tolerance * tolerance;
            return directions.All(d =>
            {
                // Since fitDir is Unitized - dot give the length of projection d onto fitDir.
                var dot = d.Dot(fitDir);
                var lengthSquared = d.LengthSquared();
                // By Pythagoras' theorem d.Length()^2 = dot^2 + distance^2.
                // If it's less than tolerance squared then the point is close enough to the fit line.
                return lengthSquared - (dot * dot) < toleranceSquared;
            });
        }

        /// <summary>
        /// Return an approximate fit line through a set of points.
        /// Not intended for statistical regression purposes.
        /// Note that the line is unit length: it shouldn't be expected
        /// to span the length of the points.
        /// </summary>
        /// <param name="points">The points to fit.</param>
        /// <returns>A line roughly running through the set of points, or null if the points are nearly coincident.</returns>
        public static Line FitLine(this IList<Vector3> points)
        {
            return FitLine(points, out _);
        }

        private static Line FitLine(this IList<Vector3> points, out IEnumerable<Vector3> directionsFromMean)
        {
            // get the mean point, presumably near the center of the pts
            var meanPt = points.Average();
            // get the points minus their mean (direction from the mean to the other points)
            var ptsMinusMean = points.Select(pt => pt - meanPt);
            // pick any non-zero vector as an alignment guide, so that a set of directions
            // that's perfectly symmetrical about the mean doesn't average out to zero
            var nonZeroPts = ptsMinusMean.Where(pt => !pt.IsZero());
            if (nonZeroPts.Count() == 0)
            {
                directionsFromMean = new List<Vector3>();
                return null;
            }
            var alignmentVector = nonZeroPts.First();
            // flip the directions so they're all pointing in the same direction as the alignment vector
            var ptsMinusMeanAligned = ptsMinusMean.Select(p => p.Dot(alignmentVector) < 0 ? p * -1 : p);
            // get average direction
            var averageDirFromMean = ptsMinusMeanAligned.Average();

            directionsFromMean = ptsMinusMean;
            return new Line(meanPt, meanPt + averageDirFromMean.Unitized());
        }

        /// <summary>
        /// Return an approximate fit line through a set of points using the least squares method.
        /// </summary>
        /// <param name="points">The points to fit. Should have at least 2 distinct points.</param>
        /// <returns>An approximate fit line through a set of points using the least squares method.
        /// If there is less than 2 distinct points, returns null.</returns>
        public static Line BestFitLine(this IList<Vector3> points)
        {
            return Line.BestFit(points);
        }

        /// <summary>
        /// Compute a transform with the origin at points[0], with
        /// an X axis along points[1]->points[0], and a normal
        /// computed using the vectors points[2]->points[1] and
        /// points[1]->points[0].
        /// </summary>
        /// <param name="points"></param>
        /// <returns></returns>
        public static Transform ToTransform(this IList<Vector3> points)
        {
            var a = (points[1] - points[0]).Unitized();
            // We need to search for a second vector that is not colinear
            // with the first. If all the vectors are tried, and one isn't
            // found that's not parallel to the first, you'll
            // get a zero-length normal.
            Vector3 b = new Vector3();
            for (var i = 2; i < points.Count; i++)
            {
                b = (points[i] - points[1]).Unitized();
                var dot = b.Dot(a);
                if (dot > -1 + Vector3.EPSILON && dot < 1 - Vector3.EPSILON)
                {
                    // Console.WriteLine("Found valid second vector.");
                    break;
                }
            }

            var n = b.Cross(a);
            var t = new Transform(points[0], a, n);
            return t;
        }

        /// <summary>
        /// Find the average of a collection of Vector3.
        /// </summary>
        /// <param name="points">The Vector3 collection to average.</param>
        /// <returns>A Vector3 representing the average.</returns>
        public static Vector3 Average(this IEnumerable<Vector3> points)
        {
            double x = 0.0, y = 0.0, z = 0.0;
            foreach (var p in points)
            {
                x += p.X;
                y += p.Y;
                z += p.Z;
            }
            var count = points.Count();
            return new Vector3(x / count, y / count, z / count);
        }

        /// <summary>
        /// Shrink a collection of Vector3 towards their average.
        /// </summary>
        /// <param name="points">The collection of Vector3 to shrink.</param>
        /// <param name="distance">The distance to shrink along the vector to average.</param>
        /// <returns></returns>
        public static Vector3[] Shrink(this Vector3[] points, double distance)
        {
            var avg = points.Average();
            var shrink = new Vector3[points.Length];
            for (var i = 0; i < shrink.Length; i++)
            {
                var p = points[i];
                shrink[i] = p + (avg - p).Unitized() * distance;
            }
            return shrink;
        }

        /// <summary>
        /// Convert a collection of Vector3 to a flat array of double.
        /// </summary>
        /// <param name="points">The collection of Vector3 to convert.</param>
        /// <returns>An array containing x,y,z,x1,y1,z1,x2,y2,z2,...</returns>
        public static double[] ToArray(this IList<Vector3> points)
        {
            var arr = new double[points.Count * 3];
            var c = 0;
            for (var i = 0; i < points.Count; i++)
            {
                var v = points[i];
                arr[c] = v.X;
                arr[c + 1] = v.Y;
                arr[c + 2] = v.Z;
                c += 3;
            }
            return arr;
        }

        /// <summary>
        /// Convert a list of vertices to a GraphicsBuffers object.
        /// </summary>
        /// <param name="vertices">The vertices to convert.</param>
        /// <returns></returns>
        public static GraphicsBuffers ToGraphicsBuffers(this IList<Vector3> vertices)
        {
            var gb = new GraphicsBuffers();

            for (var i = 0; i < vertices.Count; i++)
            {
                var v = vertices[i];
                gb.AddVertex(v, default, default, null);
                gb.AddIndex((ushort)i);
            }
            return gb;
        }

        /// <summary>
        /// Calculate the normal of the plane containing a set of points.
        /// </summary>
        /// <param name="points">The points in the plane.</param>
        /// <returns>The normal of the plane containing the points.</returns>
        internal static Vector3 NormalFromPlanarWoundPoints(this IList<Vector3> points)
        {
            var normal = new Vector3();
            for (int i = 0; i < points.Count; i++)
            {
                var p0 = points[i];
                var p1 = points[(i + 1) % points.Count];
                normal.X += (p0.Y - p1.Y) * (p0.Z + p1.Z);
                normal.Y += (p0.Z - p1.Z) * (p0.X + p1.X);
                normal.Z += (p0.X - p1.X) * (p0.Y + p1.Y);
            }
            return normal.Unitized();
        }

        /// <summary>
        /// De-duplicate a collection of Vectors, such that no two vectors in the result are within tolerance of each other.
        /// </summary>
        /// <param name="vectors">List of vectors</param>
        /// <param name="tolerance">Distance tolerance</param>
        /// <returns>A new collection of vectors with duplicates removed.</returns>
        public static IEnumerable<Vector3> UniqueWithinTolerance(this IEnumerable<Vector3> vectors, double tolerance = Vector3.EPSILON)
        {
            var output = new List<Vector3>();
            foreach (var vector in vectors)
            {
                if (output.Any(x => x.IsAlmostEqualTo(vector, tolerance)))
                {
                    continue; ;
                }
                output.Add(vector);
            }

            return output;
        }
    }

    /// <summary>
    /// WARNING! do not use this Comparer in `Distinct()` or similar methods if you care about "equality within tolerance."
    /// These methods will use `GetHashCode` rather than `Equals` to determine equality, which must necessarily
    /// ignore tolerance. It is *impossible* to create a hashing algorithm that consistently returns identical values for
    /// any two points within tolerance of each other.
    /// </summary>
    internal class Vector3Comparer : EqualityComparer<Vector3>
    {
        public override bool Equals(Vector3 x, Vector3 y)
        {
            return x.IsAlmostEqualTo(y);
        }

        public override int GetHashCode(Vector3 obj)
        {
            return obj.GetHashCode();
        }
    }
}