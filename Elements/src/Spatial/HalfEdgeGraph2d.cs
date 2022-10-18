using System;
using System.Collections.Generic;
using System.Linq;
using Elements.Geometry;

namespace Elements.Spatial
{
    /// <summary>
    /// Represents a 2D Graph with Half-edge connectivity, useful for finding polygons
    /// bounded by intersecting or overlapping edges.
    /// </summary>
    public class HalfEdgeGraph2d
    {
        internal HalfEdgeGraph2d()
        {
            Vertices = new List<Vector3>();
            EdgesPerVertex = new List<List<(int from, int to, int? tag)>>();
        }

        /// <summary>
        /// The unique vertices of this graph
        /// </summary>
        /// <value></value>
        public List<Vector3> Vertices { get; set; }

        /// <summary>
        /// The index pairs, grouped by starting vertex, representing unique half edges.
        /// </summary>
        public List<List<(int from, int to, int? tag)>> EdgesPerVertex { get; set; }

        /// <summary>
        /// Construct a 2D Half Edge Graph from a polygon and an intersecting polyline.
        /// </summary>
        /// <param name="pg">The polygon.</param>
        /// <param name="pl">The polyline.</param>
        public static HalfEdgeGraph2d Construct(Polygon pg, Polyline pl)
        {
            return Construct(new[] { pg }, new[] { pl });
        }

        /// <summary>
        /// Construct a 2D Half Edge Graph from a collection of polygons and a collection of intersecting polylines.
        /// </summary>
        /// <param name="pg">The polygons.</param>
        /// <param name="pl">The polylines.</param>
        public static HalfEdgeGraph2d Construct(IEnumerable<Polygon> pg, IEnumerable<Polyline> pl)
        {
            var plArray = pl.ToArray();
            var plSegments = pl.SelectMany(p => p.Segments()).ToArray();
            var graph = new HalfEdgeGraph2d();
            var vertices = graph.Vertices;
            var edgesPerVertex = graph.EdgesPerVertex;

            // Check each polygon segment against each polyline segment for intersections. 
            // Build up a half-edge structure.

            // for each segment, store a list of vertices. If an intersection is found, additional vertices will be added to the list for that segment.

            var polylineSplitPoints = plSegments.Select(p => new List<Vector3> { p.Start, p.End }).ToArray();
            // first we check polyline-polyline intersections, and add those to split points
            var plCount = plArray.Length;
            if (plCount > 1)
            {
                var flatListPosition = 0;
                for (int i = 0; i < plCount - 1; i++)
                {
                    // check each segment in this polyline with all segments starting after this polyline
                    // to avoid checking for intersections between a polyline and its own segments.
                    // flatListPosition keeps track of how far along the flat list of segments we should start.
                    var segmentsA = plArray[i].Segments();
                    for (int j = flatListPosition; j < plSegments.Count(); j++)
                    {
                        var otherSegment = plSegments[j];
                        for (int segAIndex = 0; segAIndex < segmentsA.Length; segAIndex++)
                        {
                            Line segA = (Line)segmentsA[segAIndex];
                            if (segA.Intersects(otherSegment, out var intersectionPt, false, true))
                            {
                                polylineSplitPoints[flatListPosition + segAIndex].Add(intersectionPt);
                                polylineSplitPoints[j].Add(intersectionPt);
                            }
                        }
                    }
                    flatListPosition += segmentsA.Count();
                }
            }
            // next we check each polygon against all polyline segments
            foreach (var polygon in pg)
            {
                var pgSegments = polygon.Segments();
                for (int i = 0; i < pgSegments.Length; i++)
                {
                    var polygonSegment = pgSegments[i];
                    // collect the vertices of each segment — if an intersection is found, additional vertices will be added to this list.
                    var polygonSegmentSplitPoints = new List<Vector3> { polygonSegment.Start, polygonSegment.End };
                    for (int j = 0; j < plSegments.Length; j++)
                    {
                        var polylineSegment = plSegments[j];
                        if (polygonSegment.Intersects(polylineSegment, out var intersectionPt, false, true))
                        {
                            polylineSplitPoints[j].Add(intersectionPt);
                            polygonSegmentSplitPoints.Add(intersectionPt);
                        }
                    }
                    // sort the unique polygon edge vertices along the segment's length, and start the halfEdge graph.
                    var pgIntersectionsOrdered = polygonSegmentSplitPoints.Distinct().OrderBy(sp => sp.DistanceTo(polygonSegment.Start)).ToArray();
                    for (int k = 0; k < pgIntersectionsOrdered.Length - 1; k++)
                    {
                        var from = pgIntersectionsOrdered[k];
                        var to = pgIntersectionsOrdered[k + 1];
                        var fromIndex = vertices.FindIndex(v => v.IsAlmostEqualTo(from));
                        if (fromIndex == -1)
                        {
                            fromIndex = vertices.Count;
                            vertices.Add(from);
                            edgesPerVertex.Add(new List<(int from, int to, int? tag)>());
                        }
                        var toIndex = vertices.FindIndex(v => v.IsAlmostEqualTo(to));
                        if (toIndex == -1)
                        {
                            toIndex = vertices.Count;
                            vertices.Add(to);
                            edgesPerVertex.Add(new List<(int from, int to, int? tag)>());
                        }
                        // only add one set of polygon halfEdges, so we don't wind up with an outer loop.
                        if (fromIndex != toIndex && !edgesPerVertex[fromIndex].Contains((fromIndex, toIndex, null)))
                        {
                            edgesPerVertex[fromIndex].Add((fromIndex, toIndex, null));
                        }
                    }
                }
            }
            // do the same with the polyline's vertices — sort and add to the halfEdge graph.
            foreach (var splitSet in polylineSplitPoints)
            {
                var splitSetOrdered = splitSet.Distinct().OrderBy(v => v.DistanceTo(splitSet[0])).ToArray();
                for (int i = 0; i < splitSetOrdered.Length - 1; i++)
                {
                    var from = splitSetOrdered[i];
                    var to = splitSetOrdered[i + 1];

                    var fromIndex = vertices.FindIndex(v => v.IsAlmostEqualTo(from));
                    if (fromIndex == -1)
                    {
                        fromIndex = vertices.Count;
                        vertices.Add(from);
                        edgesPerVertex.Add(new List<(int from, int to, int? tag)>());
                    }
                    var toIndex = vertices.FindIndex(v => v.IsAlmostEqualTo(to));
                    if (toIndex == -1)
                    {
                        toIndex = vertices.Count;
                        vertices.Add(to);
                        edgesPerVertex.Add(new List<(int from, int to, int? tag)>());
                    }
                    // add both half edges for polyline segments. If we have a splitter 
                    // lying exactly on a polygon edge, don't add it. 
                    if (fromIndex != toIndex && !edgesPerVertex[fromIndex].Contains((fromIndex, toIndex, null)) && !edgesPerVertex[toIndex].Contains((toIndex, fromIndex, null)))
                    {
                        edgesPerVertex[fromIndex].Add((fromIndex, toIndex, null));
                        edgesPerVertex[toIndex].Add((toIndex, fromIndex, null));
                    }
                }
            }
            return graph;
        }

        /// <summary>
        /// Construct a 2D half-edge graph from a collection of lines.
        /// </summary>
        /// <param name="lines">The line segments from which to construct the graph.</param>
        /// <param name="bothWays">If true, each line will create two half edges — one running each way.</param>
        public static HalfEdgeGraph2d Construct(IEnumerable<Line> lines, bool bothWays = false)
        {
            var graph = new HalfEdgeGraph2d();
            var vertices = graph.Vertices;
            var edgesPerVertex = graph.EdgesPerVertex;

            foreach (var line in lines)
            {
                var fromIndex = vertices.FindIndex(v => v.IsAlmostEqualTo(line.Start));
                if (fromIndex == -1)
                {
                    fromIndex = vertices.Count;
                    vertices.Add(line.Start);
                    edgesPerVertex.Add(new List<(int from, int to, int? tag)>());
                }
                var toIndex = vertices.FindIndex(v => v.IsAlmostEqualTo(line.End));
                if (toIndex == -1)
                {
                    toIndex = vertices.Count;
                    vertices.Add(line.End);
                    edgesPerVertex.Add(new List<(int from, int to, int? tag)>());
                }
                if (fromIndex != toIndex && !edgesPerVertex[fromIndex].Contains((fromIndex, toIndex, null)))
                {
                    edgesPerVertex[fromIndex].Add((fromIndex, toIndex, null));
                }

                if (bothWays && fromIndex != toIndex && !edgesPerVertex[toIndex].Contains((toIndex, fromIndex, null)))
                {
                    edgesPerVertex[toIndex].Add((toIndex, fromIndex, null));
                }

            }
            return graph;
        }

        /// <summary>
        /// Calculate the closed polygons in this graph.
        /// </summary>
        /// <param name="predicate">A predicate used during the final step of polygonization to determine if edges are
        /// valid.</param>
        /// <param name="normal">The normal of the plane in which graph traversal for polygon construction will occur.
        /// If no normal is provided, the +Z axis is used.</param>
        /// <returns>A collection of polygons.</returns>
        public List<Polygon> Polygonize(Func<int?, bool> predicate = null, Vector3 normal = default(Vector3))
        {
            return Polygonize(predicate, (points) => new Polygon(points), normal);
        }

        /// <summary>
        /// A generic polygonizer which can be used to construct different data results from the polygonization process —
        /// for instance, if your polygons are not planar, you can create polylines from them, or output point collections directly.
        /// </summary>
        internal List<T> Polygonize<T>(Func<int?, bool> predicate = null, Func<IList<Vector3>, T> resultProcess = null, Vector3 normal = default)
        {
            var edgesPerVertex = new List<List<(int from, int to, int? tag)>>(this.EdgesPerVertex);
            var vertices = this.Vertices;
            var newPolygons = new List<T>();

            // construct polygons from half edge graph.
            // remove edges from edgesPerVertex as they get "consumed" by a polygon,
            // and stop when you run out of edges. 
            // Guranteed to terminate because every loop step removes at least 1 edge, and
            // edges are never added.
            while (edgesPerVertex.Any(l => l.Count > 0))
            {
                var currentEdgeList = GetEdgeList(edgesPerVertex, vertices, normal);
                var currentVertexList = new List<Vector3>();

                // remove duplicate edges in the same new polygon, 
                // which will occur if we have a polyline that doesn't cross all the way through.
                var validEdges = new List<(int from, int to, int? tag)>(currentEdgeList);
                int i = 0;
                // guaranteed to terminate, since at every step we either increment i by one, or make validEdges.Count smaller by 2 (and decrement i by 1).
                // validEdges.Count-i always gets smaller, every step, until 0. 
                while (validEdges.Count > 0 && i < validEdges.Count)
                {
                    var index = (i + validEdges.Count) % validEdges.Count;
                    var nextIndex = (i + 1 + validEdges.Count) % validEdges.Count;
                    var thisEdge = validEdges[index];
                    var nextEdge = validEdges[nextIndex];
                    if (thisEdge.from == nextEdge.to)
                    {
                        // we found a degenerate section — two duplicate edges, joined at a vertex. 
                        // we remove the two duplicate edges. we have to do this in a descending sorted order 
                        // so the removal of the first one doesn't shift the position of the second one,
                        // and if we're straddling the end of the list eg (5, 0), "nextIndex" is before "index". 
                        foreach (var indexToRemove in new[] { index, nextIndex }.OrderByDescending(v => v))
                        {
                            validEdges.RemoveAt(indexToRemove);
                        }
                        // it's conceivable that the two other edges on either side of these removed edges are ALSO identical.
                        // in this case, we actually step backwards — to compare "the one before the first one we just removed" and
                        // "the one after the second one we just removed", which will now be adjacent in the list. 
                        i--;
                        // if we are at the end of the list, we have to step backwards again, because we removed the last edge.
                        if (i == validEdges.Count)
                        {
                            i--;
                        }
                    }
                    else
                    {
                        i++;
                    }
                }

                if (predicate != null)
                {
                    if (validEdges.Any(e => predicate(e.tag)))
                    {
                        continue;
                    }
                }

                foreach (var edge in validEdges)
                {
                    currentVertexList.Add(vertices[edge.to]);
                }
                // if we have a wholly-contained polyline, this cleanup can result in a totally empty list,
                // so we check before trying to construct a polygon.
                if (currentVertexList.Count > 0)
                {
                    newPolygons.Add(resultProcess(currentVertexList));
                }
            }
            return newPolygons;
        }

        /// <summary>
        /// Calculate the closed polylines in this graph.
        /// </summary>
        /// <param name="predicate">A predicate used during the final step of polylinization to determine if edges are
        /// valid.</param>
        /// <param name="normal">The normal of the plane in which graph traversal for polyline construction will occur.
        /// If no normal is provided, the +Z axis is used.</param>
        /// <returns>A collection of polylines.</returns>
        public List<Polyline> Polylinize(Func<int?, bool> predicate = null, Vector3 normal = default(Vector3))
        {
            return Polylinize(predicate, (points) => new Polyline(points), normal);
        }

        /// <summary>
        /// A generic polylinizer which can be used to construct different data results from the polylinization process —
        /// for instance, if your polylines are not planar, you can create polylines from them, or output point collections directly.
        /// </summary>
        internal List<T> Polylinize<T>(Func<int?, bool> predicate = null, Func<IList<Vector3>, T> resultProcess = null, Vector3 normal = default)
        {
            var edgesPerVertex = new List<List<(int from, int to, int? tag)>>(this.EdgesPerVertex);
            var vertices = this.Vertices;
            var newPolylines = new List<T>();

            // construct polylines from half edge graph.
            // remove edges from edgesPerVertex as they get "consumed" by a polyline,
            // and stop when you run out of edges. 
            // Guranteed to terminate because every loop step removes at least 1 edge, and
            // edges are never added.
            while (edgesPerVertex.Any(l => l.Count > 0))
            {
                var currentEdgeList = GetEdgeList(edgesPerVertex, vertices, normal, true);

                if (predicate != null)
                {
                    if (currentEdgeList.Any(e => predicate(e.tag)))
                    {
                        continue;
                    }
                }

                var currentVertexList = new List<Vector3>();
                foreach (var edge in currentEdgeList)
                {
                    currentVertexList.Add(vertices[edge.from]);
                    currentVertexList.Add(vertices[edge.to]);
                }

                // if we have a wholly-contained polyline, this cleanup can result in a totally empty list,
                // so we check before trying to construct a polyline.
                if (currentVertexList.Count > 0)
                {
                    newPolylines.Add(resultProcess(currentVertexList));
                }
            }

            return newPolylines;
        }

        /// <summary>
        /// Return edge list using picking the next segment forming the largest counter-clockwise angle with edge opposite
        /// </summary>
        private List<(int from, int to, int? tag)> GetEdgeList(List<List<(int from, int to, int? tag)>> edgesPerVertex, List<Vector3> vertices, Vector3 normal = default, bool mergePolygons = false)
        {
            var currentEdgeList = new List<(int from, int to, int? tag)>();
            // pick a starting point
            var startingSet = edgesPerVertex.First(l => l.Count > 0);
            var currentSegment = startingSet[0];
            startingSet.RemoveAt(0);
            var initialFrom = currentSegment.from;
            var cannotBeSelectedEdgeList = new List<(int from, int to, int? tag)>();

            // loop until we reach the point at which we started for this polyline loop.
            // Since we have a finite set of edges, and we consume / remove every edge we traverse,
            // we must eventually either find an edge that points back to our start, or hit
            // a dead end where no more edges are available (in which case we throw an exception) 
            while (currentSegment.to != initialFrom)
            {
                currentEdgeList.Add(currentSegment);
                var toVertex = vertices[currentSegment.to];
                var fromVertex = vertices[currentSegment.from];

                var vectorToTest = fromVertex - toVertex;
                // get all segments pointing outwards from our "to" vertex
                var possibleNextSegments = edgesPerVertex[currentSegment.to];
                if (possibleNextSegments.Count == 0)
                {
                    // this should never happen.
                    throw new Exception("Something went wrong building polylines from split results. Unable to proceed.");
                }
                // at every node, we pick the next segment forming the largest counter-clockwise angle with our opposite.
                var n = normal == default ? Vector3.ZAxis : normal;
                var nextSegment = possibleNextSegments.OrderBy(cand => vectorToTest.PlaneAngleTo(vertices[cand.to] - vertices[cand.from], n)).Last();

                possibleNextSegments.Remove(nextSegment);
                currentSegment = nextSegment;

                // if there are polygons intersecting at the starting point with the current polygon, make one polygon from them along the outer border
                if (currentSegment.to == initialFrom && mergePolygons)
                {
                    // if the angle is obtuse, then it is the inner border of the polygon
                    if (vectorToTest.PlaneAngleTo(vertices[currentSegment.to] - vertices[currentSegment.from], n) > 180)
                    {
                        continue;
                    }

                    toVertex = vertices[currentSegment.to];
                    fromVertex = vertices[currentSegment.from];

                    vectorToTest = fromVertex - toVertex;
                    possibleNextSegments = edgesPerVertex[currentSegment.to];
                    if (possibleNextSegments.Count == 0)
                    {
                        continue;
                    }

                    var innerBorderSegments = possibleNextSegments.Where(cand => vectorToTest.PlaneAngleTo(vertices[cand.to] - vertices[cand.from], n) == 0);
                    if (innerBorderSegments.Count() > 0)
                    {
                        cannotBeSelectedEdgeList.AddRange(innerBorderSegments);
                    }

                    var outerBorderSegments = possibleNextSegments.Where(cand => !cannotBeSelectedEdgeList.Contains(cand))
                                                                  .OrderBy(cand => vectorToTest.PlaneAngleTo(vertices[cand.to] - vertices[cand.from], n));
                    if (outerBorderSegments.Count() > 0)
                    {
                        currentEdgeList.Add(currentSegment);
                        nextSegment = outerBorderSegments.Last();

                        possibleNextSegments.Remove(nextSegment);
                        currentSegment = nextSegment;
                    }
                }
            }
            currentEdgeList.Add(currentSegment);

            return currentEdgeList;
        }
    }
}