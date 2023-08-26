using System;
using System.Linq;
using Newtonsoft.Json;
using System.Collections.Generic;
using Elements.Search;

namespace Elements.Geometry
{
    /// <summary>
    /// A polyline with a thickness.
    /// </summary>
    public class ThickenedPolyline
    {
        /// <summary>
        /// Construct a thickened polyline.
        /// </summary>
        /// <param name="polyline">The polyline to thicken.</param>
        /// <param name="leftWidth">The amount to thicken the polyline on its "left" side, imagining that the polyline is extending away from you. That is, if the polyline starts at (0,0,0) and follows the +Z axis, the left side extends into the -X quadrant.</param>
        /// <param name="rightWidth">The amount to thicken the polyline on its "right" side, imagining that the polyline is extending away from you. That is, if the polyline starts at (0,0,0) and follows the +Z axis, the right side extends into the +X quadrant.</param>
        [JsonConstructor]
        public ThickenedPolyline(Polyline polyline, double leftWidth, double rightWidth)
        {
            this.Polyline = polyline;
            this.LeftWidth = leftWidth;
            this.RightWidth = rightWidth;
        }

        /// <summary>
        /// Construct a thickened polyline.
        /// </summary>
        /// <param name="vertices">The vertices of the polyline.</param>
        /// <param name="leftWidth">The amount to thicken the polyline on its "left" side, imagining that the polyline is extending away from you. That is, if the polyline starts at (0,0,0) and follows the +Z axis, the left side extends into the -X quadrant.</param>
        /// <param name="rightWidth">The amount to thicken the polyline on its "right" side, imagining that the polyline is extending away from you. That is, if the polyline starts at (0,0,0) and follows the +Z axis, the right side extends into the +X quadrant.</param>
        public ThickenedPolyline(IList<Vector3> vertices, double leftWidth, double rightWidth) : this(new Polyline(vertices), leftWidth, rightWidth)
        {
        }

        /// <summary>
        /// Construct a thickened polyline.
        /// </summary>
        /// <param name="line">The line to thicken.</param>
        /// <param name="leftWidth">The amount to thicken the polyline on its "left" side, imagining that the polyline is extending away from you. That is, if the polyline starts at (0,0,0) and follows the +Z axis, the left side extends into the -X quadrant.</param>
        /// <param name="rightWidth">The amount to thicken the polyline on its "right" side, imagining that the polyline is extending away from you. That is, if the polyline starts at (0,0,0) and follows the +Z axis, the right side extends into the +X quadrant.</param>
        public ThickenedPolyline(Line line, double leftWidth, double rightWidth) : this(new Polyline(new[] { line.Start, line.End }), leftWidth, rightWidth)
        {
        }

        /// <summary>The base polyline to thicken.</summary>
        [JsonProperty("polyline", Required = Required.Default, NullValueHandling = NullValueHandling.Ignore)]
        public Polyline Polyline { get; set; }

        /// <summary>The amount to thicken the polyline on its "left" side, imagining that the polyline is extending away from you. That is, if the polyline starts at (0,0,0) and follows the +Z axis, the left side extends into the -X quadrant.</summary>
        [JsonProperty("leftWidth", Required = Required.DisallowNull, NullValueHandling = NullValueHandling.Ignore)]
        public double LeftWidth { get; set; }

        /// <summary>The amount to thicken the polyline on its "right" side, imagining that the polyline is extending away from you. That is, if the polyline starts at (0,0,0) and follows the +Z axis, the right side extends into the +X quadrant.</summary>
        [JsonProperty("rightWidth", Required = Required.DisallowNull, NullValueHandling = NullValueHandling.Ignore)]
        public double RightWidth { get; set; }

        private struct EdgeInfo
        {

            public EdgeInfo(int otherPointIndex, Vector3 otherPoint, double leftWidth, double rightWidth, bool pointingAway)
            {
                this.OtherPointIndex = otherPointIndex;
                this.OtherPoint = otherPoint;
                this.LeftWidth = leftWidth;
                this.RightWidth = rightWidth;
                this.PointingAway = pointingAway;
            }
            public int OtherPointIndex;
            public Vector3 OtherPoint;
            public double LeftWidth;
            public double RightWidth;
            public bool PointingAway;
        }

        /// <summary>
        /// Construct the thickened geometry for a collection of thickened
        /// polylines.
        /// </summary>
        /// <param name="polylines">The polylines to thicken.</param>
        /// <param name="normal">The normal of the plane in which to thicken. Z+ by default.</param>
        /// <returns>A collection of thickened geometry. Polylines with
        /// thickness > 0 will yield `offsetPolygon`s, those with thickness = 0
        /// will yield `offsetLine` segments.</returns>
        /// <remarks>This is a direct translation of the corresponding javascript method on the Hypar front end. It is important that this code match the behavior, so that the "thickened polyline" preview shows the same thing as will be generated here.</remarks>
        public static List<(Polygon offsetPolygon, Line offsetLine)> GetPolygons(IEnumerable<ThickenedPolyline> polylines, Vector3? normal = null)
        {
            var normalDir = normal ?? Vector3.ZAxis;
            if (!polylines.Any())
            {
                return new List<(Polygon offsetPolygon, Line offsetLine)>();
            }
            var resultList = new Dictionary<int, (Polygon offsetPolygon, Line offsetLine)>();
            // Initialize a graph to manage nodes and edges of the thickened polylines
            var graph = new
            {
                nodes = new List<(
                    Vector3 position,
                    List<EdgeInfo> edges,
                        Dictionary<int, Vector3[]> offsetVertexMap
                    )>(),
                edges = new List<(int a, int b, int origPlIndex)>()
            };
            // Initialize a point set for efficient spatial queries and build a collection of segments from the polylines
            var pointSet = new PointOctree<int?>(10, polylines.First().Polyline.Vertices.First(), Vector3.EPSILON);
            // Deconstruct all polylines into individual thickened segments. Each segment will be an edge in the graph.
            var segments = polylines.SelectMany((polyline) => polyline.Polyline.Segments().Select(s => (Line: s, polyline.LeftWidth, polyline.RightWidth))).ToArray();
            // build up the graph, finding existing nodes or creating new ones as needed
            for (int i = 0; i < segments.Length; i++)
            {
                (Line Line, double LeftWidth, double RightWidth) segment = segments[i];
                var ptA = segment.Line.Start;
                var ptB = segment.Line.End;

                var indexA = pointSet.GetNearby(ptA, Vector3.EPSILON).FirstOrDefault();
                var indexB = pointSet.GetNearby(ptB, Vector3.EPSILON).FirstOrDefault();
                if (indexA == null)
                {
                    indexA = graph.nodes.Count;
                    pointSet.Add(indexA, ptA);
                    graph.nodes.Add((
                        ptA,
                        new List<EdgeInfo>(),
                        new Dictionary<int, Vector3[]>()
                        ));
                }
                if (indexB == null)
                {
                    indexB = graph.nodes.Count;
                    pointSet.Add(indexB, ptB);
                    graph.nodes.Add((
                        ptB,
                        new List<EdgeInfo>(),
                        new Dictionary<int, Vector3[]>()
                        ));
                }

                graph.edges.Add((indexA.Value, indexB.Value, i));
                graph.nodes[indexA.Value].edges.Add(new EdgeInfo(
                    indexB.Value,
                    ptB,
                    segment.LeftWidth,
                    segment.RightWidth,
                    true));
                graph.nodes[indexB.Value].edges.Add(new EdgeInfo(
                    indexA.Value,
                    ptA,
                    segment.LeftWidth,
                    segment.RightWidth,
                    false
                    ));
            }
            // Walk all nodes, building up the offset vertex map for each node.
            // These vertices will ultimately become the vertices from which we
            // construct our polygons.
            foreach (var (position, edges, offsetVertexMap) in graph.nodes)
            {
                var projectionPlane = new Plane(position, normalDir);
                // for each edge, compute all the info we'll need for it: its left
                // thickness / right thickness, its plane angle to the x axis, and
                // whether it is pointing towards or away from this point. Sort the
                // results by the plane angle. 
                var edgesSorted = edges.Select((edge) =>
                {
                    var otherPoint = edge.OtherPoint;
                    // Edge vector is consistent w/r/t this node: it is always pointing away
                    var edgeVector = otherPoint - position;
                    var edgeAngle = Vector3.XAxis.PlaneAngleTo(edgeVector, normalDir);
                    var edgeLength = edgeVector.Length();
                    offsetVertexMap[edge.OtherPointIndex] = new Vector3[3];
                    offsetVertexMap[edge.OtherPointIndex][1] = position;
                    return (edge, edgeVector, edgeAngle, edgeLength);
                }).OrderBy((edge) => edge.edgeAngle).ToArray();

                // for each edge in the sorted set, find the next edge, and compute the
                // offset intersection with that edge. If the next edge is the same
                // edge, just use the perpendicular offset point. If the next edge is
                // nearly at 180 degrees, use the perpendicular offset point.
                for (int i = 0; i < edgesSorted.Length; i++)
                {
                    var edge = edgesSorted[i];
                    // If the edge is pointing away from this node, its "left" offset distance matches the "left" width of the edge. Otherwise, it matches the "right" width.
                    var nextOffsetDist = edge.edge.PointingAway ? edge.edge.LeftWidth : edge.edge.RightWidth;
                    // create a centerline for this edge which points from this node to the other point.
                    var consistentCenterLine = new[] { position, edge.edge.OtherPoint };
                    var awayDir = edge.edgeVector;
                    var perpendicular = awayDir.Cross(normalDir).Unitized();

                    // construct a virtual line which is just this edge offset by the correct offset distance
                    var leftOffsetLine = new Line(
                        consistentCenterLine[0] + perpendicular * nextOffsetDist * -1,
                        consistentCenterLine[1] + perpendicular * nextOffsetDist * -1
                    ).Projected(projectionPlane);

                    var nextEdge = edgesSorted[(i + 1) % edgesSorted.Length];
                    // create a centerline for the next edge which points from this node to the other point.
                    var nextCenterLine = new[] { position, nextEdge.edge.OtherPoint };
                    // If the next edge is pointing away from this node, its "right" offset distance matches the "right" width of the edge. Otherwise, it matches the "left" width.
                    var nextOffsetDist2 = nextEdge.edge.PointingAway ? nextEdge.edge.RightWidth : nextEdge.edge.LeftWidth;
                    var nextAwayDir = nextEdge.edgeVector;
                    var nextPerpendicular = nextAwayDir.Cross(normalDir).Unitized();

                    // construct a virtual line which is just the next edge offset by the correct offset distance
                    var rightOffsetLine = new Line(
                        nextCenterLine[0] + nextPerpendicular * nextOffsetDist2,
                        nextCenterLine[1] + nextPerpendicular * nextOffsetDist2
                    ).Projected(projectionPlane);

                    // For each pair of edges, compute an intersection point. If the
                    // lines intersect, store the intersection twice — once as the
                    // "left" point for the current edge, and again as the "right" point
                    // for the next edge. If the lines don't intersect, just use the
                    // perpendicular offset points, for a "square" end.
                    var intersects = leftOffsetLine.Intersects(rightOffsetLine, out var intersection, true);
                    var angleThreshold = 90;
                    var parallelThreshold = 3;
                    var angleDiff = (nextEdge.edgeAngle - edge.edgeAngle + 360) % 360;
                    if (intersects)
                    {
                        var maxLength = Math.Min(edge.edgeLength, nextEdge.edgeLength);
                        if (angleDiff < angleThreshold)
                        {
                            // angle < 90 degrees, acute angle
                            // we're on the "outside" of a sharp angle.
                            var toIntersectionVec = intersection - position;
                            // for very sharp angles, pull the intersection back to top out at a max length. This has the effect of creating a "cap" on the end of the polyline.
                            if (toIntersectionVec.Length() > maxLength)
                            {
                                intersection = position + toIntersectionVec.Unitized() * maxLength;
                            }
                            offsetVertexMap[edge.edge.OtherPointIndex][0] = intersection;
                            offsetVertexMap[nextEdge.edge.OtherPointIndex][2] = intersection;
                        }
                        else if (angleDiff > 360 - angleThreshold)
                        {
                            // angle > 270 degrees, explement of acute angle.
                            var squareEndLeft = leftOffsetLine.Start + awayDir.Unitized() * -1 * nextOffsetDist;
                            var squareEndRight = rightOffsetLine.Start + nextAwayDir.Unitized() * -1 * nextOffsetDist;
                            var newInt = (squareEndLeft + squareEndRight) * 0.5;
                            offsetVertexMap[edge.edge.OtherPointIndex][0] = squareEndLeft;
                            offsetVertexMap[edge.edge.OtherPointIndex][1] = newInt;
                            offsetVertexMap[nextEdge.edge.OtherPointIndex][1] = newInt;
                            offsetVertexMap[nextEdge.edge.OtherPointIndex][2] = squareEndRight;
                        }
                        else if (Math.Abs(180 - angleDiff) < parallelThreshold && intersection.DistanceTo(position) > maxLength)
                        { 
                            // angle is nearly parallel. Use the perpendicular offset points.
                            offsetVertexMap[edge.edge.OtherPointIndex][0] = leftOffsetLine.Start;
                            offsetVertexMap[nextEdge.edge.OtherPointIndex][2] = rightOffsetLine.Start;
                        }
                        else
                        {
                            // all other angles use the intersection
                            offsetVertexMap[edge.edge.OtherPointIndex][0] = intersection;
                            offsetVertexMap[nextEdge.edge.OtherPointIndex][2] = intersection;
                        }
                    }
                    else
                    {
                        // an open "square" end
                        offsetVertexMap[edge.edge.OtherPointIndex][0] = leftOffsetLine.Start;
                        offsetVertexMap[nextEdge.edge.OtherPointIndex][2] = rightOffsetLine.Start;
                    }
                }
            }
            var polygons = new List<Polygon>();
            var lines = new List<Line>();
            // For each edge in the graph, construct a new polygon from the points in the offset vertex map.
            foreach (var (a, b, origPlIndex) in graph.edges)
            {

                var abc = graph.nodes[a].offsetVertexMap[b];
                var def = graph.nodes[b].offsetVertexMap[a];
                try
                {
                    var pgonOutput = new Polygon(abc.Concat(def).ToArray());
                    resultList[origPlIndex] = (offsetPolygon: pgonOutput, offsetLine: null);
                }
                catch
                {
                    try
                    {
                        // we may have a degenerate polygon.
                        Elements.Validators.Validator.DisableValidationOnConstruction = true;
                        var pgonOutput = new Polygon(abc.Concat(def).ToArray());
                        var offsets = Profile.Offset(Profile.Offset(new Profile[] { pgonOutput }, -0.01), 0.01);
                        // get largest offset
                        var largestOffset = offsets.OrderByDescending((pgon) => Math.Abs(pgon.Area())).First();
                        resultList[origPlIndex] = (offsetPolygon: largestOffset.Perimeter, offsetLine: null);
                        Elements.Validators.Validator.DisableValidationOnConstruction = false;
                    }
                    catch
                    {
                        resultList[origPlIndex] = (offsetPolygon: null, offsetLine: new Line(abc[1], def[1]));
                    }
                }

            }
            return resultList.Values.ToList();
        }

        /// <summary>
        /// Get the thickened geometry for this polyline.
        /// </summary>
        /// <param name="normal">The normal of the plane in which to thicken. Z+ by default.</param>
        /// <returns>A collection of thickened geometry. Polylines with
        /// thickness > 0 will yield `offsetPolygon`s, those with thickness = 0
        /// will yield `offsetLine` segments.</returns>
        public List<(Polygon offsetPolygon, Line offsetLine)> GetPolygons(Vector3? normal = null)
        {
            return GetPolygons(new[] { this }, normal);
        }
    }
}
