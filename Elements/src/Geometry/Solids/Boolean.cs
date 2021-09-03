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

            var result = MergeCoplanarFaces(allFaces, Union);
            if (result != null)
            {
                foreach (var p in result)
                {
                    s.AddFace(p, mergeVerticesAndEdges: true);
                }
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

            var result = MergeCoplanarFaces(allFaces, Difference);
            if (result != null)
            {
                foreach (var p in result)
                {
                    s.AddFace(p, mergeVerticesAndEdges: true);
                }
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

            var result = MergeCoplanarFaces(allFaces, Intersect);
            if (result != null)
            {
                foreach (var p in result)
                {
                    s.AddFace(p, mergeVerticesAndEdges: true);
                }
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

            // TODO: Don't create polygons. Operate on the loops and edges directly.
            var aFaces = a.Faces.Select(f => f.Value.Outer.ToPolygon().TransformedPolygon(aTransform)).ToList();
            var bFaces = b.Faces.Select(f => f.Value.Outer.ToPolygon().TransformedPolygon(bTransform)).ToList();

            foreach (var af in aFaces)
            {
                var classifications = af.IntersectAndClassify(bFaces,
                                                              out _,
                                                              out _,
                                                              SetClassification.AOutsideB,
                                                              SetClassification.AInsideB,
                                                              SetClassification.ACoplanarB);
                allFaces.AddRange(classifications);
            }

            foreach (var bf in bFaces)
            {
                var classifications = bf.IntersectAndClassify(aFaces,
                                                              out _,
                                                              out _,
                                                              SetClassification.BOutsideA,
                                                              SetClassification.BInsideA,
                                                              SetClassification.BCoplanarA);
                allFaces.AddRange(classifications);
            }

            return allFaces;
        }

        private static List<Polygon> MergeCoplanarFaces(List<(Polygon, SetClassification)> allFaces,
                                                        Func<Polygon, Polygon, List<Polygon>> merge)
        {
            var aCoplanar = allFaces.Where(f => f.Item2 == SetClassification.ACoplanarB).GroupBy(x => x.Item1.Normal());
            var bCoplanar = allFaces.Where(f => f.Item2 == SetClassification.BCoplanarA).GroupBy(x => x.Item1.Normal());

            foreach (var aCoplanarFaceSet in aCoplanar)
            {
                foreach (var aFace in aCoplanarFaceSet)
                {
                    var bCoplanarFaceSet = bCoplanar.FirstOrDefault(x => x.Key == aCoplanarFaceSet.Key);

                    if (bCoplanarFaceSet != null)
                    {
                        foreach (var bFace in bCoplanarFaceSet)
                        {
                            return merge(aFace.Item1, bFace.Item1);
                        }
                    }
                }
            }
            return null;
        }

        private static List<Polygon> Union(Polygon a, Polygon b)
        {
            var segments = SetOperations.ClassifySegments2d(a, b, ((Vector3, Vector3, SetClassification classification) e) =>
                                            {
                                                return e.classification == SetClassification.AOutsideB || e.classification == SetClassification.BOutsideA;
                                            });
            var graph = SetOperations.BuildGraph(segments, SetClassification.None);
            return graph.Polygonize();
        }

        private static List<Polygon> Difference(Polygon a, Polygon b)
        {
            var segments = SetOperations.ClassifySegments2d(a, b, ((Vector3, Vector3, SetClassification classification) e) =>
                                            {
                                                return e.classification == SetClassification.AOutsideB || e.classification == SetClassification.BInsideA;
                                            });
            for (var i = 0; i < segments.Count; i++)
            {
                if (segments[i].classification == SetClassification.BInsideA)
                {
                    // Flip b inside a segments so that we get a graph
                    // that is correctly wound.
                    segments[i] = (segments[i].to, segments[i].from, SetClassification.BInsideA);
                }
            }
            var graph = SetOperations.BuildGraph(segments, SetClassification.None);
            return graph.Polygonize();
        }

        private static List<Polygon> Intersect(Polygon a, Polygon b)
        {
            var segments = SetOperations.ClassifySegments2d(a, b, ((Vector3, Vector3, SetClassification classification) e) =>
                                            {
                                                return e.classification == SetClassification.AInsideB || e.classification == SetClassification.BInsideA;
                                            });
            var graph = SetOperations.BuildGraph(segments, SetClassification.None);
            return graph.Polygonize();
        }
    }
}