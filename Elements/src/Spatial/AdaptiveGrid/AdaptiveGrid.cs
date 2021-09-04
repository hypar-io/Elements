using Elements.Geometry;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Elements.Spatial.AdaptiveGrid
{
    /// <summary>
    /// A graph like edge-vertex structure with planar spaces connected by vertical edges
    /// </summary>
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
        /// </summary>
        public double Tolerance { get; set; } = Vector3.EPSILON;

        /// <summary>
        /// Transformation with which planar spaces are aligned
        /// </summary>
        public Transform Transform { get; set; }

        #endregion

        #region Constructors

        /// <summary>
        /// Create an AdaptiveGrid.
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
        /// If edges of new section are intersecting with edges of other existing regions - 
        /// two regions are connected.
        /// </summary>
        /// <param name="bBox">Box which region is populated with graph.</param>
        /// <param name="keyPoints">Set of 3D points, region is split with.</param>
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
        /// If edges of new section are intersecting with edges of other existing regions - 
        /// two regions are connected.
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
        /// If edges of new section are intersecting with edges of other existing regions - 
        /// two regions are connected.
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
                    var edgeLine = edge.GetGeometry();
                    List<Vector3> intersections;
                    edgeLine.Intersects(box, out intersections);
                    // Intersections are sorted from the start point.
                    if (intersections.Count == 1)
                    {
                        //Need to find which end is inside the box. 
                        //If none - we just touched the corner
                        if (startInside)
                        {
                            var v = AddVertex(intersections[0]);
                            if (edge.EndId != v.Id)
                            {
                                AddEdge(v.Id, edge.EndId);
                            }
                            edgesToDelete.Add(edge);
                        }
                        else if (endInside)
                        {
                            var v = AddVertex(intersections[0]);
                            if (edge.StartId != v.Id)
                            {
                                AddEdge(edge.StartId, v.Id);
                            }
                            edgesToDelete.Add(edge);
                        }
                    }
                    if (intersections.Count == 2)
                    {
                        var v0 = AddVertex(intersections[0]);
                        var v1 = AddVertex(intersections[1]);
                        if (edge.StartId != v0.Id)
                        {
                            AddEdge(edge.StartId, v0.Id);
                        }
                        if (edge.EndId != v1.Id)
                        {
                            AddEdge(v1.Id, edge.EndId);
                        }
                        edgesToDelete.Add(edge);
                    }
                }
            }

            foreach (var e in edgesToDelete)
            {
                DeleteEdge(e);
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

        #endregion

        #region Private logic

        private Vertex AddVertex(Vector3 point)
        {
            if (!TryGetVertexIndex(point, out var id, Tolerance))
            {
                var zDict = GetAddressParent(_verticesLookup, point, true, Tolerance);
                id = this._vertexId;
                var vertex = new Vertex(this, id, point);
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

        private List<Edge> AddEdge(ulong startId, ulong endId, IEnumerable<Edge> edgesToIntersect)
        {
            var addedEdges = new List<Edge>();
            var startVertex = GetVertex(startId);
            var endVertex = GetVertex(endId);
            var sp = startVertex.Point;
            var ep = endVertex.Point;
            Vector3 min = new Vector3();
            Vector3 max = new Vector3();
            (min.X, max.X) = sp.X > ep.X ? (ep.X, sp.X) : (sp.X, ep.X);
            (min.X, max.X) = sp.X > ep.X ? (ep.X, sp.X) : (sp.X, ep.X);
            (min.X, max.X) = sp.X > ep.X ? (ep.X, sp.X) : (sp.X, ep.X);

            var intersectionPoints = new List<Vector3>();
            foreach (var edge in edgesToIntersect)
            {
                var edgeV0 = GetVertex(edge.StartId);
                var edgeV1 = GetVertex(edge.EndId);

                PointOrientation startZ = Orientation(edgeV0.Point.Z, min.Z, max.Z);
                PointOrientation endZ = Orientation(edgeV1.Point.Z, min.Z, max.Z);
                if (startZ == endZ && startZ != PointOrientation.Inside)
                    continue;

                PointOrientation startX = Orientation(edgeV0.Point.X, min.X, max.X);
                PointOrientation startY = Orientation(edgeV0.Point.Y, min.Y, max.Y);
                PointOrientation endX = Orientation(edgeV1.Point.X, min.X, max.X);
                PointOrientation endY = Orientation(edgeV1.Point.Y, min.Y, max.Y);

                if ((startX == endX && startX != PointOrientation.Inside) ||
                    (startY == endY && startY != PointOrientation.Inside) ||
                    !Vector3.AreCoplanar(sp, ep, edgeV0.Point, edgeV1.Point))
                {
                    continue;
                }

                var newEdgeLine = new Line(sp, ep);
                var oldEdgeLine = new Line(edgeV0.Point, edgeV1.Point);
                if (newEdgeLine.Intersects(oldEdgeLine, out var intersectionPoint))
                {
                    intersectionPoints.Add(intersectionPoint);
                    var newVertex = AddVertex(intersectionPoint);
                    if (edge.StartId != newVertex.Id)
                    {
                        AddEdge(edge.StartId, newVertex.Id);
                    }
                    if (edge.EndId != newVertex.Id)
                    {
                        AddEdge(edge.EndId, newVertex.Id);
                    }

                    DeleteEdge(edge);
                }
                else if (oldEdgeLine.Direction().IsParallelTo(newEdgeLine.Direction()))
                {
                    var isNewEdgeStartOnOldEdge = oldEdgeLine.PointOnLine(newEdgeLine.Start);
                    var isNewEdgeEndOnOldEdge = oldEdgeLine.PointOnLine(newEdgeLine.End);
                    var isOldEdgeStartOnNewEdge = newEdgeLine.PointOnLine(oldEdgeLine.Start, true);
                    var isOldEdgeEndOnNewEdge = newEdgeLine.PointOnLine(oldEdgeLine.End, true);
                    // new edge is inside old edge
                    if (isNewEdgeStartOnOldEdge && isNewEdgeEndOnOldEdge)
                    {
                        if (oldEdgeLine.Start.DistanceTo(newEdgeLine.Start) < oldEdgeLine.Start.DistanceTo(newEdgeLine.End))
                        {
                            AddEdge(edge.StartId, startVertex.Id);
                            AddEdge(edge.EndId, endVertex.Id);
                        }
                        else
                        {
                            AddEdge(edge.StartId, endVertex.Id);
                            AddEdge(edge.EndId, startVertex.Id);
                        }
                        DeleteEdge(edge);
                    }
                    // edges overlap
                    else if (isNewEdgeStartOnOldEdge || isNewEdgeEndOnOldEdge)
                    {
                        if (isOldEdgeEndOnNewEdge)
                        {
                            intersectionPoints.Add(oldEdgeLine.End);
                            if (oldEdgeLine.Start.DistanceTo(newEdgeLine.Start) < oldEdgeLine.Start.DistanceTo(newEdgeLine.End))
                            {
                                AddEdge(edge.StartId, startVertex.Id);
                            }
                            else
                            {
                                AddEdge(edge.StartId, endVertex.Id);
                            }
                        }
                        else if (isOldEdgeStartOnNewEdge)
                        {
                            intersectionPoints.Add(oldEdgeLine.Start);
                            if (oldEdgeLine.End.DistanceTo(newEdgeLine.Start) < oldEdgeLine.End.DistanceTo(newEdgeLine.End))
                            {
                                AddEdge(edge.EndId, startVertex.Id);
                            }
                            else
                            {
                                AddEdge(edge.EndId, endVertex.Id);
                            }
                        }
                        DeleteEdge(edge);
                    }
                    // old edge is inside new edge
                    else if (isOldEdgeStartOnNewEdge && isOldEdgeEndOnNewEdge)
                    {
                        intersectionPoints.Add(oldEdgeLine.Start);
                        intersectionPoints.Add(oldEdgeLine.End);
                        DeleteEdge(edge);
                    }
                }
            }
            if (intersectionPoints.Any())
            {
                intersectionPoints = intersectionPoints.OrderBy(p => p.DistanceTo(startVertex.Point)).ToList();
                intersectionPoints.Insert(0, startVertex.Point);
                intersectionPoints.Add(endVertex.Point);
                intersectionPoints = intersectionPoints.Distinct().ToList();
                for (var i = 0; i < intersectionPoints.Count - 1; i++)
                {
                    var v1 = AddVertex(intersectionPoints[i]);
                    var v2 = AddVertex(intersectionPoints[i + 1]);
                    addedEdges.Add(AddEdge(v1.Id, v2.Id));
                }
            }
            else
            {
                addedEdges.Add(AddEdge(startVertex.Id, endVertex.Id));
            }

            return addedEdges;
        }

        private Edge AddEdge(ulong vertexId1, ulong vertexId2)
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

        private void DeleteEdge(Edge edge)
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

        private Grid2d CreateGridFromPolygon(Polygon boundingPolygon)
        {
            var boundingPolygonPlane = boundingPolygon.Plane();
            var primaryAxisDirection = Transform.XAxis - Transform.XAxis.Dot(boundingPolygonPlane.Normal) * boundingPolygonPlane.Normal;
            if (primaryAxisDirection.IsZero())
            {
                primaryAxisDirection = Transform.ZAxis - Transform.ZAxis.Dot(boundingPolygonPlane.Normal) * boundingPolygonPlane.Normal;
            }
            var grid = new Grid2d(boundingPolygon, new Transform(boundingPolygon.Vertices.FirstOrDefault(),
                primaryAxisDirection, boundingPolygon.Normal));
            return grid;

        }

        private void SplitGridAtIntersectionPoints(Polygon boundingPolygon, Grid2d grid, IEnumerable<Edge> edgesToIntersect)
        {
            var boundingPolygonPlane = boundingPolygon.Plane();
            var intersectionPoints = new List<Vector3>();
            foreach (var edge in edgesToIntersect)
            {
                if (edge.GetGeometry().Intersects(boundingPolygonPlane, out var intersectionPoint)
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
                var edges = AddEdge(edge.Item1, edge.Item2, edgesToIntersect);
                edges.ForEach(e => addedEdges.Add(e));
            }

            return addedEdges;
        }

        private void AddVerticalEdges(Vector3 extrusionAxis, double height, HashSet<Edge> addedEdges)
        {
            foreach (var bottomVertex in addedEdges.SelectMany(e => e.GetVertices()).Distinct())
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
