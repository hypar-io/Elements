using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;
using Elements.Geometry;

namespace Elements.Spatial.CellComplex
{
    /// <summary>
    /// A directed edge: a representation of a edge that has direction to it so that it can be used to traverse faces
    /// </summary>
    public class HalfEdge : EdgeBase
    {
        /// <summary>
        /// ID of edge
        /// </summary>
        public long EdgeId;

        /// <summary>
        /// Edge
        /// </summary>
        /// <value></value>
        [JsonIgnore]
        public HashSet<Face> Faces = new HashSet<Face>();

        /// <summary>
        /// Represents a unique HalfEdge within a CellComplex.
        /// This is added in addition to Edge because the same line may be required to move in a different direction
        /// as we traverse the edges of a face in their correctly-wound order.
        /// Is not intended to be created or modified outside of the CellComplex class code.
        /// </summary>
        /// <param name="cellComplex">CellComplex that this belongs to</param>
        /// <param name="id"></param>
        /// <param name="edge">The undirected Edge that matches this HalfEdge</param>
        /// <param name="edgeOrderMatchesDirection">If true, start point is same as edge.vertex1Id. Otherwise, is flipped.</param>
        internal HalfEdge(CellComplex cellComplex, long id, Edge edge, bool edgeOrderMatchesDirection) : base(id, cellComplex)
        {
            this.EdgeId = edge.Id;

            if (edgeOrderMatchesDirection)
            {
                this.StartVertexId = edge.StartVertexId;
                this.EndVertexId = edge.EndVertexId;
            }
            else
            {
                this.StartVertexId = edge.EndVertexId;
                this.EndVertexId = edge.StartVertexId;
            }
        }

        /// <summary>
        /// Used for deserialization only!
        /// </summary>
        [JsonConstructor]
        internal HalfEdge(long id, long edgeId, long startVertexId, long endVertexId) : base(id, null)
        {
            this.Id = id;
            this.EdgeId = edgeId;
            this.StartVertexId = startVertexId;
            this.EndVertexId = endVertexId;
        }

        /// <summary>
        /// Gets associated Edge
        /// </summary>
        /// <returns></returns>
        public Edge GetEdge()
        {
            return this.CellComplex.GetEdge(this.EdgeId);
        }

        /// <summary>
        /// Get associated Faces
        /// </summary>
        /// <returns></returns>
        public List<Face> GetFaces()
        {
            return this.Faces.ToList();
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