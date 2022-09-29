using System;
using System.Collections.Generic;
using Elements.Geometry;
using Elements.Geometry.Solids;
using Elements.Spatial;

namespace Elements
{
    /// <summary>
    /// Operations on sets of edges.
    /// </summary>
    public static class SetOperations
    {
        /// <summary>
        /// Intersect all segments in each polygon against all segments
        /// in the other polygon, splitting segments, and classify 
        /// all the resulting segments.
        /// </summary>
        public static List<(Vector3 from, Vector3 to, SetClassification classification)> ClassifySegments2d(Polygon a,
                                                                                                            Polygon b,
                                                                                                            Func<(Vector3 from, Vector3 to, SetClassification classification), bool> filter = null)
        {
            var ap = a._plane;
            var bp = b._plane;
            if (!ap.IsCoplanar(bp))
            {
                throw new System.Exception("Set classification failed. The polygons are not coplanar.");
            }

            var classifications = new List<(Vector3 from, Vector3 to, SetClassification classification)>();

            ClassifySet(ap, a, b, Elements.SetClassification.AInsideB, Elements.SetClassification.AOutsideB, classifications, filter);
            ClassifySet(ap, b, a, Elements.SetClassification.BInsideA, Elements.SetClassification.BOutsideA, classifications, filter);

            return classifications;
        }

        private static void ClassifySet(Plane p,
                                        Polygon a,
                                        Polygon b,
                                        SetClassification insideClassification,
                                        SetClassification outsideClassification,
                                        List<(Vector3 from, Vector3 to, SetClassification classification)> classifications,
                                        Func<(Vector3 from, Vector3 to, SetClassification classification), bool> filter)
        {
            // A tests
            for (var i = 0; i < a.Vertices.Count; i++)
            {
                var intersections = 0;
                var v1 = a.Vertices[i];
                var v2 = i == a.Vertices.Count - 1 ? a.Vertices[0] : a.Vertices[i + 1];

                // The perpendicular vector to the edge is used as the test vector.
                var d = (v2 - v1).Unitized();
                var ray = new Ray(v1.Average(v2), d.Cross(p.Normal));

                // B tests
                for (var j = 0; j < b.Vertices.Count; j++)
                {
                    var v3 = b.Vertices[j];
                    var v4 = j == b.Vertices.Count - 1 ? b.Vertices[0] : b.Vertices[j + 1];
                    if (ray.Intersects(v3, v4, out _))
                    {
                        intersections++;
                    }
                }
                if (intersections % 2 == 0)
                {
                    var edge = (v1, v2, outsideClassification);
                    if (filter != null)
                    {
                        if (filter(edge))
                        {
                            classifications.Add(edge);
                        }
                    }
                    else
                    {
                        classifications.Add(edge);
                    }
                }
                else
                {
                    var edge = (v1, v2, insideClassification);
                    if (filter != null)
                    {
                        if (filter(edge))
                        {
                            classifications.Add(edge);
                        }
                    }
                    else
                    {
                        classifications.Add(edge);
                    }
                }
            }
        }

        /// <summary>
        /// Build a half edge graph from a collection of segments.
        /// </summary>
        /// <param name="set">A collection of classified segments.</param>
        /// <param name="boundaryClassifications">A list of boundary classifications where edges
        /// will be created running in both directions. This is required for situations where the graph will
        /// be expected to produce multiple polygons with shared edges.</param>
        public static HalfEdgeGraph2d BuildGraph(List<(Vector3 from, Vector3 to, SetClassification classification)> set,
                                                 IList<SetClassification> boundaryClassifications = null)
        {
            var graphVertices = new List<Vector3>();
            var graphEdges = new List<List<(int from, int to, int? tag)>>();

            // Create edges
            foreach (var (from, to, classification) in set)
            {
                var a = Solid.FindOrCreateGraphVertex(from, graphVertices, graphEdges);
                var b = Solid.FindOrCreateGraphVertex(to, graphVertices, graphEdges);
                var e1 = (a, b, 0);
                var e2 = (b, a, 0);
                if (graphEdges[a].Contains(e1) || graphEdges[b].Contains(e2))
                {
                    continue;
                }
                else
                {
                    graphEdges[a].Add(e1);
                    if (boundaryClassifications != null && boundaryClassifications.Contains(classification))
                    {
                        graphEdges[b].Add(e2);
                    }
                }
            }

            var heg = new HalfEdgeGraph2d()
            {
                Vertices = graphVertices,
                EdgesPerVertex = graphEdges
            };

            return heg;
        }
    }
}