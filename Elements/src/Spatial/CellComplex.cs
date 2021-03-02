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
        public Vector3 Value;

        /// <summary>
        /// Some optional name, if we want it
        /// </summary>
        public string Name { get; set; }

        public Vertex(long id, Vector3 point, string name = null)
        {
            this.Id = id;
            this.Value = point;
            this.Name = name;
        }
    }

    public class UV : Vertex
    {
        public UV(long id, Vector3 point, string name = null) : base(id, point, name) { }
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
        /// ID of U direction
        /// </summary>
        public Nullable<long> UId;

        /// <summary>
        /// ID of V direction
        /// </summary>
        public Nullable<long> VId;

        /// <summary>
        /// Directed segment IDs
        /// </summary>
        public List<long> DirectedSegmentIds;

        public Face(long id, List<DirectedSegment> directedSegments, UV u = null, UV v = null)
        {
            this.Id = id;
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
        public Face(long id, List<long> directedSegmentIds, Nullable<long> uId = null, Nullable<long> vId = null)
        {
            this.Id = id;
            this.DirectedSegmentIds = directedSegmentIds;
            this.UId = uId;
            this.VId = vId;

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
        private long _segmentId = 1; // we start at 1 because 0 is returned as default value from dicts

        [JsonIgnore]
        private long _directedSegmentId = 1; // we start at 1 because 0 is returned as default value from dicts

        [JsonIgnore]
        private long _vertexId = 1; // we start at 1 because 0 is returned as default value from dicts

        [JsonIgnore]
        private long _faceId = 1; // we start at 1 because 0 is returned as default value from dicts

        [JsonIgnore]
        private long _cellId = 1; // we start at 1 because 0 is returned as default value from dicts

        [JsonIgnore]
        private long _uvId = 1; // we start at 1 because 0 is returned as default value from dicts

        public Dictionary<long, Vertex> Vertices = new Dictionary<long, Vertex>();

        public Dictionary<long, UV> UVs = new Dictionary<long, UV>();

        public Dictionary<long, Segment> Segments = new Dictionary<long, Segment>();

        public Dictionary<long, DirectedSegment> DirectedSegments = new Dictionary<long, DirectedSegment>();

        public Dictionary<long, Face> Faces = new Dictionary<long, Face>();

        public Dictionary<long, Cell> Cells = new Dictionary<long, Cell>();

        [JsonIgnore]
        private Dictionary<double, Dictionary<double, Dictionary<double, long>>> verticesLookup = new Dictionary<double, Dictionary<double, Dictionary<double, long>>>();

        [JsonIgnore]
        private Dictionary<double, Dictionary<double, Dictionary<double, long>>> uvsLookup = new Dictionary<double, Dictionary<double, Dictionary<double, long>>>();

        // Indexed by tuple of (lesserVertexId, greaterVertexId)
        [JsonIgnore]
        private Dictionary<(long, long), long> segmentsLookup = new Dictionary<(long, long), long>();

        // Same as segmentsLookup, with an addition level of dictionary for whether lesserVertexId is the start point or not
        [JsonIgnore]
        private Dictionary<(long, long), Dictionary<Boolean, long>> directedSegmentsLookup = new Dictionary<(long, long), Dictionary<Boolean, long>>();

        [JsonIgnore]
        private Dictionary<string, long> facesLookup = new Dictionary<string, long>();

        [JsonIgnore]
        private Dictionary<long, HashSet<long>> segmentIdsByVertexId = new Dictionary<long, HashSet<long>>();

        [JsonIgnore]
        private Dictionary<long, HashSet<long>> directedSegmentIdsBySegmentId = new Dictionary<long, HashSet<long>>();

        [JsonIgnore]
        private Dictionary<long, HashSet<long>> faceIdsByDirectedSegmentId = new Dictionary<long, HashSet<long>>();

        [JsonIgnore]
        private Dictionary<long, HashSet<long>> cellIdsByFaceId = new Dictionary<long, HashSet<long>>();

        public CellComplex(Guid id, string name) : base(id, name)
        {

        }

        [JsonConstructor]
        public CellComplex(Guid id, string name, Dictionary<long, Vertex> vertices, Dictionary<long, Vertex> uvs, Dictionary<long, Segment> segments, Dictionary<long, DirectedSegment> directedSegments, Dictionary<long, Face> faces, Dictionary<long, Cell> cells) : base(id, name)
        {
            foreach (var vertex in vertices.Values)
            {
                var added = this.AddVertex(vertex.Value, vertex.Id);
                added.Name = vertex.Name;
            }

            foreach (var uv in uvs.Values)
            {
                this.AddUV(uv.Value, uv.Id);
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
                var u = this.GetUV(face.UId);
                var v = this.GetUV(face.VId);

                if (!this.AddFace(polygon, face.Id, u, v, out var addedFace))
                {
                    throw new Exception("Duplicate face ID found");
                }
            }

            foreach (var cell in cells.Values)
            {
                var cellFaces = cell.FaceIds.Select(fId => this.GetFace(fId)).ToList();
                var bottomFace = this.GetFace(cell.BottomFaceId);
                var topFace = this.GetFace(cell.TopFaceId);
                if (!this.AddCell(cell.Id, cellFaces, bottomFace, topFace, out var addedCell))
                {
                    throw new Exception("Duplicate cell found");
                }
            }
        }

        #region add content
        public Cell AddCell(Polygon polygon, double height, double elevation, Grid1d uGrid = null, Grid1d vGrid = null)
        {
            var elevationVector = new Vector3(0, 0, elevation);
            var heightVector = new Vector3(0, 0, height);

            var transformedPolygonBottom = (Polygon)polygon.Transformed(new Transform(elevationVector));
            var transformedPolygonTop = (Polygon)polygon.Transformed(new Transform(elevationVector + heightVector));

            UV u = null;
            UV v = null;

            if (uGrid != null)
            {
                u = this.AddUV(uGrid.Direction().Unitized());
            }
            if (vGrid != null)
            {
                v = this.AddUV(vGrid.Direction().Unitized());
            }

            var bottomFace = this.AddFace(transformedPolygonBottom, u, v);
            var topFace = this.AddFace(transformedPolygonTop, u, v);

            var faces = new List<Face>() { bottomFace, topFace };

            var up = this.AddUV(new Vector3(0, 0, 1));

            foreach (var faceEdge in transformedPolygonBottom.Segments())
            {
                var vertices = new List<Vector3>() { faceEdge.Start, faceEdge.End };
                var horizontalU = this.AddUV((faceEdge.End - faceEdge.Start).Unitized());
                vertices.Add(faceEdge.End + heightVector);
                vertices.Add(faceEdge.Start + heightVector);
                var facePoly = new Polygon(vertices);
                faces.Add(this.AddFace(facePoly, horizontalU, up));
            }

            this.AddCell(this._cellId, faces, bottomFace, topFace, out var cell);
            return cell;
        }

        public Face AddFace(Polygon polygon, UV u = null, UV v = null)
        {
            this.AddFace(polygon, this._faceId, u, v, out var face);
            return face;
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

        private Boolean AddCell(long cellId, List<Face> faces, Face bottomFace, Face topFace, out Cell cell)
        {
            if (this.Cells.ContainsKey(cellId))
            {
                cell = null;
                return false;
            }

            cell = new Cell(cellId, faces, bottomFace, topFace);
            this.Cells.Add(cell.Id, cell);

            foreach (var face in faces)
            {
                AddValue(this.cellIdsByFaceId, face.Id, cell.Id);
            }

            this._cellId = Math.Max(cellId + 1, this._cellId + 1);
            return true;
        }

        private Boolean AddFace(Polygon polygon, long idIfNew, UV u, UV v, out Face face)
        {
            var lines = polygon.Segments();
            var directedSegments = new List<DirectedSegment>();
            foreach (var line in lines)
            {
                directedSegments.Add(this.AddDirectedSegment(line));
            }

            // Face lookup hash is segment ids in ascending order
            var sortedIds = directedSegments.Select(ds => ds.SegmentId).ToList();
            sortedIds.Sort();
            var hash = String.Join(",", sortedIds);

            if (!this.facesLookup.TryGetValue(hash, out var faceId))
            {
                face = new Face(idIfNew, directedSegments, u, v);
                faceId = face.Id;
                this.facesLookup.Add(hash, faceId);
                this.Faces.Add(faceId, face);

                foreach (var directedSegment in directedSegments)
                {
                    AddValue(this.faceIdsByDirectedSegmentId, directedSegment.Id, face.Id);
                }

                this._faceId = Math.Max(idIfNew + 1, this._faceId + 1);
                return true;
            }
            else
            {
                this.Faces.TryGetValue(faceId, out face);
                return false;
            }
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

                AddValue(this.directedSegmentIdsBySegmentId, segment.Id, directedSegment.Id);

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
                this.Segments.Add(segmentId, segment);

                AddValue(this.segmentIdsByVertexId, segment.Vertex1Id, segment.Id);
                AddValue(this.segmentIdsByVertexId, segment.Vertex2Id, segment.Id);

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

        private Vertex AddVertex(Vector3 point, long id, string name = null)
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
            if (ValueExists(this.verticesLookup, point, out id))
            {
                return false;
            }

            var zDict = GetAddressParent(this.verticesLookup, point, true);

            var vertex = new Vertex(idIfNew, point);
            id = vertex.Id;

            zDict.Add(point.Z, id);
            this.Vertices.Add(id, vertex);

            this._vertexId = Math.Max(id + 1, this._vertexId + 1);
            return true;
        }

        #endregion

        #region add uv

        // TODO: this is exactly the same as addvertex except referencing different members/dicts, how can I consolidate this?

        public UV AddUV(Vector3 point)
        {
            this.AddUV(point, this._uvId, out var uvId);
            this.UVs.TryGetValue(uvId, out var uv);
            return uv;
        }

        private UV AddUV(Vector3 point, long id)
        {
            this.AddUV(point, id, out var uvId);

            if (uvId != id)
            {
                throw new Exception("This ID already exists");
            }

            this.UVs.TryGetValue(uvId, out var uv);
            return uv;
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="point"></param>
        /// <param name="idIfNew"></param>
        /// <param name="id"></param>
        /// <returns>True if vertex was added, false if it already added.</returns>
        private Boolean AddUV(Vector3 point, long idIfNew, out long id)
        {
            if (ValueExists(this.uvsLookup, point, out id))
            {
                return false;
            }

            var zDict = GetAddressParent(this.uvsLookup, point, true);

            var uv = new UV(idIfNew, point);
            id = uv.Id;

            zDict.Add(point.Z, id);
            this.UVs.Add(id, uv);

            this._uvId = Math.Max(id + 1, this._uvId + 1);
            return true;
        }

        #endregion add uv
        #endregion add content

        public Vertex GetVertex(long vertexId)
        {
            this.Vertices.TryGetValue(vertexId, out var vertex);
            return vertex;
        }

        public UV GetUV(long? uvId)
        {
            if (uvId == null)
            {
                return null;
            }
            this.UVs.TryGetValue((long)uvId, out var uv);
            return uv;
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
                this.GetVertex(segment.Vertex1Id).Value,
                this.GetVertex(segment.Vertex2Id).Value
            );
        }

        public Line GetDirectedSegmentGeometry(DirectedSegment directedSegment)
        {
            return new Line(
                this.GetVertex(directedSegment.StartVertexId).Value,
                this.GetVertex(directedSegment.EndVertexId).Value
            );
        }

        public Polygon GetFaceGeometry(Face face)
        {
            var vertices = face.DirectedSegmentIds.Select(dsId => this.GetDirectedSegmentGeometry(this.GetDirectedSegment(dsId)).Start).ToList();
            return new Polygon(vertices);
        }

        public List<Vertex> GetVerticesMatchingXY(double x, double y)
        {
            var vertices = new List<Vertex>();

            if (!this.verticesLookup.TryGetValue(x, out var yzDict))
            {
                return vertices;
            }

            if (!yzDict.TryGetValue(y, out var zDict))
            {
                return vertices;
            }

            return zDict.Values.Select(vertexId => this.GetVertex(vertexId)).ToList();
        }

        public Boolean VertexExists(Vector3 point, out long id, Nullable<double> fuzzyFactor = null)
        {
            return ValueExists(this.verticesLookup, point, out id, fuzzyFactor);
        }

        public List<Segment> GetSegments(Vertex vertex)
        {
            if (this.segmentIdsByVertexId.TryGetValue(vertex.Id, out var segmentIdSet))
            {
                return segmentIdSet.Select(segmentId => this.GetSegment(segmentId)).ToList();
            }
            return new List<Segment>();
        }

        public List<Segment> GetSegments(Face face)
        {
            return face.DirectedSegmentIds.Select(dsId => this.GetSegment(this.GetDirectedSegment(dsId).SegmentId)).ToList();
        }

        public List<Segment> GetSegments(Cell cell)
        {
            return this.GetFaces(cell).Select(face => this.GetSegments(face)).SelectMany(x => x).Distinct().ToList();
        }

        public List<DirectedSegment> GetDirectedSegments(Segment segment)
        {
            if (this.directedSegmentIdsBySegmentId.TryGetValue(segment.Id, out var directedSegmentIdSet))
            {
                return directedSegmentIdSet.Select(dsId => this.GetDirectedSegment(dsId)).ToList();
            }
            return new List<DirectedSegment>();
        }

        public List<DirectedSegment> GetDirectedSegments(Vertex vertex)
        {
            return this.GetSegments(vertex).Select(segment => this.GetDirectedSegments(segment)).SelectMany(x => x).Distinct().ToList();
        }

        public List<DirectedSegment> GetDirectedSegments(Face face)
        {
            return face.DirectedSegmentIds.Select(dsId => this.GetDirectedSegment(dsId)).ToList();
        }

        public List<Face> GetFaces(DirectedSegment directedSegment)
        {
            if (this.faceIdsByDirectedSegmentId.TryGetValue(directedSegment.Id, out var faceIdSet))
            {
                return faceIdSet.Select(faceId => this.GetFace(faceId)).ToList();
            }
            return new List<Face>();
        }

        public List<Face> GetFaces(Segment segment)
        {
            return this.GetDirectedSegments(segment).Select(directedSegment => this.GetFaces(directedSegment)).SelectMany(x => x).Distinct().ToList();
        }

        public List<Face> GetFaces(Vertex vertex)
        {
            return this.GetDirectedSegments(vertex).Select(directedSegment => this.GetFaces(directedSegment)).SelectMany(x => x).Distinct().ToList();
        }

        public List<Face> GetFaces(Cell cell)
        {
            return cell.FaceIds.Select(fid => this.GetFace(fid)).ToList();
        }

        public List<Cell> GetCells(Face face)
        {
            if (this.cellIdsByFaceId.TryGetValue(face.Id, out var cellIdSet))
            {
                return cellIdSet.Select(cellId => this.GetCell(cellId)).ToList();
            }
            return new List<Cell>();
        }

        public List<Cell> GetCells(DirectedSegment directedSegment)
        {
            return this.GetFaces(directedSegment).Select(face => this.GetCells(face)).SelectMany(x => x).Distinct().ToList();
        }

        public List<Cell> GetCells(Segment segment)
        {
            return this.GetFaces(segment).Select(face => this.GetCells(face)).SelectMany(x => x).Distinct().ToList();
        }

        public List<Cell> GetCells(Vertex vertex)
        {
            return this.GetFaces(vertex).Select(face => this.GetCells(face)).SelectMany(x => x).Distinct().ToList();
        }

        private static HashSet<long> AddValue(Dictionary<long, HashSet<long>> dict, long key, long value)
        {
            if (!dict.ContainsKey(key))
            {
                dict.Add(key, new HashSet<long>());
            }

            dict.TryGetValue(key, out var set);
            set.Add(value);
            return set;
        }

        private static Dictionary<double, long> GetAddressParent(Dictionary<double, Dictionary<double, Dictionary<double, long>>> dict, Vector3 point, Boolean addAddressIfNonExistent = false, Nullable<double> fuzzyFactor = null)
        {
            if (!TryGetValue<Dictionary<double, Dictionary<double, long>>>(dict, point.X, out var yzDict, fuzzyFactor))
            {
                yzDict = new Dictionary<double, Dictionary<double, long>>();
                if (addAddressIfNonExistent)
                {
                    dict.Add(point.X, yzDict);
                }
                else
                {
                    return null;
                }
            }

            if (!TryGetValue<Dictionary<double, long>>(yzDict, point.Y, out var zDict, fuzzyFactor))
            {
                zDict = new Dictionary<double, long>();
                if (addAddressIfNonExistent)
                {
                    yzDict.Add(point.Y, zDict);
                }
                else
                {
                    return null;
                }
            }

            return zDict;

        }

        private static Boolean ValueExists(Dictionary<double, Dictionary<double, Dictionary<double, long>>> dict, Vector3 point, out long id, Nullable<double> fuzzyFactor = null)
        {
            var zDict = GetAddressParent(dict, point, fuzzyFactor: fuzzyFactor);
            if (zDict == null)
            {
                id = 0;
                return false;
            }
            return TryGetValue<long>(zDict, point.Z, out id, fuzzyFactor);
        }

        private static Boolean TryGetValue<T>(Dictionary<double, T> dict, double key, out T value, Nullable<double> fuzzyFactor = null)
        {
            if (dict.TryGetValue(key, out value))
            {
                return true;
            }
            if (fuzzyFactor != null)
            {
                foreach (var curKey in dict.Keys)
                {
                    if (Math.Abs(curKey - key) <= fuzzyFactor)
                    {
                        value = dict[curKey];
                        return true;

                    }
                }
            }
            return false;
        }
    }
}