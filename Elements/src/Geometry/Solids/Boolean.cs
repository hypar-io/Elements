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
        /// <param name="a">The first solid.</param>
        /// <param name="aTransform">A local transformation of a.</param>
        /// <param name="b">The second solid.</param>
        /// <param name="bTransform">A local transformation of b.</param>
        /// <returns>A solid which is the union of a and b.</returns>
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
                foreach (var (perimeter, holes) in result)
                {
                    s.AddFace(perimeter, holes, mergeVerticesAndEdges: true);
                }
            }

            return s;
        }

        /// <summary>
        /// Compute the union of two solid operations.
        /// </summary>
        /// <param name="a">The first solid.</param>
        /// <param name="b">The second solid.</param>
        /// <returns>A solid which is the union of a and b.</returns>
        public static Solid Union(SolidOperation a, SolidOperation b)
        {
            return Union(a.Solid, a.LocalTransform, b.Solid, b.LocalTransform);
        }

        /// <summary>
        /// Compute the difference of two solids.
        /// </summary>
        /// <param name="a">The first solid.</param>
        /// <param name="aTransform">A local transformation of a.</param>
        /// <param name="b">The second solid.</param>
        /// <param name="bTransform">A local transformation of b.</param>
        /// <returns>A solid which is the difference of a and b.</returns>
        public static Solid Difference(Solid a, Transform aTransform, Solid b, Transform bTransform)
        {
            var allFaces = Intersect(a, aTransform, b, bTransform);

            var s = new Solid();

            // Group the faces according to their classification.
            // AOutsideB for everything outside B which should remain.
            // AInsideB for everything inside A which should become a hole.
            var outsideFaces = allFaces.Where(o => o.classification == SetClassification.AOutsideB).ToList();

            // TODO: The following is a hack because our Polygon.IntersectOneToMany
            // method returns overlapping polygons where there are disjoint polygons.
            var insideFaces = allFaces.Where(i => i.classification == SetClassification.AInsideB).ToList();

            foreach (var (polygon, classification, coplanarClassification) in outsideFaces)
            {
                if (insideFaces.Count == 0)
                {
                    s.AddFace(polygon, mergeVerticesAndEdges: true);
                }
                else
                {
                    var plane = polygon._plane;
                    var holes = new List<Polygon>();
                    foreach (var insideFace in insideFaces)
                    {
                        if (polygon.Contains3D(insideFace.polygon))
                        {
                            // We need to do this edge overlap check to ensure
                            // that we're not making a profile where the openings
                            // overlaps the edges of the perimeter, creating a face
                            // with zero thickness between the outer and inner
                            // loops.
                            var hasEdgeOverlap = false;
                            foreach (var (from, to) in polygon.Edges())
                            {
                                foreach (var edge in insideFace.polygon.Edges())
                                {
                                    if (from.DistanceTo(edge, out _).ApproximatelyEquals(0) && to.DistanceTo(edge, out _).ApproximatelyEquals(0))
                                    {
                                        hasEdgeOverlap = true;
                                        break;
                                    }
                                }
                            }

                            if (!hasEdgeOverlap)
                            {
                                if (plane.Normal.Dot(insideFace.polygon._plane.Normal).ApproximatelyEquals(1.0))
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

            // TODO: Can we invert the faces first? 
            var bInsideFaces = allFaces.Where(o => o.classification == SetClassification.BInsideA).ToList();
            foreach (var (polygon, classification, coplanarClassification) in bInsideFaces)
            {
                s.AddFace(polygon.Reversed(), mergeVerticesAndEdges: true);
            }

            var result = MergeCoplanarFaces(allFaces, Difference);
            if (result != null)
            {
                foreach (var (perimeter, holes) in result)
                {
                    s.AddFace(perimeter, holes, mergeVerticesAndEdges: true);
                }
            }

            return s;
        }

        /// <summary>
        /// Compute the difference of two solid operations.
        /// </summary>
        /// <param name="a">The first solid.</param>
        /// <param name="b">The second solid.</param>
        /// <returns>A solid which is the difference of a and b.</returns>
        public static Solid Difference(SolidOperation a, SolidOperation b)
        {
            return Difference(a.Solid, a.LocalTransform, b.Solid, b.LocalTransform);
        }

        /// <summary>
        /// Compute the intersection of two solids.
        /// </summary>
        /// <param name="a">The first solid.</param>
        /// <param name="aTransform">A local transformation of a.</param>
        /// <param name="b">The second solid.</param>
        /// <param name="bTransform">A local transformation of b.</param>
        /// <returns>A solid which is the the intersection of a and b.</returns>
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
                foreach (var (perimeter, holes) in result)
                {
                    s.AddFace(perimeter, holes, mergeVerticesAndEdges: true);
                }
            }

            return s;
        }

        /// <summary>
        /// Compute the intersection of two solid operations.
        /// </summary>
        /// <param name="a">The first solid.</param>
        /// <param name="b">The second solid.</param>
        /// <returns>A solid which is the the intersection of a and b.</returns>
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

            // An explanation of what's going on here.
            // At a high level, the act of intersecting two solids is the intersection and splitting
            // of the solid's faces against those of the other solid, and the classification of the 
            // split pieces as:
            // A inside B (ex. a piece of A which is inside B)
            // A outside B
            // B inside A
            // B outside A
            // A coplanar B
            // B coplanar A

            // 1. Sort the faces of a and b into two collections -> non-coplanar faces, and co-planar faces.
            // 2. Split and classify each non-coplanar face of a against all the faces of b which it intersects.
            // 3. Split and classify each non-coplanar face of b against all the faces of a which it intersects.
            // 3. Intersect coplanar faces or return faces which are contained in other coplanar faces. Coplanar faces
            // contained within other faces, will become holes.

            var aFaces = a.Faces.Select(f => f.Value.Outer.ToPolygon(aTransform)).ToList();
            var bFaces = b.Faces.Select(f => f.Value.Outer.ToPolygon(bTransform)).ToList();

            var aCoplanarFaces = aFaces.Where(af => bFaces.Any(bf => bf._plane.IsCoplanar(af._plane))).ToList();
            var bCoplanarFaces = bFaces.Where(bf => aFaces.Any(af => af._plane.IsCoplanar(bf._plane))).ToList();

            var aNonCoplanar = aFaces.Except(aCoplanarFaces).ToList();
            var bNonCoplanar = bFaces.Except(bCoplanarFaces).ToList();

            foreach (var af in aNonCoplanar)
            {
                var classifications = af.IntersectAndClassify(bNonCoplanar.Where(bf => bf._bounds.Intersects(af._bounds)).ToList(),
                                                              bFaces,
                                                              out _,
                                                              out _,
                                                              SetClassification.AOutsideB,
                                                              SetClassification.AInsideB);
                allFaces.AddRange(classifications);
            }

            foreach (var bf in bNonCoplanar)
            {
                var classifications = bf.IntersectAndClassify(aNonCoplanar.Where(af => af._bounds.Intersects(bf._bounds)).ToList(),
                                                              aFaces,
                                                              out _,
                                                              out _,
                                                              SetClassification.BOutsideA,
                                                              SetClassification.BInsideA);
                allFaces.AddRange(classifications);
            }

            var aCoplanarFaceSets = aCoplanarFaces.GroupBy(af => af._plane.Normal);
            var bCoplanarFaceSets = bCoplanarFaces.GroupBy(af => af._plane.Normal);

            foreach (var aCoplanarFaceSet in aCoplanarFaceSets)
            {
                foreach (var aFace in aCoplanarFaceSet)
                {
                    var bCoplanarFaceSet = bCoplanarFaceSets.FirstOrDefault(x => x.Key == aCoplanarFaceSet.Key);

                    if (bCoplanarFaceSet != null)
                    {
                        foreach (var bFace in bCoplanarFaceSet)
                        {
                            // The b face is completely contained within the a face, or is outside of it.
                            // In the former case, we return the face to become a hole. In the latter case,
                            // we return the face to become a stand-alone face.
                            if (aFace.Contains3D(bFace) || !aFace._bounds.Intersects(bFace._bounds))
                            {
                                var aa = (aFace, SetClassification.None, CoplanarSetClassification.ACoplanarB);
                                var bb = (bFace, SetClassification.None, CoplanarSetClassification.BCoplanarA);
                                if (!allFaces.Contains(aa))
                                {
                                    allFaces.Add(aa);
                                }
                                if (!allFaces.Contains(bb))
                                {
                                    allFaces.Add(bb);
                                }
                            }
                            // The b face intersects the a face. We do a planar intersection of the two faces, which
                            // adds new vertices to the a face and the b face. These faces are marked as coplanar,
                            // for later 2D boolean operations.
                            else if (aFace.Intersects2d(bFace, out List<(Vector3 result, int aSegumentIndices, int bSegmentIndices)> planarIntersectionResults, false))
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

        /// <summary>
        /// Merge all coplanar faces using the provided merge delegate.
        /// </summary>
        /// <param name="allFaces">A collection of tuples of coplanar polygon information.</param>
        /// <param name="merge">The delegate that will be used to merge the two polygons.</param>
        private static List<(Polygon perimeter, List<Polygon> holes)> MergeCoplanarFaces(List<(Polygon polygon, SetClassification setClassification, CoplanarSetClassification coplanarClassification)> allFaces,
                                                        Func<Polygon, Polygon, List<Polygon>> merge)
        {
            var aCoplanar = allFaces.Where(f => f.coplanarClassification == CoplanarSetClassification.ACoplanarB).GroupBy(x => x.polygon._plane.Normal);
            var bCoplanar = allFaces.Where(f => f.coplanarClassification == CoplanarSetClassification.BCoplanarA).GroupBy(x => x.polygon._plane.Normal);

            var results = new List<(Polygon, List<Polygon>)>();

            foreach (var aCoplanarFaceSet in aCoplanar)
            {
                foreach (var aFace in aCoplanarFaceSet)
                {
                    var plane = aFace.polygon._plane;

                    // Find all the b faces that are coplanar with the a face.
                    var bCoplanarFaceSet = bCoplanar.FirstOrDefault(x => x.Key == aCoplanarFaceSet.Key);

                    if (bCoplanarFaceSet != null)
                    {
                        var holes = new List<Polygon>();
                        foreach (var bFace in bCoplanarFaceSet)
                        {
                            if (aFace.polygon.Contains3D(bFace.polygon))
                            {
                                if (plane.Normal.Dot(bFace.polygon._plane.Normal).ApproximatelyEquals(1.0))
                                {
                                    holes.Add(bFace.polygon.Reversed());
                                }
                                else
                                {
                                    holes.Add(bFace.polygon);
                                }
                            }
                            else
                            {
                                var mergeResults = merge(aFace.polygon, bFace.polygon);
                                if (mergeResults != null)
                                {
                                    results.AddRange(mergeResults.Select(p => (p, new List<Polygon>())));
                                }
                            }
                        }

                        if (holes.Count > 0)
                        {
                            results.Add((aFace.polygon, holes));
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