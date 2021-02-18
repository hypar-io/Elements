using System;
using System.Collections.Generic;
using System.Linq;
using Elements;
using Elements.Geometry;

namespace Elements.Spatial
{
    #region subclasses
    public class Vertex
    {
        public long Id;

        public Vector3 Point;

        public Vertex(long id, Vector3 point)
        {
            this.Id = id;
            this.Point = point;
        }
    }

    public class Segment
    {
        public long Id;

        public long Vertex1;

        public long Vertex2;

        public Segment(long id, long index1, long index2)
        {
            this.Id = id;
            this.Vertex1 = index1;
            this.Vertex2 = index2;
        }
    }

    public class DirectedSegment
    {
        public long Id;

        public long SegmentId;

        public long Start;

        public long End;

        public DirectedSegment(long id, Segment segment, Boolean segmentOrderMatchesDirection)
        {
            this.Id = id;

            this.SegmentId = segment.Id;

            if (segmentOrderMatchesDirection)
            {
                this.Start = segment.Vertex1;
                this.End = segment.Vertex2;
            }
            else
            {
                this.Start = segment.Vertex2;
                this.End = segment.Vertex1;
            }
        }
    }

    public class Face
    {
        public long Id;

        public List<DirectedSegment> DirectedSegments;

        public Face(long id, List<DirectedSegment> directedSegments)
        {
            this.Id = id;
            this.DirectedSegments = directedSegments;
        }
    }

    public class Cell
    {
        public long Id;

        /// <summary>
        /// Bottom face. Can be null. Expected to be duplicated in list of faces.
        /// </summary>
        public Face BottomFace;

        /// <summary>
        /// Top face. Can be null. Expected to be duplicated in list of faces.
        /// </summary>
        public Face TopFace;

        /// <summary>
        /// All faces
        /// </summary>
        public List<Face> Faces;

        public Cell(long id, List<Face> faces, Face bottomFace, Face topFace)
        {
            this.Id = id;
            this.BottomFace = bottomFace;
            this.TopFace = topFace;
            this.Faces = faces;
        }
    }

    #endregion

    public class CellComplex : Elements.Element
    {
        private long _segmentId = 0;
        private long _directedSegmentId = 0;
        private long _vertexId = 0;
        private long _faceId = 0;
        private long _cellId = 0;

        public Dictionary<long, Vertex> Vertices = new Dictionary<long, Vertex>();
        public Dictionary<long, Segment> Segments = new Dictionary<long, Segment>();
        public Dictionary<long, DirectedSegment> DirectedSegments = new Dictionary<long, DirectedSegment>();
        public Dictionary<long, Face> Faces = new Dictionary<long, Face>();
        public Dictionary<long, Cell> Cells = new Dictionary<long, Cell>();

        private Dictionary<double, Dictionary<double, Dictionary<double, long>>> verticesLookup = new Dictionary<double, Dictionary<double, Dictionary<double, long>>>();

        // Indexed by tuple of (lesserVertexId, greaterVertexId)
        private Dictionary<(long, long), long> segmentsLookup = new Dictionary<(long, long), long>();
        // Same as segmentsLookup, with an addition level of dictionary for whether lesserVertexId is the start point or not
        private Dictionary<(long, long), Dictionary<Boolean, long>> directedSegmentsLookup = new Dictionary<(long, long), Dictionary<Boolean, long>>();

        private Dictionary<string, long> facesLookup = new Dictionary<string, long>();

        public CellComplex(Guid id, string name) : base(id, name)
        {

        }

        public CellComplex(Elements.CellComplex fromSerialization) : base(fromSerialization.Id, fromSerialization.Name)
        {
            foreach (var vertex in fromSerialization.Vertices)
            {
                this.AddVertex(vertex.Point, vertex.Id);
            }

            foreach (var segment in fromSerialization.Segments)
            {
                if (!this.AddSegment((segment.Vertex1, segment.Vertex2), segment.Id, out var addedSegment))
                {
                    throw new Exception("Duplicate segment ID found");
                }
            }

            foreach (var directedSegment in fromSerialization.DirectedSegments)
            {
                var segment = this.Segments.GetValueOrDefault(directedSegment.SegmentId);
                if (!this.AddDirectedSegment(segment, segment.Vertex2 == directedSegment.Start, directedSegment.Id, out var addedDirectedSegment))
                {
                    throw new Exception("Duplicate directed segment ID found");
                }
            }

            foreach (var face in fromSerialization.Faces)
            {
                var vertices = face.DirectedSegmentIds.Select(
                    segmentId => this.DirectedSegments.GetValueOrDefault(segmentId).Start
                ).Select(
                    vertexId => this.Vertices.GetValueOrDefault(vertexId).Point
                ).ToList();

                var polygon = new Polygon(vertices);

                if (!this.AddFace(polygon, face.Id, out var addedFace))
                {
                    throw new Exception("Duplicate face ID found");
                }
            }

            foreach (var cell in fromSerialization.Cells)
            {
                var faces = cell.FaceIds.Select(fId => this.Faces.GetValueOrDefault(fId)).ToList();
                var bottomFace = this.Faces.GetValueOrDefault(cell.BottomFaceId);
                var topFace = this.Faces.GetValueOrDefault(cell.TopFaceId);
                this.AddCell(cell.Id, faces, bottomFace, topFace);
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
                face = this.Faces.GetValueOrDefault(faceId);
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
            var segmentTuple = (segment.Vertex1, segment.Vertex2);

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
                directedSegment = this.DirectedSegments.GetValueOrDefault(directedSegmentId);

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
                segment = this.Segments.GetValueOrDefault(segmentId);
                return false;
            }
        }

        #region add vertex

        public Vertex AddVertex(Vector3 point)
        {
            this.AddVertex(point, this._vertexId, out var vertexId);
            return this.Vertices.GetValueOrDefault(vertexId);
        }

        private Vertex AddVertex(Vector3 point, long id)
        {
            this.AddVertex(point, id, out var vertexId);

            if (vertexId != id)
            {
                throw new Exception("This ID already exists");
            }

            return this.Vertices.GetValueOrDefault(vertexId);
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

        public Line GetSegmentGeometry(Segment segment)
        {
            return new Line(
                this.Vertices.GetValueOrDefault(segment.Vertex1).Point,
                this.Vertices.GetValueOrDefault(segment.Vertex2).Point
            );
        }

        public Line GetDirectedSegmentGeometry(DirectedSegment directedSegment)
        {
            return new Line(
                this.Vertices.GetValueOrDefault(directedSegment.Start).Point,
                this.Vertices.GetValueOrDefault(directedSegment.End).Point
            );
        }

        public Polygon GetFaceGeometry(Face face)
        {
            var vertices = face.DirectedSegments.Select(directedSegment => GetDirectedSegmentGeometry(directedSegment).Start).ToList();
            return new Polygon(vertices);
        }
    }
}