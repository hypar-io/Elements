using Elements.Geometry;
using Elements.Search;
using Xunit;
using System;
using System.Collections.Generic;
using Xunit.Abstractions;
using System.Linq;
using System.Diagnostics;

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

            var pts = lines.Intersections(out AdjacencyList adj);
            var arrows = adj.ToModelArrows(pts, Colors.Red);
            this.Model.AddElement(arrows);
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
            var pts = lines.Intersections(out AdjacencyList adj);
            sw.Stop();
            _output.WriteLine($"{sw.ElapsedMilliseconds}ms for finding {pts.Count()} intersections.");
            sw.Reset();

            var arrows = adj.ToModelArrows(pts, Colors.Red);
            this.Model.AddElement(arrows);
        }

        private static ModelArrows DrawArrows(Dictionary<int, List<Vector3>> pts, Random r)
        {
            var arrowData = new List<(Vector3 origin, Vector3 direction, double scale, Color? color)>();

            foreach (var ptSet in pts)
            {
                var color = r.NextColor();
                for (var i = 0; i < ptSet.Value.Count - 1; i++)
                {
                    var v1 = ptSet.Value[i];
                    var v2 = ptSet.Value[i + 1];
                    var l = v1.DistanceTo(v2);
                    if (l < Vector3.EPSILON)
                    {
                        continue;
                    }
                    arrowData.Add((v1, (v2 - v1).Unitized(), l, color));
                }
            }

            return new ModelArrows(arrowData, arrowAngle: 75);
        }
    }
}