using System.Collections.Generic;
using System.Text.Json.Serialization;
using Elements.Geometry;

namespace Elements.Flow
{
    public partial class ConnectionLocator
    {
        [JsonPropertyName("Network Reference")]
        public string NetworkReference { get; set; }

        public Line Line { get; set; }

        public string Purpose { get; set; }

        [JsonConstructor]
        public ConnectionLocator(string @networkReference, Line @line, string @purpose)
        {
            this.NetworkReference = @networkReference;
            this.Line = @line;
            this.Purpose = @purpose;
        }

        public ConnectionLocator(Section section, Line line = null)
        {
            NetworkReference = section.Tree.GetNetworkReference();
            Line = line;
            Purpose = section.Tree.Purpose;
        }

        public bool IsAlmostEqualTo(ConnectionLocator other, double tolerance = 1E-5)
        {
            return NetworkReference == other.NetworkReference
                   && Line.Direction().IsParallelTo(other.Line.Direction())
                   && Line.IsAlmostEqualTo(other.Line, true, tolerance)
                   && Purpose == other.Purpose;
        }

        public override string ToString()
        {
            return $"{this.NetworkReference}:{this.Purpose}:{this.Line}";
        }
    }
}