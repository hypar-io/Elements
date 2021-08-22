using System;
using System.Collections.Generic;
using System.Text;
using Elements.Geometry;

namespace Elements.Search
{
    /// <summary>
    /// An adjacency list.
    /// Stores an undirected graph of connected.
    /// </summary>
    public class AdjacencyList
    {
        List<LinkedList<int>> _adjacencyList;

        /// <summary>
        /// Create an adjacency list with the size of the provided collection.
        /// </summary>
        /// <param name="length">The length of the list.</param>
        public AdjacencyList(int length)
        {
            _adjacencyList = new List<LinkedList<int>>(length);

            for (int i = 0; i < _adjacencyList.Count; ++i)
            {
                _adjacencyList[i] = new LinkedList<int>();
            }
        }

        /// <summary>
        /// Create an adjacency list with no items.
        /// </summary>
        public AdjacencyList()
        {
            _adjacencyList = new List<LinkedList<int>>();
        }

        /// <summary>
        /// Add a vertex.
        /// </summary>
        /// <returns>The index of the vertex.</returns>
        public int AddVertex()
        {
            _adjacencyList.Add(new LinkedList<int>());
            var index = _adjacencyList.Count - 1;
            return index;
        }

        /// <summary>
        /// Add an edge pointing from the start to the end,
        /// at the end of the list.
        /// </summary>
        /// <param name="start">The start of the edge.</param>
        /// <param name="end">The end of the edge.</param>
        public void AddEdgeAtEnd(int start, int end)
        {
            _adjacencyList[start].AddLast(end);
        }

        /// <summary>
        /// Add an edge pointing from the start to the end,
        /// at the beginning of the list.
        /// </summary>
        /// <param name="start">The start of the edge.</param>
        /// <param name="end">The end of the edge.</param>
        public void AddEdgeAtBeginning(int start, int end)
        {
            _adjacencyList[start].AddFirst(end);
        }

        /// <summary>
        /// Get the number of vertices in the list.
        /// </summary>
        public int GetNumberOfVertices()
        {
            return _adjacencyList.Count;
        }

        /// <summary>
        /// Get the list of connected edges to the specified index.
        /// </summary>
        /// <param name="index"></param>
        public LinkedList<int> this[int index]
        {
            get
            {
                var edgeList = new LinkedList<int>(_adjacencyList[index]);
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

            foreach (LinkedList<int> list in _adjacencyList)
            {
                sb.Append("[" + i + "] -> ");

                foreach (var edge in list)
                {
                    sb.Append($"{edge} -> ");
                }

                ++i;
                sb.AppendLine();
            }
            return sb.ToString();
        }

        /// <summary>
        /// Try removing an edge.
        /// </summary>
        /// <param name="start"></param>
        /// <param name="end"></param>
        /// <returns></returns>
        public bool TryRemoveEdge(int start, int end)
        {
            return _adjacencyList[start].Remove(end);
        }
    }

    /// <summary>
    /// Adjacency list extensions.
    /// </summary>
    public static class AdjacencyListExtensions
    {
        /// <summary>
        /// Draw the adjacency list.
        /// </summary>
        /// <param name="adj"></param>
        /// <param name="pts"></param>
        /// <param name="color"></param>
        /// <returns></returns>
        public static ModelArrows ToModelArrows(this AdjacencyList adj, IList<Vector3> pts, Color? color)
        {
            var r = new Random();

            var arrowData = new List<(Vector3 origin, Vector3 direction, double scale, Color? color)>();

            for (var i = 0; i < adj.GetNumberOfVertices(); i++)
            {
                var start = pts[i];
                foreach (var end in adj[i])
                {
                    var d = (pts[end] - start).Unitized();
                    var l = pts[end].DistanceTo(start);
                    arrowData.Add((start, d, l, color));
                }
            }

            return new ModelArrows(arrowData, arrowAngle: 75);
        }
    }
}