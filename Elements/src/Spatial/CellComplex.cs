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

        /// <summary>
        /// Represents a unique Vertex within a CellComplex.
        /// Is not intended to be created or modified outside of the CellComplex class code.
        /// </summary>
        /// <param name="id"></param>
        /// <param name="point">Location of the vertex</param>
        /// <param name="name">Optional name</param>
        public Vertex(long id, Vector3 point, string name = null)
        {
            this.Id = id;
            this.Value = point;
            this.Name = name;
        }
    }

    /// <summary>
    /// A unique U or V direction in a cell complex
    /// </summary>
    public class UV : Vertex
    {
        /// <summary>
        /// Represents a unique U or V direction within a CellComplex.
        /// Is not intended to be created or modified outside of the CellComplex class code.
        /// </summary>
        /// <param name="id"></param>
        /// <param name="point">The U or V direction</param>
        /// <param name="name">Optional name</param>
        /// <returns></returns>
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

        /// <summary>
        /// Represents a unique Segment within a CellComplex.
        /// Is not intended to be created or modified outside of the CellComplex class code.
        /// </summary>
        /// <param name="id"></param>
        /// <param name="index1">The lower-index-id number for this segment</param>
        /// <param name="index2">The higher-index-id number for this segment</param>
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

        /// <summary>
        /// Represents a unique DirectedSegment within a CellComplex.
        /// This is added in addition to Segment because the same line may be required to move in a different direction
        /// as we traverse the edges of a face in their correctly-wound order.
        /// Is not intended to be created or modified outside of the CellComplex class code.
        /// </summary>
        /// <param name="id"></param>
        /// <param name="segment">The undirected Segment that matches this DirectedSegment</param>
        /// <param name="segmentOrderMatchesDirection">If true, start point is same as segment.vertex1Id. Otherwise, is flipped.</param>
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

        /// <summary>
        /// Represents a unique Face within a CellComplex.
        /// Is not intended to be created or modified outside of the CellComplex class code.
        /// </summary>
        /// <param name="id"></param>
        /// <param name="directedSegments">List of the DirectedSegments that make up this Face</param>
        /// <param name="u">Optional but highly recommended intended U direction for the Face</param>
        /// <param name="v">Optional but highly recommended intended V direction for the Face</param>
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

        /// <summary>
        /// Represents a unique Cell wtihin a CellComplex.
        /// Is not intended to be created or modified outside of the CellComplex class code.
        /// </summary>
        /// <param name="id"></param>
        /// <param name="faces">List of faces which make up this CellComplex</param>
        /// <param name="bottomFace">The bottom face for this cell (should also be included in the list of all faces)</param>
        /// <param name="topFace">The top face for this cell (should also be included in the list of all faces</param>
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

    /// <summary>
    /// A geometric voxel representation for structures and potential other
    /// </summary>
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

        /// <summary>
        /// Vertices by ID
        /// </summary>
        public Dictionary<long, Vertex> Vertices { get; private set; } = new Dictionary<long, Vertex>();

        /// <summary>
        /// U or V directions by ID
        /// </summary>
        public Dictionary<long, UV> UVs { get; private set; } = new Dictionary<long, UV>();

        /// <summary>
        /// Segments by ID
        /// </summary>
        public Dictionary<long, Segment> Segments { get; private set; } = new Dictionary<long, Segment>();

        /// <summary>
        /// DirectedSegments by ID
        /// </summary>
        public Dictionary<long, DirectedSegment> DirectedSegments { get; private set; } = new Dictionary<long, DirectedSegment>();

        /// <summary>
        /// Faces by ID
        /// </summary>
        public Dictionary<long, Face> Faces { get; private set; } = new Dictionary<long, Face>();

        /// <summary>
        /// Cells by ID
        /// </summary>
        public Dictionary<long, Cell> Cells { get; private set; } = new Dictionary<long, Cell>();

        /// <summary>
        /// Tolerance for points being considered the same.
        /// Applies individually to X, Y, and Z coordinates, not the cumulative difference!
        /// </summary>
        public double Tolerance = Vector3.EPSILON;

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

        // See GetFaceHash for how faces are identified as unique.
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

        /// <summary>
        /// Create a CellComplex
        /// </summary>
        /// <param name="id"></param>
        /// <param name="name"></param>
        /// <returns></returns>
        public CellComplex(Guid id, string name) : base(id, name)
        {

        }

        /// <summary>
        /// This constructor is intended for serialization and deserialization only.
        /// </summary>
        /// <param name="id"></param>
        /// <param name="name"></param>
        /// <param name="vertices"></param>
        /// <param name="uvs"></param>
        /// <param name="segments"></param>
        /// <param name="directedSegments"></param>
        /// <param name="faces"></param>
        /// <param name="cells"></param>
        /// <returns></returns>
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

        /// <summary>
        /// Add a cell to the CellComplex.
        /// </summary>
        /// <param name="polygon">The polygon that forms the base of this cell</param>
        /// <param name="height">The height of the cell</param>
        /// <param name="elevation">The elevation of the bottom of this cell</param>
        /// <param name="uGrid">An optional but highly recommended U grid that allows the cell's top and bottom faces to store intended directionality</param>
        /// <param name="vGrid">An optional but highly recommended V grid that allows the cell's top and bottom faces to store intended directionality</param>
        /// <returns>Created Cell</returns>
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

        /// <summary>
        /// Add a Face to the CellComplex
        /// </summary>
        /// <param name="polygon"></param>
        /// <param name="u"></param>
        /// <param name="v"></param>
        /// <returns>Created Face</returns>
        protected Face AddFace(Polygon polygon, UV u = null, UV v = null)
        {
            this.AddFace(polygon, this._faceId, u, v, out var face);
            return face;
        }

        /// <summary>
        /// Add a DirectedSegment to the CellComplex
        /// </summary>
        /// <param name="line">Line with Start and End in the expected direction</param>
        /// <returns>Created DirectedSegment</returns>
        protected DirectedSegment AddDirectedSegment(Line line)
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

        /// <summary>
        /// Internal method to add a Cell
        /// </summary>
        /// <param name="cellId"></param>
        /// <param name="faces"></param>
        /// <param name="bottomFace"></param>
        /// <param name="topFace"></param>
        /// <param name="cell"></param>
        /// <returns>Whether the cell was successfully added. Will be false if cellId already exists</returns>
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

        /// <summary>
        /// Internal method to add a Face
        /// </summary>
        /// <param name="polygon"></param>
        /// <param name="idIfNew"></param>
        /// <param name="u"></param>
        /// <param name="v"></param>
        /// <param name="face"></param>
        /// <returns>Whether the face was successfully added. Will be false if idIfNew already exists</returns>
        private Boolean AddFace(Polygon polygon, long idIfNew, UV u, UV v, out Face face)
        {
            var lines = polygon.Segments();
            var directedSegments = new List<DirectedSegment>();
            foreach (var line in lines)
            {
                directedSegments.Add(this.AddDirectedSegment(line));
            }

            var hash = GetFaceHash(directedSegments);

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

        /// <summary>
        /// Internal method to add a DirectedSegment
        /// </summary>
        /// <param name="segment"></param>
        /// <param name="segmentTupleIsInOrder"></param>
        /// <param name="idIfNew"></param>
        /// <param name="directedSegment"></param>
        /// <returns>Whether the directedSegment was successfully added. Will be false if idIfNew already exists</returns>
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

        /// <summary>
        /// Internal method to add a Segment
        /// </summary>
        /// <param name="segmentTuple"></param>
        /// <param name="idIfNew"></param>
        /// <param name="segment"></param>
        /// <returns>Whether the segment was successfully added. Will be false if idIfNew already exists</returns>
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

        /// <summary>
        /// Add or ensures the existence of a vertex
        /// </summary>
        /// <param name="point"></param>
        /// <returns></returns>
        protected Vertex AddVertex(Vector3 point)
        {
            this.AddVertex(point, this._vertexId, out var vertexId);
            this.Vertices.TryGetValue(vertexId, out var vertex);
            return vertex;
        }

        /// <summary>
        /// Attempts to add a vertex at an ID. Throws an exception if the ID already exists
        /// </summary>
        /// <param name="point"></param>
        /// <param name="id"></param>
        /// <returns>Added vertex</returns>
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
        /// Gets an existing vertex if it exists at this point, or adds it with the provided id if it is new
        /// </summary>
        /// <param name="point"></param>
        /// <param name="idIfNew"></param>
        /// <param name="id">ID of existing or added vertex</param>
        /// <returns>True if vertex was added, false if it already added.</returns>
        private Boolean AddVertex(Vector3 point, long idIfNew, out long id)
        {
            if (ValueExists(this.verticesLookup, point, out id, Tolerance))
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

        /// <summary>
        /// Add or ensures the existence of a u or v direction
        /// </summary>
        /// <param name="point"></param>
        /// <returns></returns>
        protected UV AddUV(Vector3 point)
        {
            this.AddUV(point, this._uvId, out var uvId);
            this.UVs.TryGetValue(uvId, out var uv);
            return uv;
        }

        /// <summary>
        /// Attempts to add a u or v direction at an ID. Throws an exception if the ID already exists
        /// </summary>
        /// <param name="point"></param>
        /// <param name="id"></param>
        /// <returns>Added vertex</returns>
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
        /// Gets an existing u or v value if it exists at this direction, or adds it with the provided id if it is new
        /// </summary>
        /// <param name="point"></param>
        /// <param name="idIfNew"></param>
        /// <param name="id">ID of existing or added U or V direction</param>
        /// <returns>True if vertex was added, false if it already added.</returns>
        private Boolean AddUV(Vector3 point, long idIfNew, out long id)
        {
            if (ValueExists(this.uvsLookup, point, out id, Tolerance))
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

        /// <summary>
        /// Get a Vertex by its ID
        /// </summary>
        /// <param name="vertexId"></param>
        /// <returns></returns>
        public Vertex GetVertex(long vertexId)
        {
            this.Vertices.TryGetValue(vertexId, out var vertex);
            return vertex;
        }

        /// <summary>
        /// Get a U or V direction by its ID
        /// </summary>
        /// <param name="uvId"></param>
        /// <returns></returns>
        public UV GetUV(long? uvId)
        {
            if (uvId == null)
            {
                return null;
            }
            this.UVs.TryGetValue((long)uvId, out var uv);
            return uv;
        }

        /// <summary>
        /// Get a Segment by its ID
        /// </summary>
        /// <param name="segmentId"></param>
        /// <returns></returns>
        public Segment GetSegment(long segmentId)
        {
            this.Segments.TryGetValue(segmentId, out var segment);
            return segment;
        }

        /// <summary>
        /// Get a DirectedSegment by its ID
        /// </summary>
        /// <param name="directedSegmentId"></param>
        /// <returns></returns>
        public DirectedSegment GetDirectedSegment(long directedSegmentId)
        {
            this.DirectedSegments.TryGetValue(directedSegmentId, out var directedSegment);
            return directedSegment;
        }

        /// <summary>
        /// Get a Face by its ID
        /// </summary>
        /// <param name="faceId"></param>
        /// <returns></returns>
        public Face GetFace(long? faceId)
        {
            if (faceId == null)
            {
                return null;
            }

            this.Faces.TryGetValue((long)faceId, out var face);
            return face;
        }

        /// <summary>
        /// Get a Cell by its ID
        /// </summary>
        /// <param name="cellId"></param>
        /// <returns></returns>
        public Cell GetCell(long cellId)
        {
            this.Cells.TryGetValue(cellId, out var cell);
            return cell;
        }

        /// <summary>
        /// Get the list of applicable Segments from a Vertex
        /// </summary>
        /// <param name="vertex"></param>
        /// <returns></returns>
        public List<Segment> GetSegments(Vertex vertex)
        {
            if (this.segmentIdsByVertexId.TryGetValue(vertex.Id, out var segmentIdSet))
            {
                return segmentIdSet.Select(segmentId => this.GetSegment(segmentId)).ToList();
            }
            return new List<Segment>();
        }

        /// <summary>
        /// Get the list of applicable Segments from a Face
        /// </summary>
        /// <param name="face"></param>
        /// <returns></returns>
        public List<Segment> GetSegments(Face face)
        {
            return face.DirectedSegmentIds.Select(dsId => this.GetSegment(this.GetDirectedSegment(dsId).SegmentId)).ToList();
        }

        /// <summary>
        /// Get the list of applicable Segments from a Cell
        /// </summary>
        /// <param name="cell"></param>
        /// <returns></returns>
        public List<Segment> GetSegments(Cell cell)
        {
            return this.GetFaces(cell).Select(face => this.GetSegments(face)).SelectMany(x => x).Distinct().ToList();
        }

        /// <summary>
        /// Get the list of applicable DirectedSegments from a Segment
        /// </summary>
        /// <param name="segment"></param>
        /// <returns></returns>
        public List<DirectedSegment> GetDirectedSegments(Segment segment)
        {
            if (this.directedSegmentIdsBySegmentId.TryGetValue(segment.Id, out var directedSegmentIdSet))
            {
                return directedSegmentIdSet.Select(dsId => this.GetDirectedSegment(dsId)).ToList();
            }
            return new List<DirectedSegment>();
        }

        /// <summary>
        /// Get the list of applicable DirectedSegments from a Vertex
        /// </summary>
        /// <param name="vertex"></param>
        /// <returns></returns>
        public List<DirectedSegment> GetDirectedSegments(Vertex vertex)
        {
            return this.GetSegments(vertex).Select(segment => this.GetDirectedSegments(segment)).SelectMany(x => x).Distinct().ToList();
        }

        /// <summary>
        /// Get the list of applicable DirectedSegments from a Face
        /// </summary>
        /// <param name="face"></param>
        /// <returns></returns>
        public List<DirectedSegment> GetDirectedSegments(Face face)
        {
            return face.DirectedSegmentIds.Select(dsId => this.GetDirectedSegment(dsId)).ToList();
        }

        /// <summary>
        /// Get the list of applicable Faces from a DirectedSegment
        /// </summary>
        /// <param name="directedSegment"></param>
        /// <returns></returns>
        public List<Face> GetFaces(DirectedSegment directedSegment)
        {
            if (this.faceIdsByDirectedSegmentId.TryGetValue(directedSegment.Id, out var faceIdSet))
            {
                return faceIdSet.Select(faceId => this.GetFace(faceId)).ToList();
            }
            return new List<Face>();
        }

        /// <summary>
        /// Get the list of applicable Faces from a Segment
        /// </summary>
        /// <param name="segment"></param>
        /// <returns></returns>
        public List<Face> GetFaces(Segment segment)
        {
            return this.GetDirectedSegments(segment).Select(directedSegment => this.GetFaces(directedSegment)).SelectMany(x => x).Distinct().ToList();
        }

        /// <summary>
        /// Get the list of applicable Faces from a Vertex
        /// </summary>
        /// <param name="vertex"></param>
        /// <returns></returns>
        public List<Face> GetFaces(Vertex vertex)
        {
            return this.GetDirectedSegments(vertex).Select(directedSegment => this.GetFaces(directedSegment)).SelectMany(x => x).Distinct().ToList();
        }

        /// <summary>
        /// Get the list of applicable Faces from a Cell
        /// </summary>
        /// <param name="cell"></param>
        /// <returns></returns>
        public List<Face> GetFaces(Cell cell)
        {
            return cell.FaceIds.Select(fid => this.GetFace(fid)).ToList();
        }

        /// <summary>
        /// Get the list of applicable Cells from a Face
        /// </summary>
        /// <param name="face"></param>
        /// <returns></returns>
        public List<Cell> GetCells(Face face)
        {
            if (this.cellIdsByFaceId.TryGetValue(face.Id, out var cellIdSet))
            {
                return cellIdSet.Select(cellId => this.GetCell(cellId)).ToList();
            }
            return new List<Cell>();
        }

        /// <summary>
        /// Get the list of applicable Cells from a DirectedSegment
        /// </summary>
        /// <param name="directedSegment"></param>
        /// <returns></returns>
        public List<Cell> GetCells(DirectedSegment directedSegment)
        {
            return this.GetFaces(directedSegment).Select(face => this.GetCells(face)).SelectMany(x => x).Distinct().ToList();
        }

        /// <summary>
        /// Get the list of applicable Cells from a Segment
        /// </summary>
        /// <param name="segment"></param>
        /// <returns></returns>
        public List<Cell> GetCells(Segment segment)
        {
            return this.GetFaces(segment).Select(face => this.GetCells(face)).SelectMany(x => x).Distinct().ToList();
        }

        /// <summary>
        /// Get the list of applicable Cells from a Vertex
        /// </summary>
        /// <param name="vertex"></param>
        /// <returns></returns>
        public List<Cell> GetCells(Vertex vertex)
        {
            return this.GetFaces(vertex).Select(face => this.GetCells(face)).SelectMany(x => x).Distinct().ToList();
        }

        /// <summary>
        /// Gets the neighboring Faces which share a Segment.
        /// Does not capture partially overlapping neighbor match.
        /// </summary>
        /// <param name="face">Face to get the neighbors for</param>
        /// <param name="direction">If set, casts a ray from the centroid of this Face and only returns neighbors for the Segment which intersects this ray.</param>
        /// <param name="matchDirectionToUOrV">If true, further filters results to only those where the resulting face's U or V direction matches the given direction.</param>
        /// <returns></returns>
        public List<Face> GetNeighbors(Face face, Nullable<Vector3> direction = null, Boolean matchDirectionToUOrV = false)
        {
            var segments = this.GetSegments(face);
            List<Face> faces;
            if (direction == null)
            {
                faces = segments.Select(s => this.GetFaces(s)).SelectMany(x => x).Distinct().ToList();
                return (from f in faces where f.Id != face.Id select f).ToList();
            }
            var segsWithGeos = segments.Select(segment => (segment: segment, geo: this.GetSegmentGeometry(segment))).ToList();
            var ray = new Ray(this.GetFaceGeometry(face).Centroid(), (Vector3)direction);
            var intersectingSegments = (from seg in segsWithGeos where ray.Intersects(seg.geo, out var intersection) select seg.segment).ToList();
            var facesIntersectingDs = intersectingSegments.Select(s => this.GetFaces(s)).SelectMany(x => x).Distinct().ToList();
            faces = (from f in facesIntersectingDs where f.Id != face.Id select f).ToList();
            if (matchDirectionToUOrV && ValueExists(this.uvsLookup, (Vector3)direction, out var uvId, Tolerance))
            {
                faces = (from f in faces where (f.VId == uvId || f.UId == uvId) select f).ToList();
            }
            return faces;
        }

        /// <summary>
        /// Gets the neighboring Cells which share a Face.
        /// Does not capture partially overlapping neighbor match.
        /// </summary>
        /// <param name="cell">Cell to get the neighbors for</param>
        /// <param name="direction">If set, casts a ray from the centroid of this Cell and only returns neighbors for the Face which intersects this ray.</param>
        /// <returns></returns>
        public List<Cell> GetNeighbors(Cell cell, Nullable<Vector3> direction = null)
        {
            var faces = this.GetFaces(cell);
            if (direction == null)
            {
                var cells = faces.Select(f => this.GetCells(f)).SelectMany(x => x).Distinct().ToList();
                return (from c in cells where c.Id != cell.Id select c).ToList();
            }
            var facesWithGeos = faces.Select(face => (face: face, geo: this.GetFaceGeometry(face))).ToList();
            var centroid = faces.Select(face => this.GetFaceGeometry(face).Centroid()).ToList().Average();
            var ray = new Ray(centroid, (Vector3)direction);
            var intersectingFaces = (from f in facesWithGeos where ray.Intersects(f.geo, out var intersection) select f.face).ToList();
            var cellsIntersecting = intersectingFaces.Select(f => this.GetCells(f)).SelectMany(x => x).Distinct().ToList();
            return (from f in cellsIntersecting where f.Id != cell.Id select f).ToList();
        }

        /// <summary>
        /// Get the geometry for a Segment
        /// </summary>
        /// <param name="segment"></param>
        /// <returns></returns>
        public Line GetSegmentGeometry(Segment segment)
        {
            return new Line(
                this.GetVertex(segment.Vertex1Id).Value,
                this.GetVertex(segment.Vertex2Id).Value
            );
        }

        /// <summary>
        /// Get the geometry for a DirectedSegment
        /// </summary>
        /// <param name="directedSegment"></param>
        /// <returns></returns>
        public Line GetDirectedSegmentGeometry(DirectedSegment directedSegment)
        {
            return new Line(
                this.GetVertex(directedSegment.StartVertexId).Value,
                this.GetVertex(directedSegment.EndVertexId).Value
            );
        }

        /// <summary>
        /// Get the geometry for a Face
        /// </summary>
        /// <param name="face"></param>
        /// <returns></returns>
        public Polygon GetFaceGeometry(Face face)
        {
            var vertices = face.DirectedSegmentIds.Select(dsId => this.GetDirectedSegmentGeometry(this.GetDirectedSegment(dsId)).Start).ToList();
            return new Polygon(vertices);
        }

        /// <summary>
        /// Get all vertices matching an X/Y coordinate, regardless of Z
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="fuzzyFactor">Amount of tolerance in the search against each component of the coordinate</param>
        /// <returns></returns>
        public List<Vertex> GetVerticesMatchingXY(double x, double y, Nullable<double> fuzzyFactor = null)
        {
            var vertices = new List<Vertex>();
            var zDict = GetAddressParent(this.verticesLookup, new Vector3(x, y), fuzzyFactor: fuzzyFactor);
            if (zDict == null)
            {
                return vertices;
            }
            return zDict.Values.Select(id => this.GetVertex(id)).ToList();
        }

        /// <summary>
        /// Whether a vertex location already exists in the CellComplex
        /// </summary>
        /// <param name="point"></param>
        /// <param name="id"></param>
        /// <param name="fuzzyFactor">Amount of tolerance in the search against each component of the coordinate</param>
        /// <returns></returns>
        public Boolean VertexExists(Vector3 point, out long id, Nullable<double> fuzzyFactor = null)
        {
            return ValueExists(this.verticesLookup, point, out id, fuzzyFactor);
        }

        #region private statics

        /// <summary>
        /// Add a value to a Dictionary of HashSets of longs. Used as a utility for internal lookups.
        /// </summary>
        /// <param name="dict"></param>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <returns></returns>
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

        /// <summary>
        /// In a dictionary of x, y, and z coordinates, gets last level dictionary of z values.
        /// </summary>
        /// <param name="dict"></param>
        /// <param name="point"></param>
        /// <param name="addAddressIfNonExistent">Whether to create the dictionary address if it didn't previously exist</param>
        /// <param name="fuzzyFactor">Amount of tolerance in the search against each component of the coordinate</param>
        /// <returns>Can be null if the dictionary address didn't exist previously, and we chose not to add it</returns>
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

        /// <summary>
        /// In a dictionary of x, y, and z coordinates, whether a point value is represented
        /// </summary>
        /// <param name="dict"></param>
        /// <param name="point"></param>
        /// <param name="id"></param>
        /// <param name="fuzzyFactor">Amount of tolerance in the search against each component of the coordinate</param>
        /// <returns></returns>
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

        /// <summary>
        /// A version of TryGetValue on a dictionary that optionally takes in a tolerance when running the comparison
        /// </summary>
        /// <param name="dict"></param>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <param name="fuzzyFactor"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns>Amount of tolerance in the search against each component of the coordinate</returns>
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

        /// <summary>
        /// Face lookup hash is segmentIds in ascending order.
        /// We do not directly use the `directedSegmentIds` because they could wind differently on a shared face.
        /// </summary>
        /// <param name="directedSegments"></param>
        /// <returns></returns>
        private static string GetFaceHash(List<DirectedSegment> directedSegments)
        {
            var sortedIds = directedSegments.Select(ds => ds.SegmentId).ToList();
            sortedIds.Sort();
            var hash = String.Join(",", sortedIds);
            return hash;
        }

        #endregion private statics
    }
}