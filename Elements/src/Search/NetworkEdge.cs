using System;
using System.Diagnostics;
using Elements.Geometry;

namespace Elements.Search
{
    internal class NetworkEdge
    {
        [Flags]
        internal enum VisitDirections
        {
            None,
            Straight,
            Opposite
        }

        public NetworkNode Start { get; private set; }
        public NetworkNode End { get; private set; }

        public NetworkEdge(NetworkNode start, NetworkNode end)
        {
            Start = start;
            End = end;
        }

        public NetworkNode GetOppositeNode(NetworkNode node)
        {
            if (node.Equals(Start))
            {
                return End;
            }

            if (node.Equals(End))
            {
                return Start;
            }

            Debug.Assert(false, "The node isn't an ending of an edge.");
            return null;
        }

        public Vector3 GetDirectionFrom(NetworkNode node)
        {
            if (node.Equals(Start))
            {
                return (End.Position - Start.Position).Unitized();
            }

            if (node.Equals(End))
            {
                return (Start.Position - End.Position).Unitized();
            }

            return new Vector3();
        }

        public bool IsAdjacentToNode(NetworkNode node)
        {
            return Start.Equals(node) || End.Equals(node);
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
