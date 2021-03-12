using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using Elements.Geometry;

namespace Elements.Spatial.CellComplex
{
    /// <summary>
    /// A face of a cell. Multiple cells can share the same face.
    /// </summary>
    public class Face : CellChild
    {
        /// <summary>
        /// ID of U direction
        /// </summary>
        public long? UId;

        /// <summary>
        /// ID of V direction
        /// </summary>
        public long? VId;

        /// <summary>
        /// Directed segment IDs
        /// </summary>
        public List<long> DirectedSegmentIds;

        /// <summary>
        /// Cells that reference this Face
        /// </summary>
        [JsonIgnore]
        public HashSet<Cell> Cells { get; internal set; } = new HashSet<Cell>();

        /// <summary>
        /// Represents a unique Face within a CellComplex.
        /// Is not intended to be created or modified outside of the CellComplex class code.
        /// </summary>
        /// <param name="cellComplex">CellComplex that this Face belongs to</param>
        /// <param name="id"></param>
        /// <param name="directedSegments">List of the DirectedSegments that make up this Face</param>
        /// <param name="u">Optional but highly recommended intended U direction for the Face</param>
        /// <param name="v">Optional but highly recommended intended V direction for the Face</param>
        internal Face(CellComplex cellComplex, long id, List<DirectedSegment> directedSegments, UV u = null, UV v = null) : base(id, cellComplex)
        {
            this.DirectedSegmentIds = directedSegments.Select(ds => ds.Id).ToList();
            if (u != null)
            {
                this.UId = u.Id;
            }
            if (v != null)
            {
                this.VId = v.Id;
            }
        }

        /// <summary>
        /// Used for deserialization only!
        /// </summary>
        [JsonConstructor]
        internal Face(long id, List<long> directedSegmentIds, long? uId = null, long? vId = null) : base(id, null)
        {
            this.Id = id;
            this.DirectedSegmentIds = directedSegmentIds;
            this.UId = uId;
            this.VId = vId;
        }

        /// <summary>
        /// Get the geometry for this Face
        /// </summary>
        /// <returns></returns>
        public Polygon GetGeometry()
        {
            var vertices = this.GetDirectedSegments().Select(ds => ds.GetGeometry().Start).ToList();
            return new Polygon(vertices);
        }

        /// <summary>
        /// Get associated Cells
        /// </summary>
        /// <returns></returns>
        public List<Cell> GetCells()
        {
            return this.Cells.ToList();
        }

        /// <summary>
        /// Get associated DirectedSegments
        /// </summary>
        /// <returns></returns>
        public List<DirectedSegment> GetDirectedSegments()
        {
            return this.DirectedSegmentIds.Select(dsId => CellComplex.GetDirectedSegment(dsId)).ToList();
        }

        /// <summary>
        /// Get associated Segments
        /// </summary>
        /// <returns></returns>
        public List<Segment> GetSegments()
        {
            return this.GetDirectedSegments().Select(ds => ds.GetSegment()).ToList();
        }

        /// <summary>
        /// Face lookup hash is segmentIds in ascending order.
        /// We do not directly use the `directedSegmentIds` because they could wind differently on a shared face.
        /// </summary>
        /// <param name="directedSegments"></param>
        /// <returns></returns>
        public static string GetHash(List<DirectedSegment> directedSegments)
        {
            var sortedIds = directedSegments.Select(ds => ds.SegmentId).ToList();
            sortedIds.Sort();
            var hash = String.Join(",", sortedIds);
            return hash;
        }
    }
}