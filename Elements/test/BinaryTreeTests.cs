using Elements.Search;
using Elements.Geometry;
using Xunit;
using System.Collections.Generic;
using Xunit.Abstractions;

namespace Elements.Tests
{
    public class BinaryTreeTests
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

        public BinaryTreeTests(ITestOutputHelper output)
        {
            this._output = output;
        }

        private int[] _ints = new[] { 1, 7, 5, 3, 4, 6, 2 };

        [Fact]
        public void BinaryTreeInts()
        {
            var tree = new BinaryTree<int>(new IntComparer());
            foreach (var i in _ints)
            {
                tree.Add(i);
            }

            tree.FindPredecessorSuccessor(7, out BinaryTreeNode<int> pre, out BinaryTreeNode<int> suc);
            Assert.Equal(6, pre.Data);
            Assert.Null(suc);

            tree.FindPredecessorSuccessor(1, out pre, out suc);
            Assert.Null(pre);
            Assert.Equal(2, suc.Data);
        }

        [Fact]
        public void Root()
        {
            var tree = new BinaryTree<int>(new IntComparer());
            foreach (var i in _ints)
            {
                tree.Add(i);
            }

            Assert.Equal(tree.Root.Data, 1);
            Assert.Null(tree.Root.Parent);
        }

        [Fact]
        public void Remove()
        {
            var tree = new BinaryTree<int>(new IntComparer());
            foreach (var i in _ints)
            {
                tree.Add(i);
            }

            Assert.Equal(tree.Root.Data, 1);
            tree.Remove(1);
            Assert.Equal(tree.Root.Data, 7);
        }

        [Fact]
        public void BinaryTreeLines()
        {
            var b = new Line(new Vector3(0.1, 1, 0), new Vector3(6, 1, 0)); // horizontal
            var a = new Line(new Vector3(0, 0, 0), new Vector3(5, 0, 0));   // horizontal
            var c = new Line(new Vector3(0.2, -1), new Vector3(7, 0.2));    // lr -> ur

            var lines = new[] { a, b, c };

            var tree = new BinaryTree<int>(new LineSweepSegmentComparer(lines));

            for (var i = 0; i < lines.Length; i++)
            {
                tree.Add(i);
            }

            tree.FindPredecessorSuccessor(0, out BinaryTreeNode<int> pre, out BinaryTreeNode<int> suc);
            Assert.Equal(b, lines[pre.Data]);
            Assert.Equal(c, lines[suc.Data]);
        }
    }
}