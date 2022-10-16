using Elements.Geometry;
using Elements.Search;
using Xunit;
using System;
using System.Collections.Generic;
using Xunit.Abstractions;
using System.Linq;
using System.Diagnostics;
using System.IO;
using Newtonsoft.Json;

namespace Elements.Tests
{
    public class NetworkTests : ModelTest
    {
        private readonly ITestOutputHelper _output;

        public NetworkTests(ITestOutputHelper output)
        {
            this._output = output;
            this.GenerateIfc = false;
            this.GenerateJson = false;
        }

        [Fact]
        public void BranchNodes()
        {
            var network = new Network<object>();
            var a = network.AddVertex();
            var b = network.AddVertex();
            var c = network.AddVertex();
            network.AddEdgeOneWay(a, b, null);
            network.AddEdgeOneWay(b, c, null);
            Assert.Equal(2, network.BranchNodes().Count);
        }

        [Fact]
        public void LeafNodes()
        {
            var network = new Network<object>();
            var a = network.AddVertex();
            var b = network.AddVertex();
            var c = network.AddVertex();
            network.AddEdgeOneWay(a, b, null);
            network.AddEdgeOneWay(b, c, null);
            Assert.Single(network.LeafNodes());
        }

        [Fact]
        public void CrossingLines()
        {
            this.Name = nameof(CrossingLines);

            var a = new Line(new Vector3(-2, 0), new Vector3(2, 0));
            var b = new Line(new Vector3(0, -2), new Vector3(0, 2));

            var network = Network<Line>.FromSegmentableItems(new[] { a, b }, (l) => { return l; }, out List<Vector3> allNodeLocations, out _);
            var arrows = network.ToModelArrows(allNodeLocations, Colors.Red);
            this.Model.AddElement(arrows);

            this.Model.AddElements(network.ToModelText(allNodeLocations, Colors.Black));

            Assert.Equal(5, allNodeLocations.Count);
        }

        [Fact]
        public void ElevatedLines()
        {
            var a = new Line(new Vector3(-2, 0, 1), new Vector3(2, 0, 1));
            var b = new Line(new Vector3(0, -2, 1), new Vector3(0, 2, 1));
            var pts = new[] { a, b }.Intersections();
            Assert.Single(pts);
        }

        [Fact]
        public void DuplicateLines()
        {
            var a = new Line(new Vector3(-2, 0), new Vector3(2, 0));
            var b = a;
            var pts = new[] { a, b }.Intersections();
            Assert.Empty(pts);
        }

        [Fact]
        public void ReversedDuplicateLines()
        {
            var a = new Line(new Vector3(-2, 0), new Vector3(2, 0));
            var b = new Line(new Vector3(2, 0), new Vector3(-2, 0));
            var pts = new[] { a, b }.Intersections();
            Assert.Empty(pts);
        }

        [Fact]
        public void OverlappingLines()
        {
            var a = new Line(new Vector3(-2, 0), new Vector3(2, 0));
            var b = new Line(new Vector3(-1, 0), new Vector3(1, 0)); ;
            var pts = new[] { a, b }.Intersections();
            Assert.Empty(pts);
        }

        [Fact]
        public void LineSweepSucceedsWithCoincidentPoints()
        {
            this.Name = nameof(LineSweepSucceedsWithCoincidentPoints);

            var ngon = Polygon.Ngon(4, 1);
            var lines = new List<Line>(ngon.Segments());

            // Lines with coincident left-most points, and a vertical line.
            var a = new Line(new Vector3(-2, 0.1), new Vector3(2, 0.1));
            var b = new Line(new Vector3(-0.1, -2), new Vector3(-0.1, 2));
            var c = new Line(new Vector3(-2, -0.1), new Vector3(2, -0.1));
            lines.Add(a);
            lines.Add(b);
            lines.Add(c);

            var network = Network<Line>.FromSegmentableItems(lines, (l) => { return l; }, out List<Vector3> allNodeLocations, out _);
            var arrows = network.ToModelArrows(allNodeLocations, Colors.Red);
            this.Model.AddElement(arrows);

            Assert.Equal(18, allNodeLocations.Count);

            this.Model.AddElements(network.ToModelText(allNodeLocations, Colors.Black));
        }

        [Fact]
        public void TriangleIntersections()
        {
            var ngon = Polygon.Ngon(4, 1);
            var segs = ngon.Segments();
            var lines = new List<Line>() { segs[1], segs[2] };

            // Lines with coincident left-most points, and a vertical line.
            var b = new Line(new Vector3(-0.1, -2), new Vector3(-0.1, 2));
            lines.Add(b);

            var pts = lines.Intersections();
            Assert.Equal(3, pts.Count);
        }

        [Fact]
        public void MultipleLinesIntersect()
        {
            this.Name = nameof(MultipleLinesIntersect);

            var r = new Random();
            var scale = 15;

            var lines = new List<Line>();
            for (var i = 0; i < 100; i++)
            {
                var start = new Vector3(r.NextDouble() * scale, r.NextDouble() * scale, 0);
                var end = new Vector3(r.NextDouble() * scale, r.NextDouble() * scale, 0);
                lines.Add(new Line(start, end));
            }

            var sw = new Stopwatch();
            sw.Start();
            var pts = lines.Intersections();
            sw.Stop();
            _output.WriteLine($"{sw.ElapsedMilliseconds}ms for finding {pts.Count()} intersections.");
            sw.Reset();

            var network = Network<Line>.FromSegmentableItems(lines, (l) => { return l; }, out List<Vector3> allNodeLocations, out _);
            var arrows = network.ToModelArrows(allNodeLocations, Colors.Red);
            this.Model.AddElement(arrows);
        }


        [Fact]
        public void IntersectingWallLines()
        {
            this.Name = nameof(IntersectingWallLines);

            var json = File.ReadAllText("../../../models/Geometry/IntersectingWalls.json");
            var model = Model.FromJson(json);
            var wallGroups = model.AllElementsOfType<WallByProfile>().GroupBy(w => w.Centerline.Start.Z);
            foreach (var group in wallGroups)
            {
                var network = Network<WallByProfile>.FromSegmentableItems(group.ToList(),
                                                                          (wall) => { return wall.Centerline; },
                                                                          out List<Vector3> allNodeLocations,
                                                                          out _);
                this.Model.AddElement(network.ToModelArrows(allNodeLocations, Colors.Black));
            }
        }

        [Fact]
        public void FigureEight()
        {
            this.Name = nameof(FigureEight);
            var t = new Transform();
            t.Rotate(Vector3.ZAxis, 0.0);

            var a = new Line(t.OfPoint(Vector3.Origin), t.OfPoint(new Vector3(10, 0, 0)));
            var b = new Line(t.OfPoint(new Vector3(0, 5, 0)), t.OfPoint(new Vector3(10, 5, 0)));
            var c = new Line(t.OfPoint(new Vector3(0, 10, 0)), t.OfPoint(new Vector3(10, 10, 0)));
            var d = new Line(t.OfPoint(new Vector3(5, 0, 0)), t.OfPoint(new Vector3(5, 5, 0)));
            var e = new Line(t.OfPoint(new Vector3(5, 5, 0)), t.OfPoint(new Vector3(5, 10, 0)));
            var f = new Line(t.OfPoint(Vector3.Origin), t.OfPoint(new Vector3(0, 10, 0)));
            var g = new Line(t.OfPoint(new Vector3(10, 0, 0)), t.OfPoint(new Vector3(10, 10, 0)));
            var network = Network<Line>.FromSegmentableItems(new[] { a, b, c, d, e, f, g }, (o) => { return o; }, out List<Vector3> allNodeLocations, out _, true);
            Assert.Equal(9, network.BranchNodes().Count());
            this.Model.AddElement(network.ToModelArrows(allNodeLocations, Colors.Black));
        }

        [Fact]
        public void SingleLeafTraversesOutsideAndClosedLoop()
        {
            // A vertical line with a triangle pointing to the right.
            var a = new Line(Vector3.Origin, new Vector3(0, 10, 0));
            var b = new Line(new Vector3(0, 5, 0), new Vector3(5, 3, 0));
            var c = new Line(new Vector3(5, 3, 0), new Vector3(0, 0, 0));
            var network = Network<Line>.FromSegmentableItems(new[] { a, b, c }, (o) => { return o; }, out List<Vector3> allNodeLocations, out _, true);

            var leafIndices = new List<int>();
            for (var i = 0; i < network.NodeCount(); i++)
            {
                if (network.EdgesAt(i).Count() == 1)
                {
                    leafIndices.Add(i);
                }
            }

            Assert.Single(leafIndices);

            var visitedEdges = new List<LocalEdge>();
            foreach (var leafIndex in leafIndices)
            {
                var path = network.Traverse(leafIndex, Network<Line>.TraverseSmallestPlaneAngle, allNodeLocations, visitedEdges, out List<int> visited);
                Assert.Equal(6, path.Count);
                _output.WriteLine(string.Join(',', visited));
            }
        }

        private List<Line> CreateClosedRegionTestLines()
        {
            var lines = new List<Line>();

            foreach (var line in Polygon.Rectangle(5, 5).Segments())
            {
                lines.Add(line);
            }
            var t = new Transform(new Vector3(5, 0));
            foreach (var line in Polygon.Rectangle(5, 5).TransformedPolygon(t).Segments())
            {
                var match = false;
                foreach (var otherLine in lines)
                {
                    if (otherLine.IsAlmostEqualTo(line, false))
                    {
                        match = true;
                    }
                }
                if (!match)
                {
                    lines.Add(line);
                }
            }
            lines.Add(new Line(new Vector3(0, 0), new Vector3(0, 5)));
            lines.Add(new Line(new Vector3(-5, -1), new Vector3(12.5, 1)));
            lines.Add(new Line(new Vector3(5, 0), new Vector3(5, 5)));

            return lines;
        }

        [Fact]
        public void FindAllClosedRegions()
        {
            this.Name = nameof(FindAllClosedRegions);

            var lines = CreateClosedRegionTestLines();

            foreach (var line in lines)
            {
                this.Model.AddElement(new ModelCurve(line));
            }

            var network = Network<Line>.FromSegmentableItems(lines, (item) => { return item; }, out var allNodeLocations, out _);

            var closedRegions = network.FindAllClosedRegions(allNodeLocations);

            this.Model.AddElements(network.ToModelText(allNodeLocations, Colors.Black));
            this.Model.AddElements(network.ToModelArrows(allNodeLocations, Colors.Black));

            Assert.Equal(5, closedRegions.Count);

            var r = new Random(23);
            foreach (var region in closedRegions)
            {
                try
                {
                    var p = new Polygon(region.Select(i => allNodeLocations[i]).ToList());
                    this.Model.AddElement(new Panel(p, r.NextMaterial()));
                }
                catch
                {
                    continue;
                }
            }
        }

        [Fact]
        public void FindAllClosedRegionsDoesNotLoopInfinitely()
        {
            this.Name = nameof(FindAllClosedRegionsDoesNotLoopInfinitely);
            var json = File.ReadAllText("../../../models/Geometry/BadNetwork.json");
            var lines = JsonConvert.DeserializeObject<List<Line>>(json);
            var network = Network<Line>.FromSegmentableItems(lines, (l) => { return l; }, out var allNodeLocations, out var _);
            Model.AddElements(network.ToModelText(allNodeLocations, Colors.Black));
            Model.AddElements(network.ToModelArrows(allNodeLocations, Colors.Blue));
            Model.AddElements(network.ToBoundedAreaPanels(allNodeLocations));
        }

        [Fact]
        public void RevitWallsIntersectCorrectly()
        {
            this.Name = nameof(RevitWallsIntersectCorrectly);

            var json = File.ReadAllText("../../../models/Geometry/RevitIntersectingWalls.json");
            var model = Model.FromJson(json);
            var walls = model.AllElementsOfType<WallByProfile>();
            foreach (var wall in walls)
            {
                wall.Material = BuiltInMaterials.Mass;
            }
            Assert.Equal(4, walls.Count());
            var network = Network<WallByProfile>.FromSegmentableItems(walls.ToList(),
                                                                      (wall) => { return wall.Centerline; },
                                                                      out var allNodeLocations,
                                                                      out var allIntersectionLocations);

            model.AddElements(network.ToModelArrows(allNodeLocations, Colors.Blue));
            model.AddElements(network.ToModelText(allNodeLocations, Colors.Blue));
            model.AddElements(network.ToBoundedAreaPanels(allNodeLocations));

            Assert.Equal(network.EdgesAt(0).Select(i => i.Item1), new List<int>() { 1 });
            Assert.Equal(network.EdgesAt(1).Select(i => i.Item1), new List<int>() { 0, 2, 3 });
            Assert.Equal(network.EdgesAt(2).Select(i => i.Item1), new List<int>() { 1, 4, 5 });
            Assert.Equal(network.EdgesAt(3).Select(i => i.Item1), new List<int>() { 1, 6, 7 });
            Assert.Equal(network.EdgesAt(4).Select(i => i.Item1), new List<int>() { 2 });
            Assert.Equal(network.EdgesAt(5).Select(i => i.Item1), new List<int>() { 2 });
            Assert.Equal(network.EdgesAt(6).Select(i => i.Item1), new List<int>() { 3 });
            Assert.Equal(network.EdgesAt(7).Select(i => i.Item1), new List<int>() { 3 });

            Model = model;
        }

        [Fact]
        public void TwoWayLeafNodes()
        {
            var network = new Network<object>();
            var a = network.AddVertex();
            var b = network.AddVertex();
            network.AddEdgeBothWays(a, b, null);
            Assert.Equal(2, network.LeafNodes().Count);
        }

        [Fact]
        public void LeafClipping()
        {
            this.Name = nameof(LeafClipping);
            var json = File.ReadAllText("../../../models/Geometry/GridWithLeaves.json");
            var model = JsonConvert.DeserializeObject<Model>(json);
            var grids = model.AllElementsOfType<GridLine>().ToList();

            var falseNetwork = Network<GridLine>.FromSegmentableItems(grids,
                                                                 (gl) => { return gl.Curve as Line; },
                                                                 out var falseNodeLocations,
                                                                 out var _);
            Model.AddElements(falseNetwork.ToModelArrows(falseNodeLocations, Colors.Blue));

            var network = Network<GridLine>.FromSegmentableItems(grids,
                                                                 (gl) => { return gl.Curve as Line; },
                                                                 out var allNodeLocations,
                                                                 out var _,
                                                                 removeLeaves: true);
            var r = new Random();
            var regions = network.FindAllClosedRegions(allNodeLocations);
            Model.AddElements(network.ToModelArrows(allNodeLocations, Colors.Red));
            foreach (var region in regions)
            {
                Model.AddElement(new Panel(new Polygon(region.Select(i => new Vector3(allNodeLocations[i])).ToList()), r.NextMaterial()));
            }
        }
    }
}
