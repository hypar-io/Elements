using System;
using System.Collections.Generic;
using System.Linq;
using Elements;
using Elements.Geometry;
using Newtonsoft.Json;

namespace Elements.Spatial
{
    #region subclasses

    /// <summary>
    /// A unique vertex in a cell complex
    /// </summary>
    public class Vertex
    {
        /// <summary>
        /// ID
        /// </summary>
        public long Id;

        /// <summary>
        /// Location in space
        /// </summary>
        public Vector3 Point;

        public Vertex(long id, Vector3 point)
        {
            this.Id = id;
            this.Point = point;
        }
    }

    /// <summary>
    /// A unique segment in a cell complex.
    /// </summary>
    public class Segment
    {
        /// <summary>
        /// ID
        /// </summary>
        public long Id;

        /// <summary>
        /// ID of first vertex
        /// </summary>
        public long Vertex1Id;

        /// <summary>
        /// ID of second vertex
        /// </summary>
        public long Vertex2Id;

        public Segment(long id, long index1, long index2)
        {
            this.Id = id;
            this.Vertex1Id = index1;
            this.Vertex2Id = index2;
        }
    }

    /// <summary>
    /// A directed segment: a representation of a segment that has direction to it so that it can be used to traverse faces
    /// </summary>
    public class DirectedSegment
    {
        /// <summary>
        /// ID
        /// </summary>
        public long Id;

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

        public DirectedSegment(long id, Segment segment, Boolean segmentOrderMatchesDirection)
        {
            this.Id = id;

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
        /// Used for serialization only!
        /// </summary>
        [JsonConstructor]
        public DirectedSegment(long id, long segmentId, long startVertexId, long endVertexId)
        {
            this.Id = id;
            this.SegmentId = segmentId;
            this.StartVertexId = startVertexId;
            this.EndVertexId = endVertexId;
        }
    }

    /// <summary>
    /// A face of a cell. Multiple cells can share the same face.
    /// </summary>
    public class Face
    {
        /// <summary>
        /// ID
        /// </summary>
        public long Id;

        /// <summary>
        /// Directed segment IDs
        /// </summary>
        public List<long> DirectedSegmentIds;

        public Face(long id, List<DirectedSegment> directedSegments) : this(id, directedSegments.Select(ds => ds.Id).ToList())
        {
        }

        /// <summary>
        /// Used for deserialization only!
        /// </summary>
        [JsonConstructor]
        public Face(long id, List<long> directedSegmentIds)
        {
            this.Id = id;
            this.DirectedSegmentIds = directedSegmentIds;

        }
    }

    /// <summary>
    /// A cell: a 3-dimensional closed cell within a complex
    /// </summary>
    public class Cell
    {
        /// <summary>
        /// ID
        /// </summary>
        public long Id;

        /// <summary>
        /// Bottom face. Can be null. Expected to be duplicated in list of faces.
        /// </summary>
        public Nullable<long> BottomFaceId = null;

        /// <summary>
        /// Top face. Can be null. Expected to be duplicated in list of faces.
        /// </summary>
        public Nullable<long> TopFaceId = null;

        /// <summary>
        /// All faces
        /// </summary>
        public List<long> FaceIds;

        public Cell(long id, List<Face> faces, Face bottomFace, Face topFace)
        {
            this.Id = id;
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
        public Cell(long id, List<long> faceIds, long? bottomFaceId, long? topFaceId)
        {
            this.Id = id;
            this.FaceIds = faceIds;
            this.BottomFaceId = bottomFaceId;
            this.TopFaceId = topFaceId;
        }
    }

    #endregion

    public class CellComplex : Elements.Element
    {
        [JsonIgnore]
        private long _segmentId = 0;

        [JsonIgnore]
        private long _directedSegmentId = 0;

        [JsonIgnore]
        private long _vertexId = 0;

        [JsonIgnore]
        private long _faceId = 0;

        [JsonIgnore]
        private long _cellId = 0;

        public Dictionary<long, Vertex> Vertices = new Dictionary<long, Vertex>();

        public Dictionary<long, Segment> Segments = new Dictionary<long, Segment>();

        public Dictionary<long, DirectedSegment> DirectedSegments = new Dictionary<long, DirectedSegment>();

        public Dictionary<long, Face> Faces = new Dictionary<long, Face>();

        public Dictionary<long, Cell> Cells = new Dictionary<long, Cell>();

        [JsonIgnore]
        private Dictionary<double, Dictionary<double, Dictionary<double, long>>> verticesLookup = new Dictionary<double, Dictionary<double, Dictionary<double, long>>>();

        // Indexed by tuple of (lesserVertexId, greaterVertexId)
        [JsonIgnore]
        private Dictionary<(long, long), long> segmentsLookup = new Dictionary<(long, long), long>();

        // Same as segmentsLookup, with an addition level of dictionary for whether lesserVertexId is the start point or not
        [JsonIgnore]
        private Dictionary<(long, long), Dictionary<Boolean, long>> directedSegmentsLookup = new Dictionary<(long, long), Dictionary<Boolean, long>>();

        [JsonIgnore]
        private Dictionary<string, long> facesLookup = new Dictionary<string, long>();

        public CellComplex(Guid id, string name) : base(id, name)
        {

        }

        [JsonConstructor]
        public CellComplex(Guid id, string name, Dictionary<long, Vertex> vertices, Dictionary<long, Segment> segments, Dictionary<long, DirectedSegment> directedSegments, Dictionary<long, Face> faces, Dictionary<long, Cell> cells): base(id, name)
        {
            foreach (var vertex in vertices.Values)
            {
                this.AddVertex(vertex.Point, vertex.Id);
            }

            foreach (var segment in segments.Values)
            {
                if (!this.AddSegment((segment.Vertex1Id, segment.Vertex2Id), segment.Id, out var addedSegment))
                {
                    throw new Exception("Duplicate segment ID found");
                }
            }

            foreach (var directedSegment in directedSegments.Values)
            {
                var segment = this.GetSegment(directedSegment.SegmentId);
                if (!this.AddDirectedSegment(segment, segment.Vertex1Id == directedSegment.StartVertexId, directedSegment.Id, out var addedDirectedSegment))
                {
                    throw new Exception("Duplicate directed segment ID found");
                }
            }

            foreach (var face in faces.Values)
            {
                var polygon = this.GetFaceGeometry(face);

                if (!this.AddFace(polygon, face.Id, out var addedFace))
                {
                    throw new Exception("Duplicate face ID found");
                }
            }

            foreach (var cell in cells.Values)
            {
                var cellFaces = cell.FaceIds.Select(fId => this.GetFace(fId)).ToList();
                var bottomFace = this.GetFace(cell.BottomFaceId);
                var topFace = this.GetFace(cell.TopFaceId);
                this.AddCell(cell.Id, cellFaces, bottomFace, topFace);
            }
        }

        public Cell AddCell(Polygon polygon, double height, double elevation)
        {
            var elevationVector = new Vector3(0, 0, elevation);
            var heightVector = new Vector3(0, 0, height);

            var transformedPolygonBottom = (Polygon)polygon.Transformed(new Transform(elevationVector));
            var transformedPolygonTop = (Polygon)polygon.Transformed(new Transform(elevationVector + heightVector));

            var bottomFace = this.AddFace(transformedPolygonBottom);
            var topFace = this.AddFace(transformedPolygonTop);

            var faces = new List<Face>() { bottomFace, topFace };

            foreach (var faceEdge in transformedPolygonBottom.Segments())
            {
                var vertices = new List<Vector3>() { faceEdge.Start, faceEdge.End };
                vertices.Add(faceEdge.End + heightVector);
                vertices.Add(faceEdge.Start + heightVector);
                var face = new Polygon(vertices);
                faces.Add(this.AddFace(face));
            }

            return this.AddCell(this._cellId, faces, bottomFace, topFace);
        }

        private Cell AddCell(long cellId, List<Face> faces, Face bottomFace, Face topFace)
        {
            var cell = new Cell(cellId, faces, bottomFace, topFace);
            this.Cells.Add(cell.Id, cell);
            this._cellId = Math.Max(cellId + 1, this._cellId + 1);
            return cell;
        }

        public Face AddFace(Polygon polygon)
        {
            this.AddFace(polygon, this._faceId, out var face);
            return face;

        }

        private Boolean AddFace(Polygon polygon, long idIfNew, out Face face)
        {
            // TODO: we probably also want a directed face that includes the normal?

            var lines = polygon.Segments();
            var directedSegments = new List<DirectedSegment>();
            foreach (var line in lines)
            {
                directedSegments.Add(this.AddDirectedSegment(line));
            }

            // Face lookup hash is segment ids in ascending order
            var sortedIds = directedSegments.Select(segment => segment.Id).ToList();
            sortedIds.Sort();
            var hash = String.Join(",", sortedIds);

            if (!this.facesLookup.TryGetValue(hash, out var faceId))
            {
                face = new Face(idIfNew, directedSegments);
                faceId = face.Id;
                this.facesLookup.Add(hash, faceId);
                this.Faces.Add(faceId, face);
                this._faceId = Math.Max(idIfNew + 1, this._faceId + 1);
                return true;
            }
            else
            {
                this.Faces.TryGetValue(faceId, out face);
                return false;
            }
        }

        public DirectedSegment AddDirectedSegment(Line line)
        {
            var points = new List<Vector3>() { line.Start, line.End };
            var vertices = points.Select(vertex => this.AddVertex(vertex)).ToList();
            var segmentTuple = (vertices[0].Id, vertices[1].Id);
            var segmentTupleIsInOrder = true;

            // Always index using smallest id to largest id
            if (vertices[1].Id < vertices[0].Id)
            {
                segmentTuple = (vertices[1].Id, vertices[0].Id);
                segmentTupleIsInOrder = false;
            }

            this.AddSegment(segmentTuple, this._segmentId, out var segment);
            this.AddDirectedSegment(segment, segmentTupleIsInOrder, this._directedSegmentId, out var directedSegment);
            return directedSegment;
        }

        private Boolean AddDirectedSegment(Segment segment, Boolean segmentTupleIsInOrder, long idIfNew, out DirectedSegment directedSegment)
        {
            var segmentTuple = (segment.Vertex1Id, segment.Vertex2Id);

            if (!this.directedSegmentsLookup.TryGetValue(segmentTuple, out var directedSegmentDict))
            {
                directedSegmentDict = new Dictionary<bool, long>();
                this.directedSegmentsLookup.Add(segmentTuple, directedSegmentDict);
            }

            if (!directedSegmentDict.TryGetValue(segmentTupleIsInOrder, out var directedSegmentId))
            {
                directedSegment = new DirectedSegment(idIfNew, segment, segmentTupleIsInOrder);
                directedSegmentId = directedSegment.Id;

                directedSegmentDict.Add(segmentTupleIsInOrder, directedSegmentId);
                this.DirectedSegments.Add(directedSegmentId, directedSegment);

                this._directedSegmentId = Math.Max(directedSegmentId + 1, this._directedSegmentId + 1);

                return true;
            }
            else
            {
                this.DirectedSegments.TryGetValue(directedSegmentId, out directedSegment);

                return false;
            }
        }

        private Boolean AddSegment((long, long) segmentTuple, long idIfNew, out Segment segment)
        {

            if (!this.segmentsLookup.TryGetValue(segmentTuple, out var segmentId))
            {
                segment = new Segment(idIfNew, segmentTuple.Item1, segmentTuple.Item2);
                segmentId = segment.Id;

                this.segmentsLookup[segmentTuple] = segmentId;
                this.Segments[segmentId] = segment;
                this._segmentId = Math.Max(segmentId + 1, this._segmentId + 1);

                return true;
            }
            else
            {
                this.Segments.TryGetValue(segmentId, out segment);
                return false;
            }
        }

        #region add vertex

        public Vertex AddVertex(Vector3 point)
        {
            this.AddVertex(point, this._vertexId, out var vertexId);
            this.Vertices.TryGetValue(vertexId, out var vertex);
            return vertex;
        }

        private Vertex AddVertex(Vector3 point, long id)
        {
            this.AddVertex(point, id, out var vertexId);

            if (vertexId != id)
            {
                throw new Exception("This ID already exists");
            }

            this.Vertices.TryGetValue(vertexId, out var vertex);
            return vertex;
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="point"></param>
        /// <param name="idIfNew"></param>
        /// <param name="id"></param>
        /// <returns>True if vertex was added, false if it already added.</returns>
        private Boolean AddVertex(Vector3 point, long idIfNew, out long id)
        {
            if (!this.verticesLookup.TryGetValue(point.X, out var yzDict))
            {
                yzDict = new Dictionary<double, Dictionary<double, long>>();
                this.verticesLookup.Add(point.X, yzDict);
            }

            if (!yzDict.TryGetValue(point.Y, out var zDict))
            {
                zDict = new Dictionary<double, long>();
                yzDict.Add(point.Y, zDict);
            }

            if (!zDict.TryGetValue(point.Z, out id))
            {
                var vertex = new Vertex(idIfNew, point);
                id = vertex.Id;

                zDict.Add(point.Z, id);
                this.Vertices.Add(id, vertex);

                this._vertexId = Math.Max(id + 1, this._vertexId + 1);
                return true;
            }
            else
            {
                return false;
            }
        }

        #endregion

        public Vertex GetVertex(long vertexId)
        {
            this.Vertices.TryGetValue(vertexId, out var vertex);
            return vertex;
        }

        public Segment GetSegment(long segmentId)
        {
            this.Segments.TryGetValue(segmentId, out var segment);
            return segment;
        }

        public DirectedSegment GetDirectedSegment(long directedSegmentId)
        {
            this.DirectedSegments.TryGetValue(directedSegmentId, out var directedSegment);
            return directedSegment;
        }

        public Face GetFace(long? faceId)
        {
            if (faceId == null)
            {
                return null;
            }

            this.Faces.TryGetValue((long)faceId, out var face);
            return face;
        }

        public Cell GetCell(long cellId)
        {
            this.Cells.TryGetValue(cellId, out var cell);
            return cell;
        }

        public Line GetSegmentGeometry(Segment segment)
        {
            return new Line(
                this.GetVertex(segment.Vertex1Id).Point,
                this.GetVertex(segment.Vertex2Id).Point
            );
        }

        public Line GetDirectedSegmentGeometry(long directedSegmentId)
        {
            var directedSegment = this.GetDirectedSegment(directedSegmentId);
            return new Line(
                this.GetVertex(directedSegment.StartVertexId).Point,
                this.GetVertex(directedSegment.EndVertexId).Point
            );
        }

        public Polygon GetFaceGeometry(Face face)
        {
            var vertices = face.DirectedSegmentIds.Select(dsId => GetDirectedSegmentGeometry(dsId).Start).ToList();
            return new Polygon(vertices);
        }
    }
}