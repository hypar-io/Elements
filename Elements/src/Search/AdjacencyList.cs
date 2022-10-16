using System;
using System.Collections.Generic;
using System.Text;

namespace Elements.Search
{
    /// <summary>
    /// An undirected graph of connected items of type T.
    /// </summary>
    internal class AdjacencyList<T>
    {
        readonly List<LinkedList<(int target, T data)>> _nodes;

        /// <summary>
        /// Create an adjacency list with the size of the provided collection.
        /// </summary>
        /// <param name="length">The length of the list.</param>
        public AdjacencyList(int length)
        {
            _nodes = new List<LinkedList<(int, T)>>(length);

            for (int i = 0; i < _nodes.Count; i++)
            {
                _nodes[i] = new LinkedList<(int, T)>();
            }
        }

        /// <summary>
        /// Create an adjacency list with no items.
        /// </summary>
        public AdjacencyList()
        {
            _nodes = new List<LinkedList<(int, T)>>();
        }

        /// <summary>
        /// Add a vertex.
        /// </summary>
        /// <returns>The index of the vertex.</returns>
        public int AddVertex()
        {
            _nodes.Add(new LinkedList<(int, T)>());
            var index = _nodes.Count - 1;
            return index;
        }

        /// <summary>
        /// Add an edge pointing from the start to the end,
        /// at the end of the list.
        /// </summary>
        /// <param name="start">The start of the edge.</param>
        /// <param name="end">The end of the edge.</param>
        /// <param name="data">The data associated with the edge.</param>
        public void AddEdgeAtEnd(int start, int end, T data)
        {
            if (end == start)
            {
                throw new Exception($"An edge could not be created. The start, {start}, and end, {end}, of an edge are the same.");
            }

            if (!_nodes[start].Contains((end, data)))
            {
                _nodes[start].AddLast((end, data));
            }
        }

        /// <summary>
        /// Add an edge pointing from the start to the end,
        /// at the beginning of the list.
        /// </summary>
        /// <param name="start">The start of the edge.</param>
        /// <param name="end">The end of the edge.</param>
        /// <param name="data">The data associated with the edge.</param>
        public void AddEdgeAtBeginning(int start, int end, T data)
        {
            if (end == start)
            {
                throw new Exception($"An edge could not be created. The start, {start}, and end, {end}, of an edge are the same.");
            }

            _nodes[start].AddFirst((end, data));
        }

        /// <summary>
        /// Get the number of vertices in the list.
        /// </summary>
        public int GetNumberOfVertices()
        {
            return _nodes.Count;
        }

        /// <summary>
        /// Get the list of connected edges to the specified index.
        /// </summary>
        /// <param name="index"></param>
        public LinkedList<(int, T)> this[int index]
        {
            get
            {
                var edgeList = new LinkedList<(int, T)>(_nodes[index]);
                return edgeList;
            }
        }

        /// <summary>
        /// A string representation of the adjacency list.
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            int i = 0;

            var sb = new StringBuilder();

            foreach (LinkedList<(int, T)> list in _nodes)
            {
                sb.Append("[" + i + "] -> ");

                foreach (var edge in list)
                {
                    sb.Append($"{edge.Item1} -> ");
                }

                i++;
                sb.AppendLine();
            }
            return sb.ToString();
        }

        /// <summary>
        /// Try removing an edge.
        /// </summary>
        /// <param name="start">The start index of the edge.</param>
        /// <param name="end">The end index of the edge.</param>
        /// <param name="data">The data associated with the edge.</param>
        /// <returns>True if the edge could be removed, otherwise false.</returns>
        public bool TryRemoveEdge(int start, int end, T data)
        {
            return _nodes[start].Remove((end, data));
        }

        /// <summary>
        /// Get leaf nodes.
        /// </summary>
        /// <returns></returns>
        public List<int> Leaves()
        {
            var result = new List<int>();
            for (var i = 0; i < this._nodes.Count; i++)
            {
                if (IsLeaf(i))
                {
                    result.Add(i);
                }
            }
            return result;
        }

        public void RemoveLeaves()
        {
            // Mark removals to avoid errors on trying to remove
            // during iteration.
            var removals = new List<(int index, (int target, T data) item)>();

            var leaves = Leaves();
            foreach (var leaf in leaves)
            {
                // For each of the edges connected to this leaf node.
                foreach (var edge in this._nodes[leaf])
                {
                    // For each of the target's edges, find the one
                    // that connects to this leaf node.
                    foreach (var (target, data) in this._nodes[edge.target])
                    {
                        if (target == leaf)
                        {
                            // Remove the edge that points back to 
                            // this leaf node.
                            removals.Add((edge.target, (target, data)));
                        }
                    }

                    // Remove the edge from this leaf node to
                    // the target.
                    removals.Add((leaf, edge));
                }
            }

            foreach (var (index, item) in removals)
            {
                this._nodes[index].Remove(item);
            }
        }

        /// <summary>
        /// Get all branch nodes.
        /// </summary>
        public List<int> Branches()
        {
            var result = new List<int>();
            for (var i = 0; i < this._nodes.Count; i++)
            {
                if (!IsLeaf(i))
                {
                    result.Add(i);
                }
            }
            return result;
        }

        private bool NodeHasEdgeTo(int i, int target)
        {
            foreach (var edge in this._nodes[i])
            {
                if (edge.target == target)
                {
                    return true;
                }
            }
            return false;
        }

        private bool IsLeaf(int i)
        {
            // If the node points to 1 other node, and 
            // that node points back.
            if (this._nodes[i].Count == 1)
            {
                if (NodeHasEdgeTo(this._nodes[i].First.Value.target, i))
                {
                    return true;
                }
            }

            // If the node is pointed at by only 
            // one other node and it points at nothing.
            if (this._nodes[i].Count == 0)
            {
                var pointers = 0;
                for (var j = 0; j < this._nodes.Count; j++)
                {
                    if (NodeHasEdgeTo(j, i))
                    {
                        pointers++;
                    }
                }

                return pointers == 1;
            }

            return false;
        }

        public int NodeCount()
        {
            return this._nodes.Count;
        }
    }
}