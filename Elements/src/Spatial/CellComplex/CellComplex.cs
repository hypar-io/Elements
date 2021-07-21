using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Elements.Geometry;
using Elements.Geometry.Solids;
using Newtonsoft.Json;

namespace Elements.Spatial.CellComplex
{
    /// <summary>
    /// A non-manifold cellular structure.
    /// </summary>
    /// <example>
    /// [!code-csharp[Main](../../Elements/test/CellComplexTests.cs?name=example)]
    /// </example>
    public class CellComplex : Elements.Element
    {
        /// <summary>
        /// Tolerance for points being considered the same.
        /// Applies individually to X, Y, and Z coordinates, not the cumulative difference!
        /// </summary>
        public double Tolerance = Vector3.EPSILON;

        internal ulong _edgeId = 1; // we start at 1 because 0 is returned as default value from dicts

        private ulong _vertexId = 1; // we start at 1 because 0 is returned as default value from dicts

        private ulong _faceId = 1; // we start at 1 because 0 is returned as default value from dicts

        private ulong _cellId = 1; // we start at 1 because 0 is returned as default value from dicts

        private ulong _orientationId = 1; // we start at 1 because 0 is returned as default value from dicts

        /// <summary>
        /// Vertices by ID.
        /// </summary>
        [JsonProperty]
        private Dictionary<ulong, Vertex> _vertices = new Dictionary<ulong, Vertex>();

        /// <summary>
        /// U or V directions by ID.
        /// </summary>
        [JsonProperty]
        private Dictionary<ulong, Orientation> _orientations = new Dictionary<ulong, Orientation>();

        /// <summary>
        /// Edges by ID.
        /// </summary>
        [JsonProperty]
        private Dictionary<ulong, Edge> _edges = new Dictionary<ulong, Edge>();

        /// <summary>
        /// Faces by ID.
        /// </summary>
        [JsonProperty]
        private Dictionary<ulong, Face> _faces = new Dictionary<ulong, Face>();

        /// <summary>
        /// Cells by ID.
        /// </summary>
        [JsonProperty]
        private Dictionary<ulong, Cell> _cells = new Dictionary<ulong, Cell>();

        // Vertex lookup by x, y, z coordinate.
        private Dictionary<double, Dictionary<double, Dictionary<double, ulong>>> _verticesLookup = new Dictionary<double, Dictionary<double, Dictionary<double, ulong>>>();

        // Orientation lookup by x, y, z coordinate.
        private Dictionary<double, Dictionary<double, Dictionary<double, ulong>>> _orientationsLookup = new Dictionary<double, Dictionary<double, Dictionary<double, ulong>>>();

        // See Edge.GetHash for how faces are identified as unique.
        private Dictionary<string, ulong> _edgesLookup = new Dictionary<string, ulong>();

        // See Face.GetHash for how faces are identified as unique.
        private Dictionary<string, ulong> _facesLookup = new Dictionary<string, ulong>();

        /// <summary>
        /// Create a CellComplex.
        /// </summary>
        /// <param name="id">Optional ID: If blank, a new Guid will be created.</param>
        /// <param name="name">Optional name of your CellComplex.</param>
        /// <returns></returns>
        public CellComplex(Guid id = default(Guid), string name = null) : base(id != default(Guid) ? id : Guid.NewGuid(), name) { }

        /// <summary>
        /// This constructor is intended for serialization and deserialization only.
        /// </summary>
        /// <param name="id"></param>
        /// <param name="name"></param>
        /// <param name="_vertices"></param>
        /// <param name="_orientations"></param>
        /// <param name="_edges"></param>
        /// <param name="_faces"></param>
        /// <param name="_cells"></param>
        /// <returns></returns>
        [JsonConstructor]
        internal CellComplex(
            Guid id,
            string name,
            Dictionary<ulong, Vertex> _vertices,
            Dictionary<ulong, Vertex> _orientations,
            Dictionary<ulong, Edge> _edges,
            Dictionary<ulong, Face> _faces,
            Dictionary<ulong, Cell> _cells
        ) : base(id, name)
        {
            foreach (var vertex in _vertices.Values)
            {
                var added = this.AddVertexOrOrientation<Vertex>(vertex.Value, vertex.Id);
                added.Name = vertex.Name;
            }

            foreach (var orientation in _orientations.Values)
            {
                this.AddVertexOrOrientation<Orientation>(orientation.Value, orientation.Id);
            }

            foreach (var edge in _edges.Values)
            {
                if (!this.AddEdge(new List<ulong>() { edge.StartVertexId, edge.EndVertexId }, edge.Id, out var addedEdge))
                {
                    throw new Exception("Duplicate edge ID found");
                }
            }

            foreach (var face in _faces.Values)
            {
                face.CellComplex = this; // CellComplex not included on deserialization, add it back for processing even though we will discard this and create a new one
                var polygon = face.GetGeometry();
                var orientation = face.GetOrientation();
                if (!this.AddFace(polygon, face.Id, orientation.U, orientation.V, out var addedFace))
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
        /// <param name="polygon">The polygon that forms the base of this cell.</param>
        /// <param name="height">The height of the cell.</param>
        /// <param name="elevation">The elevation of the bottom of this cell.</param>
        /// <param name="uGrid">An optional but highly recommended U grid that allows the cell's top and bottom faces to store intended directionality.</param>
        /// <param name="vGrid">An optional but highly recommended V grid that allows the cell's top and bottom faces to store intended directionality.</param>
        /// <returns>The created Cell.</returns>
        public Cell AddCell(Polygon polygon, double height, double elevation, Grid1d uGrid = null, Grid1d vGrid = null)
        {
            var elevationVector = new Vector3(0, 0, elevation);
            var heightVector = new Vector3(0, 0, height);

            var transformedPolygonBottom = (Polygon)polygon.Transformed(new Transform(elevationVector));
            var transformedPolygonTop = (Polygon)polygon.Transformed(new Transform(elevationVector + heightVector));

            Orientation u = null;
            Orientation v = null;

            if (uGrid != null)
            {
                u = this.AddVertexOrOrientation<Orientation>(uGrid.Direction().Unitized());
            }
            if (vGrid != null)
            {
                v = this.AddVertexOrOrientation<Orientation>(vGrid.Direction().Unitized());
            }

            var bottomFace = this.AddFace(transformedPolygonBottom, u, v);
            var topFace = this.AddFace(transformedPolygonTop, u, v);

            var faces = new List<Face>() { bottomFace, topFace };

            var up = this.AddVertexOrOrientation<Orientation>(new Vector3(0, 0, 1));

            foreach (var faceEdge in transformedPolygonBottom.Segments())
            {
                var vertices = new List<Vector3>() { faceEdge.Start, faceEdge.End };
                var horizontalU = this.AddVertexOrOrientation<Orientation>((faceEdge.End - faceEdge.Start).Unitized());
                vertices.Add(faceEdge.End + heightVector);
                vertices.Add(faceEdge.Start + heightVector);
                var facePoly = new Polygon(vertices);
                faces.Add(this.AddFace(facePoly, horizontalU, up));
            }

            this.AddCell(this._cellId, faces, bottomFace, topFace, out var cell);
            return cell;
        }

        /// <summary>
        /// Add a Face to the CellComplex.
        /// </summary>
        /// <param name="polygon">A polygon representing a unique Face.</param>
        /// <param name="u">Orientation of U axis.</param>
        /// <param name="v">Orientation of V axis.</param>
        /// <returns>The created Face.</returns>
        internal Face AddFace(Polygon polygon, Orientation u = null, Orientation v = null)
        {
            this.AddFace(polygon, this._faceId, u, v, out var face);
            return face;
        }

        /// <summary>
        /// Add a Edge to the CellComplex.
        /// </summary>
        /// <param name="line">Line with Start and End in the expected direction.</param>
        /// <returns>The created edge.</returns>
        private Edge AddEdge(Line line)
        {
            var points = new List<Vector3>() { line.Start, line.End };
            var vertices = points.Select(vertex => this.AddVertexOrOrientation<Vertex>(vertex)).ToList();
            this.AddEdge(vertices.Select(v => v.Id).ToList(), this._edgeId, out var edge);
            return edge;
        }

        /// <summary>
        /// The lowest-level method to add a Cell: all other AddCell methods will eventually call this one.
        /// </summary>
        /// <param name="cellId"></param>
        /// <param name="faces"></param>
        /// <param name="bottomFace"></param>
        /// <param name="topFace"></param>
        /// <param name="cell"></param>
        /// <returns>Whether the cell was successfully added. Will be false if cellId already exists.</returns>
        private Boolean AddCell(ulong cellId, List<Face> faces, Face bottomFace, Face topFace, out Cell cell)
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
        /// The lowest-level method to add a Face: all other AddFace methods will eventually call this one.
        /// </summary>
        /// <param name="polygon"></param>
        /// <param name="idIfNew"></param>
        /// <param name="u"></param>
        /// <param name="v"></param>
        /// <param name="face"></param>
        /// <returns>Whether the face was successfully added. Will be false if idIfNew already exists</returns>
        private Boolean AddFace(Polygon polygon, ulong idIfNew, Orientation u, Orientation v, out Face face)
        {
            var lines = polygon.Segments();
            var edges = new List<Edge>();
            foreach (var line in lines)
            {
                edges.Add(this.AddEdge(line));
            }

            var hash = Face.GetHash(edges);

            if (!this._facesLookup.TryGetValue(hash, out var faceId))
            {
                face = new Face(this, idIfNew, edges, u, v);
                faceId = face.Id;
                this._facesLookup.Add(hash, faceId);
                this._faces.Add(faceId, face);

                foreach (var edge in edges)
                {
                    edge.Faces.Add(face);
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
        /// The lowest-level method to add an Edge: all other AddEdge methods will eventually call this one.
        /// </summary>
        /// <param name="vertexIds"></param>
        /// <param name="idIfNew"></param>
        /// <param name="edge"></param>
        /// <returns>Whether the edge was successfully added. Will be false if idIfNew already exists.</returns>
        internal Boolean AddEdge(List<ulong> vertexIds, ulong idIfNew, out Edge edge)
        {
            var hash = Edge.GetHash(vertexIds);

            if (!this._edgesLookup.TryGetValue(hash, out var edgeId))
            {
                edge = new Edge(this, idIfNew, vertexIds[0], vertexIds[1]);
                edgeId = edge.Id;

                this._edgesLookup[hash] = edgeId;
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

        /// <summary>
        /// Utility to get the address dictionary of Vertices or Orientations
        /// </summary>
        /// <typeparam name="T">Vertex or Orientation.</typeparam>
        /// <returns></returns>
        private Dictionary<ulong, T> GetVertexOrOrientationDictionary<T>() where T : VertexBase<T>
        {
            if (typeof(T) != typeof(Orientation) && typeof(T) != typeof(Vertex))
            {
                throw new Exception("Unsupported type provided, expected Vertex or Orientation");
            }
            return typeof(T) == typeof(Orientation) ? this._orientations as Dictionary<ulong, T> : this._vertices as Dictionary<ulong, T>;
        }

        /// <summary>
        /// Utility to get the lookup dictionary of Vertices or Orientations.
        /// </summary>
        /// <typeparam name="T">Vertex or Orientation.</typeparam>
        /// <returns></returns>
        private Dictionary<double, Dictionary<double, Dictionary<double, ulong>>> GetVertexOrOrientationLookup<T>() where T : VertexBase<T>
        {
            if (typeof(T) != typeof(Orientation) && typeof(T) != typeof(Vertex))
            {
                throw new Exception("Unsupported type provided, expected Vertex or Orientation");
            }
            return typeof(T) == typeof(Orientation) ? this._orientationsLookup : this._verticesLookup;
        }

        /// <summary>
        /// Add a Vertex or an Orientation by its location or vector direction.
        /// </summary>
        /// <param name="point">This represents the point in space if this is a Vertex, or the orientation vector if it is an Orientation.</param>
        /// <typeparam name="T">Vertex or Orientation.</typeparam>
        /// <returns>The created or existing Vertex or Orientation.</returns>
        internal T AddVertexOrOrientation<T>(Vector3 point) where T : VertexBase<T>
        {
            var newId = typeof(T) == typeof(Orientation) ? this._orientationId : this._vertexId;
            var dict = GetVertexOrOrientationDictionary<T>();
            this.AddVertexOrOrientation<T>(point, newId, out var addedId);
            dict.TryGetValue(addedId, out var vertexOrOrientation);
            return vertexOrOrientation as T;
        }

        /// <summary>
        /// Add a Vertex or an Orientation by its location or vector direction, and its ID.
        /// This should only be used during deserialization, when we know exactly what our IDs are ahead of time.
        /// </summary>
        /// <param name="point">This represents the point in space if this is a Vertex, or the orientation vector if it is an Orientation.</param>
        /// <param name="id">ID of the item to create.</param>
        /// <typeparam name="T">Vertex or Orientation.</typeparam>
        /// <returns>The created Vertex or Orientation.</returns>
        private T AddVertexOrOrientation<T>(Vector3 point, ulong id) where T : VertexBase<T>
        {
            var dict = GetVertexOrOrientationDictionary<T>();

            this.AddVertexOrOrientation<T>(point, id, out var addedId);
            if (addedId != id)
            {
                throw new Exception("This ID already exists");
            }
            dict.TryGetValue(addedId, out var vertexOrOrientation);
            return vertexOrOrientation as T;
        }

        /// <summary>
        /// The lowest-level method to add a Vertex or Orientation: all other AddVertexOrOrientation methods will eventually call this one.
        /// </summary>
        /// <param name="point">This represents the point in space if this is a Vertex, or the orientation vector if it is an Orientation.</param>
        /// <param name="idIfNew">ID to use if we want to create a new item.</param>
        /// <param name="id">ID of created or existing item.</param>
        /// <typeparam name="T">Vertex or Orientation.</typeparam>
        /// <returns>Whether the item was successfully added. Will be false if idIfNew already exists.</returns>
        private Boolean AddVertexOrOrientation<T>(Vector3 point, ulong idIfNew, out ulong id) where T : VertexBase<T>
        {
            var lookups = this.GetVertexOrOrientationLookup<T>();
            var dict = this.GetVertexOrOrientationDictionary<T>();
            var isOrientation = typeof(T) == typeof(Orientation);
            if (ValueExists(lookups, point, out id, Tolerance))
            {
                return false;
            }
            var zDict = GetAddressParent(lookups, point, true);
            var vertexOrOrientation = isOrientation ? new Orientation(this, idIfNew, point) as T : new Vertex(this, idIfNew, point) as T;
            id = vertexOrOrientation.Id;
            zDict.Add(point.Z, id);
            dict.Add(id, vertexOrOrientation);
            if (isOrientation)
            {
                this._orientationId = Math.Max(id + 1, this._orientationId + 1);
            }
            else
            {
                this._vertexId = Math.Max(id + 1, this._vertexId + 1);
            }
            return true;
        }

        #endregion add content

        /// <summary>
        /// Get a Vertex by its ID.
        /// </summary>
        /// <param name="vertexId"></param>
        /// <returns></returns>
        public Vertex GetVertex(ulong vertexId)
        {
            this._vertices.TryGetValue(vertexId, out var vertex);
            return vertex;
        }

        /// <summary>
        /// Get all Vertices.
        /// </summary>
        /// <returns></returns>
        public List<Vertex> GetVertices()
        {
            return this._vertices.Values.ToList();
        }

        /// <summary>
        /// Get a U or V direction by its ID.
        /// </summary>
        /// <param name="orientationId"></param>
        /// <returns></returns>
        public Orientation GetOrientation(ulong? orientationId)
        {
            if (orientationId == null)
            {
                return null;
            }
            this._orientations.TryGetValue((ulong)orientationId, out var orientation);
            return orientation;
        }

        /// <summary>
        /// Get all Orientations.
        /// </summary>
        /// <returns></returns>
        internal List<Orientation> GetOrientations()
        {
            return this._orientations.Values.ToList();
        }

        /// <summary>
        /// Get a Edge by its ID.
        /// </summary>
        /// <param name="edgeId"></param>
        /// <returns></returns>
        public Edge GetEdge(ulong edgeId)
        {
            this._edges.TryGetValue(edgeId, out var edge);
            return edge;
        }

        /// <summary>
        /// Get all Edges.
        /// </summary>
        /// <returns></returns>
        public List<Edge> GetEdges()
        {
            return this._edges.Values.ToList();
        }

        /// <summary>
        /// Get a Face by its ID.
        /// </summary>
        /// <param name="faceId"></param>
        /// <returns></returns>
        public Face GetFace(ulong? faceId)
        {
            if (faceId == null)
            {
                return null;
            }
            this._faces.TryGetValue((ulong)faceId, out var face);
            return face;
        }

        /// <summary>
        /// Get all Faces.
        /// </summary>
        /// <returns></returns>
        public List<Face> GetFaces()
        {
            return this._faces.Values.ToList();
        }

        /// <summary>
        /// Get a Cell by its ID.
        /// </summary>
        /// <param name="cellId"></param>
        /// <returns></returns>
        public Cell GetCell(ulong cellId)
        {
            this._cells.TryGetValue(cellId, out var cell);
            return cell;
        }

        /// <summary>
        /// Get all Cells.
        /// </summary>
        /// <returns></returns>
        public List<Cell> GetCells()
        {
            return this._cells.Values.ToList();
        }

        /// <summary>
        /// Get the associated Vertex that is closest to a point.
        /// </summary>
        /// <param name="point"></param>
        /// <returns></returns>
        public Vertex GetClosestVertex(Vector3 point)
        {
            return Vertex.GetClosest<Vertex>(this.GetVertices(), point);
        }

        /// <summary>
        /// Get the associated Edge that is closest to a point.
        /// </summary>
        /// <param name="point"></param>
        /// <returns></returns>
        public Edge GetClosestEdge(Vector3 point)
        {
            return Edge.GetClosest<Edge>(this.GetEdges(), point);
        }

        /// <summary>
        /// Get the associated Face that is closest to a point.
        /// </summary>
        /// <param name="point"></param>
        /// <returns></returns>
        public Face GetClosestFace(Vector3 point)
        {
            return Face.GetClosest<Face>(this.GetFaces(), point);
        }

        /// <summary>
        /// Get the associated Cell that is closest to a point.
        /// </summary>
        /// <param name="point"></param>
        /// <returns></returns>
        public Cell GetClosestCell(Vector3 point)
        {
            return Cell.GetClosest<Cell>(this.GetCells(), point);
        }

        /// <summary>
        /// Get all vertices matching an X/Y coordinate, regardless of Z.
        /// </summary>
        /// <param name="x">X coordinate.</param>
        /// <param name="y">Y coordinate.</param>
        /// <param name="fuzzyFactor">Amount of tolerance in the search against each component of the coordinate.</param>
        /// <returns></returns>
        public List<Vertex> GetVerticesMatchingXY(double x, double y, Nullable<double> fuzzyFactor = null)
        {
            var vertices = new List<Vertex>();
            var zDict = GetAddressParent(this._verticesLookup, new Vector3(x, y), fuzzyFactor: fuzzyFactor);
            if (zDict == null)
            {
                return vertices;
            }
            return zDict.Values.Select(id => this.GetVertex(id)).ToList();
        }

        /// <summary>
        /// Whether a vertex location already exists in the CellComplex.
        /// </summary>
        /// <param name="point"></param>
        /// <param name="id">The ID of the Vertex, if a match is found.</param>
        /// <param name="fuzzyFactor">Amount of tolerance in the search against each component of the coordinate.</param>
        /// <returns></returns>
        public Boolean VertexExists(Vector3 point, out ulong id, Nullable<double> fuzzyFactor = null)
        {
            return ValueExists(this._verticesLookup, point, out id, fuzzyFactor);
        }

        /// <summary>
        /// Add a value to a Dictionary of HashSets of longs. Used as a utility for internal lookups.
        /// </summary>
        /// <param name="dict"></param>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        private static HashSet<ulong> AddValue(Dictionary<ulong, HashSet<ulong>> dict, ulong key, ulong value)
        {
            if (!dict.ContainsKey(key))
            {
                dict.Add(key, new HashSet<ulong>());
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
        /// <param name="addAddressIfNonExistent">Whether to create the dictionary address if it didn't previously exist.</param>
        /// <param name="fuzzyFactor">Amount of tolerance in the search against each component of the coordinate.</param>
        /// <returns>The created or existing last level of values. This can be null if the dictionary address didn't exist previously, and we chose not to add it.</returns>
        private static Dictionary<double, ulong> GetAddressParent(Dictionary<double, Dictionary<double, Dictionary<double, ulong>>> dict, Vector3 point, Boolean addAddressIfNonExistent = false, Nullable<double> fuzzyFactor = null)
        {
            if (!TryGetValue<Dictionary<double, Dictionary<double, ulong>>>(dict, point.X, out var yzDict, fuzzyFactor))
            {
                yzDict = new Dictionary<double, Dictionary<double, ulong>>();
                if (addAddressIfNonExistent)
                {
                    dict.Add(point.X, yzDict);
                }
                else
                {
                    return null;
                }
            }

            if (!TryGetValue<Dictionary<double, ulong>>(yzDict, point.Y, out var zDict, fuzzyFactor))
            {
                zDict = new Dictionary<double, ulong>();
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
        /// In a dictionary of x, y, and z coordinates, whether a point value is represented.
        /// </summary>
        /// <param name="dict"></param>
        /// <param name="point"></param>
        /// <param name="id">ID of the found vertex or orientation, if found.</param>
        /// <param name="fuzzyFactor">Amount of tolerance in the search against each component of the coordinate.</param>
        /// <returns></returns>
        private static Boolean ValueExists(Dictionary<double, Dictionary<double, Dictionary<double, ulong>>> dict, Vector3 point, out ulong id, Nullable<double> fuzzyFactor = null)
        {
            var zDict = GetAddressParent(dict, point, fuzzyFactor: fuzzyFactor);
            if (zDict == null)
            {
                id = 0;
                return false;
            }
            return TryGetValue<ulong>(zDict, point.Z, out id, fuzzyFactor);
        }

        /// <summary>
        /// A version of TryGetValue on a dictionary that optionally takes in a tolerance when running the comparison.
        /// </summary>
        /// <param name="dict"></param>
        /// <param name="key">Number to search for.</param>
        /// <param name="value">Value if match was found.</param>
        /// <param name="fuzzyFactor">Amount of tolerance in the search for the key.</param>
        /// <typeparam name="T">The type of the dictionary values.</typeparam>
        /// <returns>Whether a match was found.</returns>
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
        /// Construct model elements to visualize the cell complex.
        /// </summary>
        /// <param name="debug">If true, vertices and edge directions will be visualized.</param>
        /// <returns>A collection of elements.</returns>
        public List<Element> ToModelElements(bool debug = false)
        {
            var elements = new List<Element>();

            var r = new Random();

            var cells = this.GetCells();
            var mesh = new Mesh();
            if (cells.Any())
            {
                foreach (var c in this.GetCells())
                {
                    // if (debug)
                    // {
                    //     var avg = c.GetVertices().Select(v => v.Value).ToList().Average();
                    //     elements.Add(Draw.Text($"Cell:{c.Id}", avg, Vector3.ZAxis, 5));
                    // }

                    var color = r.NextColor();
                    foreach (var f in c.GetFaces())
                    {
                        DrawFace(ref mesh, f, color, elements, debug);
                    }
                }
            }
            else
            {
                foreach (var f in this.GetFaces())
                {
                    var color = r.NextColor();
                    DrawFace(ref mesh, f, color, elements, debug);
                }
            }

            mesh.ComputeNormals();
            elements.Add(new MeshElement(mesh, new Material("Cells", new Color(1.0, 1.0, 1.0, 0.5), doubleSided: true, unlit: true)));

            if (!debug)
            {
                return elements;
            }

            foreach (var v in this._vertices)
            {
                elements.Add(Draw.Cube(v.Value.Value, v.Key.ToString(), BuiltInMaterials.YAxis, 0.1));
            }

            var offset = 0.5;

            foreach (var v in this._vertices)
            {
                elements.Add(Draw.Text($"Vertex:{v.Value.Id}\nFaces: {string.Join(",", v.Value.GetFaces().Select(f => f.Id.ToString()))}", v.Value.Value, Vector3.ZAxis));
            }

            foreach (var e in this._edges)
            {
                var m = BuiltInMaterials.Default;
                if (e.Value.Faces.Count == 1)
                {
                    m = BuiltInMaterials.XAxis;
                }
                else if (e.Value.Faces.Count == 2)
                {
                    m = BuiltInMaterials.YAxis;
                }
                else if (e.Value.Faces.Count > 2)
                {
                    m = BuiltInMaterials.ZAxis;
                }
                var de = e.Value;
                var a1 = GetVertex(de.StartVertexId).Value;
                var b1 = GetVertex(de.EndVertexId).Value;
                var d1 = (b1 - a1).Unitized();
                var t = new Transform(a1 + d1 * a1.DistanceTo(b1) / 2, d1);
                var o = a1.DistanceTo(b1) < 2 * offset ? 0.0 : offset;
                elements.AddRange(Draw.Arrow(new Line(a1 + d1 * o, b1 - d1 * o), m, 0.05, 0.3));

                elements.Add(Draw.Text($"Edge:{e.Value.Id}\nFaces: {string.Join(",", e.Value.GetFaces().Select(f => f.Id.ToString()))}", t.Origin, t.XAxis));
            }

            return elements;
        }

        private void DrawFace(ref Mesh mesh, Face f, Color color, List<Element> elements, bool debug)
        {
            var p = f.GetGeometry();
            Solid.TessellateInternal(p.ToContourVertexArray(), null, p.Normal(), ref mesh, color: color);

            if (debug)
            {
                elements.Add(Draw.Text($"Face:{f.Id}", p.PointInternal(), p.Normal(), 3));
            }
        }

        /// <summary>
        /// Remove a face from the cell complex.
        /// </summary>
        /// <param name="face">The face to remove.</param>
        public void RemoveFace(Face face)
        {
            var edges = face.GetEdges();
            var hash = Face.GetHash(edges);
            foreach (var e in edges)
            {
                e.Faces.Remove(face);
            }
            this._facesLookup.Remove(hash);
            this._faces.Remove(face.Id);

            foreach (var c in face.GetCells())
            {
                c.FaceIds.Remove(face.Id);
                if (c.BottomFaceId == face.Id)
                {
                    c.BottomFaceId = null;
                }
                else if (c.TopFaceId == face.Id)
                {
                    c.TopFaceId = null;
                }
            }
        }

        /// <summary>
        /// Split the face with a polyline.
        /// If the face is adjacent to the polyline, the method will 
        /// return true and the vertex collection will correspond 
        /// with the result if the polygon had been split.
        /// </summary>
        /// <param name="face">The face to split.</param>
        /// <param name="poly">The trimming polyline.</param>
        /// <param name="faces">A collection of faces resulting from the split.</param>
        /// <param name="newExternalVertices">A collection of new vertices created
        /// by this operation, which lie on the boundary of the original face.</param>
        /// <param name="newInternalVertices">A collection of new vertices created 
        /// by this operation, which are internal to the original face.</param>
        /// <param name="perimeterSplit">Should the perimeter be split initially?
        /// Set this value to false when you know that external split vertices on
        /// the target face have already been created.</param>
        /// <returns>True if a split occurred or the face is adjcent
        /// to the splitting polyline, otherwise false.</returns>
        public bool TrySplitFace(Face face,
                                 Polyline poly,
                                 out List<Face> faces,
                                 out Dictionary<ulong, Vertex> newExternalVertices,
                                 out Dictionary<ulong, Vertex> newInternalVertices,
                                 bool perimeterSplit = true)
        {
            faces = new List<Face>();
            newInternalVertices = null;
            newExternalVertices = null;

            if (this.GetFace(face.Id) == null)
            {
                return false;
            }

            var existingVertices = face.GetVertices();

            newExternalVertices = new Dictionary<ulong, Vertex>();
            newInternalVertices = new Dictionary<ulong, Vertex>();

            if (perimeterSplit)
            {
                // Sweep through all segments and intersect to create
                // edges and vertices that will be used in the second
                // pass to create the split polygons.

                // TODO: This is slow because we're doing intersections
                // which are later computed by the polgon split operation
                // as well. We do them here using TrySplitEdge because it
                // correctly re-links faces. We should add a data parameter 
                // to edges in a half edge graph, where we can store face
                // connectivity information to allow face relinking.
                var segments = poly.Segments();
                foreach (var e in face.GetEdges())
                {
                    var av = this.GetVertex(e.StartVertexId);
                    var a = av.Value;
                    var bv = this.GetVertex(e.EndVertexId);
                    var b = bv.Value;
                    var s = new Line(a, b);
                    foreach (var s1 in segments)
                    {
                        // If a polygon has been split before, the end
                        // points of the segments will be internal vertices.
                        if (s1.Start.IsAlmostEqualTo(a))
                        {
                            if (!newInternalVertices.ContainsKey(av.Id))
                            {
                                newInternalVertices.Add(av.Id, av);
                            }
                            continue;
                        }

                        if (s1.End.IsAlmostEqualTo(b))
                        {
                            if (!newInternalVertices.ContainsKey(bv.Id))
                            {
                                newInternalVertices.Add(bv.Id, bv);
                            }
                            continue;
                        }

                        if (s.Intersects(s1, out Vector3 result))
                        {
                            // Check intersection coincidence with 
                            // an existing vertex.
                            if (result.IsAlmostEqualTo(a))
                            {
                                if (!newExternalVertices.ContainsKey(av.Id))
                                {
                                    newExternalVertices.Add(av.Id, av);
                                    continue;
                                }
                            }
                            else if (result.IsAlmostEqualTo(b))
                            {
                                if (!newExternalVertices.ContainsKey(bv.Id))
                                {
                                    newExternalVertices.Add(bv.Id, bv);
                                    continue;
                                }
                            }

                            if (TrySplitEdge(e, result, out Vertex vertex))
                            {
                                if (!newExternalVertices.ContainsKey(vertex.Id))
                                {
                                    newExternalVertices.Add(vertex.Id, vertex);
                                }
                            }
                        }
                    }
                }

                // There was no intersection with the face's perimeter.
                if (newExternalVertices.Count == 0 && newInternalVertices.Count == 0)
                {
                    return false;
                }
            }

            // Create a polygon and use a polyline split to split
            // it into parts which will become new faces.
            var p = face.GetGeometry();
            var splitPolys = p.Split(new[] { poly });

            // If we only get one polygon back, we can
            // assume that the split resulted in the 
            // original face.
            // Send back a result that looks as if the face
            // was split, but using the existing vertices and faces.
            if (splitPolys.Count == 1)
            {
                faces.Add(face);

                // External vertices are all those along an internal edge
                // which are not also internal or located on the original.
                var internals = newInternalVertices.Select(v => v.Value).ToList();
                var externals = newInternalVertices.SelectMany(v => v.Value.GetEdges())
                                                   .SelectMany(e => e.GetVertices())
                                                   .Where(v => existingVertices.Contains(v))
                                                   .Where(v => !internals.Contains(v));
                foreach (var v in externals)
                {
                    if (!newExternalVertices.ContainsKey(v.Id))
                    {
                        newExternalVertices.Add(v.Id, v);
                    }
                }
                return true;
            }
            else
            {
                var faceCells = face.GetCells();
                foreach (var split in splitPolys)
                {
                    var f = this.AddFace(split);
                    faces.Add(f);
                    foreach (var v in f.GetVerticesUnordered())
                    {
                        // Avoid adding this if it's in the external collection
                        // or in the original vertices
                        if (!newExternalVertices.ContainsKey(v.Id) && !existingVertices.Contains(v))
                        {
                            if (!newInternalVertices.ContainsKey(v.Id))
                            {
                                newInternalVertices.Add(v.Id, v);
                            }
                        }
                    }
                    foreach (var c in faceCells)
                    {
                        c.FaceIds.Add(f.Id);
                    }
                }

                this.RemoveFace(face);

                return true;
            }
        }

        /// <summary>
        /// Split the edge with a plane.
        /// </summary>
        /// <param name="edge">The edge to split.</param>
        /// <param name="plane">The plane which splits the edge.</param>
        /// <param name="vertex">The vertex at the split.</param>
        /// <returns>True if the split is successful, otherwise false.</returns>
        public bool TrySplitEdge(Edge edge, Plane plane, out Vertex vertex)
        {
            if (edge.GetGeometry().Intersects(plane, out Vector3 result))
            {
                return TrySplitEdge(edge, result, out vertex);
            }
            vertex = null;
            return false;
        }

        /// <summary>
        /// Split the edge with a point.
        /// </summary>
        /// <param name="edge">The edge to split.</param>
        /// <param name="point">The point at which to split the edge.</param>
        /// <param name="vertex">The vertex at the split.</param>
        /// <returns>True if the split was successful, or false if the specified point
        /// does not split the edge or the edge could not be split.</returns>
        public bool TrySplitEdge(Edge edge, Vector3 point, out Vertex vertex)
        {
            vertex = null;

            if (this.GetEdge(edge.Id) == null)
            {
                return false;
            }

            var a = this.GetVertex(edge.StartVertexId);
            var b = this.GetVertex(edge.EndVertexId);

            if (point.IsAlmostEqualTo(a.Value))
            {
                vertex = a;
                return true;
            }
            else if (point.IsAlmostEqualTo(b.Value))
            {
                vertex = b;
                return true;
            }

            var l = new Line(a.Value, b.Value);
            if (point.DistanceTo(l, out Vector3 closestPoint) > Vector3.EPSILON)
            {
                return false;
            }

            // |                              |
            // |            face              |
            // |                              |
            // a-------------x----------------b <- Add a new vertex on edge at x.
            // |                              |    Create a new edge xb.
            // |                              |
            // |            face              |
            // |                              |

            // Add a new vertex to the cell complex
            var x = this.AddVertexOrOrientation<Vertex>(point);
            var id1 = this.Id;
            var id2 = x.Id;

            // Remove this edge from the b collection
            b.Edges.Remove(edge);

            // Create a new edge from the new vertex
            // to the existing end vertex.
            this.AddEdge(new List<ulong> { x.Id, edge.EndVertexId }, this._edgeId, out Edge xb);

            RemoveEdgeFromLookup(edge);

            // Adjust this edge to point to the new vertex.
            edge.EndVertexId = x.Id;

            AddEdgeToLookup(edge);

            // Add the existing edge to the new vertex's collection.
            x.Edges.Add(edge);

            // Add this edge to the b collection
            b.Edges.Add(xb);

            // Add the new edge into the adjacent face collections.
            foreach (var f in edge.Faces)
            {
                f.EdgeIds.Add(xb.Id);
                xb.Faces.Add(f);
            }

            vertex = x;
            return true;
        }

        /// <summary>
        /// Split a cell with a polygon.
        /// </summary>
        /// <param name="cell">The cell to split.</param>
        /// <param name="polygon">The polygon which will split this cell.</param>
        /// <param name="cells">The cells resulting from this split.</param>
        /// <returns>True if the split was successful, or false if the polygon does
        /// not split the cell.</returns>
        public bool TrySplitCell(Cell cell, Polygon polygon, out List<Cell> cells)
        {
            cells = new List<Cell>();

            var nonVerticalFaceGroups = cell.GetNonVerticalFaces()
                                       .GroupBy(f => f.GetCentroid().Z)
                                       .OrderBy(g => g.Key)
                                       .ToList();

            var externalVertexGroups = new List<List<Vertex>>();
            var internalVertexGroups = new List<List<Vertex>>();
            var splitFaceGroups = new List<List<Face>>();

            foreach (var group in nonVerticalFaceGroups)
            {
                var trimPolygon = polygon.TransformedPolygon(new Transform(new Vector3(0, 0, group.Key)));
                var groupFaces = new List<Face>();
                var externalVertices = new List<Vertex>();
                var internalVertices = new List<Vertex>();
                foreach (var face in group)
                {
                    if (!this.TrySplitFace(face,
                                    trimPolygon,
                                    out List<Face> faces,
                                    out Dictionary<ulong, Vertex> exVertices,
                                    out Dictionary<ulong, Vertex> inVertices))
                    {
                        continue;
                    }
                    externalVertices.AddRange(exVertices.Values);
                    internalVertices.AddRange(inVertices.Values);
                    groupFaces.AddRange(faces);
                }
                splitFaceGroups.Add(groupFaces);
                externalVertexGroups.Add(externalVertices.Distinct().ToList());
                internalVertexGroups.Add(internalVertices.Distinct().ToList());
            }

            // The following transpose hack is based on the assumption
            // that non-vertical face splitting, regardless of whether all the
            // faces are parallel, will result in face "layers" where the faces
            // align vertically from one to the next. By tranposing the data,
            // we go from "layers" of faces to "columns" of faces, which notionally
            // represent the bottom and top of the cell.
            // When cells faces are highly skewed in relation to horizontal
            // (i.e., almost vertical), the split ordering may differ and this
            // might explode.
            var faceGroups = splitFaceGroups.SelectMany(inner => inner.Select((item, index) => new { item, index }))
                                            .GroupBy(i => i.index, i => i.item)
                                            .Select(g => g.ToList())
                                            .ToList();


            // Split the external faces first, so that the second 
            // pass which creates the internal faces, has new edges
            // to find.
            for (var i = 0; i < externalVertexGroups.Count - 1; i++)
            {
                var bottom = externalVertexGroups[i];
                var top = externalVertexGroups[i + 1];
                for (var j = 0; j < Math.Min(bottom.Count, top.Count); j++)
                {
                    var a = bottom[j];
                    // var b = top[j];
                    var b = top.First(v => v.Value.X.ApproximatelyEquals(a.Value.X) && v.Value.Y.ApproximatelyEquals(a.Value.Y));
                    var candidateFaces = a.GetFaces().Intersect(b.GetFaces());
                    var pline = new Polyline(new[] { a.Value, b.Value });
                    foreach (var sideFace in candidateFaces)
                    {
                        if (!this.TrySplitFace(sideFace,
                                              pline,
                                              out List<Face> sideFaces,
                                              out Dictionary<ulong, Vertex> newSideExternalVertices,
                                              out Dictionary<ulong, Vertex> newSideInternalVertices,
                                              false))
                        {
                            continue;
                        }
                    }
                }
            }

            for (var i = 0; i < internalVertexGroups.Count - 1; i++)
            {
                var bottom = internalVertexGroups[i];
                var top = internalVertexGroups[i + 1];
                var bottomEdges = bottom.SelectMany(v => v.GetEdges()).Distinct().ToList();
                var topEdges = top.SelectMany(v => v.GetEdges()).Distinct().ToList();
                foreach (var internalEdge in bottomEdges)
                {
                    var a = this.GetVertex(internalEdge.StartVertexId).Value;
                    var b = this.GetVertex(internalEdge.EndVertexId).Value;
                    Polygon p = null;

                    // Find the top edge that matches in direction.
                    foreach (var topEdge in topEdges)
                    {
                        var c = this.GetVertex(topEdge.StartVertexId).Value;
                        var d = this.GetVertex(topEdge.EndVertexId).Value;

                        if (c.X.ApproximatelyEquals(a.X)
                            && c.Y.ApproximatelyEquals(a.Y)
                            && d.X.ApproximatelyEquals(b.X)
                            && d.Y.ApproximatelyEquals(b.Y))
                        {
                            // forward
                            p = new Polygon(a, b, d, c);
                            break;
                        }
                        else if (c.X.ApproximatelyEquals(b.X)
                            && c.Y.ApproximatelyEquals(b.Y)
                            && d.X.ApproximatelyEquals(a.X)
                            && d.Y.ApproximatelyEquals(a.Y))
                        {
                            //backward
                            p = new Polygon(a, b, c, d);
                            break;
                        }
                    }
                    if (p != null)
                    {
                        this.AddFace(p);
                    }
                }
            }

            for (var i = 0; i < faceGroups.Count; i++)
            {
                var faceSet = faceGroups[i];
                var bottom = faceSet[0];
                var sideFaces = bottom.GetEdges().SelectMany(e => e.GetFaces().Where(f => f.GetNormal().IsHorizontal()));
                this.AddCell(this._cellId, faceSet.Concat(sideFaces).ToList(), null, null, out Cell newCell);
                cells.Add(newCell);
            }

            if (cells.Any())
            {
                this._cells.Remove(cell.Id);
            }

            return true;
        }

        private void RemoveEdgeFromLookup(Edge edge)
        {
            var existingHash = Edge.GetHash(new List<ulong> { edge.StartVertexId, edge.EndVertexId });
            this._edgesLookup.Remove(existingHash);
            this._edges.Remove(edge.Id);
        }

        private void AddEdgeToLookup(Edge edge)
        {
            var newHash = Edge.GetHash(new List<ulong> { edge.StartVertexId, edge.EndVertexId });
            this._edgesLookup[newHash] = edge.Id;
            this._edges.Add(edge.Id, edge);
        }

        internal bool HasDuplicateEdges()
        {
            var edges = this.GetEdges();
            foreach (var e in edges)
            {
                foreach (var e1 in edges)
                {
                    if (e == e1)
                    {
                        continue;
                    }

                    if (e1.StartVertexId == e.StartVertexId && e1.EndVertexId == e.EndVertexId)
                    {
                        Console.WriteLine($"Found duplicates {e.Id} and {e1.Id}");
                        return true;
                    }

                    if (e1.StartVertexId == e.EndVertexId && e1.EndVertexId == e.StartVertexId)
                    {
                        Console.WriteLine($"Found duplicates {e.Id} and {e1.Id}");
                        return true;
                    }
                }
            }
            return false;
        }

        /// <summary>
        /// A string representation of the structure of the cell complex.
        /// </summary>
        public override string ToString()
        {
            var sb = new StringBuilder();
            foreach (var c in this.GetCells())
            {
                var faces = c.GetFaces();
                sb.AppendLine($"Cell: {c.Id} ({faces.Count} faces)");
                foreach (var f in faces)
                {
                    sb.AppendLine($"\tFace: {f.Id}");
                    foreach (var e in f.GetEdges())
                    {
                        sb.AppendLine($"\t\tEdge: {e.Id},  Vertices: {e.StartVertexId} -> {e.EndVertexId}");
                    }
                }
            }
            return sb.ToString();
        }

    }
}