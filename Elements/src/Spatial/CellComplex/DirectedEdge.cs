using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;
using Elements.Geometry;

namespace Elements.Spatial.CellComplex
{
    /// <summary>
    /// A directed uniqueEdge: a representation of a uniqueEdge that has direction to it so that it can be used to traverse faces
    /// </summary>
    public class DirectedEdge : EdgeBase
    {
        /// <summary>
        /// ID of uniqueEdge
        /// </summary>
        public long EdgeId;

        /// <summary>
        /// Edge
        /// </summary>
        /// <value></value>
        [JsonIgnore]
        public HashSet<Face> Faces = new HashSet<Face>();

        /// <summary>
        /// Represents a unique DirectedEdge within a CellComplex.
        /// This is added in addition to Edge because the same line may be required to move in a different direction
        /// as we traverse the uniqueEdges of a face in their correctly-wound order.
        /// Is not intended to be created or modified outside of the CellComplex class code.
        /// </summary>
        /// <param name="cellComplex">CellComplex that this belongs to</param>
        /// <param name="id"></param>
        /// <param name="uniqueEdge">The undirected Edge that matches this DirectedEdge</param>
        /// <param name="uniqueEdgeOrderMatchesDirection">If true, start point is same as uniqueEdge.vertex1Id. Otherwise, is flipped.</param>
        internal DirectedEdge(CellComplex cellComplex, long id, UniqueEdge uniqueEdge, bool uniqueEdgeOrderMatchesDirection) : base(id, cellComplex)
        {
            this.EdgeId = uniqueEdge.Id;

            if (uniqueEdgeOrderMatchesDirection)
            {
                this.StartVertexId = uniqueEdge.StartVertexId;
                this.EndVertexId = uniqueEdge.EndVertexId;
            }
            else
            {
                this.StartVertexId = uniqueEdge.EndVertexId;
                this.EndVertexId = uniqueEdge.StartVertexId;
            }
        }

        /// <summary>
        /// Used for deserialization only!
        /// </summary>
        [JsonConstructor]
        internal DirectedEdge(long id, long uniqueEdgeId, long startVertexId, long endVertexId) : base(id, null)
        {
            this.EdgeId = uniqueEdgeId;
            this.StartVertexId = startVertexId;
            this.EndVertexId = endVertexId;
        }

        /// <summary>
        /// Gets associated Edge
        /// </summary>
        /// <returns></returns>
        public UniqueEdge GetUniqueEdge()
        {
            return this.CellComplex.GetUniqueEdge(this.EdgeId);
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