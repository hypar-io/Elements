using Elements.Search;
using Elements.Geometry;
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
        public void BinaryTreeInts()
        {
            var ints = new[] { 1, 7, 5, 3, 4, 6, 2 };
            var tree = new BinaryTree<int>(new IntComparer());
            foreach (var i in ints)
            {
                tree.Add(i);
            }

            tree.FindPredecessorSuccessor(7, out Node<int> pre, out Node<int> suc);
            Assert.Equal(6, pre.Data);
            Assert.Null(suc);

            tree.FindPredecessorSuccessor(1, out pre, out suc);
            Assert.Null(pre);
            Assert.Equal(2, suc.Data);

            tree.FindPredecessorSuccessors(3, out List<Node<int>> pres, out List<Node<int>> succs);

            _output.WriteLine(string.Join(',', pres.Select(p => p.Data)));
            _output.WriteLine(string.Join(',', succs.Select(s => s.Data)));

        }

        [Fact]
        public void BinaryTreeLines()
        {
            var b = new Line(new Vector3(0.1, 1, 0), new Vector3(6, 1, 0));
            var a = new Line(new Vector3(0, 0, 0), new Vector3(5, 0, 0));
            var c = new Line(new Vector3(0.2, -1, 0), new Vector3(7, 0.2));

            var lines = new[] { a, b, c };

            var tree = new BinaryTree<Line>(new LineSweepSegmentComparer());

            foreach (var line in lines)
            {
                tree.Add(line);
            }

            tree.FindPredecessorSuccessor(a, out Node<Line> pre, out Node<Line> suc);
            Assert.Equal(b, pre.Data);
            Assert.Equal(c, suc.Data);
        }

        [Fact]
        public void LineSweepSucceedsWithCoincidentPoints()
        {
            this.Name = nameof(LineSweepSucceedsWithCoincidentPoints);

            var ngon = Polygon.Ngon(4, 1);
            var lines = new List<Line>(ngon.Segments());

            // var a = new Line(new Vector3(-2, 0.1), new Vector3(2, 0.1));
            var b = new Line(new Vector3(-0.1, -2), new Vector3(-0.1, 2));
            // var c = new Line(new Vector3(-2, -0.1), new Vector3(2, -0.1));
            // lines.Add(a);
            lines.Add(b);
            // lines.Add(c);

            var pts = LineSweep.FromSegments(lines);

            foreach (var pt in pts)
            {
                this.Model.AddElement(new ModelCurve(Polygon.Rectangle(0.01, 0.01).TransformedPolygon(new Transform(pt))));
            }
            foreach (var line in lines)
            {
                this.Model.AddElement(new ModelCurve(line));
            }
        }

        [Fact]
        public void MultipleLinesIntersect()
        {
            this.Name = nameof(MultipleLinesIntersect);

            var r = new Random();
            var scale = 5.0;

            var lines = new List<Line>();
            for (var i = 0; i < 100; i++)
            {
                var start = new Vector3(r.NextDouble() * scale, r.NextDouble() * scale, 0);
                var end = new Vector3(r.NextDouble() * scale, r.NextDouble() * scale, 0);
                lines.Add(new Line(start, end));
            }

            var sw = new Stopwatch();
            sw.Start();
            var pts = LineSweep.FromSegments(lines);
            sw.Stop();
            _output.WriteLine($"{sw.ElapsedMilliseconds}ms for finding {pts.Count} intersections.");

            foreach (var pt in pts)
            {
                this.Model.AddElement(new ModelCurve(Polygon.Rectangle(0.01, 0.01).TransformedPolygon(new Transform(pt))));
            }
            foreach (var line in lines)
            {
                this.Model.AddElement(new ModelCurve(line));
            }

        }
    }
}