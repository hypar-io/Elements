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

        [Fact]
        public void DsuInitTest()
        {
            var dsu = new DisjointSetUnion(0);
            dsu = new DisjointSetUnion(1000);
        }

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

        [Fact]
        public void BinaryHeapTest()
        {
            var heap = new BinaryHeap<double, int>();
            (double, int) tmp;
            // the following code was used to generate the test
            /*
            import random as rnd
            rnd.seed(123)

            m = 100
            a = []
            for _ in range(m):
                n = len(a)
                q1 = 5 / (5 + n)
                q2 = n / (n + 20)

                r = rnd.random()

                if r < q1:
                    k = rnd.random()
                    v = rnd.randint(-10000, 10000)
                    a.append((k,v))
                    a = sorted(a)
                    print("heap.Insert({}, {});".format(k, v))
                elif r < q1+q2:
                    k, v = a[-1]
                    a = a[:-1]
                    print("tmp = heap.Extract(); Assert.Equal({}, tmp.Item1, 12); Assert.Equal({}, tmp.Item2);".format(k, v))
                else:
                    k, v = a[-1]
                    print("tmp = heap.Max; Assert.Equal({}, tmp.Item1, 12); Assert.Equal({}, tmp.Item2);".format(k, v))
            */
            heap.Insert(0.08718667752263232, 3344);
            heap.Insert(0.8385035164743577, -8750);
            heap.Insert(0.5623187149479814, 1167);
            tmp = heap.Max; Assert.Equal(0.8385035164743577, tmp.Item1, 12); Assert.Equal(-8750, tmp.Item2);
            heap.Insert(0.3372166571092755, 937);
            tmp = heap.Extract(); Assert.Equal(0.8385035164743577, tmp.Item1, 12); Assert.Equal(-8750, tmp.Item2);
            heap.Insert(0.9059055629095121, -7130);
            tmp = heap.Max; Assert.Equal(0.9059055629095121, tmp.Item1, 12); Assert.Equal(-7130, tmp.Item2);
            heap.Insert(0.00659504022791213, 4690);
            heap.Insert(0.043887451982234094, -5342);
            heap.Insert(0.9066079782041607, -439);
            tmp = heap.Max; Assert.Equal(0.9066079782041607, tmp.Item1, 12); Assert.Equal(-439, tmp.Item2);
            tmp = heap.Extract(); Assert.Equal(0.9066079782041607, tmp.Item1, 12); Assert.Equal(-439, tmp.Item2);
            heap.Insert(0.8378376441951344, -3);
            heap.Insert(0.801496591292033, -3222);
            tmp = heap.Max; Assert.Equal(0.9059055629095121, tmp.Item1, 12); Assert.Equal(-7130, tmp.Item2);
            tmp = heap.Extract(); Assert.Equal(0.9059055629095121, tmp.Item1, 12); Assert.Equal(-7130, tmp.Item2);
            tmp = heap.Extract(); Assert.Equal(0.8378376441951344, tmp.Item1, 12); Assert.Equal(-3, tmp.Item2);
            tmp = heap.Max; Assert.Equal(0.801496591292033, tmp.Item1, 12); Assert.Equal(-3222, tmp.Item2);
            heap.Insert(0.8929413092827728, 6801);
            tmp = heap.Extract(); Assert.Equal(0.8929413092827728, tmp.Item1, 12); Assert.Equal(6801, tmp.Item2);
            tmp = heap.Extract(); Assert.Equal(0.801496591292033, tmp.Item1, 12); Assert.Equal(-3222, tmp.Item2);
            tmp = heap.Max; Assert.Equal(0.5623187149479814, tmp.Item1, 12); Assert.Equal(1167, tmp.Item2);
            tmp = heap.Extract(); Assert.Equal(0.5623187149479814, tmp.Item1, 12); Assert.Equal(1167, tmp.Item2);
            tmp = heap.Max; Assert.Equal(0.3372166571092755, tmp.Item1, 12); Assert.Equal(937, tmp.Item2);
            heap.Insert(0.4194962484726431, 6713);
            heap.Insert(0.7541731718588721, -4054);
            tmp = heap.Max; Assert.Equal(0.7541731718588721, tmp.Item1, 12); Assert.Equal(-4054, tmp.Item2);
            heap.Insert(0.4869051550185245, -1419);
            heap.Insert(0.33691615279170617, 2818);
            tmp = heap.Max; Assert.Equal(0.7541731718588721, tmp.Item1, 12); Assert.Equal(-4054, tmp.Item2);
            tmp = heap.Max; Assert.Equal(0.7541731718588721, tmp.Item1, 12); Assert.Equal(-4054, tmp.Item2);
            tmp = heap.Extract(); Assert.Equal(0.7541731718588721, tmp.Item1, 12); Assert.Equal(-4054, tmp.Item2);
            tmp = heap.Max; Assert.Equal(0.4869051550185245, tmp.Item1, 12); Assert.Equal(-1419, tmp.Item2);
            heap.Insert(0.586201750398854, 1911);
            heap.Insert(0.35191368309820115, -2492);
            tmp = heap.Extract(); Assert.Equal(0.586201750398854, tmp.Item1, 12); Assert.Equal(1911, tmp.Item2);
            tmp = heap.Extract(); Assert.Equal(0.4869051550185245, tmp.Item1, 12); Assert.Equal(-1419, tmp.Item2);
            tmp = heap.Max; Assert.Equal(0.4194962484726431, tmp.Item1, 12); Assert.Equal(6713, tmp.Item2);
            heap.Insert(0.2877023219450794, 6212);
            tmp = heap.Max; Assert.Equal(0.4194962484726431, tmp.Item1, 12); Assert.Equal(6713, tmp.Item2);
            tmp = heap.Extract(); Assert.Equal(0.4194962484726431, tmp.Item1, 12); Assert.Equal(6713, tmp.Item2);
            heap.Insert(0.48381148264314666, -4181);
            heap.Insert(0.5363800186254077, 2405);
            tmp = heap.Max; Assert.Equal(0.5363800186254077, tmp.Item1, 12); Assert.Equal(2405, tmp.Item2);
            heap.Insert(0.13472760063663358, -3670);
            heap.Insert(0.7988888796571457, -1090);
            tmp = heap.Max; Assert.Equal(0.7988888796571457, tmp.Item1, 12); Assert.Equal(-1090, tmp.Item2);
            tmp = heap.Extract(); Assert.Equal(0.7988888796571457, tmp.Item1, 12); Assert.Equal(-1090, tmp.Item2);
            heap.Insert(0.6175846313134489, 6722);
            tmp = heap.Extract(); Assert.Equal(0.6175846313134489, tmp.Item1, 12); Assert.Equal(6722, tmp.Item2);
            heap.Insert(0.4332003138771612, -9720);
            tmp = heap.Max; Assert.Equal(0.5363800186254077, tmp.Item1, 12); Assert.Equal(2405, tmp.Item2);
            tmp = heap.Extract(); Assert.Equal(0.5363800186254077, tmp.Item1, 12); Assert.Equal(2405, tmp.Item2);
            tmp = heap.Extract(); Assert.Equal(0.48381148264314666, tmp.Item1, 12); Assert.Equal(-4181, tmp.Item2);
            tmp = heap.Extract(); Assert.Equal(0.4332003138771612, tmp.Item1, 12); Assert.Equal(-9720, tmp.Item2);
            heap.Insert(0.3224579636941851, 5483);
            heap.Insert(0.5615882153423916, 3568);
            tmp = heap.Extract(); Assert.Equal(0.5615882153423916, tmp.Item1, 12); Assert.Equal(3568, tmp.Item2);
            tmp = heap.Extract(); Assert.Equal(0.35191368309820115, tmp.Item1, 12); Assert.Equal(-2492, tmp.Item2);
            tmp = heap.Max; Assert.Equal(0.3372166571092755, tmp.Item1, 12); Assert.Equal(937, tmp.Item2);
            tmp = heap.Max; Assert.Equal(0.3372166571092755, tmp.Item1, 12); Assert.Equal(937, tmp.Item2);
            tmp = heap.Extract(); Assert.Equal(0.3372166571092755, tmp.Item1, 12); Assert.Equal(937, tmp.Item2);
            tmp = heap.Max; Assert.Equal(0.33691615279170617, tmp.Item1, 12); Assert.Equal(2818, tmp.Item2);
            tmp = heap.Max; Assert.Equal(0.33691615279170617, tmp.Item1, 12); Assert.Equal(2818, tmp.Item2);
            heap.Insert(0.23496343749846516, -7432);
            tmp = heap.Max; Assert.Equal(0.33691615279170617, tmp.Item1, 12); Assert.Equal(2818, tmp.Item2);
            tmp = heap.Extract(); Assert.Equal(0.33691615279170617, tmp.Item1, 12); Assert.Equal(2818, tmp.Item2);
            tmp = heap.Max; Assert.Equal(0.3224579636941851, tmp.Item1, 12); Assert.Equal(5483, tmp.Item2);
            heap.Insert(0.3061637591282965, -459);
            tmp = heap.Max; Assert.Equal(0.3224579636941851, tmp.Item1, 12); Assert.Equal(5483, tmp.Item2);
            tmp = heap.Extract(); Assert.Equal(0.3224579636941851, tmp.Item1, 12); Assert.Equal(5483, tmp.Item2);
            heap.Insert(0.974929566217395, 6944);
            heap.Insert(0.16680171938446642, 9310);
            tmp = heap.Extract(); Assert.Equal(0.974929566217395, tmp.Item1, 12); Assert.Equal(6944, tmp.Item2);
            heap.Insert(0.1360157911087615, -9325);
            heap.Insert(0.8710772077623383, 4783);
            heap.Insert(0.7911091579073943, -9075);
            heap.Insert(0.32485631757523614, 6935);
            tmp = heap.Extract(); Assert.Equal(0.8710772077623383, tmp.Item1, 12); Assert.Equal(4783, tmp.Item2);
            heap.Insert(0.5001225106774085, 7873);
            tmp = heap.Max; Assert.Equal(0.7911091579073943, tmp.Item1, 12); Assert.Equal(-9075, tmp.Item2);
            heap.Insert(0.5701853285735908, -6958);
            heap.Insert(0.5063014868143146, -3340);
            tmp = heap.Max; Assert.Equal(0.7911091579073943, tmp.Item1, 12); Assert.Equal(-9075, tmp.Item2);
            tmp = heap.Extract(); Assert.Equal(0.7911091579073943, tmp.Item1, 12); Assert.Equal(-9075, tmp.Item2);
            heap.Insert(0.5005473575022951, -3542);
            tmp = heap.Extract(); Assert.Equal(0.5701853285735908, tmp.Item1, 12); Assert.Equal(-6958, tmp.Item2);
            tmp = heap.Max; Assert.Equal(0.5063014868143146, tmp.Item1, 12); Assert.Equal(-3340, tmp.Item2);
            tmp = heap.Max; Assert.Equal(0.5063014868143146, tmp.Item1, 12); Assert.Equal(-3340, tmp.Item2);
            heap.Insert(0.26824979904759294, -2433);
            heap.Insert(0.31702820274040777, 1178);
            tmp = heap.Extract(); Assert.Equal(0.5063014868143146, tmp.Item1, 12); Assert.Equal(-3340, tmp.Item2);
            heap.Insert(0.8905098275022165, 7504);
            heap.Insert(0.9018928891715335, 6131);
            tmp = heap.Max; Assert.Equal(0.9018928891715335, tmp.Item1, 12); Assert.Equal(6131, tmp.Item2);
            tmp = heap.Max; Assert.Equal(0.9018928891715335, tmp.Item1, 12); Assert.Equal(6131, tmp.Item2);
            tmp = heap.Extract(); Assert.Equal(0.9018928891715335, tmp.Item1, 12); Assert.Equal(6131, tmp.Item2);
            tmp = heap.Max; Assert.Equal(0.8905098275022165, tmp.Item1, 12); Assert.Equal(7504, tmp.Item2);
            tmp = heap.Extract(); Assert.Equal(0.8905098275022165, tmp.Item1, 12); Assert.Equal(7504, tmp.Item2);
        }

        [Fact]
        public void SteinerTreeTest()
        {
            var graph = new SteinerTreeCalculator(6);
            graph.AddEdge(0, 1, 7);
            graph.AddEdge(2, 4, 3);
            graph.AddEdge(5, 3, 4);
            graph.AddEdge(1, 2, 1);
            graph.AddEdge(5, 4, 10);
            graph.AddEdge(2, 3, 1);
            graph.AddEdge(0, 5, 6);
            graph.AddEdge(4, 3, 1);
            graph.AddEdge(1, 5, 5);

            var edges = new List<(int, int, double)>(graph.GetTree(new int[4] {0, 2, 4, 5 }));
            Assert.Equal(4, edges.Count);
        }
    }
}
