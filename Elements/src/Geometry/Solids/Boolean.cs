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
            foreach (var p in allFaces.Where(o => o.classification == SetClassification.AOutsideB || o.classification == SetClassification.BOutsideA).Select(o => o.polygon))
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
        public static Solid Difference(Solid a, Transform aTransform, Solid b, Transform bTransform)
        {
            var allFaces = Intersect(a, aTransform, b, bTransform);

            var s = new Solid();

            var outsideFaces = allFaces.Where(o => o.classification == SetClassification.AOutsideB).ToList();

            // Group the faces according to their classification.
            // AOutsideB for everything outside B which should remain.
            // AInsideB for everything inside A which should become a hole.
            foreach (var (polygon, classification, coplanarClassification) in outsideFaces)
            {
                // TODO: The following is a hack because our Polygon.IntersectOneToMany
                // method returns overlapping polygons where there are disjoint polygons.
                var insideFaces = allFaces.Where(i => i.classification == SetClassification.AInsideB).ToList();

                if (insideFaces.Count == 0)
                {
                    s.AddFace(polygon, mergeVerticesAndEdges: true);
                }
                else
                {
                    var plane = polygon.Plane();
                    var holes = new List<Polygon>();
                    foreach (var insideFace in insideFaces)
                    {
                        var plane1 = insideFace.polygon.Plane();
                        if (plane1.IsCoplanar(plane))
                        {
                            // We need to do this containment check to ensure
                            // that we're not making a profile where the openings
                            // overlaps the edges of the profile, creating a face
                            // with zero thickness between the outer and inner
                            // loops.

                            if (polygon.Contains(insideFace.polygon))
                            {
                                if (plane.Normal.Dot(plane1.Normal).ApproximatelyEquals(1.0))
                                {
                                    holes.Add(insideFace.polygon.Reversed());
                                }
                                else
                                {
                                    holes.Add(insideFace.polygon);
                                }
                            }
                        }
                    }
                    s.AddFace(polygon, holes, mergeVerticesAndEdges: true);
                }
            }

            foreach (var p in allFaces.Where(o => o.classification == SetClassification.BInsideA).Select(o => o.polygon))
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
            foreach (var p in allFaces.Where(o => o.classification == SetClassification.AInsideB || o.classification == SetClassification.BInsideA).Select(o => o.polygon))
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

        private static List<(Polygon polygon, SetClassification classification, CoplanarSetClassification coplanarClassification)> Intersect(Solid a,
                                                                                                                                             Transform aTransform,
                                                                                                                                             Solid b,
                                                                                                                                             Transform bTransform)
        {
            var allFaces = new List<(Polygon, SetClassification, CoplanarSetClassification)>();

            // TODO: Don't create polygons. Operate on the loops and edges directly.
            // TODO: Support holes. We drop the inner loop information here currently.
            var aFaces = a.Faces.Select(f => f.Value.Outer.ToPolygon().TransformedPolygon(aTransform)).ToList();
            var bFaces = b.Faces.Select(f => f.Value.Outer.ToPolygon().TransformedPolygon(bTransform)).ToList();

            var aCoplanarFaces = aFaces.Where(af => bFaces.Any(bf => bf.Plane().IsCoplanar(af.Plane()))).ToList();
            var bCoplanarFaces = bFaces.Where(bf => aFaces.Any(af => af.Plane().IsCoplanar(bf.Plane()))).ToList();

            var aNonCoplanar = aFaces.Except(aCoplanarFaces).ToList();
            var bNonCoplanar = bFaces.Except(bCoplanarFaces).ToList();

            foreach (var af in aNonCoplanar)
            {
                var classifications = af.IntersectAndClassify(bNonCoplanar,
                                                              out _,
                                                              out _,
                                                              SetClassification.AOutsideB,
                                                              SetClassification.AInsideB);
                allFaces.AddRange(classifications);
            }

            foreach (var bf in bNonCoplanar)
            {
                var classifications = bf.IntersectAndClassify(aNonCoplanar,
                                                              out _,
                                                              out _,
                                                              SetClassification.BOutsideA,
                                                              SetClassification.BInsideA);
                allFaces.AddRange(classifications);
            }

            var aCoplanarFaceSets = aCoplanarFaces.GroupBy(af => af.Normal());
            var bCoplanarFaceSets = bCoplanarFaces.GroupBy(af => af.Normal());

            foreach (var aCoplanarFaceSet in aCoplanarFaceSets)
            {
                foreach (var aFace in aCoplanarFaceSet)
                {
                    var bCoplanarFaceSet = bCoplanarFaceSets.FirstOrDefault(x => x.Key == aCoplanarFaceSet.Key);

                    if (bCoplanarFaceSet != null)
                    {
                        foreach (var bFace in bCoplanarFaceSet)
                        {
                            if (aFace.Intersects2d(bFace, out List<(Vector3 result, int aSegumentIndices, int bSegmentIndices)> planarIntersectionResults, false))
                            {
                                var result = planarIntersectionResults.Select(r => r.result).ToList();
                                aFace.Split(result);
                                allFaces.Add((aFace, SetClassification.None, CoplanarSetClassification.ACoplanarB));
                                bFace.Split(result);
                                allFaces.Add((bFace, SetClassification.None, CoplanarSetClassification.BCoplanarA));
                            }
                        }
                    }
                }
            }

            return allFaces;
        }

        private static List<Polygon> MergeCoplanarFaces(List<(Polygon polygon, SetClassification setClassification, CoplanarSetClassification coplanarClassification)> allFaces,
                                                        Func<Polygon, Polygon, List<Polygon>> merge)
        {
            var aCoplanar = allFaces.Where(f => f.coplanarClassification == CoplanarSetClassification.ACoplanarB).GroupBy(x => x.polygon.Normal());
            var bCoplanar = allFaces.Where(f => f.coplanarClassification == CoplanarSetClassification.BCoplanarA).GroupBy(x => x.polygon.Normal());

            var results = new List<Polygon>();

            foreach (var aCoplanarFaceSet in aCoplanar)
            {
                foreach (var aFace in aCoplanarFaceSet)
                {
                    var bCoplanarFaceSet = bCoplanar.FirstOrDefault(x => x.Key == aCoplanarFaceSet.Key);

                    if (bCoplanarFaceSet != null)
                    {
                        foreach (var bFace in bCoplanarFaceSet)
                        {
                            var mergeResults = merge(aFace.polygon, bFace.polygon);
                            if (mergeResults != null)
                            {
                                results.AddRange(mergeResults);
                            }
                        }
                    }
                }
            }
            return results;
        }

        private static List<Polygon> Union(Polygon a, Polygon b)
        {
            var segments = SetOperations.ClassifySegments2d(a, b, ((Vector3, Vector3, SetClassification classification) e) =>
                                            {
                                                return e.classification == SetClassification.AOutsideB || e.classification == SetClassification.BOutsideA;
                                            });
            var graph = SetOperations.BuildGraph(segments);
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
            var graph = SetOperations.BuildGraph(segments);
            return graph.Polygonize();
        }

        private static List<Polygon> Intersect(Polygon a, Polygon b)
        {
            var segments = SetOperations.ClassifySegments2d(a, b, ((Vector3, Vector3, SetClassification classification) e) =>
                                            {
                                                return e.classification == SetClassification.AInsideB || e.classification == SetClassification.BInsideA;
                                            });
            var graph = SetOperations.BuildGraph(segments);
            return graph.Polygonize();
        }
    }
}