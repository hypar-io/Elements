using System;
using System.Collections.Generic;
using System.Text;

namespace Elements.Search
{
    /// <summary>
    /// An adjacency list.
    /// Stores an undirected graph of connected nodes.
    /// </summary>
    internal class AdjacencyList<T>
    {
        List<LinkedList<(int, T)>> _nodes;

        /// <summary>
        /// Create an adjacency list with the size of the provided collection.
        /// </summary>
        /// <param name="length">The length of the list.</param>
        public AdjacencyList(int length)
        {
            _nodes = new List<LinkedList<(int, T)>>(length);

            for (int i = 0; i < _nodes.Count; ++i)
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
                throw new Exception("What the fuck happened here?");
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

                ++i;
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

        /// <summary>
        /// Get all branch nodes.
        /// </summary>
        /// <returns></returns>
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

        private bool IsLeaf(int i)
        {
            return this._nodes[i].Count == 0;
        }

        public int NodeCount()
        {
            return this._nodes.Count;
        }
    }
}