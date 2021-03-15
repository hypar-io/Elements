using System;
using System.Collections.Generic;
using System.Linq;
using Elements.Geometry.Solids;
using Elements.Geometry;
using Newtonsoft.Json;

namespace Elements.Spatial.CellComplex
{
    /// <summary>
    /// A cell: a 3-dimensional closed cell within a complex
    /// </summary>
    public class Cell : CellChild<Extrude>
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

        [JsonIgnore]
        private Extrude _geometry;

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
            if (bottomFace != null && topFace != null)
            {
                var bottom = this.GetBottomFace().GetGeometry();
                var top = this.GetTopFace().GetGeometry();
                this._geometry = new Extrude(bottom, top.Centroid().Z - bottom.Centroid().Z, new Vector3(0, 0, 1), false);
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
        /// Get a Solid representing this Geometry
        /// </summary>
        /// <returns></returns>
        public override Extrude GetGeometry()
        {
            return this._geometry;
        }

        /// <summary>
        /// Get top face, if defined
        /// </summary>
        /// <returns></returns>
        public Face GetTopFace()
        {
            return this.TopFaceId == null ? null : this.CellComplex.GetFace(this.TopFaceId);
        }

        /// <summary>
        /// Get bottom face, if defined
        /// </summary>
        /// <returns></returns>
        public Face GetBottomFace()
        {
            return this.BottomFaceId == null ? null : this.CellComplex.GetFace(this.BottomFaceId);
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
        /// Get the closest associated face to the supplied position
        /// </summary>
        /// <param name="position"></param>
        /// <returns></returns>
        public Face GetClosestFace(Vector3 position)
        {
            return this.GetFaces().OrderBy(f => position.DistanceTo(f.GetGeometry())).ToList().First();
        }

        /// <summary>
        /// Get associated Segments
        /// </summary>
        /// <returns></returns>
        public List<Segment> GetSegments()
        {
            return this.GetFaces().Select(face => face.GetSegments()).SelectMany(x => x).Distinct().ToList();
        }

        /// <summary>
        /// Get the closest associated segment to the supplied position
        /// </summary>
        /// <param name="position"></param>
        /// <returns></returns>
        public Segment GetClosestSegment(Vector3 position)
        {
            return this.GetSegments().OrderBy(s => position.DistanceTo(s.GetGeometry())).ToList().First();
        }


        /// <summary>
        /// Get associated Vertices
        /// </summary>
        /// <returns></returns>
        public List<Vertex> GetVertices(Vector3? point = null)
        {
            return this.GetFaces().Select(f => f.GetVertices()).SelectMany(x => x).Distinct().ToList();
        }

        /// <summary>
        /// Get the closest associated vertex to the supplied position
        /// </summary>
        /// <param name="position"></param>
        /// <returns></returns>
        public Vertex GetClosestVertex(Vector3 position)
        {
            return this.GetVertices().OrderBy(v => position.DistanceTo(v.Value)).ToList().First();
        }

        /// <summary>
        /// Get list of Cells that are neighbors
        /// </summary>
        /// <returns></returns>
        public List<Cell> GetNeighbors()
        {
            return this.GetFaces().Select(f => f.GetCells()).SelectMany(x => x).Distinct().Where(c => c.Id != this.Id).ToList();
        }

        /// <summary>
        /// Get a list of the neighbor that shares a specific Face with this cell.
        /// Can be null if the face is not shared.
        /// </summary>
        /// <param name="face">Shared face</param>
        /// <returns></returns>
        public Cell GetNeighbor(Face face)
        {
            if (face == null)
            {
                return null;
            }

            var cells = face.GetCells().Where(cell => cell.Id != this.Id).ToList();

            if (cells.Count == 0)
            {
                return null;
            }
            if (cells.Count > 1)
            {
                throw new Exception("Cells should only have one neighbor which share this face");
            }

            return cells[0];
        }

        /// <summary>
        /// Get the closest associated cell to the supplied position, calculating to the approximate center of the cell
        /// </summary>
        /// <param name="position"></param>
        /// <returns></returns>
        public Cell GetClosestCell(Vector3 position)
        {
            return this.GetNeighbors().OrderBy(cell => cell.DistanceTo(position)).ToList().First();
        }

        /// <summary>
        /// Shortest distance to a point
        /// </summary>
        /// <param name="point"></param>
        /// <returns></returns>
        public override double DistanceTo(Vector3 point)
        {
            var extrude = this.GetGeometry() as Extrude;
            var bottom = extrude.Profile.Perimeter;
            var bottomZ = bottom.Centroid().Z;
            var topZ = (bottom.Centroid() + extrude.Direction * extrude.Height).Z;
            var isInside = point.Z >= bottomZ && point.Z <= topZ && bottom.Contains(new Vector3(point.X, point.Y, bottomZ));
            if (isInside)
            {
                return 0;
            }
            var minDistance = double.PositiveInfinity;
            foreach (var face in extrude.Solid.Faces.Values)
            {
                minDistance = Math.Min(minDistance, point.DistanceTo(face.Outer.ToPolygon()));
            }
            return minDistance;
        }
    }
}