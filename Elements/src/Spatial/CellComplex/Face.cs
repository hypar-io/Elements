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
    public class Face : CellChild<Polygon>
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
        public override Polygon GetGeometry()
        {
            return new Polygon(this.GetVertices().Select(v => v.Value).ToList());
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
        /// Get associated Vertices
        /// </summary>
        /// <returns></returns>
        public List<Vertex> GetVertices()
        {
            return this.GetDirectedSegments().Select(ds => this.CellComplex.GetVertex(ds.StartVertexId)).ToList();
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

        /// <summary>
        /// Get the normal vector for this Face
        /// </summary>
        /// <returns></returns>
        public Vector3 GetNormal()
        {
            return this.GetGeometry().Normal();
        }

        /// <summary>
        /// Whether this Face is parallel to another Face
        /// </summary>
        /// <param name="face"></param>
        /// <returns></returns>
        public bool IsParallel(Face face)
        {
            return face.GetNormal().Equals(this.GetNormal());
        }

        /// <summary>
        /// Get a list of all neighbors of this face.
        /// A neighbor is defined as a Face which shares any segment.
        /// </summary>
        /// <param name="parallel">If true, only returns faces that are oriented the same way as this face</param>
        /// <param name="includeSharedVertices">If true, includes faces that share a vertex as well as faces that share a segment</param>
        /// <returns></returns>
        public List<Face> GetNeighbors(bool parallel = false, bool includeSharedVertices = false)
        {
            var groupedFaces = includeSharedVertices ? this.GetVertices().Select(v => v.GetFaces()).ToList() : this.GetSegments().Select(s => s.GetFaces()).ToList();
            var faces = groupedFaces.SelectMany(x => x).Distinct().Where(f => f.Id != this.Id).ToList();
            if (parallel)
            {
                return faces.Where(f => this.IsParallel(f)).ToList();
            }
            else
            {
                return faces;
            }
        }

        /// <summary>
        /// Get a list of neighbor faces that share a specific segment
        /// </summary>
        /// <param name="segment"></param>
        /// <param name="parallel">Whether to only return faces that are parallel to this face.</param>
        /// <returns></returns>
        public List<Face> GetNeighbors(Segment segment, bool parallel = false)
        {
            if (!parallel)
            {
                return segment.GetFaces().Where(face => face.Id != this.Id).ToList();
            }
            else
            {
                return segment.GetFaces().Where(face => face.Id != this.Id && this.IsParallel(face)).ToList();
            }
        }

        // /// <summary>
        // /// Gets the neighboring Faces which share a Segment.
        // /// Does not capture partially overlapping neighbor match.
        // /// </summary>
        // /// <param name="face">Face to get the neighbors for</param>
        // /// <param name="direction">If set, casts a ray from the centroid of this Face and only returns neighbors for the Segment which intersects this ray.</param>
        // /// <param name="matchDirectionToUOrV">If true, further filters results to only those where the resulting face's U or V direction matches the given direction.</param>
        // /// <returns></returns>
        // public List<Face> GetNeighbors(Face face, Nullable<Vector3> direction = null, Boolean matchDirectionToUOrV = false)
        // {
        //     var segments = face.GetSegments();
        //     List<Face> faces;
        //     if (direction == null)
        //     {
        //         faces = segments.Select(s => s.GetFaces()).SelectMany(x => x).Distinct().ToList();
        //         return (from f in faces where f.Id != face.Id select f).ToList();
        //     }
        //     var segsWithGeos = segments.Select(segment => (segment: segment, geo: segment.GetGeometry())).ToList();
        //     var ray = new Ray(face.GetGeometry().Centroid(), (Vector3)direction);
        //     var intersectingSegments = (from seg in segsWithGeos where ray.Intersects(seg.geo, out var intersection) select seg.segment).ToList();
        //     var facesIntersectingDs = intersectingSegments.Select(s => s.GetFaces()).SelectMany(x => x).Distinct().ToList();
        //     faces = (from f in facesIntersectingDs where f.Id != face.Id select f).ToList();
        //     if (matchDirectionToUOrV && ValueExists(this.uvsLookup, (Vector3)direction, out var uvId, Tolerance))
        //     {
        //         faces = (from f in faces where (f.VId == uvId || f.UId == uvId) select f).ToList();
        //     }
        //     return faces;
        // }

    }
}