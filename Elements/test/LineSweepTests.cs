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
    public class LineSweepTests : ModelTest
    {
        private class IntComparer : IComparer<int>
        {
            public int Compare(int x, int y)
            {
                if (x < y)
                {
                    return -1;
                }
                else if (x > y)
                {
                    return 1;
                }
                return 0;
            }
        }

        private readonly ITestOutputHelper _output;

        public LineSweepTests(ITestOutputHelper output)
        {
            this._output = output;
            this.GenerateIfc = false;
            this.GenerateJson = false;
        }

        [Fact]
        public void CrossingLines()
        {
            this.Name = nameof(CrossingLines);

            // Lines with coincident left-most points, and a vertical line.
            var a = new Line(new Vector3(-2, 0), new Vector3(2, 0));
            var b = new Line(new Vector3(0, -2), new Vector3(0, 2));

            var pts = new[] { a, b }.Intersections<Line>((line) => { return line; }, out AdjacencyList<Line> adj);
            var arrows = adj.ToModelArrows(pts, Colors.Red);
            this.Model.AddElement(arrows);

            var textData = new List<(Vector3 location, Vector3 facingDirection, Vector3 lineDirection, string text, Color? color)>();
            for (var i = 0; i < pts.Count; i++)
            {
                textData.Add((pts[i], Vector3.ZAxis, Vector3.XAxis, $"[{i}]:{string.Join(',', adj[i].Select(x => x.Item1))}", Colors.Black));
            }
            this.Model.AddElement(new ModelText(textData, FontSize.PT24));

            Assert.Equal(5, pts.Count);
        }

        [Fact]
        public void ElevatedLines()
        {
            var a = new Line(new Vector3(-2, 0, 1), new Vector3(2, 0, 1));
            var b = new Line(new Vector3(0, -2, 1), new Vector3(0, 2, 1));
            var pts = new[] { a, b }.Intersections<Line>((line) => { return line; }, out AdjacencyList<Line> adj);
            Assert.Equal(5, pts.Count);
        }

        [Fact]
        public void DuplicateLines()
        {
            var a = new Line(new Vector3(-2, 0), new Vector3(2, 0));
            var b = a;
            var pts = new[] { a, b }.Intersections<Line>((line) => { return line; }, out AdjacencyList<Line> adj);
            Assert.Equal(2, pts.Count);
        }

        [Fact]
        public void ReversedDuplicateLines()
        {
            var a = new Line(new Vector3(-2, 0), new Vector3(2, 0));
            var b = new Line(new Vector3(2, 0), new Vector3(-2, 0));
            var pts = new[] { a, b }.Intersections<Line>((line) => { return line; }, out AdjacencyList<Line> adj);
            Assert.Equal(2, pts.Count);
        }

        [Fact]
        public void OverlappingLines()
        {
            var a = new Line(new Vector3(-2, 0), new Vector3(2, 0));
            var b = new Line(new Vector3(-1, 0), new Vector3(1, 0)); ;
            var pts = new[] { a, b }.Intersections<Line>((line) => { return line; }, out AdjacencyList<Line> adj);
            Assert.Equal(4, pts.Count);
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

            var pts = lines.Intersections((line) => { return line; }, out AdjacencyList<Line> adj);
            var arrows = adj.ToModelArrows(pts, Colors.Red);
            this.Model.AddElement(arrows);

            Assert.Equal(18, pts.Count);

            var textData = new List<(Vector3 location, Vector3 facingDirection, Vector3 lineDirection, string text, Color? color)>();
            for (var i = 0; i < pts.Count; i++)
            {
                textData.Add((pts[i], Vector3.ZAxis, Vector3.XAxis, $"[{i}]:{string.Join(',', adj[i].Select(x => x.Item1))}", Colors.Black));
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

            var pts = lines.Intersections((line) => { return line; }, out AdjacencyList<Line> adj);
            Assert.Equal(7, pts.Count);
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
            var pts = lines.Intersections((line) => { return line; }, out AdjacencyList<Line> adj);
            sw.Stop();
            _output.WriteLine($"{sw.ElapsedMilliseconds}ms for finding {pts.Count()} intersections.");
            sw.Reset();

            var arrows = adj.ToModelArrows(pts, Colors.Red);
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
                var pts = group.Distinct().ToList().Intersections<WallByProfile>((wall) => { return wall.Centerline; }, out AdjacencyList<WallByProfile> adj);
                this.Model.AddElement(adj.ToModelArrows(pts, Colors.Black));
            }
        }

        [Fact]
        public void PerpendicularLines()
        {
            var p = Polygon.Rectangle(5, 5);
            var pts = p.Segments().Intersections<Line>((line) => { return line; }, out AdjacencyList<Line> adj);
            Assert.Equal(4, pts.Count);
        }
    }
}