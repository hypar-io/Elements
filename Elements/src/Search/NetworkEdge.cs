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

        public bool IsAdjacentToNode(NetworkNode node)
        {
            return Start.Equals(node) || End.Equals(node);
        }

        public override string ToString()
        {
            return $"({Start.Id}; {End.Id})";
        }
    }
}
