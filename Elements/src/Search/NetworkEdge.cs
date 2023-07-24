using System.Diagnostics;
using Elements.Geometry;

namespace Elements.Search
{
    internal class NetworkEdge
    {
        public NetworkNode Start { get; private set; }
        public NetworkNode End { get; private set; }
        public Vector3 Direction { get; private set; }
        public bool IsVisited { get; set; } = false;
        public NetworkEdge Opposite { get; private set; }

        public NetworkEdge(NetworkNode start, NetworkNode end)
        {
            visitDirections = VisitDirections.None;
            Start = start;
            End = end;
            Direction = (end.Position - start.Position).Unitized();
            Opposite = new NetworkEdge(end, start, this);
        }

        private NetworkEdge(NetworkNode start, NetworkNode end, NetworkEdge opposite)
        {
            Start = start;
            End = end;
            Opposite = opposite;
            Direction = opposite.Direction.Negate();
        }

        public NetworkNode GetOppositeNode(NetworkNode node)
        {
            if (Start.Equals(node))
            {
                return End;
            }

            if (End.Equals(node))
            {
                return Start;
            }

            Debug.Assert(false, $"The edge {this} isn't adjacent to the node {node}, so it cannot have a node that is opposite to it.");
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

            Debug.Assert(false, $"The edge {this} isn't adjacent to the node {node}, so the direction from it along the edge cannot be computed.");
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
