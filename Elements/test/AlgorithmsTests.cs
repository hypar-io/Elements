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
            {
                foreach (var ed in tree[i])
                {
                    edges.Add((i, ed.Key, ed.Value));
                }
            }
            Assert.Equal(8, edges.Count);
            Assert.True(edges.Contains((0, 5, 6)) && edges.Contains((5, 0, 6)));
            Assert.True(edges.Contains((3, 5, 4)) && edges.Contains((5, 3, 4)));
            Assert.True(edges.Contains((3, 4, 1)) && edges.Contains((4, 3, 1)));
            Assert.True(edges.Contains((3, 2, 1)) && edges.Contains((2, 3, 1)));
        }

        [Fact]
        void TreapDoubleTest()
        {
            var t = new Treap<double>();
            Func<double, double> cube = x => x * x * x;
            Func<double, int> f = x => (int)Math.Floor(Math.Sqrt(Math.Floor(cube(x))));
            Func<double, int> g = x => (int)Math.Floor(x + 1.0d / (x + 1));
            TreapIterator<double> it;

            // Step 1
            t.Insert(3.477131488046385);
            it = t.Find(1, g);
            Assert.True(it.IsEnd);
            it = t.LowerBound(10, f);
            Assert.True(it.IsEnd);
            it = t.GetIterator(1);
            Assert.True(it.IsEnd);
            it = it + (-1);
            Assert.False(it.IsEnd);
            Assert.Equal(0, it.Index);
            Assert.Equal(3.477131488046385, it.Item, 12);
            // Step 2
            t.Insert(0.3887144797393699);
            it = t.Find(1, g);
            Assert.False(it.IsEnd);
            Assert.Equal(0, it.Index);
            Assert.Equal(0.3887144797393699, it.Item, 12);
            it = t.UpperBound(6, f);
            Assert.True(it.IsEnd);
            it = t.GetIterator(2);
            Assert.True(it.IsEnd);
            it = it + (-1);
            Assert.False(it.IsEnd);
            Assert.Equal(1, it.Index);
            Assert.Equal(3.477131488046385, it.Item, 12);
            // Step 3
            t.Insert(4.17085148313954);
            it = t.Find(2, g);
            Assert.True(it.IsEnd);
            it = t.LowerBound(4, f);
            Assert.False(it.IsEnd);
            Assert.Equal(1, it.Index);
            Assert.Equal(3.477131488046385, it.Item, 12);
            it = t.GetIterator(0);
            Assert.False(it.IsEnd);
            Assert.Equal(0, it.Index);
            Assert.Equal(0.3887144797393699, it.Item, 12);
            it = it + (3);
            Assert.True(it.IsEnd);
            // Step 4
            t.Insert(0.3814326497941739);
            it = t.Find(3, g);
            Assert.False(it.IsEnd);
            Assert.Equal(2, it.Index);
            Assert.Equal(3.477131488046385, it.Item, 12);
            it = t.UpperBound(5, f);
            Assert.False(it.IsEnd);
            Assert.Equal(2, it.Index);
            Assert.Equal(3.477131488046385, it.Item, 12);
            it = t.GetIterator(1);
            Assert.False(it.IsEnd);
            Assert.Equal(1, it.Index);
            Assert.Equal(0.3887144797393699, it.Item, 12);
            it = it + (2);
            Assert.False(it.IsEnd);
            Assert.Equal(3, it.Index);
            Assert.Equal(4.17085148313954, it.Item, 12);
            // Step 5
            t.Insert(1.347093631892136);
            it = t.Find(2, g);
            Assert.True(it.IsEnd);
            it = t.UpperBound(8, f);
            Assert.True(it.IsEnd);
            it = t.GetIterator(2);
            Assert.False(it.IsEnd);
            Assert.Equal(2, it.Index);
            Assert.Equal(1.347093631892136, it.Item, 12);
            it = it + (0);
            Assert.False(it.IsEnd);
            Assert.Equal(2, it.Index);
            Assert.Equal(1.347093631892136, it.Item, 12);
            // Step 6
            t.EraseAll(0.3814326497941739);
            it = t.Find(2, g);
            Assert.True(it.IsEnd);
            it = t.UpperBound(0, f);
            Assert.False(it.IsEnd);
            Assert.Equal(1, it.Index);
            Assert.Equal(1.347093631892136, it.Item, 12);
            it = t.GetIterator(2);
            Assert.False(it.IsEnd);
            Assert.Equal(2, it.Index);
            Assert.Equal(3.477131488046385, it.Item, 12);
            it = it + (-2);
            Assert.False(it.IsEnd);
            Assert.Equal(0, it.Index);
            Assert.Equal(0.3887144797393699, it.Item, 12);
            // Step 7
            t.EraseAll(1.347093631892136);
            it = t.Find(3, g);
            Assert.False(it.IsEnd);
            Assert.Equal(1, it.Index);
            Assert.Equal(3.477131488046385, it.Item, 12);
            it = t.LowerBound(0, f);
            Assert.False(it.IsEnd);
            Assert.Equal(0, it.Index);
            Assert.Equal(0.3887144797393699, it.Item, 12);
            it = t.GetIterator(3);
            Assert.True(it.IsEnd);
            it = it + (0);
            Assert.True(it.IsEnd);
            // Step 8
            t.EraseAll(4.17085148313954);
            it = t.Find(3, g);
            Assert.False(it.IsEnd);
            Assert.Equal(1, it.Index);
            Assert.Equal(3.477131488046385, it.Item, 12);
            it = t.UpperBound(9, f);
            Assert.True(it.IsEnd);
            it = t.GetIterator(2);
            Assert.True(it.IsEnd);
            it = it + (-1);
            Assert.False(it.IsEnd);
            Assert.Equal(1, it.Index);
            Assert.Equal(3.477131488046385, it.Item, 12);
            // Step 9
            t.Insert(4.565181172560389);
            it = t.Find(4, g);
            Assert.False(it.IsEnd);
            Assert.Equal(2, it.Index);
            Assert.Equal(4.565181172560389, it.Item, 12);
            it = t.LowerBound(0, f);
            Assert.False(it.IsEnd);
            Assert.Equal(0, it.Index);
            Assert.Equal(0.3887144797393699, it.Item, 12);
            it = t.GetIterator(3);
            Assert.True(it.IsEnd);
            it = it + (-2);
            Assert.False(it.IsEnd);
            Assert.Equal(1, it.Index);
            Assert.Equal(3.477131488046385, it.Item, 12);
            // Step 10
            it = t.Find(3.477131488046385);
            t.Erase(it);
            it = t.Find(5, g);
            Assert.True(it.IsEnd);
            it = t.UpperBound(4, f);
            Assert.False(it.IsEnd);
            Assert.Equal(1, it.Index);
            Assert.Equal(4.565181172560389, it.Item, 12);
            it = t.GetIterator(0);
            Assert.False(it.IsEnd);
            Assert.Equal(0, it.Index);
            Assert.Equal(0.3887144797393699, it.Item, 12);
            it = it + (1);
            Assert.False(it.IsEnd);
            Assert.Equal(1, it.Index);
            Assert.Equal(4.565181172560389, it.Item, 12);
            // Step 11
            t.Insert(2.131673274757697);
            it = t.Find(1, g);
            Assert.False(it.IsEnd);
            Assert.Equal(0, it.Index);
            Assert.Equal(0.3887144797393699, it.Item, 12);
            it = t.UpperBound(2, f);
            Assert.False(it.IsEnd);
            Assert.Equal(1, it.Index);
            Assert.Equal(2.131673274757697, it.Item, 12);
            it = t.GetIterator(3);
            Assert.True(it.IsEnd);
            it = it + (-3);
            Assert.False(it.IsEnd);
            Assert.Equal(0, it.Index);
            Assert.Equal(0.3887144797393699, it.Item, 12);
            // Step 12
            t.Insert(0.6625603376421318);
            it = t.Find(1, g);
            Assert.False(it.IsEnd);
            Assert.Equal(0, it.Index);
            Assert.Equal(0.3887144797393699, it.Item, 12);
            it = t.UpperBound(0, f);
            Assert.False(it.IsEnd);
            Assert.Equal(2, it.Index);
            Assert.Equal(2.131673274757697, it.Item, 12);
            it = t.GetIterator(3);
            Assert.False(it.IsEnd);
            Assert.Equal(3, it.Index);
            Assert.Equal(4.565181172560389, it.Item, 12);
            it = it + (-1);
            Assert.False(it.IsEnd);
            Assert.Equal(2, it.Index);
            Assert.Equal(2.131673274757697, it.Item, 12);
            // Step 13
            t.Insert(0.7452219466928417);
            it = t.Find(3, g);
            Assert.True(it.IsEnd);
            it = t.LowerBound(5, f);
            Assert.False(it.IsEnd);
            Assert.Equal(4, it.Index);
            Assert.Equal(4.565181172560389, it.Item, 12);
            it = t.GetIterator(0);
            Assert.False(it.IsEnd);
            Assert.Equal(0, it.Index);
            Assert.Equal(0.3887144797393699, it.Item, 12);
            it = it + (5);
            Assert.True(it.IsEnd);
            // Step 14
            t.Insert(1.076024707367829);
            it = t.Find(5, g);
            Assert.True(it.IsEnd);
            it = t.UpperBound(3, f);
            Assert.False(it.IsEnd);
            Assert.Equal(5, it.Index);
            Assert.Equal(4.565181172560389, it.Item, 12);
            it = t.GetIterator(4);
            Assert.False(it.IsEnd);
            Assert.Equal(4, it.Index);
            Assert.Equal(2.131673274757697, it.Item, 12);
            it = it + (1);
            Assert.False(it.IsEnd);
            Assert.Equal(5, it.Index);
            Assert.Equal(4.565181172560389, it.Item, 12);
            // Step 15
            t.Insert(4.52922390360878);
            it = t.Find(3, g);
            Assert.True(it.IsEnd);
            it = t.LowerBound(9, f);
            Assert.False(it.IsEnd);
            Assert.Equal(5, it.Index);
            Assert.Equal(4.52922390360878, it.Item, 12);
            it = t.GetIterator(7);
            Assert.True(it.IsEnd);
            it = it + (-7);
            Assert.False(it.IsEnd);
            Assert.Equal(0, it.Index);
            Assert.Equal(0.3887144797393699, it.Item, 12);
            // Step 16
            t.Insert(1.6453813765877212);
            it = t.Find(3, g);
            Assert.True(it.IsEnd);
            it = t.UpperBound(2, f);
            Assert.False(it.IsEnd);
            Assert.Equal(5, it.Index);
            Assert.Equal(2.131673274757697, it.Item, 12);
            it = t.GetIterator(7);
            Assert.False(it.IsEnd);
            Assert.Equal(7, it.Index);
            Assert.Equal(4.565181172560389, it.Item, 12);
            it = it + (-5);
            Assert.False(it.IsEnd);
            Assert.Equal(2, it.Index);
            Assert.Equal(0.7452219466928417, it.Item, 12);
            // Step 17
            t.Insert(1.8545647134977932);
            it = t.Find(2, g);
            Assert.False(it.IsEnd);
            Assert.Equal(4, it.Index);
            Assert.Equal(1.6453813765877212, it.Item, 12);
            it = t.UpperBound(9, f);
            Assert.True(it.IsEnd);
            it = t.GetIterator(0);
            Assert.False(it.IsEnd);
            Assert.Equal(0, it.Index);
            Assert.Equal(0.3887144797393699, it.Item, 12);
            it = it + (0);
            Assert.False(it.IsEnd);
            Assert.Equal(0, it.Index);
            Assert.Equal(0.3887144797393699, it.Item, 12);
            // Step 18
            t.Insert(2.0308421099467595);
            it = t.Find(3, g);
            Assert.True(it.IsEnd);
            it = t.UpperBound(2, f);
            Assert.False(it.IsEnd);
            Assert.Equal(7, it.Index);
            Assert.Equal(2.131673274757697, it.Item, 12);
            it = t.GetIterator(0);
            Assert.False(it.IsEnd);
            Assert.Equal(0, it.Index);
            Assert.Equal(0.3887144797393699, it.Item, 12);
            it = it + (3);
            Assert.False(it.IsEnd);
            Assert.Equal(3, it.Index);
            Assert.Equal(1.076024707367829, it.Item, 12);
            // Step 19
            t.EraseAll(4.565181172560389);
            it = t.Find(1, g);
            Assert.False(it.IsEnd);
            Assert.Equal(0, it.Index);
            Assert.Equal(0.3887144797393699, it.Item, 12);
            it = t.LowerBound(10, f);
            Assert.True(it.IsEnd);
            it = t.GetIterator(6);
            Assert.False(it.IsEnd);
            Assert.Equal(6, it.Index);
            Assert.Equal(2.0308421099467595, it.Item, 12);
            it = it + (-1);
            Assert.False(it.IsEnd);
            Assert.Equal(5, it.Index);
            Assert.Equal(1.8545647134977932, it.Item, 12);
            // Step 20
            t.Insert(4.579398394068253);
            it = t.Find(2, g);
            Assert.False(it.IsEnd);
            Assert.Equal(4, it.Index);
            Assert.Equal(1.6453813765877212, it.Item, 12);
            it = t.LowerBound(5, f);
            Assert.False(it.IsEnd);
            Assert.Equal(8, it.Index);
            Assert.Equal(4.52922390360878, it.Item, 12);
            it = t.GetIterator(7);
            Assert.False(it.IsEnd);
            Assert.Equal(7, it.Index);
            Assert.Equal(2.131673274757697, it.Item, 12);
            it = it + (-7);
            Assert.False(it.IsEnd);
            Assert.Equal(0, it.Index);
            Assert.Equal(0.3887144797393699, it.Item, 12);
            // Step 21
            t.Insert(0.6899124905288484);
            it = t.Find(3, g);
            Assert.True(it.IsEnd);
            it = t.LowerBound(0, f);
            Assert.False(it.IsEnd);
            Assert.Equal(0, it.Index);
            Assert.Equal(0.3887144797393699, it.Item, 12);
            it = t.GetIterator(2);
            Assert.False(it.IsEnd);
            Assert.Equal(2, it.Index);
            Assert.Equal(0.6899124905288484, it.Item, 12);
            it = it + (3);
            Assert.False(it.IsEnd);
            Assert.Equal(5, it.Index);
            Assert.Equal(1.6453813765877212, it.Item, 12);
            // Step 22
            t.EraseAll(2.131673274757697);
            it = t.Find(2, g);
            Assert.False(it.IsEnd);
            Assert.Equal(5, it.Index);
            Assert.Equal(1.6453813765877212, it.Item, 12);
            it = t.LowerBound(4, f);
            Assert.False(it.IsEnd); Assert.Equal(8, it.Index);
            Assert.Equal(4.52922390360878, it.Item, 12);
            it = t.GetIterator(0);
            Assert.False(it.IsEnd);
            Assert.Equal(0, it.Index);
            Assert.Equal(0.3887144797393699, it.Item, 12);
            it = it + (2);
            Assert.False(it.IsEnd);
            Assert.Equal(2, it.Index);
            Assert.Equal(0.6899124905288484, it.Item, 12);
            // Step 23
            t.Erase(4.579398394068253);
            it = t.Find(3, g);
            Assert.True(it.IsEnd);
            it = t.UpperBound(8, f);
            Assert.False(it.IsEnd);
            Assert.Equal(8, it.Index);
            Assert.Equal(4.52922390360878, it.Item, 12);
            it = t.GetIterator(9);
            Assert.True(it.IsEnd);
            it = it + (-2);
            Assert.False(it.IsEnd);
            Assert.Equal(7, it.Index);
            Assert.Equal(2.0308421099467595, it.Item, 12);
            // Step 24
            t.Insert(2.9367582896535795);
            it = t.Find(5, g);
            Assert.True(it.IsEnd);
            it = t.UpperBound(9, f);
            Assert.True(it.IsEnd);
            it = t.GetIterator(8);
            Assert.False(it.IsEnd);
            Assert.Equal(8, it.Index);
            Assert.Equal(2.9367582896535795, it.Item, 12);
            it = it + (-2);
            Assert.False(it.IsEnd);
            Assert.Equal(6, it.Index);
            Assert.Equal(1.8545647134977932, it.Item, 12);
            // Step 25
            t.EraseAll(0.3887144797393699);
            it = t.Find(5, g);
            Assert.True(it.IsEnd);
            it = t.UpperBound(3, f);
            Assert.False(it.IsEnd);
            Assert.Equal(7, it.Index);
            Assert.Equal(2.9367582896535795, it.Item, 12);
            it = t.GetIterator(8);
            Assert.False(it.IsEnd);
            Assert.Equal(8, it.Index);
            Assert.Equal(4.52922390360878, it.Item, 12);
            it = it + (-6);
            Assert.False(it.IsEnd);
            Assert.Equal(2, it.Index);
            Assert.Equal(0.7452219466928417, it.Item, 12);
            // Step 26
            t.EraseAll(2.0308421099467595);
            it = t.Find(1, g);
            Assert.False(it.IsEnd);
            Assert.Equal(0, it.Index);
            Assert.Equal(0.6625603376421318, it.Item, 12);
            it = t.UpperBound(9, f);
            Assert.True(it.IsEnd);
            it = t.GetIterator(6);
            Assert.False(it.IsEnd);
            Assert.Equal(6, it.Index);
            Assert.Equal(2.9367582896535795, it.Item, 12);
            it = it + (1);
            Assert.False(it.IsEnd);
            Assert.Equal(7, it.Index);
            Assert.Equal(4.52922390360878, it.Item, 12);
            // Step 27
            t.EraseAll(1.8545647134977932);
            it = t.Find(1, g);
            Assert.False(it.IsEnd);
            Assert.Equal(0, it.Index);
            Assert.Equal(0.6625603376421318, it.Item, 12);
            it = t.UpperBound(11, f);
            Assert.True(it.IsEnd);
            it = t.GetIterator(3);
            Assert.False(it.IsEnd);
            Assert.Equal(3, it.Index);
            Assert.Equal(1.076024707367829, it.Item, 12);
            it = it + (2);
            Assert.False(it.IsEnd);
            Assert.Equal(5, it.Index);
            Assert.Equal(2.9367582896535795, it.Item, 12);
            // Step 28
            it = t.Find(0.6625603376421318);
            t.Erase(it);
            it = t.Find(5, g);
            Assert.True(it.IsEnd);
            it = t.LowerBound(5, f);
            Assert.False(it.IsEnd);
            Assert.Equal(4, it.Index);
            Assert.Equal(2.9367582896535795, it.Item, 12);
            it = t.GetIterator(6);
            Assert.True(it.IsEnd);
            it = it + (-4);
            Assert.False(it.IsEnd);
            Assert.Equal(2, it.Index);
            Assert.Equal(1.076024707367829, it.Item, 12);
            // Step 29
            t.Insert(2.759940940867644);
            it = t.Find(5, g);
            Assert.True(it.IsEnd);
            it = t.UpperBound(5, f);
            Assert.False(it.IsEnd);
            Assert.Equal(6, it.Index);
            Assert.Equal(4.52922390360878, it.Item, 12);
            it = t.GetIterator(7);
            Assert.True(it.IsEnd);
            it = it + (-2);
            Assert.False(it.IsEnd);
            Assert.Equal(5, it.Index);
            Assert.Equal(2.9367582896535795, it.Item, 12);
            // Step 30
            t.Erase(2.9367582896535795);
            it = t.Find(5, g);
            Assert.True(it.IsEnd);
            it = t.LowerBound(10, f);
            Assert.True(it.IsEnd);
            it = t.GetIterator(2);
            Assert.False(it.IsEnd);
            Assert.Equal(2, it.Index);
            Assert.Equal(1.076024707367829, it.Item, 12);
            it = it + (0);
            Assert.False(it.IsEnd);
            Assert.Equal(2, it.Index);
            Assert.Equal(1.076024707367829, it.Item, 12);
        }
    }
}
