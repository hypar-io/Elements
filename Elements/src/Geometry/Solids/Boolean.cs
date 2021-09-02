using System.Collections.Generic;
using System;
using System.Linq;

namespace Elements.Geometry.Solids
{
    /// <summary>
    /// Boolean operations on solids.
    /// </summary>
    public partial class Solid
    {
        /// <summary>
        /// Compute the union of two solids.
        /// </summary>
        /// <param name="a"></param>
        /// <param name="aTransform"></param>
        /// <param name="b"></param>
        /// <param name="bTransform"></param>
        /// <returns></returns>
        public static Solid Union(Solid a, Transform aTransform, Solid b, Transform bTransform)
        {
            var allFaces = Intersect(a, aTransform, b, bTransform);

            var s = new Solid();
            foreach (var p in allFaces.Where(o => o.Item2 == SetClassification.AOutsideB || o.Item2 == SetClassification.BOutsideA).Select(o => o.Item1))
            {
                s.AddFace(p, mergeVerticesAndEdges: true);
            }

            return s;
        }

        /// <summary>
        /// Compute the union of two solid operations.
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns></returns>
        public static Solid Union(SolidOperation a, SolidOperation b)
        {
            return Union(a.Solid, a.LocalTransform, b.Solid, b.LocalTransform);
        }

        /// <summary>
        /// Compute the difference of two solids.
        /// </summary>
        /// <param name="a"></param>
        /// <param name="aTransform"></param>
        /// <param name="b"></param>
        /// <param name="bTransform"></param>
        /// <returns></returns>
        public static Solid Difference(Solid a, Transform aTransform, Solid b, Transform bTransform)
        {
            var allFaces = Intersect(a, aTransform, b, bTransform);

            var s = new Solid();
            foreach (var p in allFaces.Where(o => o.Item2 == SetClassification.AOutsideB).Select(o => o.Item1))
            {
                s.AddFace(p, mergeVerticesAndEdges: true);
            }

            foreach (var p in allFaces.Where(o => o.Item2 == SetClassification.BInsideA).Select(o => o.Item1))
            {
                s.AddFace(p.Reversed(), mergeVerticesAndEdges: true);
            }

            return s;
        }

        /// <summary>
        /// Compute the difference of two solid operations.
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns></returns>
        public static Solid Difference(SolidOperation a, SolidOperation b)
        {
            return Difference(a.Solid, a.LocalTransform, b.Solid, b.LocalTransform);
        }

        /// <summary>
        /// Compute the intersection of two solids.
        /// </summary>
        /// <param name="a"></param>
        /// <param name="aTransform"></param>
        /// <param name="b"></param>
        /// <param name="bTransform"></param>
        /// <returns></returns>
        public static Solid Intersection(Solid a, Transform aTransform, Solid b, Transform bTransform)
        {
            var allFaces = Intersect(a, aTransform, b, bTransform);

            var s = new Solid();
            foreach (var p in allFaces.Where(o => o.Item2 == SetClassification.AInsideB || o.Item2 == SetClassification.BInsideA).Select(o => o.Item1))
            {
                s.AddFace(p, mergeVerticesAndEdges: true);
            }

            return s;
        }

        /// <summary>
        /// Compute the intersection of two solid operations.
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns></returns>
        public static Solid Intersection(SolidOperation a, SolidOperation b)
        {
            return Intersection(a.Solid, a.LocalTransform, b.Solid, b.LocalTransform);
        }

        private static List<(Polygon, SetClassification)> Intersect(Solid a, Transform aTransform, Solid b, Transform bTransform)
        {
            var allFaces = new List<(Polygon, SetClassification)>();

            var aFaces = a.Faces.Select(f => f.Value.Outer.ToPolygon().TransformedPolygon(aTransform)).ToList();
            var bFaces = b.Faces.Select(f => f.Value.Outer.ToPolygon().TransformedPolygon(bTransform)).ToList();

            foreach (var af in aFaces)
            {
                var classification = af.IntersectAndClassify(bFaces, out _, out _);
                foreach (var c in classification)
                {
                    allFaces.Add((c.Item1, c.Item2 == LocalClassification.Outside ? SetClassification.AOutsideB : SetClassification.AInsideB));
                }
            }

            foreach (var bf in bFaces)
            {
                var classification = bf.IntersectAndClassify(aFaces, out _, out _);
                foreach (var c in classification)
                {
                    allFaces.Add((c.Item1, c.Item2 == LocalClassification.Outside ? SetClassification.BOutsideA : SetClassification.BInsideA));
                }
            }

            return allFaces;
        }
    }
}