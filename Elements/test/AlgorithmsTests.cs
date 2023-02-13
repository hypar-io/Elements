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
        public void TreapTest()
        {
            var t = new Treap<double>();
            Func<int, int> sqr = x => x * x;
            Func<double, int> f = x => sqr((int)Math.Floor(x));
            TreapIterator<double> it = null;

            t.Insert(6.95426297609277);
            it = t.LowerBound<int>(73, f);
            Assert.True(it.IsEnd);
            t.Insert(4.668336850892139);
            it = t.UpperBound<int>(99, f);
            Assert.True(it.IsEnd);
            t.Erase(4.668336850892139);
            it = t.LowerBound<int>(74, f);
            Assert.True(it.IsEnd);
            t.Erase(6.95426297609277);
            it = t.UpperBound<int>(88, f);
            Assert.True(it.IsEnd);
            t.Insert(9.968305186093815);
            it = t.UpperBound<int>(46, f);
            Assert.False(it.IsEnd);
            Assert.Equal(9.968305186093815, it.Get, 12);
            t.Insert(7.2758999021797095);
            it = t.LowerBound<int>(72, f);
            Assert.False(it.IsEnd);
            Assert.Equal(9.968305186093815, it.Get, 12);
            t.Insert(9.461649947205427);
            it = t.LowerBound<int>(72, f);
            Assert.False(it.IsEnd);
            Assert.Equal(9.461649947205427, it.Get, 12);
            t.Insert(6.341147827834378);
            it = t.UpperBound<int>(9, f);
            Assert.False(it.IsEnd);
            Assert.Equal(6.341147827834378, it.Get, 12);
            t.Insert(7.769533068454868);
            it = t.UpperBound<int>(32, f);
            Assert.False(it.IsEnd);
            Assert.Equal(6.341147827834378, it.Get, 12);
            t.Insert(7.514583784635568);
            it = t.UpperBound<int>(43, f);
            Assert.False(it.IsEnd);
            Assert.Equal(7.2758999021797095, it.Get, 12);
            t.Insert(3.3516617169510354);
            it = t.LowerBound<int>(11, f);
            Assert.False(it.IsEnd);
            Assert.Equal(6.341147827834378, it.Get, 12);
            t.Insert(1.3657418286513001);
            it = t.UpperBound<int>(47, f);
            Assert.False(it.IsEnd);
            Assert.Equal(7.2758999021797095, it.Get, 12);
            t.Insert(0.04604221033719047);
            it = t.UpperBound<int>(2, f);
            Assert.False(it.IsEnd);
            Assert.Equal(3.3516617169510354, it.Get, 12);
            t.Insert(3.910145978780913);
            it = t.LowerBound<int>(10, f);
            Assert.False(it.IsEnd);
            Assert.Equal(6.341147827834378, it.Get, 12);
            t.Insert(3.1139312769220395);
            it = t.UpperBound<int>(13, f);
            Assert.False(it.IsEnd);
            Assert.Equal(6.341147827834378, it.Get, 12);
            t.Insert(2.5257839781072278);
            it = t.UpperBound<int>(37, f);
            Assert.False(it.IsEnd);
            Assert.Equal(7.2758999021797095, it.Get, 12);
            t.Insert(1.1644555376648569);
            it = t.UpperBound<int>(100, f);
            Assert.True(it.IsEnd);
            t.Erase(3.910145978780913);
            it = t.LowerBound<int>(79, f);
            Assert.False(it.IsEnd);
            Assert.Equal(9.461649947205427, it.Get, 12);
            t.Insert(8.909218330191965);
            it = t.UpperBound<int>(8, f);
            Assert.False(it.IsEnd);
            Assert.Equal(3.1139312769220395, it.Get, 12);
            t.Erase(8.909218330191965);
            it = t.UpperBound<int>(41, f);
            Assert.False(it.IsEnd);
            Assert.Equal(7.2758999021797095, it.Get, 12);
            t.Insert(9.778862801144165);
            it = t.UpperBound<int>(17, f);
            Assert.False(it.IsEnd);
            Assert.Equal(6.341147827834378, it.Get, 12);
            t.Erase(7.514583784635568);
            it = t.UpperBound<int>(30, f);
            Assert.False(it.IsEnd);
            Assert.Equal(6.341147827834378, it.Get, 12);
            t.Insert(4.993021988651475);
            it = t.UpperBound<int>(20, f);
            Assert.False(it.IsEnd);
            Assert.Equal(6.341147827834378, it.Get, 12);
            t.Insert(9.961401425557991);
            it = t.LowerBound<int>(71, f);
            Assert.False(it.IsEnd);
            Assert.Equal(9.461649947205427, it.Get, 12);
            t.Insert(4.542821006542316);
            it = t.UpperBound<int>(54, f);
            Assert.False(it.IsEnd);
            Assert.Equal(9.461649947205427, it.Get, 12);
            t.Erase(9.961401425557991);
            it = t.LowerBound<int>(90, f);
            Assert.True(it.IsEnd);
            t.Erase(4.993021988651475);
            it = t.LowerBound<int>(42, f);
            Assert.False(it.IsEnd);
            Assert.Equal(7.2758999021797095, it.Get, 12);
            t.Insert(6.362479679379018);
            it = t.LowerBound<int>(91, f);
            Assert.True(it.IsEnd);
            t.Insert(2.0763767101690735);
            it = t.UpperBound<int>(81, f);
            Assert.True(it.IsEnd);
            t.Insert(3.081700759859334);
            it = t.UpperBound<int>(76, f);
            Assert.False(it.IsEnd);
            Assert.Equal(9.461649947205427, it.Get, 12);
            t.Insert(2.917931795345816);
            it = t.LowerBound<int>(94, f);
            Assert.True(it.IsEnd);
            t.Erase(3.081700759859334);
            it = t.UpperBound<int>(49, f);
            Assert.False(it.IsEnd);
            Assert.Equal(9.461649947205427, it.Get, 12);
            t.Insert(1.5455745882032579);
            it = t.UpperBound<int>(59, f);
            Assert.False(it.IsEnd);
            Assert.Equal(9.461649947205427, it.Get, 12);
            t.Erase(7.2758999021797095);
            it = t.UpperBound<int>(49, f);
            Assert.False(it.IsEnd);
            Assert.Equal(9.461649947205427, it.Get, 12);
            t.Erase(9.968305186093815);
            it = t.LowerBound<int>(60, f);
            Assert.False(it.IsEnd);
            Assert.Equal(9.461649947205427, it.Get, 12);
            t.Erase(0.04604221033719047);
            it = t.UpperBound<int>(87, f);
            Assert.True(it.IsEnd);
            t.Erase(1.1644555376648569);
            it = t.UpperBound<int>(39, f);
            Assert.False(it.IsEnd);
            Assert.Equal(7.769533068454868, it.Get, 12);
            t.Insert(0.8706102360098822);
            it = t.LowerBound<int>(48, f);
            Assert.False(it.IsEnd);
            Assert.Equal(7.769533068454868, it.Get, 12);
            t.Insert(1.640156058632718);
            it = t.UpperBound<int>(58, f);
            Assert.False(it.IsEnd);
            Assert.Equal(9.461649947205427, it.Get, 12);
            t.Erase(7.769533068454868);
            it = t.UpperBound<int>(76, f);
            Assert.False(it.IsEnd);
            Assert.Equal(9.461649947205427, it.Get, 12);
            t.Erase(0.8706102360098822);
            it = t.LowerBound<int>(32, f);
            Assert.False(it.IsEnd);
            Assert.Equal(6.341147827834378, it.Get, 12);
            t.Insert(3.321827142348303);
            it = t.LowerBound<int>(40, f);
            Assert.False(it.IsEnd);
            Assert.Equal(9.461649947205427, it.Get, 12);
            t.Insert(2.0444424601961764);
            it = t.LowerBound<int>(74, f);
            Assert.False(it.IsEnd);
            Assert.Equal(9.461649947205427, it.Get, 12);
            t.Erase(2.0444424601961764);
            it = t.UpperBound<int>(97, f);
            Assert.True(it.IsEnd);
            t.Insert(0.5744047511014572);
            it = t.LowerBound<int>(49, f);
            Assert.False(it.IsEnd);
            Assert.Equal(9.461649947205427, it.Get, 12);
            t.Erase(2.5257839781072278);
            it = t.UpperBound<int>(40, f);
            Assert.False(it.IsEnd);
            Assert.Equal(9.461649947205427, it.Get, 12);
            t.Erase(4.542821006542316);
            it = t.UpperBound<int>(44, f);
            Assert.False(it.IsEnd);
            Assert.Equal(9.461649947205427, it.Get, 12);
            t.Insert(7.699804678387859);
            it = t.UpperBound<int>(1, f);
            Assert.False(it.IsEnd);
            Assert.Equal(2.0763767101690735, it.Get, 12);
            t.Erase(1.5455745882032579);
            it = t.UpperBound<int>(9, f);
            Assert.False(it.IsEnd);
            Assert.Equal(6.341147827834378, it.Get, 12);
            t.Insert(2.7834535910992377);
            it = t.UpperBound<int>(51, f);
            Assert.False(it.IsEnd);
            Assert.Equal(9.461649947205427, it.Get, 12);
        }
    }
}
