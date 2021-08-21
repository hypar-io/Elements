using System;
using System.Collections.Generic;
using System.Text;
using Elements.Geometry;

namespace Elements.Search
{
    /// <summary>
    /// An generic adjacency list.
    /// Stores an undirected graph of connected nodes which reference
    /// data of type T.
    /// </summary>
    /// <typeparam name="T">The type of data referenced by the 'nodes' of the 
    /// adjacency list.</typeparam>
    public class AdjacencyList<T>
    {
        LinkedList<(int id, T data)>[] _adjacencyList;

        /// <summary>
        /// Create an adjacency list.
        /// </summary>
        /// <param name="length">The length of the list.</param>
        public AdjacencyList(int length)
        {
            _adjacencyList = new LinkedList<(int id, T data)>[length];

            for (int i = 0; i < _adjacencyList.Length; ++i)
            {
                _adjacencyList[i] = new LinkedList<(int id, T data)>();
            }
        }

        /// <summary>
        /// Add an edge pointing from the start to the end,
        /// at the end of the list.
        /// </summary>
        /// <param name="start">The start of the edge.</param>
        /// <param name="end">The end of the edge.</param>
        /// <param name="data">The data stored on the edge.</param>
        public void AddEdgeAtEnd(int start, int end, T data)
        {
            _adjacencyList[start].AddLast((end, data));
        }

        /// <summary>
        /// Add an edge pointing from the start to the end,
        /// at the beginning of the list.
        /// </summary>
        /// <param name="start">The start of the edge.</param>
        /// <param name="end">The end of the edge.</param>
        /// <param name="data">The data stored on the edge.</param>
        public void AddEdgeAtBeginning(int start, int end, T data)
        {
            _adjacencyList[start].AddFirst((end, data));
        }

        /// <summary>
        /// Get the number of vertices in the list.
        /// </summary>
        public int GetNumberOfVertices()
        {
            return _adjacencyList.Length;
        }

        /// <summary>
        /// Get the list of connected edges to the specified index.
        /// </summary>
        /// <param name="index"></param>
        public LinkedList<(int id, T data)> this[int index]
        {
            get
            {
                LinkedList<(int id, T data)> edgeList
                               = new LinkedList<(int id, T data)>(_adjacencyList[index]);

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

            foreach (LinkedList<(int id, T element)> list in _adjacencyList)
            {
                sb.Append("[" + i + "] -> ");

                foreach ((int id, T element) edge in list)
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
        /// <param name="start"></param>
        /// <param name="end"></param>
        /// <param name="element"></param>
        /// <returns></returns>
        public bool TryRemoveEdge(int start, int end, T element)
        {
            (int vertex, T element) edge = (end, element);

            return _adjacencyList[start].Remove(edge);
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
        public static ModelArrows ToModelArrows(this AdjacencyList<Vector3> adj, IList<Vector3> pts, Color? color)
        {
            var r = new Random();

            var arrowData = new List<(Vector3 origin, Vector3 direction, double scale, Color? color)>();

            for (var i = 0; i < adj.GetNumberOfVertices(); i++)
            {
                var start = pts[i];
                foreach (var end in adj[i])
                {
                    var d = (pts[end.id] - start).Unitized();
                    var l = pts[end.id].DistanceTo(start);
                    arrowData.Add((start, d, l, Colors.Red));
                }
            }

            return new ModelArrows(arrowData, arrowAngle: 75);
        }
    }
}