using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;
using Elements.Geometry;

namespace Elements.Spatial.CellComplex
{
    /// <summary>
    /// A directed segment: a representation of a segment that has direction to it so that it can be used to traverse faces
    /// </summary>
    public class DirectedSegment : CellChild<Line>
    {
        /// <summary>
        /// ID of segment
        /// </summary>
        public long SegmentId;

        /// <summary>
        /// ID of start vertex
        /// </summary>
        public long StartVertexId;

        /// <summary>
        /// ID of end vertex
        /// </summary>
        public long EndVertexId;

        /// <summary>
        /// Segment
        /// </summary>
        /// <value></value>
        [JsonIgnore]
        public HashSet<Face> Faces = new HashSet<Face>();

        /// <summary>
        /// Represents a unique DirectedSegment within a CellComplex.
        /// This is added in addition to Segment because the same line may be required to move in a different direction
        /// as we traverse the edges of a face in their correctly-wound order.
        /// Is not intended to be created or modified outside of the CellComplex class code.
        /// </summary>
        /// <param name="cellComplex">CellComplex that this belongs to</param>
        /// <param name="id"></param>
        /// <param name="segment">The undirected Segment that matches this DirectedSegment</param>
        /// <param name="segmentOrderMatchesDirection">If true, start point is same as segment.vertex1Id. Otherwise, is flipped.</param>
        internal DirectedSegment(CellComplex cellComplex, long id, Segment segment, bool segmentOrderMatchesDirection) : base(id, cellComplex)
        {
            this.SegmentId = segment.Id;

            if (segmentOrderMatchesDirection)
            {
                this.StartVertexId = segment.Vertex1Id;
                this.EndVertexId = segment.Vertex2Id;
            }
            else
            {
                this.StartVertexId = segment.Vertex2Id;
                this.EndVertexId = segment.Vertex1Id;
            }
        }

        /// <summary>
        /// Used for deserialization only!
        /// </summary>
        [JsonConstructor]
        internal DirectedSegment(long id, long segmentId, long startVertexId, long endVertexId) : base(id, null)
        {
            this.Id = id;
            this.SegmentId = segmentId;
            this.StartVertexId = startVertexId;
            this.EndVertexId = endVertexId;
        }

        /// <summary>
        /// Get the geometry for this DirectedSegment
        /// </summary>
        /// <returns></returns>
        public override Line GetGeometry()
        {
            return new Line(
                this.CellComplex.GetVertex(this.StartVertexId).Value,
                this.CellComplex.GetVertex(this.EndVertexId).Value
            );
        }

        /// <summary>
        /// Gets associated Segment
        /// </summary>
        /// <returns></returns>
        public Segment GetSegment()
        {
            return this.CellComplex.GetSegment(this.SegmentId);
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