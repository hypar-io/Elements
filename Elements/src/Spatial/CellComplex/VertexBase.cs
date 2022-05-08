using Elements.Geometry;
using System.Text.Json.Serialization;

namespace Elements.Spatial.CellComplex
{
    /// <summary>
    /// A unique Vertex or Orientation in a cell complex.
    /// </summary>
    public abstract class VertexBase<ChildClass> : ChildBase<ChildClass, Vector3> where ChildClass : ChildBase<ChildClass, Vector3>
    {
        /// <summary>
        /// Location in space if this is a Vertex, or a vector direction if this is an Orientation.
        /// </summary>
        public Vector3 Value;

        /// <summary>
        /// An optional user-supplied name.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Represents one Vertex or Orientation in a CellComplex.
        /// Is not intended to be created or modified outside of the CellComplex class code.
        /// </summary>
        /// <param name="cellComplex">CellComplex that this belongs to</param>
        /// <param name="id"></param>
        /// <param name="point">Location of the vertex</param>
        /// <param name="name">Optional name</param>
        internal VertexBase(CellComplex cellComplex, ulong id, Vector3 point, string name = null) : base(id, cellComplex)
        {
            this.Value = point;
            this.Name = name;
        }

        /// <summary>
        /// For deserialization only!
        /// </summary>
        /// <param name="id"></param>
        /// <param name="value"></param>
        /// <param name="name"></param>
        [JsonConstructor]
        public VertexBase(ulong id, Vector3 value, string name = null) : base(id, null)
        {
            this.Value = value;
            this.Name = name;
        }

        /// <summary>
        /// Get the Vector3 that represents this Vertex or Orientation.
        /// </summary>
        public override Vector3 GetGeometry()
        {
            return this.Value;
        }

        /// <summary>
        ///  Get the shortest distance from a point to the geometry representing this vertex.
        /// </summary>
        /// <param name="point"></param>
        public override double DistanceTo(Vector3 point)
        {
            return point.DistanceTo(this.GetGeometry());
        }
    }
}