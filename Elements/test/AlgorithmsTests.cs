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
        /// Tests whether the Algorithms.BinaryHeap data structure works properly
        /// </summary>
        [Fact]
        public void BinaryHeapTest()
        {
            var heap = new BinaryHeap<double, int>();
            (double, int) tmp;
            // the following code was used to generate the test
            /*
            import random as rnd
            rnd.seed(123)

            m = 27
            n0 = 10
            a = []
            for _ in range(m):
                n = len(a)
                q1 = n0 / (n0 + n)
    
                r = rnd.random()
    
                if r < q1:
                    k = rnd.random()
                    v = rnd.randint(-10000, 10000)
                    a.append((k,v))
                    a = sorted(a)
                    print("heap.Insert({}, {});".format(k, v))
                else:
                    k, v = a[-1]
                    a = a[:-1]
                    print("tmp = heap.Extract(); Assert.Equal({}, tmp.Item1, 12); Assert.Equal({}, tmp.Item2);".format(k, v))
            */
            heap.Insert(0.08718667752263232, 3344);
            heap.Insert(0.8385035164743577, -8750);
            heap.Insert(0.5623187149479814, 1167);
            tmp = heap.Extract(); Assert.Equal(0.8385035164743577, tmp.Item1, 12); Assert.Equal(-8750, tmp.Item2);
            heap.Insert(0.3372166571092755, 937);
            heap.Insert(0.16377684475236043, 4295);
            tmp = heap.Extract(); Assert.Equal(0.5623187149479814, tmp.Item1, 12); Assert.Equal(1167, tmp.Item2);
            tmp = heap.Extract(); Assert.Equal(0.3372166571092755, tmp.Item1, 12); Assert.Equal(937, tmp.Item2);
            heap.Insert(0.00659504022791213, 4690);
            heap.Insert(0.043887451982234094, -5342);
            heap.Insert(0.9066079782041607, -439);
            tmp = heap.Extract(); Assert.Equal(0.9066079782041607, tmp.Item1, 12); Assert.Equal(-439, tmp.Item2);
            heap.Insert(0.2653217052615633, -8796);
            tmp = heap.Extract(); Assert.Equal(0.2653217052615633, tmp.Item1, 12); Assert.Equal(-8796, tmp.Item2);
            heap.Insert(0.801496591292033, -3222);
            tmp = heap.Extract(); Assert.Equal(0.801496591292033, tmp.Item1, 12); Assert.Equal(-3222, tmp.Item2);
            heap.Insert(0.5648140371541568, 330);
            heap.Insert(0.8929413092827728, 6801);
            heap.Insert(0.5385194319363926, 9546);
            tmp = heap.Extract(); Assert.Equal(0.8929413092827728, tmp.Item1, 12); Assert.Equal(6801, tmp.Item2);
            heap.Insert(0.6684702219031471, 2272);
            heap.Insert(0.847341781513193, -4054);
            tmp = heap.Extract(); Assert.Equal(0.847341781513193, tmp.Item1, 12); Assert.Equal(-4054, tmp.Item2);
            heap.Insert(0.4869051550185245, -1419);
            heap.Insert(0.33691615279170617, 2818);
            tmp = heap.Extract(); Assert.Equal(0.6684702219031471, tmp.Item1, 12); Assert.Equal(2272, tmp.Item2);
            tmp = heap.Extract(); Assert.Equal(0.5648140371541568, tmp.Item1, 12); Assert.Equal(330, tmp.Item2);
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
