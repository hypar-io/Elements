using Elements.Geometry;

namespace Elements.Search
{
    internal class NetworkNode
    {
        public int Id { get; private set; }
        public Vector3 Position { get; private set; }
        public int CountOfVisits { get; private set; }

        public NetworkNode(int id, Vector3 pos)
        {
            Id = id;
            Position = pos;
            CountOfVisits = 0;
        }

        public override bool Equals(object obj)
        {
            if (!(obj is NetworkNode))
            {
                return false;
            }

            return Id == ((NetworkNode)obj).Id;
        }

        public override int GetHashCode()
        {
            return Id.GetHashCode();
        }

        public void MarkVisited()
        {
            CountOfVisits++;
        }

        public override string ToString()
        {
            return $"{Id}: {Position}";
        }
    }
}
