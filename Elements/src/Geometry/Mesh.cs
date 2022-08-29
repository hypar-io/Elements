using Elements.Search;
using Elements.Serialization.JSON;
using LibTessDotNet.Double;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using static Elements.Units;

namespace Elements.Geometry
{
    /// <summary>
    /// A triangle mesh.
    /// </summary>
    [JsonConverter(typeof(MeshConverter))]
    public partial class Mesh
    {

        private double _maxTriangleSize = 0;
        private PointOctree<Vertex> _octree = null;
        private readonly Dictionary<int, Vertex> _vertexMap = new Dictionary<int, Vertex>();

        /// <summary>The mesh's vertices.</summary>
        [JsonProperty("Vertices", Required = Required.DisallowNull, NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore)]
        public IList<Vertex> Vertices { get; set; }

        /// <summary>The mesh's triangles.</summary>
        [JsonProperty("Triangles", Required = Required.DisallowNull, NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore)]
        public IList<Triangle> Triangles { get; set; }

        private BBox3 _bbox = new BBox3(new List<Vector3> { });

        /// <summary>
        /// The mesh's bounding box.
        /// </summary>
        public BBox3 BoundingBox => _bbox;

        /// <summary>
        /// Construct a mesh.
        /// </summary>
        /// <param name="vertices">The vertices of the mesh.</param>
        /// <param name="triangles">The triangles of the mesh.</param>
        [JsonConstructor]
        public Mesh(IList<Vertex> @vertices, IList<Triangle> @triangles)
        {
            Vertices = new List<Vertex>();
            Triangles = new List<Triangle>();
            foreach (var v in @vertices)
            {
                AddVertex(v);
            }
            foreach (var t in @triangles)
            {
                AddTriangle(t);
            }
        }

        /// <summary>
        /// Create a new mesh from another mesh by copying vertices and triangles.
        /// </summary>
        public Mesh(Mesh mesh)
        {
            Vertices = new List<Vertex>();
            Triangles = new List<Triangle>();
            foreach (var v in mesh.Vertices)
            {
                AddVertex(new Vertex(v.Position, v.Normal, v.Color, v.Index, v.UV));
            }
            foreach (var triangle in mesh.Triangles)
            {
                var triangleVertices = triangle.Vertices.Select(v => Vertices[v.Index]).ToList();
                AddTriangle(new Triangle(triangleVertices, triangle.Normal));
            }
        }

        /// <summary>
        /// Construct an empty mesh.
        /// </summary>
        public Mesh()
        {
            // An empty mesh.
            this.Vertices = new List<Vertex>();
            this.Triangles = new List<Triangle>();
        }

        /// <summary>
        /// Construct a mesh from an STL file.
        /// </summary>
        /// <param name="stlPath">The path to the STL file.</param>
        /// <param name="unit">The length unit used in the file.</param>
        /// <returns></returns>
        public static Mesh FromSTL(string stlPath, LengthUnit unit = LengthUnit.Millimeter)
        {
            List<Vertex> vertexCache = new List<Vertex>();
            var mesh = new Mesh();

            var conversion = Units.GetConversionToMeters(unit);

            using (var reader = new StreamReader(stlPath))
            {
                string line;
                while ((line = reader.ReadLine()) != null)
                {
                    line = line.TrimStart();

                    if (line.StartsWith("facet"))
                    {
                        vertexCache.Clear();
                    }

                    if (line.StartsWith("vertex"))
                    {
                        var splits = line.Split(' ');
                        var x = double.Parse(splits[1]) * conversion;
                        var y = double.Parse(splits[2]) * conversion;
                        var z = double.Parse(splits[3]) * conversion;
                        var v = new Vertex(new Vector3(x, y, z));
                        mesh.AddVertex(v);
                        vertexCache.Add(v);
                    }

                    if (line.StartsWith("endfacet"))
                    {
                        var t = new Triangle(vertexCache[0], vertexCache[1], vertexCache[2]);
                        if (!t.HasDuplicatedVertices(out _))
                        {
                            mesh.AddTriangle(t);
                        }
                    }
                }
            }
            mesh.ComputeNormals();
            return mesh;
        }

        /// <summary>
        /// Get a string representation of the mesh.
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return $@"
Vertices:{Vertices.Count},
Triangles:{Triangles.Count}";
        }

        /// <summary>
        /// Compute the vertex normals by averaging
        /// the normals of the incident faces.
        /// </summary>
        public void ComputeNormals()
        {
            foreach (var v in this.Vertices)
            {
                if (v.Triangles.Count == 0)
                {
                    v.Normal = default(Vector3);
                    continue;
                }
                var avg = new Vector3();
                foreach (var t in v.Triangles)
                {
                    avg += t.Normal;
                }
                v.Normal = (avg / v.Triangles.Count).Unitized();
            }
        }

        /// <summary>
        /// Remaps the mesh's UVs to the bounds. For surfaces close to
        /// horizontal, this has the effect of a planar projection.
        /// </summary>
        internal void MapUVsToBounds()
        {
            for (var i = 0; i < this.Vertices.Count; i++)
            {
                var v = this.Vertices[i];
                var uv1 = _bbox.UVWAtPoint(v.Position);
                v.UV = new UV(uv1.X, uv1.Y);
            }
        }

        /// <summary>
        /// Get all buffers required for rendering.
        /// </summary>
        public GraphicsBuffers GetBuffers()
        {
            var buffers = new GraphicsBuffers();

            for (var i = 0; i < this.Vertices.Count; i++)
            {
                var v = this.Vertices[i];
                buffers.AddVertex(v.Position, v.Normal, v.UV, v.Color);
            }

            for (var i = 0; i < this.Triangles.Count; i++)
            {
                var t = this.Triangles[i];
                buffers.AddIndex((ushort)t.Vertices[0].Index);
                buffers.AddIndex((ushort)t.Vertices[1].Index);
                buffers.AddIndex((ushort)t.Vertices[2].Index);
            }

            return buffers;
        }

        /// <summary>
        /// Add a triangle to the mesh.
        /// </summary>
        /// <param name="a">The first vertex.</param>
        /// <param name="b">The second vertex.</param>
        /// <param name="c">The third vertex.</param>
        public Triangle AddTriangle(Vertex a, Vertex b, Vertex c)
        {
            var t = new Triangle(a, b, c);
            if (!Validators.Validator.DisableValidationOnConstruction && t.HasDuplicatedVertices(out Vector3 duplicate))
            {
                throw new ArgumentException($"Not a valid Triangle.  Duplicate vertex at {duplicate}.");
            }
            for (int i = 0; i < 3; i++)
            {
                var sideLength = t.Vertices[i].Position.DistanceTo(t.Vertices[(i + 1) % 3].Position);
                if (sideLength > this._maxTriangleSize)
                {
                    this._maxTriangleSize = sideLength;
                }
            }
            this.Triangles.Add(t);
            return t;
        }

        /// <summary>
        /// Add a triangle to the mesh.
        /// </summary>
        /// <param name="t">The triangle to add.</param>
        public Triangle AddTriangle(Triangle t)
        {
            if (t.HasDuplicatedVertices(out Vector3 duplicate))
            {
                throw new ArgumentException($"Not a valid Triangle.  Duplicate vertex at {duplicate}.");
            }
            for (int i = 0; i < 3; i++)
            {
                var sideLength = t.Vertices[i].Position.DistanceTo(t.Vertices[(i + 1) % 3].Position);
                if (sideLength > this._maxTriangleSize)
                {
                    this._maxTriangleSize = sideLength;
                }
            }
            this.Triangles.Add(t);
            return t;
        }

        /// <summary>
        /// Add a vertex to the mesh.
        /// </summary>
        /// <param name="position">The position of the vertex.</param>
        /// <param name="normal">The vertex's normal.</param>
        /// <param name="color">The vertex's color.</param>
        /// <param name="merge">If true, and a vertex already exists with a 
        /// position within Vector3.EPSILON, that vertex will be returned.</param>
        /// <param name="edgeAngle">If merge is true, vertices will be 
        /// merged if the angle between them is less than edgeAngle.</param>
        /// <param name="uv">The texture coordinate of the vertex.</param>
        /// <returns>The newly created vertex.</returns>
        public Vertex AddVertex(Vector3 position,
                                UV uv = default(UV),
                                Vector3 normal = default(Vector3),
                                Color color = default(Color),
                                bool merge = false,
                                double edgeAngle = 30.0)
        {
            var v = new Vertex(position, normal, color);

            if (merge)
            {
                var search = GetOctree().GetNearby(position, Vector3.EPSILON);
                if (search.Length > 0)
                {
                    var angle = search[0].Normal.AngleTo(normal);
                    if (angle < edgeAngle)
                    {
                        return search[0];
                    }
                }
            }
            // If the octree is null, do nothing — we'll build it when we need it. If we've already constructed it, let's keep it up to date.
            this._octree?.Add(v, position);

            v.UV = uv;
            this.Vertices.Add(v);
            v.Index = (this.Vertices.Count) - 1;
            this._bbox.Extend(v.Position);
            return v;
        }

        /// <summary>
        /// Find a vertex using the Tag property, or create a new vertex.
        /// </summary>
        /// <param name="position">The vertex's position.</param>
        /// <param name="tag">The vertex's tag.</param>
        /// <param name="uv">The vertex's texture coordinate.</param>
        /// <param name="normal">The vertex's normal.</param>
        /// <param name="color">The vertex's color.</param>
        /// <returns>An existing vertex if a match is found, otherwise
        /// a new vertex.</returns>
        internal Vertex FindOrCreateVertex(Vector3 position,
                                int tag,
                                UV uv = default,
                                Vector3 normal = default,
                                Color color = default)
        {
            if (_vertexMap.ContainsKey(tag))
            {
                return _vertexMap[tag];
            }

            var v = new Vertex(position, normal, color)
            {
                UV = uv,
                Tag = tag
            };
            Vertices.Add(v);
            v.Index = Vertices.Count - 1;
            this._octree?.Add(v, position);
            _vertexMap.Add(tag, v);
            this._bbox.Extend(v.Position);
            return v;
        }

        /// <summary>
        /// Add a vertex to the mesh.
        /// </summary>
        /// <param name="v">The vertex to add.</param>
        public Vertex AddVertex(Vertex v)
        {
            this.Vertices.Add(v);
            // If the octree is null, do nothing — we'll build it when we need it. If we've already constructed it, let's keep it up to date.
            this._octree?.Add(v, v.Position);
            this._bbox.Extend(v.Position);
            v.Index = (this.Vertices.Count) - 1;
            return v;
        }

        private PointOctree<Vertex> GetOctree()
        {
            if (_octree == null)
            {
                _octree = new PointOctree<Vertex>(Math.Max(_bbox.Max.DistanceTo(_bbox.Min), 100), _bbox.PointAt(0.5, 0.5, 0.5), Vector3.EPSILON);
                // make sure existing vertices are added to the octree — we're initializing it for the first time
                foreach (var v in Vertices)
                {
                    _octree.Add(v, v.Position);
                }
            }
            return _octree;
        }

        /// <summary>
        /// Calculate the volume of the mesh.
        /// This value will be inexact for open meshes.
        /// </summary>
        /// <returns>The volume of the mesh.</returns>
        public double Volume()
        {
            return Math.Abs(this.Triangles.Sum(t => SignedVolumeOfTriangle(t)));
        }

        internal void AddMesh(Mesh mesh)
        {
            for (var i = 0; i < mesh.Vertices.Count; i++)
            {
                AddVertex(mesh.Vertices[i]);
            }

            for (var i = 0; i < mesh.Triangles.Count; i++)
            {
                AddTriangle(mesh.Triangles[i]);
            }
        }

        private bool IsInvalid(Vertex v)
        {
            if (v.Position.IsNaN())
            {
                return true;
            }

            if (v.Normal.IsNaN())
            {
                return true;
            }

            return false;
        }

        /// <summary>
        /// Get the open edges of this mesh.
        /// </summary>
        public List<Line> GetNakedEdges()
        {
            var edges = new List<Line>();
            foreach (var t in this.Triangles)
            {
                for (var j = 0; j < 3; j++)
                {
                    var v = t.Vertices[j];
                    var v2 = t.Vertices[(j + 1) % 3];
                    var edgeTriangles = v.Triangles.Intersect(v2.Triangles);
                    if (edgeTriangles.Count() == 1)
                    {
                        edges.Add(new Line(v.Position, v2.Position));
                    }
                }
            }
            return edges;
        }

        /// <summary>
        /// Get the naked edges of this mesh as polylines
        /// </summary>
        /// <returns></returns>
        public List<Polyline> GetNakedBoundaries()
        {
            var lines = this.GetNakedEdges();
            var heg = Elements.Spatial.HalfEdgeGraph2d.Construct(lines);
            var polygons = heg.Polygonize(null, (points) =>
            {
                var polyline = new Polyline(points);
                // we have to add in the starting vertex to make this a closed polyline.
                polyline.Vertices.Add(points[0]);
                return polyline;
            });
            return polygons;
        }

        /// <summary>
        /// Does the provided ray intersect this mesh mesh?
        /// </summary>
        /// <param name="ray">The Ray to intersect.</param>
        /// <param name="intersection">The location of intersection.</param>
        /// <returns>True if an intersection result occurs.
        /// False if no intersection occurs.</returns>
        public bool Intersects(Ray ray, out Vector3 intersection)
        {
            var nearbyVertices = GetOctree().GetNearby(ray, _maxTriangleSize).ToList();
            var nearbyTriangles = nearbyVertices.SelectMany(v => v.Triangles).Distinct();
            intersection = default;
            foreach (var t in nearbyTriangles)
            {
                if (ray.Intersects(t, out intersection))
                {
                    return true;
                }
            }
            return false;
        }

        private double SignedVolumeOfTriangle(Triangle t)
        {
            var p1 = t.Vertices[0].Position;
            var p2 = t.Vertices[1].Position;
            var p3 = t.Vertices[2].Position;

            var v321 = p3.X * p2.Y * p1.Z;
            var v231 = p2.X * p3.Y * p1.Z;
            var v312 = p3.X * p1.Y * p2.Z;
            var v132 = p1.X * p3.Y * p2.Z;
            var v213 = p2.X * p1.Y * p3.Z;
            var v123 = p1.X * p2.Y * p3.Z;
            return (1.0 / 6.0) * (-v321 + v231 + v312 - v132 - v213 + v123);
        }
    }

    internal static class TessExtensions
    {
        internal static Vector3 ToVector3(this Vec3 v)
        {
            return new Vector3(v.X, v.Y, v.Z);
        }

        internal static Mesh ToMesh(this Tess tess,
                                    Transform transform = null,
                                    Color color = default,
                                    Vector3 normal = default)
        {
            var faceMesh = new Mesh();
            (Vector3 U, Vector3 V) basis = (default(Vector3), default(Vector3));

            for (var i = 0; i < tess.ElementCount; i++)
            {
                var a = tess.Vertices[tess.Elements[i * 3]].Position.ToVector3();
                var b = tess.Vertices[tess.Elements[i * 3 + 1]].Position.ToVector3();
                var c = tess.Vertices[tess.Elements[i * 3 + 2]].Position.ToVector3();

                if (transform != null)
                {
                    a = transform.OfPoint(a);
                    b = transform.OfPoint(b);
                    c = transform.OfPoint(c);
                }

                if (i == 0)
                {
                    // Calculate the texture space basis vectors
                    // from the first triangle. This is acceptable
                    // for planar faces.
                    // TODO: Update this when we support non-planar faces.
                    // https://gamedev.stackexchange.com/questions/172352/finding-texture-coordinates-for-plane
                    basis = Tessellation.Tessellation.ComputeBasisAndNormalForTriangle(a, b, c, out Vector3 naturalNormal);
                    if (normal == default)
                    {
                        normal = naturalNormal;
                    }
                }

                var v1 = faceMesh.AddVertex(a, new UV(basis.U.Dot(a), basis.V.Dot(a)), normal, color: color);
                var v2 = faceMesh.AddVertex(b, new UV(basis.U.Dot(b), basis.V.Dot(b)), normal, color: color);
                var v3 = faceMesh.AddVertex(c, new UV(basis.U.Dot(c), basis.V.Dot(c)), normal, color: color);

                faceMesh.AddTriangle(v1, v2, v3);
            }

            return faceMesh;
        }

    }
}