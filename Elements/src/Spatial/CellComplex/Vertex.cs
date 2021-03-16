using Elements.Geometry;
using System.Collections.Generic;
using Newtonsoft.Json;
using System.Linq;

namespace Elements.Spatial.CellComplex
{
    /// <summary>
    /// A unique vertex in a cell complex
    /// </summary>
    public class Vertex : CellChild<Vector3>
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
        /// All edges connected to this Vertex
        /// </summary>
        [JsonIgnore]
        public HashSet<Edge> Edges { get; internal set; } = new HashSet<Edge>();

        /// <summary>
        /// Represents a unique Vertex within a CellComplex.
        /// Is not intended to be created or modified outside of the CellComplex class code.
        /// </summary>
        /// <param name="cellComplex">CellComplex that this belongs to</param>
        /// <param name="id"></param>
        /// <param name="point">Location of the vertex</param>
        /// <param name="name">Optional name</param>
        internal Vertex(CellComplex cellComplex, ulong id, Vector3 point, string name = null) : base(id, cellComplex)
        {
            this.Value = point;
            this.Name = name;
        }

        [JsonConstructor]
        internal Vertex(ulong id, Vector3 point, string name = null) : base(id, null)
        {
            this.Value = point;
            this.Name = name;
        }

        /// <summary>
        /// Get the Vector3 that represents this Vertex
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

        /// <summary>
        /// Get associated Edges
        /// </summary>
        /// <returns></returns>
        public List<Edge> GetEdges()
        {
            return this.Edges.ToList();
        }

        /// <summary>
        /// Get associated DirectedEdges
        /// </summary>
        /// <returns></returns>
        public List<DirectedEdge> GetDirectedEdges()
        {
            return this.GetEdges().Select(edge => edge.GetDirectedEdges()).SelectMany(x => x).Distinct().ToList();
        }

        /// <summary>
        /// Get associated Faces
        /// </summary>
        /// <returns></returns>
        public List<Face> GetFaces()
        {
            return this.GetDirectedEdges().Select(ds => ds.GetFaces()).SelectMany(x => x).Distinct().ToList();
        }

        /// <summary>
        /// Get associated Cells
        /// </summary>
        /// <returns></returns>
        public List<Cell> GetCells()
        {
            return this.GetFaces().Select(face => face.GetCells()).SelectMany(x => x).Distinct().ToList();
        }
    }
}