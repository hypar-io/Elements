using System;
using System.Collections.Generic;
using System.Linq;
using Elements;
using Elements.Geometry;
using Newtonsoft.Json;

namespace Elements.Spatial.CellComplex
{
    /// <summary>
    /// A unique segment in a cell complex.
    /// </summary>
    public class Segment : CellChild
    {
        /// <summary>
        /// ID of first vertex
        /// </summary>
        public long Vertex1Id;

        /// <summary>
        /// ID of second vertex
        /// </summary>
        public long Vertex2Id;

        /// <summary>
        /// DirectedSegments that reference this Segment
        /// </summary>
        [JsonIgnore]
        public HashSet<DirectedSegment> DirectedSegments = new HashSet<DirectedSegment>();

        /// <summary>
        /// Represents a unique Segment within a CellComplex.
        /// Is not intended to be created or modified outside of the CellComplex class code.
        /// </summary>
        /// <param name="cellComplex">CellComplex that this belongs to</param>
        /// <param name="id"></param>
        /// <param name="index1">The lower-index-id number for this segment</param>
        /// <param name="index2">The higher-index-id number for this segment</param>
        internal Segment(CellComplex cellComplex, long id, long index1, long index2) : base(id, cellComplex)
        {
            this.Vertex1Id = index1;
            this.Vertex2Id = index2;
        }

        [JsonConstructor]
        internal Segment(long id, long index1, long index2) : base(id, null)
        {
            this.Vertex1Id = index1;
            this.Vertex2Id = index2;
        }

        /// <summary>
        /// Get the geometry for this Segment
        /// </summary>
        /// <returns></returns>
        public Line GetGeometry()
        {
            return new Line(
                this.CellComplex.GetVertex(this.Vertex1Id).Value,
                this.CellComplex.GetVertex(this.Vertex2Id).Value
            );
        }

        /// <summary>
        /// Get associated DirectedSegments
        /// </summary>
        /// <returns></returns>
        public List<DirectedSegment> GetDirectedSegments()
        {
            return this.DirectedSegments.ToList();
        }

        /// <summary>
        /// Get associated Faces
        /// </summary>
        /// <returns></returns>
        public List<Face> GetFaces()
        {
            return this.GetDirectedSegments().Select(ds => ds.GetFaces()).SelectMany(x => x).Distinct().ToList();
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