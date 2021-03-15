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
        private long _uniqueEdgeId = 1; // we start at 1 because 0 is returned as default value from dicts

        [JsonIgnore]
        private long _directedEdgeId = 1; // we start at 1 because 0 is returned as default value from dicts

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
        private Dictionary<long, UniqueEdge> _uniqueEdges = new Dictionary<long, UniqueEdge>();

        /// <summary>
        /// DirectedEdges by ID
        /// </summary>
        [JsonProperty]
        private Dictionary<long, DirectedEdge> _directedEdges = new Dictionary<long, DirectedEdge>();

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
        private Dictionary<string, long> uniqueEdgesLookup = new Dictionary<string, long>();

        // Same as uniqueEdgesLookup, with an addition level of dictionary for whether lesserVertexId is the start point or not
        [JsonIgnore]
        private Dictionary<(long, long), Dictionary<Boolean, long>> directedEdgesLookup = new Dictionary<(long, long), Dictionary<Boolean, long>>();

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
        /// <param name="_uniqueEdges"></param>
        /// <param name="_directedEdges"></param>
        /// <param name="_faces"></param>
        /// <param name="_cells"></param>
        /// <returns></returns>
        [JsonConstructor]
        internal CellComplex(
            Guid id,
            string name,
            Dictionary<long, Vertex> _vertices,
            Dictionary<long, Vertex> _uvs,
            Dictionary<long, UniqueEdge> _uniqueEdges,
            Dictionary<long, DirectedEdge> _directedEdges,
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

            foreach (var uniqueEdge in _uniqueEdges.Values)
            {
                if (!this.AddEdge(new List<long>() { uniqueEdge.StartVertexId, uniqueEdge.EndVertexId }, uniqueEdge.Id, out var addedEdge))
                {
                    throw new Exception("Duplicate uniqueEdge ID found");
                }
            }

            foreach (var directedEdge in _directedEdges.Values)
            {
                var uniqueEdge = this.GetEdge(directedEdge.EdgeId);
                if (!this.AddDirectedEdge(uniqueEdge, uniqueEdge.StartVertexId == directedEdge.StartVertexId, directedEdge.Id, out var addedDirectedEdge))
                {
                    throw new Exception("Duplicate directed uniqueEdge ID found");
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
        /// Add a DirectedEdge to the CellComplex
        /// </summary>
        /// <param name="line">Line with Start and End in the expected direction</param>
        /// <returns>Created DirectedEdge</returns>
        internal DirectedEdge AddDirectedEdge(Line line)
        {
            var points = new List<Vector3>() { line.Start, line.End };
            var vertices = points.Select(vertex => this.AddVertexOrUV<Vertex>(vertex)).ToList();
            this.AddEdge(vertices.Select(v => v.Id).ToList(), this._uniqueEdgeId, out var uniqueEdge);
            var dirMatchesEdge = vertices[0].Id == uniqueEdge.StartVertexId;
            this.AddDirectedEdge(uniqueEdge, dirMatchesEdge, this._directedEdgeId, out var directedEdge);
            return directedEdge;
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
            var directedEdges = new List<DirectedEdge>();
            foreach (var line in lines)
            {
                directedEdges.Add(this.AddDirectedEdge(line));
            }

            var hash = Face.GetHash(directedEdges);

            if (!this.facesLookup.TryGetValue(hash, out var faceId))
            {
                face = new Face(this, idIfNew, directedEdges, u, v);
                faceId = face.Id;
                this.facesLookup.Add(hash, faceId);
                this._faces.Add(faceId, face);

                foreach (var directedEdge in directedEdges)
                {
                    directedEdge.Faces.Add(face);
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
        /// Internal method to add a DirectedEdge
        /// </summary>
        /// <param name="uniqueEdge"></param>
        /// <param name="uniqueEdgeTupleIsInOrder"></param>
        /// <param name="idIfNew"></param>
        /// <param name="directedEdge"></param>
        /// <returns>Whether the directedEdge was successfully added. Will be false if idIfNew already exists</returns>
        private Boolean AddDirectedEdge(UniqueEdge uniqueEdge, Boolean uniqueEdgeTupleIsInOrder, long idIfNew, out DirectedEdge directedEdge)
        {
            var uniqueEdgeTuple = (uniqueEdge.StartVertexId, uniqueEdge.EndVertexId);

            if (!this.directedEdgesLookup.TryGetValue(uniqueEdgeTuple, out var directedEdgeDict))
            {
                directedEdgeDict = new Dictionary<bool, long>();
                this.directedEdgesLookup.Add(uniqueEdgeTuple, directedEdgeDict);
            }

            if (!directedEdgeDict.TryGetValue(uniqueEdgeTupleIsInOrder, out var directedEdgeId))
            {
                directedEdge = new DirectedEdge(this, idIfNew, uniqueEdge, uniqueEdgeTupleIsInOrder);
                directedEdgeId = directedEdge.Id;

                directedEdgeDict.Add(uniqueEdgeTupleIsInOrder, directedEdgeId);
                this._directedEdges.Add(directedEdgeId, directedEdge);

                uniqueEdge.DirectedEdges.Add(directedEdge);

                this._directedEdgeId = Math.Max(directedEdgeId + 1, this._directedEdgeId + 1);

                return true;
            }
            else
            {
                this._directedEdges.TryGetValue(directedEdgeId, out directedEdge);

                return false;
            }
        }

        /// <summary>
        /// Internal method to add a Edge
        /// </summary>
        /// <param name="vertexIds"></param>
        /// <param name="idIfNew"></param>
        /// <param name="uniqueEdge"></param>
        /// <returns>Whether the uniqueEdge was successfully added. Will be false if idIfNew already exists</returns>
        private Boolean AddEdge(List<long> vertexIds, long idIfNew, out UniqueEdge uniqueEdge)
        {
            var hash = UniqueEdge.GetHash(vertexIds);

            if (!this.uniqueEdgesLookup.TryGetValue(hash, out var uniqueEdgeId))
            {
                uniqueEdge = new UniqueEdge(this, idIfNew, vertexIds[0], vertexIds[1]);
                uniqueEdgeId = uniqueEdge.Id;

                this.uniqueEdgesLookup[hash] = uniqueEdgeId;
                this._uniqueEdges.Add(uniqueEdgeId, uniqueEdge);

                this.GetVertex(uniqueEdge.StartVertexId).Edges.Add(uniqueEdge);
                this.GetVertex(uniqueEdge.EndVertexId).Edges.Add(uniqueEdge);

                this._uniqueEdgeId = Math.Max(uniqueEdgeId + 1, this._uniqueEdgeId + 1);

                return true;
            }
            else
            {
                this._uniqueEdges.TryGetValue(uniqueEdgeId, out uniqueEdge);
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
        /// <param name="uniqueEdgeId"></param>
        /// <returns></returns>
        public UniqueEdge GetEdge(long uniqueEdgeId)
        {
            this._uniqueEdges.TryGetValue(uniqueEdgeId, out var uniqueEdge);
            return uniqueEdge;
        }

        /// <summary>
        /// Get all Edges
        /// </summary>
        /// <returns></returns>
        public List<UniqueEdge> GetEdges()
        {
            return this._uniqueEdges.Values.ToList();
        }

        /// <summary>
        /// Get a DirectedEdge by its ID
        /// </summary>
        /// <param name="directedEdgeId"></param>
        /// <returns></returns>
        public DirectedEdge GetDirectedEdge(long directedEdgeId)
        {
            this._directedEdges.TryGetValue(directedEdgeId, out var directedEdge);
            return directedEdge;
        }

        /// <summary>
        /// Get all DirectedEdges
        /// </summary>
        /// <returns></returns>
        public List<DirectedEdge> GetDirectedEdges()
        {
            return this._directedEdges.Values.ToList();
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