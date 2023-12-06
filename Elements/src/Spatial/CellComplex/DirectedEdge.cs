using System.Text.Json.Serialization;
using System.Collections.Generic;
using System.Linq;

namespace Elements.Spatial.CellComplex
{
    /// <summary>
    /// A directed edge: a representation of a edge that has direction to it so that it can be used to traverse faces.
    /// This is essentially equivalent to the concept of a half edge, except it can be related to more than just a single Face.
    /// There is a maximum of two DirectedEdges per Edge.
    /// This class is completely internal and only used for utilities inside of CellComplex.
    /// </summary>
    public class DirectedEdge : EdgeBase<DirectedEdge>
    {
        /// <summary>
        /// ID of the associated, direction-agnostic Edge.
        /// </summary>
        public ulong EdgeId;

        /// <summary>
        /// The unique Faces that are associated with this DirectedEdge.
        /// </summary>
        /// <value></value>
        [JsonIgnore]
        internal HashSet<Face> Faces = new HashSet<Face>();

        /// <summary>
        /// Represents a unique DirectedEdge within a CellComplex.
        /// This is added in addition to Edge because the same line may be required to move in a different direction
        /// as we traverse the edges of a face in their correctly-wound order.
        /// Is not intended to be created or modified outside of the CellComplex class code.
        /// </summary>
        /// <param name="cellComplex">CellComplex that this DirectedEdge belongs to.</param>
        /// <param name="id">ID of this DirectedEdge.</param>
        /// <param name="edge">The undirected Edge that matches this DirectedEdge</param>
        /// <param name="edgeOrderMatchesDirection">If true, start point is same as edge.StartVertexId. Otherwise, is flipped.</param>
        internal DirectedEdge(CellComplex cellComplex, ulong id, Edge edge, bool edgeOrderMatchesDirection) : base(id, cellComplex)
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
        public DirectedEdge(ulong id, ulong edgeId, ulong startVertexId, ulong endVertexId) : base(id, null)
        {
            this.EdgeId = edgeId;
            this.StartVertexId = startVertexId;
            this.EndVertexId = endVertexId;
        }

        /// <summary>
        /// Gets associated Edge.
        /// </summary>
        /// <returns></returns>
        internal Edge GetEdge()
        {
            return this.CellComplex.GetEdge(this.EdgeId);
        }

        /// <summary>
        /// Get associated Faces.
        /// </summary>
        /// <returns></returns>
        internal List<Face> GetFaces()
        {
            return this.Faces.ToList();
        }
    }
}