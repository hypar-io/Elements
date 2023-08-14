namespace Elements.Geometry
{
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

        /// <summary>The base polyline to thicken.</summary>
        [Newtonsoft.Json.JsonProperty("polyline", Required = Newtonsoft.Json.Required.Default, NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore)]
        public Polyline Polyline { get; set; }

        /// <summary>The amount to thicken the polyline on its "left" side, imagining that the polyline is extending away from you. That is, if the polyline starts at (0,0,0) and follows the +Z axis, the left side extends into the -X quadrant.</summary>
        [Newtonsoft.Json.JsonProperty("leftWidth", Required = Newtonsoft.Json.Required.DisallowNull, NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore)]
        public double LeftWidth { get; set; }

        /// <summary>The amount to thicken the polyline on its "right" side, imagining that the polyline is extending away from you. That is, if the polyline starts at (0,0,0) and follows the +Z axis, the right side extends into the +X quadrant.</summary>
        [Newtonsoft.Json.JsonProperty("rightWidth", Required = Newtonsoft.Json.Required.DisallowNull, NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore)]
        public double RightWidth { get; set; }

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
            if (!polylines.Any())
            {
                return new();
            }
            var resultList = new Dictionary<int, (Polygon offsetPolygon, Line offsetLine)>();
            // Initialize a graph to manage nodes and edges of the thickened polylines
            var graph = new
            {
                nodes = new List<(
                    Vector3 position,
                    List<(
                        int otherPointIndex,
                        Vector3 otherPoint,
                        double leftWidth,
                        double rightWidth,
                        bool pointingAway
                        )> edges,
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
                        new(),
                        new()
                        ));
                }
                if (indexB == null)
                {
                    indexB = graph.nodes.Count;
                    pointSet.Add(indexB, ptB);
                    graph.nodes.Add((
                        ptB,
                        new(),
                        new()
                        ));
                }

                graph.edges.Add((indexA.Value, indexB.Value, i));
                graph.nodes[indexA.Value].edges.Add((
                    indexB.Value,
                    ptB,
                    segment.LeftWidth,
                    segment.RightWidth,
                    true));
                graph.nodes[indexB.Value].edges.Add((
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
                var projectionPlane = new Plane(position, normal ?? Vector3.ZAxis);
                var edgesSorted = edges.Select((edge) =>
                {
                    var otherPoint = edge.otherPoint;
                    var edgeVector = otherPoint - position;
                    var edgeAngle = Vector3.XAxis.PlaneAngleTo(edgeVector, normal ?? Vector3.ZAxis);
                    var edgeLength = edgeVector.Length();
                    offsetVertexMap[edge.otherPointIndex] = new Vector3[3];
                    offsetVertexMap[edge.otherPointIndex][1] = position;
                    return (edge, edgeVector, edgeAngle, edgeLength);
                }).OrderBy((edge) => edge.edgeAngle).ToArray();
                for (int i = 0; i < edgesSorted.Length; i++)
                {
                    var edge = edgesSorted[i];
                    var nextOffsetDist = edge.edge.pointingAway ? edge.edge.leftWidth : edge.edge.rightWidth;
                    var consistentCenterLine = new[] { position, edge.edge.otherPoint };
                    var awayDir = edge.edgeVector;
                    var perpendicular = awayDir.Cross(normal ?? Vector3.ZAxis).Unitized();
                    var leftOffsetLine = new Line(
                        consistentCenterLine[0] + perpendicular * nextOffsetDist * -1,
                        consistentCenterLine[1] + perpendicular * nextOffsetDist * -1
                    ).Projected(projectionPlane);

                    var nextEdge = edgesSorted[(i + 1) % edgesSorted.Length];
                    var nextCenterLine = new[] { position, nextEdge.edge.otherPoint };
                    var nextOffsetDist2 = nextEdge.edge.pointingAway ? nextEdge.edge.rightWidth : nextEdge.edge.leftWidth;
                    var nextAwayDir = nextEdge.edgeVector;
                    var nextPerpendicular = nextAwayDir.Cross(normal ?? Vector3.ZAxis).Unitized();
                    var rightOffsetLine = new Line(
                        nextCenterLine[0] + nextPerpendicular * nextOffsetDist2,
                        nextCenterLine[1] + nextPerpendicular * nextOffsetDist2
                    ).Projected(projectionPlane);
                    var intersects = leftOffsetLine.Intersects(rightOffsetLine, out var intersection, true);
                    var angleThreshold = 90;
                    var angleDiff = (nextEdge.edgeAngle - edge.edgeAngle + 360) % 360;
                    if (intersects)
                    {
                        var maxLength = Math.Min(edge.edgeLength, nextEdge.edgeLength);
                        if (angleDiff < angleThreshold)
                        {
                            // acute angle
                            var toIntersectionVec = intersection - position;
                            if (toIntersectionVec.Length() > maxLength)
                            {
                                intersection = position + toIntersectionVec.Unitized() * maxLength;
                            }
                            offsetVertexMap[edge.edge.otherPointIndex][0] = intersection;
                            offsetVertexMap[nextEdge.edge.otherPointIndex][2] = intersection;
                        }
                        else if (angleDiff > 360 - angleThreshold)
                        {
                            // reflex angle
                            var squareEndLeft = leftOffsetLine.Start + awayDir.Unitized() * -1 * nextOffsetDist;
                            var squareEndRight = rightOffsetLine.Start + nextAwayDir.Unitized() * -1 * nextOffsetDist;
                            var newInt = (squareEndLeft + squareEndRight) * 0.5;
                            offsetVertexMap[edge.edge.otherPointIndex][0] = squareEndLeft;
                            offsetVertexMap[edge.edge.otherPointIndex][1] = newInt;
                            offsetVertexMap[nextEdge.edge.otherPointIndex][1] = newInt;
                            offsetVertexMap[nextEdge.edge.otherPointIndex][2] = squareEndRight;
                        }
                        else if (Math.Abs(360 - angleDiff) < angleThreshold / 2 && intersection.DistanceTo(position) > maxLength)
                        {
                            offsetVertexMap[edge.edge.otherPointIndex][0] = leftOffsetLine.Start;
                            offsetVertexMap[nextEdge.edge.otherPointIndex][2] = rightOffsetLine.Start;
                        }
                        else
                        {
                            offsetVertexMap[edge.edge.otherPointIndex][0] = intersection;
                            offsetVertexMap[nextEdge.edge.otherPointIndex][2] = intersection;
                        }
                    }
                    else
                    {
                        offsetVertexMap[edge.edge.otherPointIndex][0] = leftOffsetLine.Start;
                        offsetVertexMap[nextEdge.edge.otherPointIndex][2] = rightOffsetLine.Start;
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
    }
}
