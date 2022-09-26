using System;
using System.Collections.Generic;
using System.Text;

namespace Elements.Search
{
    /// <summary>
    /// A binary search tree.
    /// A custom comparer can be used to provide the addition logic for the tree.
    /// </summary>
    /// <typeparam name="T">The type of data referenced by the tree's nodes.</typeparam>
    internal class BinaryTree<T>
    {
        private IComparer<T> _comparer;

        /// <summary>
        /// Create a binary tree.
        /// </summary>
        /// <param name="comparer">A comparer of T, used to order nodes
        /// during insertion.</param>
        public BinaryTree(IComparer<T> comparer)
        {
            _comparer = comparer;
        }

        /// <summary>
        /// The root of the tree.
        /// </summary>
        public BinaryTreeNode<T> Root { get; set; }

        /// <summary>
        /// Find the predecessor and successor values in the tree.
        /// </summary>
        /// <param name="data">The </param>
        /// <param name="predecessor"></param>
        /// <param name="successor"></param>
        public void FindPredecessorSuccessor(T data, out BinaryTreeNode<T> predecessor, out BinaryTreeNode<T> successor)
        {
            var root = this.Root;
            predecessor = null;
            successor = null;

            while (root != null)
            {
                if (root.Data.Equals(data))
                {
                    if (root.Right != null)
                    {
                        successor = root.Right;
                        while (successor.Left != null)
                        {
                            successor = successor.Left;
                        }
                    }
                    if (root.Left != null)
                    {
                        predecessor = root.Left;
                        while (predecessor.Right != null)
                        {
                            predecessor = predecessor.Right;
                        }
                    }
                    return;
                }
                else
                {
                    var compare = _comparer.Compare(root.Data, data);
                    if (compare < 0)
                    {
                        predecessor = root;
                        root = root.Right;
                    }
                    else
                    {
                        successor = root;
                        root = root.Left;
                    }
                }
            }
        }

        /// <summary>
        /// Find all predecessors and successors of a value in the tree.
        /// </summary>
        /// <param name="data">The value around which to search.</param>
        /// <param name="predecessors">A collection of predecessor values.</param>
        /// <param name="successors">A collection of successor values.</param>
        public void FindPredecessorSuccessors(T data, out List<BinaryTreeNode<T>> predecessors, out List<BinaryTreeNode<T>> successors)
        {
            predecessors = new List<BinaryTreeNode<T>>();
            successors = new List<BinaryTreeNode<T>>();

            FindPredecessorSuccessor(data, out BinaryTreeNode<T> predecessor, out BinaryTreeNode<T> successor);

            while (predecessor != null)
            {
                predecessors.Add(predecessor);
                FindPredecessorSuccessor(predecessor.Data, out predecessor, out _);
            }

            while (successor != null)
            {
                successors.Add(successor);
                FindPredecessorSuccessor(successor.Data, out _, out successor);
            }
        }

        /// <summary>
        /// Add a value to the tree.
        /// </summary>
        /// <param name="value">The value to add.</param>
        /// <returns>True if the value was added successfully, otherwise false.</returns>
        public bool Add(T value)
        {
            BinaryTreeNode<T> before = null;
            var after = this.Root;

            while (after != null)
            {
                before = after;

                var compare = _comparer.Compare(value, after.Data);
                if (compare < 0)
                {
                    after = after.Left;
                }
                else if (compare > 0)
                {
                    after = after.Right;
                }
                else
                {
                    return false;
                }
            }

            BinaryTreeNode<T> newNode = new BinaryTreeNode<T>
            {
                Data = value
            };

            if (this.Root == null)
            {
                this.Root = newNode;
            }
            else
            {
                var compare = _comparer.Compare(value, before.Data);
                if (compare < 0)
                {
                    before.Left = newNode;
                }
                else
                {
                    before.Right = newNode;
                }
                newNode.Parent = before;
            }

            return true;
        }

        /// <summary>
        /// Find a value in the tree.
        /// </summary>
        /// <param name="value">The value to find.</param>
        /// <returns>The node containing the value.</returns>
        public BinaryTreeNode<T> Find(T value)
        {
            return this.Find(value, this.Root);
        }

        /// <summary>
        /// Remove a value from the tree.
        /// </summary>
        /// <param name="value">The value to remove.</param>
        public void Remove(T value)
        {
            this.Root = Remove(this.Root, value);
        }

        private BinaryTreeNode<T> Remove(BinaryTreeNode<T> parent, T key)
        {
            if (parent == null) return parent;

            var compare = _comparer.Compare(key, parent.Data);
            if (compare < 0)
            {
                parent.Left = Remove(parent.Left, key);
            }
            else if (compare > 0)
            {
                parent.Right = Remove(parent.Right, key);
            }
            else
            {
                if (parent.Left == null)
                {
                    return parent.Right;
                }
                else if (parent.Right == null)
                {
                    return parent.Left;
                }

                parent.Data = MinValue(parent.Right);
                parent.Right = Remove(parent.Right, parent.Data);
            }

            return parent;
        }

        private T MinValue(BinaryTreeNode<T> node)
        {
            T minv = node.Data;

            while (node.Left != null)
            {
                minv = node.Left.Data;
                node = node.Left;
            }

            return minv;
        }

        private BinaryTreeNode<T> Find(T value, BinaryTreeNode<T> parent)
        {
            if (parent == null)
            {
                return null;
            }

            var compare = _comparer.Compare(value, parent.Data);
            if (compare == 0)
            {
                return parent;
            }
            if (compare < 0)
            {
                return Find(value, parent.Left);
            }
            else
            {
                return Find(value, parent.Right);
            }
        }

        /// <summary>
        /// Get the depth of the tree.
        /// </summary>
        /// <returns>The maximum depth of the tree.</returns>
        public int GetTreeDepth()
        {
            return this.GetTreeDepth(this.Root);
        }

        private int GetTreeDepth(BinaryTreeNode<T> parent)
        {
            return parent == null ? 0 : Math.Max(GetTreeDepth(parent.Left), GetTreeDepth(parent.Right)) + 1;
        }

        public override string ToString()
        {
            var sb = new StringBuilder();
            if (Root != null)
            {
                sb.AppendLine($"ROOT: {Root.Data}");
                Root.Print(sb, "\t");
            }
            return sb.ToString();
        }
    }
}