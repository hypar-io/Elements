using Elements.Geometry;
using Elements.Search;
using Xunit;
using System;
using System.Collections.Generic;
using Xunit.Abstractions;
using System.Linq;
using System.Diagnostics;
using System.IO;

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

            var textData = new List<(Vector3 location, Vector3 facingDirection, Vector3 lineDirection, string text, Color? color)>();
            for (var i = 0; i < allNodeLocations.Count; i++)
            {
                textData.Add((allNodeLocations[i], Vector3.ZAxis, Vector3.XAxis, $"[{i}]:{string.Join(',', network.EdgesAt(i).Select(x => x.Item1))}", Colors.Black));
            }
            this.Model.AddElement(new ModelText(textData, FontSize.PT24));

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

            var textData = new List<(Vector3 location, Vector3 facingDirection, Vector3 lineDirection, string text, Color? color)>();
            for (var i = 0; i < allNodeLocations.Count; i++)
            {
                textData.Add((allNodeLocations[i], Vector3.ZAxis, Vector3.XAxis, $"[{i}]:{string.Join(',', network.EdgesAt(i).Select(x => x.Item1))}", Colors.Black));
            }
            this.Model.AddElement(new ModelText(textData, FontSize.PT24));
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
        public void PerpendicularLines()
        {
            var p = Polygon.Rectangle(5, 5);
            var pts = p.Segments().Intersections();
            Assert.Single(pts);
        }
    }
}