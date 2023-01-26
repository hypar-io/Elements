using System;
using System.Collections.Generic;
using Elements.Algorithms;
using Elements.Tests;
using System.Linq;
using Xunit;

namespace Elements.Algorithms.Tests
{
    public class AlgorithmsTests : ModelTest
    {
        public AlgorithmsTests()
        {
            this.GenerateIfc = false;
        }

        /// <summary>
        /// Tests whether the Algorithms.DisjointSetUnion initializes without exceptions
        /// </summary>
        [Fact]
        public void DsuInitTest()
        {
            var dsu = new DisjointSetUnion(0);
            dsu = new DisjointSetUnion(1000);
        }

        /// <summary>
        /// tests whether the Algorithms.DisjointSetUnion works properly
        /// </summary>
        [Fact]
        public void DsuTest()
        {
            var dsu = new DisjointSetUnion(10);
            bool res;
            res = dsu.AddEdge(0, 1); Assert.True(res);
            res = dsu.AddEdge(9, 3); Assert.True(res);
            res = dsu.AddEdge(9, 6); Assert.True(res);
            res = dsu.AddEdge(6, 0); Assert.True(res);
            res = dsu.AddEdge(6, 2); Assert.True(res);
            res = dsu.AddEdge(5, 3); Assert.True(res);
            res = dsu.AddEdge(1, 3); Assert.True(!res);
            res = dsu.AddEdge(9, 0); Assert.True(!res);
            res = dsu.AddEdge(8, 7); Assert.True(res);
            res = dsu.AddEdge(5, 6); Assert.True(!res);
            res = dsu.AddEdge(9, 2); Assert.True(!res);

            Assert.Equal(3, dsu.NumComponents);
            Assert.Equal(dsu.GetParent(0), dsu.GetParent(1));
            Assert.Equal(dsu.GetParent(0), dsu.GetParent(2));
            Assert.Equal(dsu.GetParent(0), dsu.GetParent(3));
            Assert.Equal(dsu.GetParent(0), dsu.GetParent(5));
            Assert.Equal(dsu.GetParent(0), dsu.GetParent(6));
            Assert.Equal(dsu.GetParent(0), dsu.GetParent(9));
            Assert.Equal(dsu.GetParent(7), dsu.GetParent(8));
            Assert.NotEqual(dsu.GetParent(0), dsu.GetParent(4));
            Assert.NotEqual(dsu.GetParent(0), dsu.GetParent(8));
            Assert.NotEqual(dsu.GetParent(8), dsu.GetParent(4));
            Assert.Equal(7, dsu.ComponentSize(0));
            Assert.Equal(2, dsu.ComponentSize(7));
            Assert.Equal(1, dsu.ComponentSize(4));
        }

        /// <summary>
        /// Tests whether the first, simple version of the Steiner tree calculation works well
        /// </summary>
        [Fact]
        public void SteinerTreeMk1Test()
        {
            var graph = new SteinerTreeCalculator(7);
            graph.AddEdge(0, 1, 7);
            graph.AddEdge(2, 4, 3);
            graph.AddEdge(5, 3, 4);
            graph.AddEdge(1, 2, 1);
            graph.AddEdge(5, 4, 10);
            graph.AddEdge(2, 3, 1);
            graph.AddEdge(0, 5, 6);
            graph.AddEdge(4, 3, 1);
            graph.AddEdge(1, 5, 5);

            var edges = new HashSet<(int, int, double)>(graph.GetTreeMk1(new int[4] {0, 2, 4, 5 }));
            Assert.Equal(4, edges.Count);
            Assert.True(edges.Contains((0, 5, 6)) || edges.Contains((5, 0, 6)));
            Assert.True(edges.Contains((3, 5, 4)) || edges.Contains((5, 3, 4)));
            Assert.True(edges.Contains((3, 4, 1)) || edges.Contains((4, 3, 1)));
            Assert.True(edges.Contains((3, 2, 1)) || edges.Contains((2, 3, 1)));
        }

        /// <summary>
        /// Tests whether the second, more customizable version of the Steiner tree calculation works well
        /// </summary>
        [Fact]
        public void SteinerTreeMk2Test()
        {
            var graph = new SteinerTreeCalculator(7);
            graph.AddEdge(0, 1, 7);
            graph.AddEdge(2, 4, 3);
            graph.AddEdge(5, 3, 4);
            graph.AddEdge(1, 2, 1);
            graph.AddEdge(5, 4, 10);
            graph.AddEdge(2, 3, 1);
            graph.AddEdge(0, 5, 6);
            graph.AddEdge(4, 3, 1);
            graph.AddEdge(1, 5, 5);

            var tree = graph.GetTreeMk2(new int[4] { 0, 2, 4, 5 });
            var edges = new HashSet<(int, int, double)>();
            for (int i = 0; i < 7; ++i)
                foreach (var ed in tree[i])
                    edges.Add((i, ed.Key, ed.Value));
            Assert.Equal(8, edges.Count);
            Assert.True(edges.Contains((0, 5, 6)) && edges.Contains((5, 0, 6)));
            Assert.True(edges.Contains((3, 5, 4)) && edges.Contains((5, 3, 4)));
            Assert.True(edges.Contains((3, 4, 1)) && edges.Contains((4, 3, 1)));
            Assert.True(edges.Contains((3, 2, 1)) && edges.Contains((2, 3, 1)));
        }
    }
}
