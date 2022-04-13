using Elements.Geometry;
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
        /// Transformation with which planar spaces are aligned
        /// </summary>
        public Transform Transform { get; set; }

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
                var addedEdges = AddFromGrid(grid, edgesBefore);
                AddVerticalEdges(extrusionAxis, zCells[i].Domain.Length, addedEdges);
                if (i == zCells.Count - 1)
                {
                    var transformedPolygonTop = boundingPolygon.TransformedPolygon(
                        new Transform(zCells[i].Domain.Max * extrusionAxis));
                    grid = CreateGridFromPolygon(transformedPolygonTop);
                    SplitGrid(grid, keyPoints);
                    SplitGridAtIntersectionPoints(boundingPolygon, grid, edgesBefore);
                    AddFromGrid(grid, edgesBefore);
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
        /// Intersect the box with existent edges and cut any portion of the edge, or whole edge,
        /// that is inside the box. Note that no new connections are created afterwards.
        /// </summary>
        /// <param name="box">Boding box to subtract</param>
        public void SubtractBox(BBox3 box)
        {
            List<Edge> edgesToDelete = new List<Edge>();
            foreach (var edge in GetEdges())
            {
                var start = GetVertex(edge.StartId);
                var end = GetVertex(edge.EndId);
                PointOrientation startZ = Orientation(start.Point.Z, box.Min.Z, box.Max.Z);
                PointOrientation endZ = Orientation(end.Point.Z, box.Min.Z, box.Max.Z);
                if (startZ == endZ && startZ != PointOrientation.Inside)
                    continue;

                //Z coordinates and X/Y are treated differently.
                //If edge lies on one of X or Y planes of the box - it's not treated as "Inside" and edge is kept.
                //If edge lies on one of Z planes - it's still "Inside", so edge is cut or removed.
                //This is because we don't want travel under or over obstacles on elevation where they start/end.
                PointOrientation startX = OrientationTolerance(start.Point.X, box.Min.X, box.Max.X);
                PointOrientation startY = OrientationTolerance(start.Point.Y, box.Min.Y, box.Max.Y);
                PointOrientation endX = OrientationTolerance(end.Point.X, box.Min.X, box.Max.X);
                PointOrientation endY = OrientationTolerance(end.Point.Y, box.Min.Y, box.Max.Y);

                if ((startX == endX && startX != PointOrientation.Inside) ||
                    (startY == endY && startY != PointOrientation.Inside))
                    continue;

                bool startInside = startZ == PointOrientation.Inside &&
                                   startX == PointOrientation.Inside &&
                                   startY == PointOrientation.Inside;

                bool endInside = endZ == PointOrientation.Inside &&
                                 endX == PointOrientation.Inside &&
                                 endY == PointOrientation.Inside;


                if (startInside && endInside)
                {
                    edgesToDelete.Add(edge);
                }
                else
                {
                    var edgeLine = GetLine(edge);
                    List<Vector3> intersections;
                    edgeLine.Intersects(box, out intersections);
                    // If no intersection found than outside point is exactly within tolerance.
                    // But since Intersects works on 0 to 1 range internally, mismatch in interpretation
                    // is possible. Cut the edge in this case.
                    if (intersections.Count == 0)
                    {
                        edgesToDelete.Add(edge);
                    }
                    // Intersections are sorted from the start point.
                    else if (intersections.Count == 1)
                    {
                        //Need to find which end is inside the box.
                        //If none - we just touched the corner
                        if (startInside || endInside)
                        {
                            edgesToDelete.Add(edge);
                        }
                    }
                    if (intersections.Count == 2)
                    {
                        edgesToDelete.Add(edge);
                    }
                }
            }

            foreach (var e in edgesToDelete)
            {
                RemoveEdge(e);
            }
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
        /// Add a Vertex and connect in to one or more other vertices.
        /// </summary>
        /// <param name="point">Position of required Vertex.</param>
        /// <param name="connections">Ids of other vertices to connect new Vertex with.</param>
        /// <returns>New Vertex or existing one if it's within grid tolerance.</returns>
        public Vertex AddVertex(Vector3 point, IList<Vertex> connections)
        {
            if (connections == null || !connections.Any())
            {
                throw new ArgumentException("Vertex should be connected to at least one other Vertex");
            }

            Vertex v = AddVertex(point);
            foreach (var c in connections)
            {
                AddEdge(v.Id, c.Id);
            }

            return v;
        }

        /// <summary>
        /// Create connected chain of vertices. If chain intersects itself - intersection vertices are created as well.
        /// New vertices are not connected with other vertices, except in the case then one or more added vertices already exist in the grid.
        /// </summary>
        /// <param name="points">List of points to connect by edges. Must have at least two points.</param>
        /// <returns>New vertices in order. Vertices at intersection points are presented more than once.</returns>
        public List<Vertex> AddVertexStrip(IList<Vector3> points)
        {
            if (points.Count < 2)
            {
                throw new ArgumentException("At least two points required");
            }

            var vertices = new List<Vertex>();
            vertices.Add(AddVertex(points[0]));
            for (int i = 1; i < points.Count; i++)
            {
                var tailVertex = AddVertex(points[i], new List<Vertex> { vertices.Last() });

                for (int j = 0; j < vertices.Count - 1; j++)
                {
                    if (Line.Intersects(vertices.Last().Point, tailVertex.Point,
                                        vertices[j].Point, vertices[j + 1].Point,
                                        out var intersection))
                    {
                        var cross = AddVertex(intersection, new List<Vertex> {
                            tailVertex, vertices.Last(),  vertices[j], vertices[j + 1] });
                        RemoveEdge(tailVertex.GetEdge(vertices.Last().Id));
                        RemoveEdge(vertices[j].GetEdge(vertices[j + 1].Id));
                        vertices.Insert(j + 1, cross);
                        vertices.Add(cross);
                        j++;
                    }
                }
                vertices.Add(tailVertex);
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
            var newVertex = AddVertex(position, new List<Vertex> { startVertex, endVertex });
            RemoveEdge(edge);
            return newVertex;
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
        /// Add an Edge or return the exiting one with given indexes.
        /// </summary>
        /// <param name="vertexId1">Index of the first Vertex</param>
        /// <param name="vertexId2">Index of the second Vertex</param>
        /// <returns>New or existing Edge.</returns>
        public Edge AddEdge(ulong vertexId1, ulong vertexId2)
        {
            if (vertexId1 == vertexId2)
            {
                throw new ArgumentException("Can't create edge. The vertices of the edge cannot be the same.", $"{vertexId1}, {vertexId2}");
            }

            var hash = Edge.GetHash(new List<ulong> { vertexId1, vertexId2 });

            if (!this._edgesLookup.TryGetValue(hash, out var edgeId))
            {
                var edge = new Edge(this, this._edgeId, vertexId1, vertexId2);
                edgeId = edge.Id;

                this._edgesLookup[hash] = edgeId;
                this._edges.Add(edgeId, edge);

                this.GetVertex(edge.StartId).Edges.Add(edge);
                this.GetVertex(edge.EndId).Edges.Add(edge);

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
            this._edgesLookup.Remove(hash);
            this._edges.Remove(edge.Id);

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

        #endregion

        #region Private logic

        /// <summary>
        /// Add a Vertex or return existing one if it's withing grid tolerance.
        /// Doesn't connect new Vertex to the grid with edges.
        /// </summary>
        /// <param name="point">Position of required vertex</param>
        /// <returns>New or existing Vertex.</returns>
        private Vertex AddVertex(Vector3 point)
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
                addedEdges.Add(AddEdge(edge.Item1, edge.Item2));
            }

            return addedEdges;
        }

        private void AddVerticalEdges(Vector3 extrusionAxis, double height, HashSet<Edge> addedEdges)
        {
            foreach (var bottomVertex in addedEdges.SelectMany(e => GetVertices(e)).Distinct())
            {
                var heightVector = height * extrusionAxis;
                var topVertex = AddVertex(bottomVertex.Point + heightVector);
                AddEdge(bottomVertex.Id, topVertex.Id);
            }
        }

        private PointOrientation OrientationTolerance(double x, double start, double end)
        {
            PointOrientation po = PointOrientation.Inside;
            if (x - start < Tolerance)
                po = PointOrientation.Low;
            else if (x - end > -Tolerance)
                po = PointOrientation.Hi;
            return po;
        }

        private PointOrientation Orientation(double x, double start, double end)
        {
            PointOrientation po = PointOrientation.Inside;
            if (x < start)
                po = PointOrientation.Low;
            else if (x > end)
                po = PointOrientation.Hi;
            return po;
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
