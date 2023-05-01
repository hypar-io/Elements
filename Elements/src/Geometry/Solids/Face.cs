using System.Text;

namespace Elements.Geometry.Solids
{
    /// <summary>
    /// A Solid Face.
    /// </summary>
    public class Face
    {
        /// <summary>
        /// The Id of the Face.
        /// </summary>
        public uint Id { get; }

        /// <summary>
        /// A CCW wound list of Edges.
        /// </summary>
        public Loop Outer { get; internal set; }

        /// <summary>
        /// A collection of CW wound Edges.
        /// </summary>
        public Loop[] Inner { get; internal set; }

        /// <summary>
        /// Construct a Face.
        /// </summary>
        /// <param name="id"></param>
        /// <param name="outer">The outer loop of the Face.</param>
        /// <param name="inner">The inner loops of the Face.</param>
        internal Face(uint id, Loop outer, Loop[] inner)
        {
            this.Id = id;
            this.Outer = outer;
            outer.Face = this;
            this.Inner = inner;
            if (this.Inner != null)
            {
                foreach (var loop in inner)
                {
                    loop.Face = this;
                }
            }
        }

        /// <summary>
        /// The string representation of the Face.
        /// </summary>
        public override string ToString()
        {
            var sb = new StringBuilder();
            foreach (var he in this.Outer.Edges)
            {
                sb.AppendLine($"HalfEdge: {he.ToString()}");
            }
            return sb.ToString();
        }
    }
}