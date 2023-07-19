using System.Diagnostics;
using Elements.Geometry;

namespace Elements.Search
{
    internal class NetworkEdge
    {
        public NetworkNode Start { get; private set; }
        public NetworkNode End { get; private set; }

        public NetworkEdge(NetworkNode start, NetworkNode end)
        {
            visitDirections = VisitDirections.None;
            Start = start;
            End = end;
        }

        /// <summary>
        /// Mark a vertex as having been visited from the specified index.
        /// </summary>
        /// <param name="start">The index of the vertex from which the edge is visited.</param>
        public void MarkAsVisited(NetworkNode start)
        {
            if (Start.Equals(start))
            {
                visitDirections |= VisitDirections.Straight;
            }
            else if (End.Equals(start))
            {
                visitDirections |= VisitDirections.Opposite;
            }
        }

        /// <summary>
        /// Is this edge visited from the provided vertex?
        /// </summary>
        /// <param name="node">The node from which the vertex is visited.</param>
        /// <returns>Returns true if the edge was visited from the vertex.</returns>
        public bool IsVisitedFromVertex(NetworkNode node)
        {
            if (Start.Equals(node))
            {
                return visitDirections.HasFlag(VisitDirections.Straight);
            }

            if (End.Equals(node))
            {
                return visitDirections.HasFlag(VisitDirections.Opposite);
            }

            return false;
        }

        internal VisitDirections visitDirections;

        public override string ToString()
        {
            return $"({Start.Id}; {End.Id})";
        }
    }
}
