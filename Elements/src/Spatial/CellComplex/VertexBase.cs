using Elements.Geometry;
using System.Collections.Generic;
using Newtonsoft.Json;
using System.Linq;

namespace Elements.Spatial.CellComplex
{
    /// <summary>
    /// A unique vertex in a cell complex
    /// </summary>
    public abstract class VertexBase : ChildBase<Vector3>
    {
        /// <summary>
        /// Location in space
        /// </summary>
        public Vector3 Value;

        /// <summary>
        /// Some optional name, if we want it
        /// </summary>
        public string Name { get; set; }

        /// <summary>
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

        [JsonConstructor]
        internal VertexBase(ulong id, Vector3 point, string name = null) : base(id, null)
        {
            this.Value = point;
            this.Name = name;
        }

        /// <summary>
        /// Get the Vector3 that represents this Vertex or Orientation
        /// </summary>
        /// <returns></returns>
        public override Vector3 GetGeometry()
        {
            return this.Value;
        }

        /// <summary>
        /// Get the shortest distance to a given point
        /// </summary>
        /// <param name="point"></param>
        /// <returns></returns>
        public override double DistanceTo(Vector3 point)
        {
            return point.DistanceTo(this.GetGeometry());
        }
    }
}