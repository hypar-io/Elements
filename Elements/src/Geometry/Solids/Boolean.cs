using System.Collections.Generic;
using System;
using System.Linq;

namespace Elements.Geometry.Solids
{
    public static class SolidBoolean
    {
        /// <summary>
        /// Compute the union of two solids.
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns></returns>
        public static Solid Union(Solid a, Solid b)
        {
            var allFaces = Intersect(a, b);

            var s = new Solid();
            foreach (var p in allFaces.Where(o => o.Item2 == SetClassification.AOutsideB || o.Item2 == SetClassification.BOutsideA).Select(o => o.Item1))
            {
                s.AddFace(p);
            }

            return s;
        }

        /// <summary>
        /// Compute the difference of two solids.
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns></returns>
        public static Solid Difference(Solid a, Solid b)
        {
            var allFaces = Intersect(a, b);

            var s = new Solid();
            foreach (var p in allFaces.Where(o => o.Item2 == SetClassification.AOutsideB).Select(o => o.Item1))
            {
                s.AddFace(p);
            }

            foreach (var p in allFaces.Where(o => o.Item2 == SetClassification.BInsideA).Select(o => o.Item1))
            {
                s.AddFace(p.Reversed());
            }

            return s;
        }

        /// <summary>
        /// Compute the intersection of two solids.
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns></returns>
        public static Solid Intersection(Solid a, Solid b)
        {
            var allFaces = Intersect(a, b);

            var s = new Solid();
            foreach (var p in allFaces.Where(o => o.Item2 == SetClassification.AInsideB || o.Item2 == SetClassification.BInsideA).Select(o => o.Item1))
            {
                s.AddFace(p);
            }

            return s;
        }

        private static List<(Polygon, SetClassification)> Intersect(Solid a, Solid b)
        {
            var r = new Random();
            var allFaces = new List<(Polygon, SetClassification)>();

            var aFaces = a.Faces.Select(f => f.Value.Outer.ToPolygon()).ToList();
            var bFaces = b.Faces.Select(f => f.Value.Outer.ToPolygon()).ToList();

            foreach (var af in aFaces)
            {
                var classification = af.IntersectAndClassify(r, bFaces, out _, out _);
                foreach (var c in classification)
                {
                    allFaces.Add((c.Item1, c.Item2 == LocalClassification.Outside ? SetClassification.AOutsideB : SetClassification.AInsideB));
                }
            }

            foreach (var bf in bFaces)
            {
                var classification = bf.IntersectAndClassify(r, aFaces, out _, out _);
                foreach (var c in classification)
                {
                    allFaces.Add((c.Item1, c.Item2 == LocalClassification.Outside ? SetClassification.BOutsideA : SetClassification.BInsideA));
                }
            }

            return allFaces;
        }
    }
}