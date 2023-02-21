using Elements.Geometry;
using Elements.Search;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Elements.Spatial.AdaptiveGrid
{
    /// <summary>
    /// A graph like edge-vertex structure with planar spaces connected by vertical edges.
    /// The grid doesn't do any intersections when new sections are added, they are stitched
    /// only by common vertices. Make sure that regions that are added into the graph are
    /// aligned with respect to boundaries and split points.
    /// </summary>
    /// <example>
    /// [!code-csharp[Main](../../Elements/test/AdaptiveGridTests.cs?name=example)]
    /// </example>
    public class AdaptiveGrid
    {
        #region Private fields

        private ulong _edgeId = 1; // we start at 1 because 0 is returned as default value from dicts

        private ulong _vertexId = 1; // we start at 1 because 0 is returned as default value from dicts

        /// <summary>
        /// Vertices by ID.
        /// </summary>
        [JsonProperty]
        private Dictionary<ulong, Vertex> _vertices = new Dictionary<ulong, Vertex>();

        /// <summary>
        /// Edges by ID.
        /// </summary>
        [JsonProperty]
        private Dictionary<ulong, Edge> _edges = new Dictionary<ulong, Edge>();

        // See Edge.GetHash for how edges are identified as unique.
        private Dictionary<string, ulong> _edgesLookup = new Dictionary<string, ulong>();

        // Vertex lookup by x, y, z coordinate.
        private Dictionary<double, Dictionary<double, Dictionary<double, ulong>>> _verticesLookup = new Dictionary<double, Dictionary<double, Dictionary<double, ulong>>>();

        #endregion

        #region Private enums

        /// <summary>
        /// Convenient way to store information if some number is smaller, larger or inside a number range.
        /// </summary>
        private enum PointOrientation
        {
            Low,
            Inside,
            Hi
        }

        #endregion

        #region Properties

        /// <summary>
        /// Tolerance for points being considered the same.
        /// Applies individually to X, Y, and Z coordinates, not the cumulative difference!
        /// Tolerance is twice the epsilon to make sure graph has no cracks when new sections are added.
        /// </summary>
        public double Tolerance { get; } = Vector3.EPSILON * 2;

        /// <summary>
        /// Maximum distance for line segments of hints lines to extend to other existing edges.
        /// </summary>
        public double HintExtendDistance { get; set; } = 3;

        /// <summary>
        /// Transformation with which planar spaces are aligned
        /// </summary>
        public Transform Transform { get; set; }

        /// <summary>
        /// Grid boundary used in obstacle perimeter clipping. Can be null.
        /// </summary>
        public Polygon Boundaries { get; set; }

        #endregion

        #region Constructors

        /// <summary>
        /// Create default AdaptiveGrid
        /// </summary>
        /// <returns></returns>
        public AdaptiveGrid()
        {
            Transform = new Transform();
        }

        /// <summary>
        /// Create an AdaptiveGrid with custom transformation.
        /// </summary>
        /// <param name="transform">Transformation, grid is aligned with.</param>
        /// <returns></returns>
        public AdaptiveGrid(Transform transform)
        {
            Transform = transform;
        }

        #endregion

        #region Public logic

        /// <summary>
        /// Add graph section using bounding box, divided by a set of key points.
        /// Key points don't respect "MinimumResolution" at the moment.
        /// Any vertices that already exist are not created but reused.
        /// This way new region is connected with the rest of the graph.
        /// </summary>
        /// <param name="bBox">Box which region is populated with graph.</param>
        /// <param name="keyPoints">Set of 3D points, region is split with.</param>
        /// <example>
        /// [!code-csharp[Main](../../Elements/test/AdaptiveGridTests.cs?name=example2)]
        /// </example>
        public void AddFromBbox(BBox3 bBox, List<Vector3> keyPoints)
        {
            var height = bBox.Max.Z - bBox.Min.Z;
            var boundary = new Polygon(new List<Vector3>
            {
                new Vector3(bBox.Min.X, bBox.Min.Y),
                new Vector3(bBox.Min.X, bBox.Max.Y),
                new Vector3(bBox.Max.X, bBox.Max.Y),
                new Vector3(bBox.Max.X, bBox.Min.Y)
            }).TransformedPolygon(new Transform(new Vector3(0, 0, bBox.Min.Z)));
            AddFromExtrude(boundary, Vector3.ZAxis, height, keyPoints);
        }

        /// <summary>
        /// Add graph section using polygon, extruded in given direction.
        /// Any vertices that already exist are not created but reused.
        /// This way new region is connected with the rest of the graph.
        /// </summary>
        /// <param name="boundingPolygon">Base polygon</param>
        /// <param name="extrusionAxis">Extrusion direction</param>
        /// <param name="distance">Height of polygon extrusion</param>
        /// <param name="keyPoints">Set of 3D points, region is split with.</param>
        public void AddFromExtrude(Polygon boundingPolygon, Vector3 extrusionAxis, double distance, List<Vector3> keyPoints)
        {
            var gridZ = new Grid1d(new Line(boundingPolygon.Start, boundingPolygon.Start + distance * extrusionAxis));
            gridZ.SplitAtPoints(keyPoints);
            var edgesBefore = GetEdges();

            var zCells = gridZ.GetCells();
            for (var i = 0; i < zCells.Count; i++)
            {
                var elevationVector = zCells[i].Domain.Min * extrusionAxis;
                var transformedPolygonBottom = boundingPolygon.TransformedPolygon(new Transform(elevationVector));
                var grid = CreateGridFromPolygon(transformedPolygonBottom);
                SplitGrid(grid, keyPoints);
                SplitGridAtIntersectionPoints(boundingPolygon, grid, edgesBefore);
                var addedEdges = AddFromGridWithBoundingPolygon(grid, transformedPolygonBottom, edgesBefore);
                AddVerticalEdges(extrusionAxis, zCells[i].Domain.Length, addedEdges);
                if (i == zCells.Count - 1)
                {
                    var transformedPolygonTop = boundingPolygon.TransformedPolygon(
                        new Transform(zCells[i].Domain.Max * extrusionAxis));
                    grid = CreateGridFromPolygon(transformedPolygonTop);
                    SplitGrid(grid, keyPoints);
                    SplitGridAtIntersectionPoints(boundingPolygon, grid, edgesBefore);
                    AddFromGridWithBoundingPolygon(grid, transformedPolygonTop, edgesBefore);
                }
            }
        }

        /// <summary>
        /// Add single planar region to the graph section using polygon.
        /// Any vertices that already exist are not created but reused.
        /// This way new region is connected with the rest of the graph.
        /// </summary>
        /// <param name="boundingPolygon">Base polygon</param>
        /// <param name="keyPoints">Set of 3D points, region is split with.</param>
        /// <returns></returns>
        public HashSet<Edge> AddFromPolygon(Polygon boundingPolygon, IEnumerable<Vector3> keyPoints)
        {
            var grid = CreateGridFromPolygon(boundingPolygon);
            var edgesBefore = GetEdges();
            SplitGrid(grid, keyPoints);
            SplitGridAtIntersectionPoints(boundingPolygon, grid, edgesBefore);
            return AddFromGrid(grid, edgesBefore);
        }

        /// <summary>
        /// Add single planar region to the graph section using polygon.
        /// Any vertices that already exist are not created but reused.
        /// This way new region is connected with the rest of the graph.
        /// </summary>
        /// <param name="boundingPolygon">Base polygon</param>
        /// <param name="keyPoints">Set of 3D points, region is split with.</param>
        /// <returns></returns>
        public HashSet<Edge> AddFromPolygonMk2(Polygon boundingPolygon, IEnumerable<Vector3> keyPoints)
        {
            var grid = CreateGridFromPolygon(boundingPolygon);
            var edgesBefore = GetEdges();
            SplitGrid(grid, keyPoints);
            SplitGridAtIntersectionPoints(boundingPolygon, grid, edgesBefore);
            return AddFromGridWithBoundingPolygon(grid, boundingPolygon, edgesBefore);
        }

        /// <summary>
        /// Intersect the grid with a list of obstacles.
        /// </summary>
        /// <param name="obstacles">List of obstacles.</param>
        /// <returns>True if any obstacle intersects with any edge on the grid.</returns>
        public bool SubtractObstacles(IEnumerable<Obstacle> obstacles)
        {
            bool intersected = false;
            foreach (var obstacle in obstacles)
            {
                intersected &= SubtractObstacle(obstacle);
            }
            return intersected;
        }

        /// <summary>
        /// Intersect the grid with an obstacle, defined from a set of points with offset.
        /// </summary>
        /// <param name="obstacle">Obstacle object.</param>
        /// <returns>True if obstacle intersects with any edge on the grid.</returns>
        public bool SubtractObstacle(Obstacle obstacle)
        {
            var frame = obstacle.Orientation == null ? Transform : obstacle.Orientation;
            var toGrid = frame.Inverted();
            List<Vector3> localPoints = obstacle.Points.Select(p => toGrid.OfPoint(p)).ToList();
            BBox3 localBox = new BBox3(localPoints);

            var edgesToDelete = new List<Edge>();
            var edgesToAdd = new List<(Vertex Anchor, Edge Edge, Vector3 New)>();

            foreach (var edge in GetEdges())
            {
                var start = GetVertex(edge.StartId);
                var end = GetVertex(edge.EndId);
                var localStartP = toGrid.OfPoint(start.Point);
                var localEndP = toGrid.OfPoint(end.Point);

                //Z coordinates and X/Y are treated differently.
                //If edge lies on one of X or Y planes of the box - it's not treated as "Inside" and edge is kept.
                //If edge lies on one of Z planes - it's still "Inside", so edge is cut or removed.
                //This is because we don't want travel under or over obstacles on elevation where they start/end.
                if (!IsLineInDomain((localStartP, localEndP), (localBox.Min, localBox.Max),
                    -Tolerance, 0, out bool startInside, out bool endInside))
                {
                    continue;
                }

                if (startInside && endInside)
                {
                    edgesToDelete.Add(edge);
                }
                else
                {
                    var localLine = new Line(localStartP, localEndP);
                    List<Vector3> intersections;
                    localLine.Intersects(localBox, out intersections, tolerance: Tolerance);
                    if (intersections.Count == 1)
                    {
                        //Need to find which end is inside the box.
                        //If none - we just touched the corner
                        var intersection = frame.OfPoint(intersections[0]);
                        if (startInside)
                        {
                            edgesToDelete.Add(edge);
                            edgesToAdd.Add((end, edge, intersection));
                        }
                        else if (endInside)
                        {
                            edgesToDelete.Add(edge);
                            edgesToAdd.Add((start, edge, intersection));
                        }
                        else
                        {
                            edgesToAdd.Add((null, edge, intersection));
                        }
                    }
                    else if (intersections.Count == 2)
                    {
                        edgesToDelete.Add(edge);
                        var startIntersection = frame.OfPoint(intersections[0]);
                        var endIntersection = frame.OfPoint(intersections[1]);
                        edgesToAdd.Add((start, edge, startIntersection));
                        edgesToAdd.Add((end, edge, endIntersection));
                    }
                }
            }

            //TODO: this code builds perimeters, elevation by elevation, but do not connect them vertically.
            if (obstacle.AddPerimeterEdges && edgesToAdd.Any())
            {
                var corners = localBox.Corners().Take(4).Select(c => frame.OfPoint(c)).ToList();
                var intersectionsByElevations = edgesToAdd.GroupBy(
                    e => e.New.Z, new DoubleToleranceComparer(Tolerance));
                foreach (var group in intersectionsByElevations)
                {
                    var intersections = group.Select(i => i.New);

                    var plane = new Plane(new Vector3(0, 0, group.Key), Vector3.ZAxis);
                    var cornersAtElevation = corners.Select(
                        c => c.ProjectAlong(frame.ZAxis, plane)).ToList();

                    AddEdgesOnLine(cornersAtElevation[0], cornersAtElevation[1], intersections, obstacle.AllowOutsideBoudary);
                    AddEdgesOnLine(cornersAtElevation[1], cornersAtElevation[2], intersections, obstacle.AllowOutsideBoudary);
                    AddEdgesOnLine(cornersAtElevation[2], cornersAtElevation[3], intersections, obstacle.AllowOutsideBoudary);
                    AddEdgesOnLine(cornersAtElevation[3], cornersAtElevation[0], intersections, obstacle.AllowOutsideBoudary);

                    foreach (var item in group)
                    {
                        if (item.Anchor != null)
                        {
                            if (!item.Anchor.Point.IsAlmostEqualTo(item.New, Tolerance))
                            {
                                Vertex v = AddVertex(item.New);
                                AddInsertEdge(v.Id, item.Anchor.Id);
                            }
                        }
                        else
                        {
                            CutEdge(item.Edge, item.New);
                        }
                    }
                }
            }

            foreach (var e in edgesToDelete)
            {
                RemoveEdge(e);
            }

            return edgesToDelete.Any();
        }

        /// <summary>
        /// Get a Vertex by its ID.
        /// </summary>
        /// <param name="vertexId"></param>
        /// <returns></returns>
        public Vertex GetVertex(ulong vertexId)
        {
            this._vertices.TryGetValue(vertexId, out var vertex);
            return vertex;
        }

        /// <summary>
        /// Get all Vertices.
        /// </summary>
        /// <returns></returns>
        public List<Vertex> GetVertices()
        {
            return this._vertices.Values.ToList();
        }

        /// <summary>
        /// Get all Edges.
        /// </summary>
        /// <returns></returns>
        public List<Edge> GetEdges()
        {
            return this._edges.Values.ToList();
        }

        /// <summary>
        /// Whether a vertex location already exists in the AdaptiveGrid.
        /// </summary>
        /// <param name="point"></param>
        /// <param name="id">The ID of the Vertex, if a match is found.</param>
        /// <param name="tolerance">Amount of tolerance in the search against each component of the coordinate.</param>
        /// <returns></returns>
        public bool TryGetVertexIndex(Vector3 point, out ulong id, double? tolerance = null)
        {
            var zDict = GetAddressParent(_verticesLookup, point, tolerance: tolerance);
            if (zDict == null)
            {
                id = 0;
                return false;
            }
            return TryGetValue(zDict, point.Z, out id, tolerance);
        }

        /// <summary>
        /// Add a Vertex or return existing one if it's withing grid tolerance.
        /// Doesn't connect new Vertex to the grid with edges.
        /// </summary>
        /// <param name="point">Position of required vertex</param>
        /// <returns>New or existing Vertex.</returns>
        public Vertex AddVertex(Vector3 point)
        {
            if (!TryGetVertexIndex(point, out var id, Tolerance))
            {
                var zDict = GetAddressParent(_verticesLookup, point, true, Tolerance);
                id = this._vertexId;
                var vertex = new Vertex(id, point);
                zDict[point.Z] = id;
                _vertices[id] = vertex;
                this._vertexId++;
            }

            return GetVertex(id);
        }

        /// <summary>
        /// Add a Vertex and connect in to one or more other vertices.
        /// </summary>
        /// <param name="point">Position of required Vertex.</param>
        /// <param name="strategy">Vertex insertion strategy.</param>
        /// <param name="cut">Should new edges be intersected with existing edges.</param>
        /// <returns>New Vertex or existing one if it's within grid tolerance.</returns>
        public Vertex AddVertex(Vector3 point, IAddVertexStrategy strategy, bool cut = true)
        {
            return strategy.Add(this, point, cut);
        }

        /// <summary>
        /// Execution style for AddVertices function.
        /// Each option performs more operations than the previous one.
        /// </summary>
        public enum VerticesInsertionMethod
        {
            /// <summary>
            /// Just put vertices into the grid without connecting them.
            /// Inserted vertices are returned in order.
            /// </summary>
            Insert,

            /// <summary>
            /// Insert vertices and connect them to each other.
            /// Inserted vertices are returned in order.
            /// </summary>
            Connect,

            /// <summary>
            /// Insert vertices and connect them to each other.
            /// Find any intersections between new edges.
            /// Inserted vertices are returned in order including self intersection vertices twice. 
            /// 
            /// </summary>
            ConnectAndSelfIntersect,

            /// <summary>
            /// Insert vertices and connect them to each other.
            /// New vertices are inserted where new edges intersect with existing edges.
            /// All vertices are returned in order from first vertex to the last including all intersection vertices.
            /// </summary>
            ConnectAndCut,

            /// <summary>
            /// Insert vertices and connect them to each other.
            /// New vertices are inserted where new edges intersect with existing edges.
            /// Each vertex is extended in direction of two neighbor edges until first hit.
            /// Extensions are done even if vertex is already on an edge.
            /// All vertices are returned in order from first vertex to the last including all intersection and extension vertices.
            /// </summary>
            ConnectCutAndExtend
        }

        /// <summary>
        /// Create a chain of vertices. Exact behavior depends on the method used. 
        /// </summary>
        /// <param name="points">List of points to insert. Must have at least two points.</param>
        /// <param name="method">Insertion method.</param>
        /// <returns>Vertices in order between provided points. Depends on used method.</returns>
        public List<Vertex> AddVertices(IList<Vector3> points, VerticesInsertionMethod method)
        {
            if (points.Count < 2)
            {
                throw new ArgumentException("At least two points required");
            }

            if (method == VerticesInsertionMethod.ConnectCutAndExtend)
            {
                return AddVerticesWithCustomExtension(points, HintExtendDistance);
            }

            var vertices = new List<Vertex>();
            vertices.Add(AddVertex(points[0]));
            for (int i = 1; i < points.Count; i++)
            {
                if (points[i].IsAlmostEqualTo(vertices.Last().Point, Tolerance))
                {
                    continue;
                }

                var tailVertex = AddVertex(points[i]);
                if (method == VerticesInsertionMethod.ConnectAndCut)
                {
                    var edges = AddCutEdge(vertices.Last().Id, tailVertex.Id);
                    var lastId = vertices.Last().Id;
                    foreach (var e in edges)
                    {
                        var otherId = e.StartId == lastId ? e.EndId : e.StartId;
                        vertices.Add(GetVertex(otherId));
                        lastId = otherId;
                    }
                }
                else if (method == VerticesInsertionMethod.ConnectAndSelfIntersect)
                {
                    AddInsertEdge(tailVertex.Id, vertices.Last().Id);
                    for (int j = 0; j < vertices.Count - 1; j++)
                    {
                        if (Line.Intersects(vertices.Last().Point, tailVertex.Point,
                                            vertices[j].Point, vertices[j + 1].Point,
                                            out var intersection))
                        {
                            var cross = AddVertex(intersection, new ConnectVertexStrategy(
                                tailVertex, vertices.Last(), vertices[j], vertices[j + 1]), cut: false);
                            RemoveEdge(tailVertex.GetEdge(vertices.Last().Id));
                            RemoveEdge(vertices[j].GetEdge(vertices[j + 1].Id));
                            vertices.Insert(j + 1, cross);
                            vertices.Add(cross);
                            j++;
                        }
                    }
                    vertices.Add(tailVertex);
                }
                else if (method == VerticesInsertionMethod.Connect)
                {
                    AddInsertEdge(tailVertex.Id, vertices.Last().Id);
                    vertices.Add(tailVertex);
                }
                else if (method == VerticesInsertionMethod.Insert)
                {
                    vertices.Add(tailVertex);
                }
            }
            return vertices;
        }

        /// <summary>
        /// Split provided edge by given point. Edge is removed and replaced by two new edges.
        /// New vertex position is not required to be in the edge line.
        /// </summary>
        /// <param name="edge">Edge to cut.</param>
        /// <param name="position">Cut position where new Vertex is created.</param>
        /// <returns>New Vertex at cut position.</returns>
        public Vertex CutEdge(Edge edge, Vector3 position)
        {
            var startVertex = GetVertex(edge.StartId);
            var endVertex = GetVertex(edge.EndId);
            if (!position.IsAlmostEqualTo(startVertex.Point, Tolerance) &&
                !position.IsAlmostEqualTo(endVertex.Point, Tolerance))
            {
                var newVertex = AddVertex(position, new ConnectVertexStrategy(startVertex, endVertex));
                RemoveEdge(edge);
                return newVertex;
            }
            return null;
        }

        /// <summary>
        /// Get associated Vertices.
        /// </summary>
        /// <returns></returns>
        public List<Vertex> GetVertices(Edge edge)
        {
            return new List<Vertex>() { GetVertex(edge.StartId), GetVertex(edge.EndId) };
        }

        /// <summary>
        /// Get the geometry that represents this Edge or DirectedEdge.
        /// </summary>
        /// <returns></returns>
        public Line GetLine(Edge edge)
        {
            return new Line(GetVertex(edge.StartId).Point, GetVertex(edge.EndId).Point);
        }

        /// <summary>
        /// Find closest Vertex on the grid to given location.
        /// If several vertices are no the same closest distance - first found is returned.
        /// </summary>
        /// <param name="location">Position to which closest Vertex is searched.</param>
        /// <returns>Closest Vertex</returns>
        public Vertex ClosestVertex(Vector3 location)
        {
            double lowestDist = double.MaxValue;
            Vertex closest = null;
            foreach (var v in GetVertices())
            {
                double dist = v.Point.DistanceTo(location);
                if (dist < lowestDist)
                {
                    lowestDist = dist;
                    closest = v;
                }
            }
            return closest;
        }

        /// <summary>
        /// Find closest Edge on the grid to given location.
        /// If several edges are no the same closest distance - first found is returned.
        /// </summary>
        /// <param name="location">Position to which closest Vertex is searched.</param>
        /// <param name="point">Closest point of the found edge line.</param>
        /// <returns>Closest Edge</returns>
        public Edge ClosestEdge(Vector3 location, out Vector3 point)
        {
            double lowestDist = double.MaxValue;
            Edge closestEdge = null;
            point = Vector3.Origin;
            foreach (var e in GetEdges())
            {
                var start = GetVertex(e.StartId);
                var end = GetVertex(e.EndId);
                double dist = location.DistanceTo((start.Point, end.Point), out var closest);
                if (dist < lowestDist)
                {
                    lowestDist = dist;
                    closestEdge = e;
                    point = closest;
                }
            }
            return closestEdge;
        }

        /// <summary>
        /// Add an edge between two vertices represented by their ids.
        /// </summary>
        /// <param name="vertexId1">Id of the first vertex.</param>
        /// <param name="vertexId2">Id of the second vertex.</param>
        /// <param name="cut">Intersect new edge with existing edges.</param>
        /// <returns>Edges between two vertices. Single if cut is false.</returns>
        public List<Edge> AddEdge(ulong vertexId1, ulong vertexId2, bool cut = true)
        {
            if (cut)
            {
                return AddCutEdge(vertexId1, vertexId2);
            }
            else
            {
                return new List<Edge> { AddInsertEdge(vertexId1, vertexId2) };
            }
        }

        /// <summary>
        /// Add an edge between two vertices.
        /// </summary>
        /// <param name="a">First vertex.</param>
        /// <param name="b">Second vertex.</param>
        /// <param name="cut">Intersect new edge with existing edges.</param>
        /// <returns>Edges between two vertices. Single if cut is false.</returns>
        public List<Edge> AddEdge(Vertex a, Vertex b, bool cut = true)
        {
            return AddEdge(a.Id, b.Id, cut);
        }

        /// <summary>
        /// Add an edge between two vertices represented by their position.
        /// Positions that are not yet present in the grid are created as new vertices.
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <param name="cut">Intersect new edge with existing edges.</param>
        /// <returns>Edges between two vertices. Single if cut is false.</returns>
        public List<Edge> AddEdge(Vector3 a, Vector3 b, bool cut = true)
        {
            var vertexA = AddVertex(a);
            var vertexB = AddVertex(b);
            return AddEdge(vertexA.Id, vertexB.Id, cut);
        }

        /// <summary>
        /// Remove the Vertex from the grid.
        /// All it's edges are removed as well, including any neighbor
        /// vertices that are left without edges.
        /// </summary>
        /// <param name="v">Vertex to delete.</param>
        public void RemoveVertex(Vertex v)
        {
            //If there are no edges - delete the vertex manually.
            if (!v.Edges.Any())
            {
                DeleteVertex(v.Id);
                return;
            }

            //Otherwise, edges will remove it's orphan vertices.
            foreach (var edge in v.Edges.ToList())
            {
                RemoveEdge(edge);
            }
        }

        /// <summary>
        /// Remove the Edge from the grid.
        /// </summary>
        /// <param name="edge">Edge to delete</param>
        public void RemoveEdge(Edge edge)
        {
            var hash = Edge.GetHash(new List<ulong> { edge.StartId, edge.EndId });
            if (!this._edgesLookup.Remove(hash))
            {
                return;
            }

            if (!this._edges.Remove(edge.Id))
            {
                return;
            }

            var startVertexEdges = this.GetVertex(edge.StartId).Edges;
            startVertexEdges.Remove(edge);
            if (!startVertexEdges.Any())
            {
                DeleteVertex(edge.StartId);
            }
            var endVertexEdges = this.GetVertex(edge.EndId).Edges;
            endVertexEdges.Remove(edge);
            if (!endVertexEdges.Any())
            {
                DeleteVertex(edge.EndId);
            }
        }

        /// <summary>
        /// Store points of edges both vertices of which are located at the given plane.
        /// Use with InsertSnapshot to duplicate vertices to a new elevation,
        /// while allowing modification of the original edges before duplication takes place.
        /// </summary>
        /// <param name="plane">Plane to retrieve edges from.</param>
        /// <param name="edgesToCheck">Optional. Edges to check, all by default .</param>
        /// <returns>Position pair for each edge stored.</returns>
        public List<(Vector3 Start, Vector3 End)> SnapshotEdgesOnPlane(
            Plane plane, IEnumerable<Edge> edgesToCheck = null)
        {
            if (edgesToCheck == null)
            {
                edgesToCheck = GetEdges();
            }

            List<(Vector3, Vector3)> snapshot = new List<(Vector3, Vector3)>();
            foreach (var e in edgesToCheck)
            {
                var sv = GetVertex(e.StartId);
                var ev = GetVertex(e.EndId);
                if (sv != null && Math.Abs(sv.Point.DistanceTo(plane)) < Tolerance &&
                    ev != null && Math.Abs(ev.Point.DistanceTo(plane)) < Tolerance)
                {
                    snapshot.Add((sv.Point, ev.Point));
                }
            }
            return snapshot;
        }

        /// <summary>
        /// Duplicate stored edges with transformation applied.
        /// Use with InsertSnapshot to move a list of existing or previously existed edges to the new location,
        /// for example, copy edges from one elevation to another.
        /// </summary>
        /// <param name="storedEdges">Edge positions to duplicate.</param>
        /// <param name="transform">Transformation to apply to all of the new edges.</param>
        /// <param name="connect">Optional. Connect each new vertex with it's original vertex if it still exist.</param>
        public void InsertSnapshot(
            List<(Vector3 Start, Vector3 End)> storedEdges, Transform transform, bool connect = true)
        {
            HashSet<ulong> alreadyConnected = new HashSet<ulong>();

            foreach (var (Start, End) in storedEdges)
            {
                var newSV = AddVertex(transform.OfPoint(Start));
                var newEV = AddVertex(transform.OfPoint(End));
                AddEdge(newSV.Id, newEV.Id);

                if (connect)
                {
                    // The same vertex can be part of multiple edges.
                    // Cache to avoid expensive cut operations.
                    if (!alreadyConnected.Contains(newSV.Id) && 
                        TryGetVertexIndex(Start, out var id, Tolerance))
                    {
                        AddEdge(newSV.Id, id);
                        alreadyConnected.Add(newSV.Id);
                    }

                    if (!alreadyConnected.Contains(newEV.Id) &&
                        TryGetVertexIndex(End, out id, Tolerance))
                    {
                        AddEdge(newEV.Id, id);
                        alreadyConnected.Add(newEV.Id);
                    }
                }
            }
        }

        #endregion

        #region Private logic

        /// <summary>
        /// Add an Edge or return the exiting one with given indexes.
        /// </summary>
        /// <param name="vertexId1">Index of the first Vertex</param>
        /// <param name="vertexId2">Index of the second Vertex</param>
        /// <returns>New or existing Edge.</returns>
        private Edge AddInsertEdge(ulong vertexId1, ulong vertexId2)
        {
            if (vertexId1 == vertexId2)
            {
                throw new ArgumentException("Can't create edge. The vertices of the edge cannot be the same.", $"{vertexId1}, {vertexId2}");
            }

            var hash = Edge.GetHash(new List<ulong> { vertexId1, vertexId2 });

            if (!this._edgesLookup.TryGetValue(hash, out var edgeId))
            {
                var startVertex = this.GetVertex(vertexId1);
                var endVertex = this.GetVertex(vertexId2);

                if (startVertex == null)
                {
                    throw new ArgumentException("Can't create edge. Start vertex id is not present in the grid.", $"{vertexId1}");
                }

                if (endVertex == null)
                {
                    throw new ArgumentException("Can't create edge. End vertex id is not present in the grid.", $"{vertexId2}");
                }

                var edge = new Edge(this, this._edgeId, vertexId1, vertexId2);
                edgeId = edge.Id;

                this._edgesLookup[hash] = edgeId;
                this._edges.Add(edgeId, edge);

                startVertex.Edges.Add(edge);
                endVertex.Edges.Add(edge);

                this._edgeId++;
                return edge;
            }
            else
            {
                this._edges.TryGetValue(edgeId, out var edge);
                return edge;
            }
        }

        /// <summary>
        /// Add an edge between two vertices and intersect it with other edges on the grid. 
        /// </summary>
        /// <param name="startId">Index of start vertex.</param>
        /// <param name="endId">Index of end vertex.</param>
        /// <returns>Ordered list of edges between start and end vertices.</returns>
        private List<Edge> AddCutEdge(ulong startId, ulong endId)
        {
            var startVertex = GetVertex(startId);
            var endVertex = GetVertex(endId);

            if (startVertex == null)
            {
                throw new ArgumentException("Can't create edge. Start vertex id is not present in the grid.", $"{startId}");
            }

            if (endVertex == null)
            {
                throw new ArgumentException("Can't create edge. End vertex id is not present in the grid.", $"{endId}");
            }

            var sp = startVertex.Point;
            var ep = endVertex.Point;
            List<Edge> edgesToRemove = new List<Edge>();
            var intersectionPoints = new List<Vector3>();

            foreach (var edge in GetEdges())
            {
                var edgeV0 = GetVertex(edge.StartId);
                var edgeV1 = GetVertex(edge.EndId);

                if ((startId == edgeV0.Id && endId == edgeV1.Id) ||
                    (startId == edgeV1.Id && endId == edgeV0.Id))
                {
                    continue;
                }

                (double minX, double maxX) = edgeV0.Point.X < edgeV1.Point.X ?
                    (edgeV0.Point.X, edgeV1.Point.X) : (edgeV1.Point.X, edgeV0.Point.X);
                (double minY, double maxY) = edgeV0.Point.Y < edgeV1.Point.Y ?
                    (edgeV0.Point.Y, edgeV1.Point.Y) : (edgeV1.Point.Y, edgeV0.Point.Y);
                (double minZ, double maxZ) = edgeV0.Point.Z < edgeV1.Point.Z ?
                    (edgeV0.Point.Z, edgeV1.Point.Z) : (edgeV1.Point.Z, edgeV0.Point.Z);
                //Positive tolerance means that space, tolerance outside the min max box is still considered inside.
                if (!IsLineInDomain((sp, ep), (new Vector3(minX, minY, minZ), new Vector3(maxX, maxY, maxZ)),
                    Tolerance, Tolerance, out _, out _))
                {
                    continue;
                }

                var newEdgeLine = new Line(sp, ep);
                var oldEdgeLine = new Line(edgeV0.Point, edgeV1.Point);
                if (newEdgeLine.Intersects(oldEdgeLine, out var intersectionPoint, includeEnds: true))
                {
                    intersectionPoints.Add(intersectionPoint);
                    var newVertex = AddVertex(intersectionPoint);
                    if (edge.StartId != newVertex.Id && edge.EndId != newVertex.Id)
                    {
                        AddInsertEdge(edge.StartId, newVertex.Id);
                        AddInsertEdge(edge.EndId, newVertex.Id);
                        edgesToRemove.Add(edge);
                    }
                }
                else if (oldEdgeLine.Direction().IsParallelTo(newEdgeLine.Direction()))
                {
                    var isNewEdgeStartOnOldEdge = oldEdgeLine.PointOnLine(newEdgeLine.Start);
                    var isNewEdgeEndOnOldEdge = oldEdgeLine.PointOnLine(newEdgeLine.End);
                    var isOldEdgeStartOnNewEdge = newEdgeLine.PointOnLine(oldEdgeLine.Start, true);
                    var isOldEdgeEndOnNewEdge = newEdgeLine.PointOnLine(oldEdgeLine.End, true);
                    // new edge is inside old edge
                    if (isNewEdgeStartOnOldEdge && isNewEdgeEndOnOldEdge &&
                        AddEdgeInsideExisting(edgeV0, edgeV1, startVertex, endVertex))
                    {
                        edgesToRemove.Add(edge);
                    }
                    // edges overlap
                    else if (isNewEdgeStartOnOldEdge || isNewEdgeEndOnOldEdge)
                    {
                        if (isOldEdgeEndOnNewEdge &&
                            AddEdgeOverlappingExisting(edgeV0, edgeV1, startVertex, endVertex, true))
                        {
                            edgesToRemove.Add(edge);
                        }

                        if (isOldEdgeStartOnNewEdge &&
                            AddEdgeOverlappingExisting(edgeV0, edgeV1, startVertex, endVertex, false))
                        {
                            edgesToRemove.Add(edge);
                        }
                    }
                    // old edge is inside new edge
                    else if (isOldEdgeStartOnNewEdge && isOldEdgeEndOnNewEdge)
                    {
                        intersectionPoints.Add(oldEdgeLine.Start);
                        intersectionPoints.Add(oldEdgeLine.End);
                    }
                }
            }

            List<Edge> addedEdges = null;
            if (intersectionPoints.Any())
            {
                addedEdges = AddEdgesInBetween(startVertex, endVertex,
                    intersectionPoints.OrderBy(p => p.DistanceTo(startVertex.Point)));
            }
            else
            {
                addedEdges = new List<Edge>() { AddInsertEdge(startVertex.Id, endVertex.Id) };
            }

            foreach (var edge in edgesToRemove)
            {
                RemoveEdge(edge);
            }

            return addedEdges;
        }

        private bool AddEdgeInsideExisting(Vertex oldV0, Vertex oldV1,
                                           Vertex newV0, Vertex newV1)
        {
            bool startInserted = false;
            bool endInserted = false;
            if (oldV0.Point.DistanceTo(newV0.Point) < oldV0.Point.DistanceTo(newV1.Point))
            {
                if (oldV0.Id != newV0.Id)
                {
                    AddInsertEdge(oldV0.Id, newV0.Id);
                    startInserted = true;
                }

                if (oldV1.Id != newV1.Id)
                {
                    AddInsertEdge(oldV1.Id, newV1.Id);
                    endInserted = true;
                }
            }
            else
            {
                if (oldV0.Id != newV1.Id)
                {
                    AddInsertEdge(oldV0.Id, newV1.Id);
                    endInserted = true;
                }

                if (oldV1.Id != newV0.Id)
                {
                    AddInsertEdge(oldV1.Id, newV0.Id);
                    startInserted = true;
                }
            }
            return startInserted || endInserted;
        }

        private bool AddEdgeOverlappingExisting(Vertex oldV0, Vertex oldV1,
                                                Vertex newV0, Vertex newV1,
                                                bool isOldEdgeEndOnNewEdge)
        {
            bool inserted = false;
            var oldStart = isOldEdgeEndOnNewEdge ? oldV0 : oldV1;
            var oldEnd = isOldEdgeEndOnNewEdge ? oldV1 : oldV0;
            if (oldStart.Point.DistanceTo(newV0.Point) < oldStart.Point.DistanceTo(newV1.Point))
            {
                if (newV0.Id != oldEnd.Id)
                {
                    AddInsertEdge(oldStart.Id, newV0.Id);
                    inserted = true;
                }
            }
            else
            {
                if (newV1.Id != oldEnd.Id)
                {
                    AddInsertEdge(oldStart.Id, newV1.Id);
                    inserted = true;
                }
            }
            return inserted;
        }

        private List<Edge> AddEdgesInBetween(Vertex start, Vertex end, IEnumerable<Vector3> points)
        {
            var addedEdges = new List<Edge>();
            var v1 = start;
            foreach (var p in points)
            {
                if (!v1.Point.IsAlmostEqualTo(p, Tolerance))
                {
                    var v2 = AddVertex(p);
                    addedEdges.Add(AddInsertEdge(v1.Id, v2.Id));
                    v1 = v2;
                }
            }

            if (v1 != end)
            {
                addedEdges.Add(AddInsertEdge(v1.Id, end.Id));
            }
            return addedEdges;
        }

        private void AddEdgesOnLine(Vector3 start, Vector3 end, IEnumerable<Vector3> candidates, bool allowEdgesOutsideBoudnary)
        {
            if (Boundaries != null && !allowEdgesOutsideBoudnary)
            {
                var boundary2d = new Polygon(Boundaries.Vertices.Select(v => new Vector3(v.X, v.Y)).ToList());
                var inside = new Line(new Vector3(start.X, start.Y), new Vector3(end.X, end.Y)).Trim(boundary2d, out var _);
                if (!inside.Any())
                {
                    return;
                }

                var fi = inside.First();
                start = new Vector3(fi.Start.X, fi.Start.Y, start.Z);
                end = new Vector3(fi.End.X, fi.End.Y, end.Z);
            }

            var onLine = candidates.Where(x => Line.PointOnLine(x, start, end));
            var ordered = onLine.OrderBy(x => (x - start).Dot(end - start));
            var strip = new List<Vector3>();
            strip.Add(start);
            foreach (var point in ordered)
            {
                if (!point.IsAlmostEqualTo(start, Tolerance) &&
                    !point.IsAlmostEqualTo(end, Tolerance) &&
                    !point.IsAlmostEqualTo(strip.Last(), Tolerance))
                {
                    strip.Add(point);
                }
            }
            strip.Add(end);

            AddVertices(strip, VerticesInsertionMethod.Connect);
        }

        /// <summary>
        /// Intersect points into grid and connect them into edges.
        /// New edges are intersected along intersection points.
        /// End points of each segment are extended up to given distance until the next hit on both sides.
        /// If not extended, point is connected to the grid at its position.
        /// </summary>
        /// <param name="points">Points to add and connect to the grid.</param>
        /// <param name="extendDistance">Distance at which lines are extended to existing edges.</param>
        /// <returns>Vertices in order they are inserted, including already existing. Can contain duplicates.</returns>
        public List<Vertex> AddVerticesWithCustomExtension(IList<Vector3> points, double extendDistance)
        {
            List<Vertex> vertices = new List<Vertex>();

            for (int i = 0; i < points.Count - 1; i++)
            {
                var segmentLength = (points[i + 1] - points[i]).Length();
                // Find any intersections between infinite segment and grid eges.
                var hits = IntersectGraph(points[i], points[i + 1]);
                // If none - just insert segment into grid. 
                if (!hits.Any())
                {
                    var start = AddVertex(points[i]);
                    var end = AddVertex(points[i + 1]);
                    if (!vertices.Any() || vertices.Last().Id != start.Id)
                    {
                        vertices.Add(start);
                    }

                    if (start.Id != end.Id)
                    {
                        AddEdge(points[i], points[i + 1], false);
                        vertices.Add(end);
                    }
                    continue;
                }

                // Each segment is extended both sides if next intersection is less than extendDistance away.
                // Extend start or add is as is if it's too far away.
                int index = LastNegativeHit(hits, segmentLength);
                Vertex lastCut;
                if (index < 0 || hits[index].DistanceAlongLine < -extendDistance)
                {
                    lastCut = AddVertex(points[i]);
                }
                else if (hits[index].DistanceAlongEdge.ApproximatelyEquals(0, Tolerance))
                {
                    lastCut = GetVertex(hits[index].Edge.StartId);
                }
                else if (hits[index].DistanceAlongEdge.ApproximatelyEquals(hits[index].EdgeLength, Tolerance))
                {
                    lastCut = GetVertex(hits[index].Edge.EndId);
                }
                else
                {
                    var startPoint = GetVertex(hits[index].Edge.StartId).Point;
                    var endPoint = GetVertex(hits[index].Edge.EndId).Point;
                    var cutPoint = startPoint + hits[index].DistanceAlongEdge * (endPoint - startPoint).Unitized();
                    lastCut = CutEdge(hits[index].Edge, cutPoint);
                }

                // Ignore consequent duplicate vertices. Duplicates are still possible though.
                index++;
                if (!vertices.Any() || vertices.Last().Id != lastCut.Id)
                {
                    vertices.Add(lastCut);
                }

                // Insert any ordered intersection between start and end
                while (index < hits.Count && hits[index].DistanceAlongLine < segmentLength + Tolerance)
                {
                    var newCut = InsertHit(hits[index], lastCut);
                    if (newCut != null)
                    {
                        vertices.Add(newCut);
                        lastCut = newCut;
                    }
                    index++;
                }

                // Snap segment end to the first outside intersection if it's not too far away.
                // If it's not there or too far away - just insert end point as is.
                if (!hits[index - 1].DistanceAlongLine.ApproximatelyEquals(segmentLength, Tolerance))
                {
                    var hit = index < hits.Count ? hits[index] : (null, 0, 0, 0);
                    var finalCut = InsertFinalCut(hit, lastCut, points[i + 1], segmentLength + extendDistance);
                    if (finalCut != null)
                    {
                        vertices.Add(finalCut);
                    }
                }
            }
            return vertices;
        }

        #region AddVerticesWithCustomExtension helper functions

        private List<(Edge Edge, double DistanceAlongLine, double DistanceAlongEdge, double EdgeLength)> IntersectGraph(
            Vector3 start, Vector3 end)
        {
            var hits = new List<(Edge Edge, double D1, double D2, double L2)>();
            foreach (var edge in GetEdges())
            {
                var startVertex = GetVertex(edge.StartId);
                var endVertex = GetVertex(edge.EndId);
                var edgeStartPoint = startVertex.Point;
                var edgeEndPoint = endVertex.Point;
                if (edgeStartPoint.IsAlmostEqualTo(edgeEndPoint))
                {
                    continue;
                }

                if (Line.Intersects(start, end, edgeStartPoint, edgeEndPoint, out var result, true, true))
                {
                    var dot1 = (result - start).Dot((end - start).Unitized());
                    var dot2 = (result - edgeStartPoint).Dot((edgeEndPoint - edgeStartPoint).Unitized());
                    var l2 = (edgeEndPoint - edgeStartPoint).Length();
                    if (dot2 > -Vector3.EPSILON && dot2 < l2 + Vector3.EPSILON)
                    {
                        hits.Add((edge, dot1, dot2, l2));
                    }
                }
            }

            return hits.OrderBy(h => h.D1).ToList();
        }

        private int LastNegativeHit(
            List<(Edge edge, double LineParam, double EdgeParam, double EdgeLength)> hits,
            double segmentLength)
        {
            int index = -1;
            for (int j = 0; j < hits.Count; j++)
            {
                if (hits[j].LineParam < -Tolerance)
                {
                    index = j;
                }
                else
                {
                    if (index < 0)
                    {
                        if (hits[j].LineParam > segmentLength + Tolerance)
                        {
                            index = -1;
                        }
                        else
                        {
                            index = j;
                        }
                    }
                    break;
                }
            }
            return index;
        }

        private Vertex InsertHit(
            (Edge Edge, double LineParam, double EdgeParam, double EdgeLength) hit,
            Vertex lastCut)
        {
            Vertex newCut = null;
            if (hit.EdgeParam.ApproximatelyEquals(0, Tolerance))
            {
                if (hit.Edge.StartId != lastCut.Id)
                {
                    newCut = GetVertex(hit.Edge.StartId);
                    AddInsertEdge(lastCut.Id, newCut.Id);
                }
            }
            else if (hit.EdgeParam.ApproximatelyEquals(hit.EdgeLength, Tolerance))
            {
                if (hit.Edge.EndId != lastCut.Id)
                {
                    newCut = GetVertex(hit.Edge.EndId);
                    AddInsertEdge(lastCut.Id, newCut.Id);
                }
            }
            else
            {
                var startPoint = GetVertex(hit.Edge.StartId).Point;
                var endPoint = GetVertex(hit.Edge.EndId).Point;
                var cutPoint = startPoint + hit.EdgeParam * (endPoint - startPoint).Unitized();
                newCut = CutEdge(hit.Edge, cutPoint);
                if (newCut.Id != lastCut.Id)
                {
                    AddInsertEdge(lastCut.Id, newCut.Id);
                }
            }
            return newCut;
        }

        private Vertex InsertFinalCut(
            (Edge Edge, double DistanceAlongLine, double DistanceAlongEdge, double EdgeLength) hit,
            Vertex lastCut, Vector3 endPoint, double maxDistance)
        {
            Vertex finalCut;
            if (hit.Edge == null || hit.DistanceAlongLine > maxDistance)
            {
                finalCut = AddVertex(endPoint);
                if (finalCut.Id != lastCut.Id)
                {
                    AddInsertEdge(lastCut.Id, finalCut.Id);
                }
            }
            else
            {
                finalCut = InsertHit(hit, lastCut);
            }
            return finalCut;
        }

        #endregion

        private void DeleteVertex(ulong id)
        {
            var vertex = _vertices[id];
            _vertices.Remove(id);
            var zDict = GetAddressParent(_verticesLookup, vertex.Point, tolerance: Tolerance);
            if (zDict == null)
            {
                return;
            }
            zDict.Remove(vertex.Point.Z);

            TryGetValue(_verticesLookup, vertex.Point.X, out var yzDict, Tolerance);
            if (zDict.Count == 0)
            {
                yzDict.Remove(vertex.Point.Y);
            }
            if (yzDict.Count == 0)
            {
                _verticesLookup.Remove(vertex.Point.X);
            }
        }

        private Grid2d CreateGridFromPolygon(Polygon boundingPolygon)
        {
            var boundingPolygonPlane = boundingPolygon.Plane();
            var primaryAxisDirection = Transform.XAxis - Transform.XAxis.Dot(boundingPolygonPlane.Normal) * boundingPolygonPlane.Normal;
            if (primaryAxisDirection.IsZero())
            {
                primaryAxisDirection = Transform.ZAxis - Transform.ZAxis.Dot(boundingPolygonPlane.Normal) * boundingPolygonPlane.Normal;
            }
            var grid = new Grid2d(boundingPolygon, new Transform(boundingPolygon.Vertices.FirstOrDefault(),
                primaryAxisDirection, boundingPolygon.Normal()));
            return grid;

        }

        private void SplitGridAtIntersectionPoints(Polygon boundingPolygon, Grid2d grid, IEnumerable<Edge> edgesToIntersect)
        {
            var boundingPolygonPlane = boundingPolygon.Plane();
            var intersectionPoints = new List<Vector3>();
            foreach (var edge in edgesToIntersect)
            {
                if (GetLine(edge).Intersects(boundingPolygonPlane, out var intersectionPoint)
                    && boundingPolygon.Contains(intersectionPoint))
                {
                    intersectionPoints.Add(intersectionPoint);
                }
            }
            grid.U.SplitAtPoints(intersectionPoints);
            grid.V.SplitAtPoints(intersectionPoints);
        }

        private void SplitGrid(Grid2d grid, IEnumerable<Vector3> keyPoints)
        {
            grid.U.SplitAtPoints(keyPoints);
            grid.V.SplitAtPoints(keyPoints);
        }

        private HashSet<Edge> AddFromGrid(Grid2d grid, IEnumerable<Edge> edgesToIntersect)
        {
            var cells = grid.GetCells();
            var addedEdges = new HashSet<Edge>();
            var edgeCandidates = new HashSet<(ulong, ulong)>();

            Action<Vector3, Vector3> add = (Vector3 start, Vector3 end) =>
            {
                var v0 = AddVertex(start);
                var v1 = AddVertex(end);
                if (v0 != v1)
                {
                    var pair = v0.Id < v1.Id ? (v0.Id, v1.Id) : (v1.Id, v0.Id);
                    edgeCandidates.Add(pair);
                }
            };

            foreach (var cell in cells)
            {
                foreach (var cellGeometry in cell.GetTrimmedCellGeometry())
                {
                    var polygon = (Polygon)cellGeometry;
                    for (int i = 0; i < polygon.Vertices.Count - 1; i++)
                    {
                        add(polygon.Vertices[i], polygon.Vertices[i + 1]);
                    }
                    add(polygon.Vertices.Last(), polygon.Vertices.First());
                }
            }

            foreach (var edge in edgeCandidates)
            {
                addedEdges.Add(AddInsertEdge(edge.Item1, edge.Item2));
            }

            return addedEdges;
        }

        private List<double> gridUDividers(Grid1d grid)
        {
            if (grid == null)
            {
                return new List<double>();
            }
            if (grid.IsSingleCell)
            {
                return new List<double> { grid.Domain.Min, grid.Domain.Max };
            }
            var ans = grid.Cells.Select(gridUDividers).Select(l => l.First()).ToList();
            ans.Add(grid.Cells.Last().Domain.Max);
            return ans;
        }

        private Transform gridFromUVTransform(Grid2d grid)
        {
            var uDomain = grid.U.curveDomain;
            var vDomain = grid.V.curveDomain;
            Vector3 xAxis = (grid.U.Evaluate(uDomain.Max) - grid.U.Evaluate(uDomain.Min)) / uDomain.Length;
            Vector3 yAxis = (grid.V.Evaluate(vDomain.Max) - grid.V.Evaluate(vDomain.Min)) / vDomain.Length;
            Vector3 zAxis = xAxis.Cross(yAxis);
            var fromGrid = new Transform(grid.U.Evaluate(uDomain.Min) + grid.V.Evaluate(vDomain.Min), xAxis, yAxis, zAxis).Concatenated(grid.fromGrid);
            return fromGrid;
        }

        private List<(Vector3 from, Vector3 to)> splitSegmentWithPoints((Vector3 from, Vector3 to) line, List<Vector3> points)
        {
            var resultingSegments = new List<(Vector3 from, Vector3 to)>();
            var lineVector = line.to - line.from;
            points.Add(line.from);
            points.Add(line.to);
            points.Sort((p, q) => Math.Sign(lineVector.Dot(p - q)));
            for (int i = 1; i < points.Count; ++i)
            {
                resultingSegments.Add((points[i - 1], points[i]));
            }
            return resultingSegments;
        }

        private List<(Vector3 from, Vector3 to)> splitSegmentsWithPoints(List<Line> segments, double u, List<double> coords, bool coordsAreX, List<Vector3> intersectionPoints)
        {
            var swapXYAxes = new Transform(new Vector3(0, 0, 0), Vector3.YAxis, Vector3.XAxis, Vector3.ZAxis);
            if (coordsAreX)
            {
                segments = segments.Select(l => l.TransformedLine(swapXYAxes)).ToList();
            }

            segments = segments.Select(l => l.Start.Y < l.End.Y ? l : l.Reversed()).ToList();
            segments.Sort((l1, l2) => l1.Start.Y.CompareTo(l2.Start.Y));
            var resultingSegments = new List<(Vector3 from, Vector3 to)>();
            var newIntersectionPoints = new List<Vector3>();

            for (int segmentId = 0, yId = 0; segmentId < segments.Count; ++segmentId)
            {
                var previousPoint = segments[segmentId].Start;
                var lastPoint = segments[segmentId].End;
                newIntersectionPoints.Add(previousPoint);
                newIntersectionPoints.Add(lastPoint);
                while (yId < coords.Count && coords[yId] < previousPoint.Y)
                {
                    ++yId;
                }
                while (yId < coords.Count && coords[yId] < lastPoint.Y)
                {
                    var nextPoint = new Vector3(u, coords[yId]);
                    newIntersectionPoints.Add(nextPoint);
                    resultingSegments.Add((previousPoint, nextPoint));
                    previousPoint = nextPoint;
                    ++yId;
                }
                resultingSegments.Add((previousPoint, lastPoint));
            }

            if (coordsAreX)
            {
                segments = segments.Select(l => l.TransformedLine(swapXYAxes)).ToList();
                resultingSegments = resultingSegments.Select(s => (swapXYAxes.OfPoint(s.from), swapXYAxes.OfPoint(s.to))).ToList();
                newIntersectionPoints = newIntersectionPoints.Select(p => swapXYAxes.OfPoint(p)).ToList();
            }

            intersectionPoints.AddRange(newIntersectionPoints);

            return resultingSegments;
        }

        private HashSet<Edge> AddFromGridWithBoundingPolygon(Grid2d grid, Polygon boundingPolygon, IEnumerable<Edge> edgesToIntersect)
        {
            if (grid.Cells.Count == 0) return new HashSet<Edge>();

            var addedEdges = new HashSet<Edge>();
            var edgeCandidates = new HashSet<(ulong, ulong)>();

            var uList = gridUDividers(grid.U);
            var vList = gridUDividers(grid.V);

            var fromGrid = gridFromUVTransform(grid);
            var toGrid = fromGrid.Inverted();

            Action<Vector3, Vector3> add = (Vector3 start, Vector3 end) =>
            {
                start = fromGrid.OfPoint(start);
                end = fromGrid.OfPoint(end);

                var v0 = AddVertex(start);
                var v1 = AddVertex(end);
                if (v0 != v1)
                {
                    var pair = v0.Id < v1.Id ? (v0.Id, v1.Id) : (v1.Id, v0.Id);
                    edgeCandidates.Add(pair);
                }
            };

            var uvPolygon = (Polygon)boundingPolygon.Transformed(toGrid);

            var newSegments = new List<(Vector3 from, Vector3 to)>();
            var intersectionPoints = new List<Vector3>();

            foreach (var u in uList)
            {
                var verticalLine = new Line(new Vector3(u, vList.First()), new Vector3(u, vList.Last()));
                var currentInternalSegments = verticalLine.Trim(uvPolygon, out _, includeCoincidenceAtEdge: true);
                newSegments.AddRange(splitSegmentsWithPoints(currentInternalSegments, u, vList, false, intersectionPoints));
            }
            foreach (var v in vList)
            {
                var horizontalLine = new Line(new Vector3(uList.First(), v), new Vector3(uList.Last(), v));
                var currentInternalSegments = horizontalLine.Trim(uvPolygon, out _, includeCoincidenceAtEdge: true);
                newSegments.AddRange(splitSegmentsWithPoints(currentInternalSegments, v, uList, true, intersectionPoints));
            }

            newSegments.AddRange(uvPolygon.Edges().ToList().SelectMany(e => splitSegmentWithPoints(e, intersectionPoints.Where(p => new Line(e.from, e.to).PointOnLine(p)).ToList())));

            newSegments.ForEach(s => add(s.from, s.to));

            edgeCandidates.ToList().ForEach(edge => addedEdges.Add(AddInsertEdge(edge.Item1, edge.Item2)));

            return addedEdges;
        }

        private void AddVerticalEdges(Vector3 extrusionAxis, double height, HashSet<Edge> addedEdges)
        {
            foreach (var bottomVertex in addedEdges.SelectMany(e => GetVertices(e)).Distinct())
            {
                var heightVector = height * extrusionAxis;
                var topVertex = AddVertex(bottomVertex.Point + heightVector);
                AddInsertEdge(bottomVertex.Id, topVertex.Id);
            }
        }

        /// <summary>
        /// Check where number lies in comparison to given domain.
        /// </summary>
        /// <param name="number">Number to check.</param>
        /// <param name="domain">Min-Max range.</param>
        /// <param name="tolerance">How far outside the range numbers are considered inside.
        /// When tolerance positive - range is increased by it, when negative - decreased.</param>
        /// <returns>Low, Inside or High.</returns>
        private PointOrientation NumberOrientation(
            double number,
            (double Min, double Max) domain,
            double tolerance)
        {
            PointOrientation po = PointOrientation.Inside;
            if (number - domain.Min < -tolerance)
                po = PointOrientation.Low;
            else if (number - domain.Max > tolerance)
                po = PointOrientation.Hi;
            return po;
        }

        /// <summary>
        /// Check if line lies in certain domain.
        /// This happens if any line point is Inside domain at any coordinate or
        /// two points are on different sides on the domain - Hi and Low.
        /// </summary>
        /// <param name="line">Line to check.</param>
        /// <param name="domain">Min-Max range in 3 coordinates.</param>
        /// <param name="xyTolerance">Tolerance for X and Y coordinates.</param>
        /// <param name="zTolerance">Tolerance for Z coordinate.</param>
        /// <param name="startInside">Is start point of line inside the domain.</param>
        /// <param name="endInside">Is end point of line inside the domain.</param>
        /// <returns>Low, Inside or High.</returns>
        private bool IsLineInDomain(
            (Vector3 Start, Vector3 End) line,
            (Vector3 Min, Vector3 Max) domain,
            double xyTolerance, double zTolerance, 
            out bool startInside, out bool endInside)
        {
            startInside = false;
            endInside = false;
            PointOrientation startZ = NumberOrientation(line.Start.Z, (domain.Min.Z, domain.Max.Z), zTolerance);
            PointOrientation endZ = NumberOrientation(line.End.Z, (domain.Min.Z, domain.Max.Z), zTolerance);
            if (startZ == endZ && startZ != PointOrientation.Inside)
            {
                return false;
            }

            PointOrientation startX = NumberOrientation(line.Start.X, (domain.Min.X, domain.Max.X), xyTolerance);
            PointOrientation startY = NumberOrientation(line.Start.Y, (domain.Min.Y, domain.Max.Y), xyTolerance);
            PointOrientation endX = NumberOrientation(line.End.X, (domain.Min.X, domain.Max.X), xyTolerance);
            PointOrientation endY = NumberOrientation(line.End.Y, (domain.Min.Y, domain.Max.Y), xyTolerance);

            if ((startX == endX && startX != PointOrientation.Inside) ||
                (startY == endY && startY != PointOrientation.Inside))
            {
                return false;
            }

            startInside = startZ == PointOrientation.Inside &&
                          startX == PointOrientation.Inside &&
                          startY == PointOrientation.Inside;

            endInside = endZ == PointOrientation.Inside &&
                        endX == PointOrientation.Inside &&
                        endY == PointOrientation.Inside;
            return true;
        }

        /// <summary>
        /// A version of TryGetValue on a dictionary that optionally takes in a tolerance when running the comparison.
        /// </summary>
        /// <param name="dict"></param>
        /// <param name="key">Number to search for.</param>
        /// <param name="value">Value if match was found.</param>
        /// <param name="tolerance">Amount of tolerance in the search for the key.</param>
        /// <typeparam name="T">The type of the dictionary values.</typeparam>
        /// <returns>Whether a match was found.</returns>
        private static bool TryGetValue<T>(Dictionary<double, T> dict, double key, out T value, double? tolerance = null)
        {
            if (dict.TryGetValue(key, out value))
            {
                return true;
            }
            if (tolerance != null)
            {
                foreach (var curKey in dict.Keys)
                {
                    if (Math.Abs(curKey - key) <= tolerance)
                    {
                        value = dict[curKey];
                        return true;
                    }
                }
            }
            return false;
        }

        /// <summary>
        /// In a dictionary of x, y, and z coordinates, gets last level dictionary of z values.
        /// </summary>
        /// <param name="dict"></param>
        /// <param name="point"></param>
        /// <param name="addAddressIfNonExistent">Whether to create the dictionary address if it didn't previously exist.</param>
        /// <param name="tolerance">Amount of tolerance in the search against each component of the coordinate.</param>
        /// <returns>The created or existing last level of values. This can be null if the dictionary address didn't exist previously, and we chose not to add it.</returns>
        private static Dictionary<double, ulong> GetAddressParent(Dictionary<double, Dictionary<double, Dictionary<double, ulong>>> dict, Vector3 point, bool addAddressIfNonExistent = false, double? tolerance = null)
        {
            if (!TryGetValue(dict, point.X, out var yzDict, tolerance))
            {
                if (addAddressIfNonExistent)
                {
                    yzDict = new Dictionary<double, Dictionary<double, ulong>>();
                    dict.Add(point.X, yzDict);
                }
                else
                {
                    return null;
                }
            }

            if (!TryGetValue(yzDict, point.Y, out var zDict, tolerance))
            {
                if (addAddressIfNonExistent)
                {
                    zDict = new Dictionary<double, ulong>();
                    yzDict.Add(point.Y, zDict);
                }
                else
                {
                    return null;
                }
            }

            return zDict;
        }

        #endregion
    }
}
