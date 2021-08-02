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
            if (points.Count < 3) return true;

            //TODO: https://github.com/hypar-io/sdk/issues/54
            // Ensure that all triple products are equal to 0.
            // a.Dot(b.Cross(c));
            var a = points[0];
            var b = points[1];
            var c = points[2];
            var ab = b - a;
            var ac = c - a;
            for (var i = 3; i < points.Count; i++)
            {
                var d = points[i];
                var cd = d - a;
                var tp = ab.Dot(ac.Cross(cd));
                if (Math.Abs(tp) > Vector3.EPSILON)
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
        public static bool AreCollinear(this IList<Vector3> points)
        {
            if (points == null || points.Count == 0)
            {
                throw new ArgumentException("Cannot test collinearity of an empty list");
            }
            if (points.Distinct(new Vector3Comparer()).Count() < 3)
            {
                return true;
            }
            var testVector = (points[1] - points[0]).Unitized();
            // in general this loop should not execute. This is just a check in case the first two points are
            // coincident.
            while (testVector.IsZero()) //loop until you find an initial vector that isn't zero-length
            {
                points.RemoveAt(0);
                if (points.Count < 3)
                {
                    return true;
                }
                testVector = (points[1] - points[0]).Unitized();
            }
            for (int i = 2; i < points.Count; i++)
            {
                var nextVector = (points[i] - points[i - 1]).Unitized();
                if (nextVector.IsZero()) // coincident points may be safely skipped
                {
                    continue;
                }
                if (Math.Abs(nextVector.Dot(testVector)) < (1 - Vector3.EPSILON))
                {
                    return false;
                }
            }
            return true;
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
        public static Vector3 Average(this IList<Vector3> points)
        {
            double x = 0.0, y = 0.0, z = 0.0;
            foreach (var p in points)
            {
                x += p.X;
                y += p.Y;
                z += p.Z;
            }
            return new Vector3(x / points.Count, y / points.Count, z / points.Count);
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
    }

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