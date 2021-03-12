using System;
using System.Collections.Generic;
using System.Linq;
using Elements;
using Elements.Geometry;
using Newtonsoft.Json;

namespace Elements.Spatial.CellComplex
{
    /// <summary>
    /// A cell: a 3-dimensional closed cell within a complex
    /// </summary>
    public class Cell : CellChild
    {
        /// <summary>
        /// Bottom face. Can be null. Expected to be duplicated in list of faces.
        /// </summary>
        public long? BottomFaceId = null;

        /// <summary>
        /// Top face. Can be null. Expected to be duplicated in list of faces.
        /// </summary>
        public long? TopFaceId = null;

        /// <summary>
        /// All faces
        /// </summary>
        public List<long> FaceIds;

        /// <summary>
        /// Represents a unique Cell wtihin a CellComplex.
        /// Is not intended to be created or modified outside of the CellComplex class code.
        /// </summary>
        /// <param name="cellComplex">CellComplex that this belongs to</param>
        /// <param name="id"></param>
        /// <param name="faces">List of faces which make up this CellComplex</param>
        /// <param name="bottomFace">The bottom face for this cell (should also be included in the list of all faces)</param>
        /// <param name="topFace">The top face for this cell (should also be included in the list of all faces</param>
        internal Cell(CellComplex cellComplex, long id, List<Face> faces, Face bottomFace, Face topFace) : base(id, cellComplex)
        {
            if (bottomFace != null)
            {
                this.BottomFaceId = bottomFace.Id;
            }
            if (topFace != null)
            {
                this.TopFaceId = topFace.Id;
            }
            this.FaceIds = faces.Select(ds => ds.Id).ToList();
        }

        /// <summary>
        /// Used for deserialization only!
        /// </summary>
        [JsonConstructor]
        internal Cell(long id, List<long> faceIds, long? bottomFaceId, long? topFaceId) : base(id, null)
        {
            this.FaceIds = faceIds;
            this.BottomFaceId = bottomFaceId;
            this.TopFaceId = topFaceId;
        }

        /// <summary>
        /// Get associated Faces
        /// </summary>
        /// <returns></returns>
        public List<Face> GetFaces()
        {
            return this.FaceIds.Select(fId => this.CellComplex.GetFace(fId)).ToList();
        }

        /// <summary>
        /// Get associated Segments
        /// </summary>
        /// <returns></returns>
        public List<Segment> GetSegments()
        {
            return this.GetFaces().Select(face => face.GetSegments()).SelectMany(x => x).Distinct().ToList();
        }
    }
}