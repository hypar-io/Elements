using System;
using System.Collections.Generic;
using System.Linq;
using Elements.Geometry;
using Newtonsoft.Json;

namespace Elements.Spatial.CellComplex
{
    /// <summary>
    /// A geometric voxel representation for structures and potential other
    /// </summary>
    public class CellComplex : Elements.Element
    {
        /// <summary>
        /// Tolerance for points being considered the same.
        /// Applies individually to X, Y, and Z coordinates, not the cumulative difference!
        /// </summary>
        [JsonProperty]
        public double Tolerance = Vector3.EPSILON;

        [JsonIgnore]
        private long _edgeId = 1; // we start at 1 because 0 is returned as default value from dicts

        [JsonIgnore]
        private long _halfEdgeId = 1; // we start at 1 because 0 is returned as default value from dicts

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
        [JsonProperty]
        private Dictionary<long, Vertex> _vertices = new Dictionary<long, Vertex>();

        /// <summary>
        /// U or V directions by ID
        /// </summary>
        [JsonProperty]
        private Dictionary<long, UV> _uvs = new Dictionary<long, UV>();

        /// <summary>
        /// Edges by ID
        /// </summary>
        [JsonProperty]
        private Dictionary<long, Edge> _edges = new Dictionary<long, Edge>();

        /// <summary>
        /// HalfEdges by ID
        /// </summary>
        [JsonProperty]
        private Dictionary<long, HalfEdge> _halfEdges = new Dictionary<long, HalfEdge>();

        /// <summary>
        /// Faces by ID
        /// </summary>
        [JsonProperty]
        private Dictionary<long, Face> _faces = new Dictionary<long, Face>();

        /// <summary>
        /// Cells by ID
        /// </summary>
        [JsonProperty]
        private Dictionary<long, Cell> _cells = new Dictionary<long, Cell>();

        [JsonIgnore]
        private Dictionary<double, Dictionary<double, Dictionary<double, long>>> verticesLookup = new Dictionary<double, Dictionary<double, Dictionary<double, long>>>();

        [JsonIgnore]
        private Dictionary<double, Dictionary<double, Dictionary<double, long>>> uvsLookup = new Dictionary<double, Dictionary<double, Dictionary<double, long>>>();

        // See Edge.GetHash for how faces are identified as unique.
        [JsonIgnore]
        private Dictionary<string, long> edgesLookup = new Dictionary<string, long>();

        // Same as edgesLookup, with an addition level of dictionary for whether lesserVertexId is the start point or not
        [JsonIgnore]
        private Dictionary<(long, long), Dictionary<Boolean, long>> halfEdgesLookup = new Dictionary<(long, long), Dictionary<Boolean, long>>();

        // See Face.GetHash for how faces are identified as unique.
        [JsonIgnore]
        private Dictionary<string, long> facesLookup = new Dictionary<string, long>();

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
        /// <param name="_vertices"></param>
        /// <param name="_uvs"></param>
        /// <param name="_edges"></param>
        /// <param name="_halfEdges"></param>
        /// <param name="_faces"></param>
        /// <param name="_cells"></param>
        /// <returns></returns>
        [JsonConstructor]
        internal CellComplex(
            Guid id,
            string name,
            Dictionary<long, Vertex> _vertices,
            Dictionary<long, Vertex> _uvs,
            Dictionary<long, Edge> _edges,
            Dictionary<long, HalfEdge> _halfEdges,
            Dictionary<long, Face> _faces,
            Dictionary<long, Cell> _cells
        ) : base(id, name)
        {
            foreach (var vertex in _vertices.Values)
            {
                var added = this.AddVertexOrUV<Vertex>(vertex.Value, vertex.Id);
                added.Name = vertex.Name;
            }

            foreach (var uv in _uvs.Values)
            {
                this.AddVertexOrUV<UV>(uv.Value, uv.Id);
            }

            foreach (var edge in _edges.Values)
            {
                if (!this.AddEdge(new List<long>() { edge.StartVertexId, edge.EndVertexId }, edge.Id, out var addedEdge))
                {
                    throw new Exception("Duplicate edge ID found");
                }
            }

            foreach (var halfEdge in _halfEdges.Values)
            {
                var edge = this.GetEdge(halfEdge.EdgeId);
                if (!this.AddHalfEdge(edge, edge.StartVertexId == halfEdge.StartVertexId, halfEdge.Id, out var addedHalfEdge))
                {
                    throw new Exception("Duplicate directed edge ID found");
                }
            }

            foreach (var face in _faces.Values)
            {
                face.CellComplex = this; // CellComplex not included on deserialization, add it back for processing even though we will discard this and create a new one
                var polygon = face.GetGeometry();
                var u = this.GetUV(face.UId);
                var v = this.GetUV(face.VId);

                if (!this.AddFace(polygon, face.Id, u, v, out var addedFace))
                {
                    throw new Exception("Duplicate face ID found");
                }
            }

            foreach (var cell in _cells.Values)
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
                u = this.AddVertexOrUV<UV>(uGrid.Direction().Unitized());
            }
            if (vGrid != null)
            {
                v = this.AddVertexOrUV<UV>(vGrid.Direction().Unitized());
            }

            var bottomFace = this.AddFace(transformedPolygonBottom, u, v);
            var topFace = this.AddFace(transformedPolygonTop, u, v);

            var faces = new List<Face>() { bottomFace, topFace };

            var up = this.AddVertexOrUV<UV>(new Vector3(0, 0, 1));

            foreach (var faceEdge in transformedPolygonBottom.Segments())
            {
                var vertices = new List<Vector3>() { faceEdge.Start, faceEdge.End };
                var horizontalU = this.AddVertexOrUV<UV>((faceEdge.End - faceEdge.Start).Unitized());
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
        internal Face AddFace(Polygon polygon, UV u = null, UV v = null)
        {
            this.AddFace(polygon, this._faceId, u, v, out var face);
            return face;
        }

        /// <summary>
        /// Add a HalfEdge to the CellComplex
        /// </summary>
        /// <param name="line">Line with Start and End in the expected direction</param>
        /// <returns>Created HalfEdge</returns>
        internal HalfEdge AddHalfEdge(Line line)
        {
            var points = new List<Vector3>() { line.Start, line.End };
            var vertices = points.Select(vertex => this.AddVertexOrUV<Vertex>(vertex)).ToList();
            this.AddEdge(vertices.Select(v => v.Id).ToList(), this._edgeId, out var edge);
            var dirMatchesEdge = vertices[0].Id == edge.StartVertexId;
            this.AddHalfEdge(edge, dirMatchesEdge, this._halfEdgeId, out var halfEdge);
            return halfEdge;
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
            if (this._cells.ContainsKey(cellId))
            {
                cell = null;
                return false;
            }

            cell = new Cell(this, cellId, faces, bottomFace, topFace);
            this._cells.Add(cell.Id, cell);

            foreach (var face in faces)
            {
                face.Cells.Add(cell);
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
            var halfEdges = new List<HalfEdge>();
            foreach (var line in lines)
            {
                halfEdges.Add(this.AddHalfEdge(line));
            }

            var hash = Face.GetHash(halfEdges);

            if (!this.facesLookup.TryGetValue(hash, out var faceId))
            {
                face = new Face(this, idIfNew, halfEdges, u, v);
                faceId = face.Id;
                this.facesLookup.Add(hash, faceId);
                this._faces.Add(faceId, face);

                foreach (var halfEdge in halfEdges)
                {
                    halfEdge.Faces.Add(face);
                }

                this._faceId = Math.Max(idIfNew + 1, this._faceId + 1);
                return true;
            }
            else
            {
                this._faces.TryGetValue(faceId, out face);
                return false;
            }
        }

        /// <summary>
        /// Internal method to add a HalfEdge
        /// </summary>
        /// <param name="edge"></param>
        /// <param name="edgeTupleIsInOrder"></param>
        /// <param name="idIfNew"></param>
        /// <param name="halfEdge"></param>
        /// <returns>Whether the halfEdge was successfully added. Will be false if idIfNew already exists</returns>
        private Boolean AddHalfEdge(Edge edge, Boolean edgeTupleIsInOrder, long idIfNew, out HalfEdge halfEdge)
        {
            var edgeTuple = (edge.StartVertexId, edge.EndVertexId);

            if (!this.halfEdgesLookup.TryGetValue(edgeTuple, out var halfEdgeDict))
            {
                halfEdgeDict = new Dictionary<bool, long>();
                this.halfEdgesLookup.Add(edgeTuple, halfEdgeDict);
            }

            if (!halfEdgeDict.TryGetValue(edgeTupleIsInOrder, out var halfEdgeId))
            {
                halfEdge = new HalfEdge(this, idIfNew, edge, edgeTupleIsInOrder);
                halfEdgeId = halfEdge.Id;

                halfEdgeDict.Add(edgeTupleIsInOrder, halfEdgeId);
                this._halfEdges.Add(halfEdgeId, halfEdge);

                edge.HalfEdges.Add(halfEdge);

                this._halfEdgeId = Math.Max(halfEdgeId + 1, this._halfEdgeId + 1);

                return true;
            }
            else
            {
                this._halfEdges.TryGetValue(halfEdgeId, out halfEdge);

                return false;
            }
        }

        /// <summary>
        /// Internal method to add a Edge
        /// </summary>
        /// <param name="edgeTuple"></param>
        /// <param name="idIfNew"></param>
        /// <param name="edge"></param>
        /// <returns>Whether the edge was successfully added. Will be false if idIfNew already exists</returns>
        private Boolean AddEdge(List<long> vertexIds, long idIfNew, out Edge edge)
        {
            var hash = Edge.GetHash(vertexIds);

            if (!this.edgesLookup.TryGetValue(hash, out var edgeId))
            {
                edge = new Edge(this, idIfNew, vertexIds[0], vertexIds[1]);
                edgeId = edge.Id;

                this.edgesLookup[hash] = edgeId;
                this._edges.Add(edgeId, edge);

                this.GetVertex(edge.StartVertexId).Edges.Add(edge);
                this.GetVertex(edge.EndVertexId).Edges.Add(edge);

                this._edgeId = Math.Max(edgeId + 1, this._edgeId + 1);

                return true;
            }
            else
            {
                this._edges.TryGetValue(edgeId, out edge);
                return false;
            }
        }

        private Dictionary<long, T> GetVertexOrUVDictionary<T>() where T : Vertex
        {
            if (typeof(T) != typeof(UV) && typeof(T) != typeof(Vertex))
            {
                throw new Exception("Unsupported type provided, expected Vertex or UV");
            }
            return typeof(T) == typeof(UV) ? this._uvs as Dictionary<long, T> : this._vertices as Dictionary<long, T>;
        }

        private Dictionary<double, Dictionary<double, Dictionary<double, long>>> GetVertexOrUVLookup<T>() where T : Vertex
        {
            if (typeof(T) != typeof(UV) && typeof(T) != typeof(Vertex))
            {
                throw new Exception("Unsupported type provided, expected Vertex or UV");
            }
            return typeof(T) == typeof(UV) ? this.uvsLookup : this.verticesLookup;
        }

        private T AddVertexOrUV<T>(Vector3 point) where T : Vertex
        {
            var newId = typeof(T) == typeof(UV) ? this._uvId : this._vertexId;
            var dict = GetVertexOrUVDictionary<T>();
            this.AddVertexOrUV<T>(point, newId, out var addedId);
            dict.TryGetValue(addedId, out var vertexOrUV);
            return vertexOrUV as T;
        }

        private T AddVertexOrUV<T>(Vector3 point, long id) where T : Vertex
        {
            var dict = GetVertexOrUVDictionary<T>();

            this.AddVertexOrUV<T>(point, id, out var addedId);
            if (addedId != id)
            {
                throw new Exception("This ID already exists");
            }
            dict.TryGetValue(addedId, out var vertexOrUV);
            return vertexOrUV as T;
        }

        private Boolean AddVertexOrUV<T>(Vector3 point, long idIfNew, out long id) where T : Vertex
        {
            var lookups = this.GetVertexOrUVLookup<T>();
            var dict = this.GetVertexOrUVDictionary<T>();
            var isUV = typeof(T) == typeof(UV);
            if (ValueExists(lookups, point, out id, Tolerance))
            {
                return false;
            }
            var zDict = GetAddressParent(lookups, point, true);
            var vertexOrUV = isUV ? new UV(this, idIfNew, point) as T : new Vertex(this, idIfNew, point) as T;
            id = vertexOrUV.Id;
            zDict.Add(point.Z, id);
            dict.Add(id, vertexOrUV);
            if (isUV)
            {
                this._uvId = Math.Max(id + 1, this._uvId + 1);
            }
            else
            {
                this._vertexId = Math.Max(id + 1, this._vertexId + 1);
            }
            return true;
        }

        #endregion add content

        /// <summary>
        /// Get a Vertex by its ID
        /// </summary>
        /// <param name="vertexId"></param>
        /// <returns></returns>
        public Vertex GetVertex(long vertexId)
        {
            this._vertices.TryGetValue(vertexId, out var vertex);
            return vertex;
        }

        /// <summary>
        /// Get all Vertices
        /// </summary>
        /// <returns></returns>
        public List<Vertex> GetVertices()
        {
            return this._vertices.Values.ToList();
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
            this._uvs.TryGetValue((long)uvId, out var uv);
            return uv;
        }

        /// <summary>
        /// Get all UVs
        /// </summary>
        /// <returns></returns>
        public List<UV> GetUVs()
        {
            return this._uvs.Values.ToList();
        }

        /// <summary>
        /// Get a Edge by its ID
        /// </summary>
        /// <param name="edgeId"></param>
        /// <returns></returns>
        public Edge GetEdge(long edgeId)
        {
            this._edges.TryGetValue(edgeId, out var edge);
            return edge;
        }

        /// <summary>
        /// Get all Edges
        /// </summary>
        /// <returns></returns>
        public List<Edge> GetEdges()
        {
            return this._edges.Values.ToList();
        }

        /// <summary>
        /// Get a HalfEdge by its ID
        /// </summary>
        /// <param name="halfEdgeId"></param>
        /// <returns></returns>
        public HalfEdge GetHalfEdge(long halfEdgeId)
        {
            this._halfEdges.TryGetValue(halfEdgeId, out var halfEdge);
            return halfEdge;
        }

        /// <summary>
        /// Get all HalfEdges
        /// </summary>
        /// <returns></returns>
        public List<HalfEdge> GetHalfEdges()
        {
            return this._halfEdges.Values.ToList();
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

            this._faces.TryGetValue((long)faceId, out var face);
            return face;
        }

        /// <summary>
        /// Get all Faces
        /// </summary>
        /// <returns></returns>
        public List<Face> GetFaces()
        {
            return this._faces.Values.ToList();
        }

        /// <summary>
        /// Get a Cell by its ID
        /// </summary>
        /// <param name="cellId"></param>
        /// <returns></returns>
        public Cell GetCell(long cellId)
        {
            this._cells.TryGetValue(cellId, out var cell);
            return cell;
        }

        /// <summary>
        /// Get all Cells
        /// </summary>
        /// <returns></returns>
        public List<Cell> GetCells()
        {
            return this._cells.Values.ToList();
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

    }
}