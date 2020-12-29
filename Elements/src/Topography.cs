using System;
using System.Collections.Generic;
using System.Linq;
using Elements.Geometry;
using Elements.Geometry.Solids;
using LibTessDotNet.Double;
using Newtonsoft.Json;
using Vertex = Elements.Geometry.Vertex;

namespace Elements
{
    /// <summary>
    /// A topographic mesh defined by an array of elevation values.
    /// </summary>
    /// <example>
    /// [!code-csharp[Main](../../Elements/test/TopographyTests.cs?name=example)]
    /// </example>
    [UserElement]
    public class Topography : GeometricElement
    {
        private double _minElevation = double.PositiveInfinity;

        private double _maxElevation = double.NegativeInfinity;

        /// <summary>
        /// The maximum elevation of the topography.
        /// </summary>
        public double MaxElevation => _maxElevation;

        /// <summary>
        /// The minimum elevation of the topography.
        /// </summary>
        public double MinElevation => _minElevation;

        /// <summary>
        /// A flat list of elevation data which is used to generate the topographic
        /// mesh's vertices. The elevations will be used with the RowWidth property
        /// to convert the flat list into a square grid.
        /// </summary>
        public double[] Elevations { get; }

        /// <summary>
        /// The origin of the topography.
        /// </summary>
        public Vector3 Origin { get; }

        /// <summary>
        /// The number of cells 'across' the topography.
        /// </summary>
        public int RowWidth { get; }

        /// <summary>
        /// The width of a cell.
        /// </summary>
        public double CellWidth { get; }

        /// <summary>
        /// The height of a cell.
        /// </summary>
        public double CellHeight { get; }

        /// <summary>
        /// The topography's mesh.
        /// </summary>
        [JsonIgnore]
        public Mesh Mesh
        {
            get { return this.FirstRepresentationOfType<MeshRepresentation>().Mesh; }
        }

        /// <summary>
        /// Create a topography.
        /// </summary>
        /// <param name="origin">The origin of the topography.</param>
        /// <param name="width">The width the topography. When constructed from a set of elevations, the
        /// width and the length of the topography will be the same.</param>
        /// <param name="elevations">An array of elevation samples which will be converted to a square array of width.</param>
        /// <param name="material">The topography's material.</param>
        /// <param name="transform">The topography's transform.</param>
        /// <param name="id">The topography's id.</param>
        /// <param name="name">The topography's name.</param>
        public Topography(Vector3 origin,
                          double width,
                          double[] elevations,
                          Material material = null,
                          Transform transform = null,
                          Guid id = default(Guid),
                          string name = null) : base(transform != null ? transform : new Transform(),
                                                     new List<Representation>() { new MeshRepresentation(material != null ? material : BuiltInMaterials.Topography) },
                                                     false,
                                                     id == null ? Guid.NewGuid() : id,
                                                     name)
        {
            this.Origin = origin;
            this.Elevations = elevations;
            this.RowWidth = (int)Math.Sqrt(elevations.Length);
            this.CellWidth = width / (this.RowWidth - 1);
            this.CellHeight = this.CellWidth;
            var mesh = GenerateMesh(elevations, origin, this.RowWidth, this.CellWidth, this.CellWidth);
            this._minElevation = mesh.MinElevation;
            this._maxElevation = mesh.MaxElevation;

            var rep = FirstRepresentationOfType<MeshRepresentation>();
            rep.Mesh = mesh.Mesh;
        }

        [JsonConstructor]
        internal Topography(double[] elevations,
                            Vector3 origin,
                            int rowWidth,
                            double cellWidth,
                            double cellHeight,
                            Transform transform,
                            IList<Representation> representations,
                            Guid id,
                            string name) : base(transform,
                                                representations,
                                                false,
                                                id,
                                                name)
        {
            this.Elevations = elevations;
            this.Origin = origin;
            this.RowWidth = rowWidth;
            this.CellWidth = cellWidth;
            this.CellHeight = cellHeight;
            var mesh = GenerateMesh(elevations, origin, rowWidth, cellWidth, cellHeight);
            this._minElevation = mesh.MinElevation;
            this._maxElevation = mesh.MaxElevation;

            var rep = FirstRepresentationOfType<MeshRepresentation>();
            rep.Mesh = mesh.Mesh;
        }

        private static (Mesh Mesh, double MaxElevation, double MinElevation) GenerateMesh(
            double[] elevations,
            Vector3 origin,
            int rowWidth,
            double cellWidth,
            double cellHeight)
        {
            var minElevation = double.MaxValue;
            var maxElevation = double.MinValue;
            var mesh = new Mesh();
            var triangles = (Math.Sqrt(elevations.Length) - 1) * rowWidth * 2;

            for (var y = 0; y < rowWidth; y++)
            {
                for (var x = 0; x < rowWidth; x++)
                {
                    var ei = x + y * rowWidth;
                    var el = elevations[ei];
                    minElevation = Math.Min(minElevation, el);
                    maxElevation = Math.Max(maxElevation, el);

                    var u = (double)x / (double)(rowWidth - 1);
                    var v = (double)y / (double)(rowWidth - 1);

                    // Shrink the UV space slightly to avoid
                    // visible edges on applied textures.
                    var uvTol = 0.001;
                    u = u == 0.0 ? uvTol : u;
                    v = v == 0.0 ? uvTol : v;
                    u = u == 1.0 ? 1 - uvTol : u;
                    v = v == 1.0 ? 1 - uvTol : v;

                    var uv = new UV(u, v);

                    mesh.AddVertex(origin + new Vector3(x * cellWidth, y * cellHeight, el), uv: uv);

                    if (y > 0 && x > 0)
                    {
                        var i = x + y * rowWidth;

                        // Top triangle
                        var a = mesh.Vertices[i];
                        var b = mesh.Vertices[i - 1];
                        var c = mesh.Vertices[i - rowWidth];
                        var tt = mesh.AddTriangle(a, b, c);

                        // Bottom triangle
                        var d = mesh.Vertices[i - 1];
                        var e = mesh.Vertices[i - 1 - rowWidth];
                        var f = mesh.Vertices[i - rowWidth];
                        var tb = mesh.AddTriangle(d, e, f);
                    }
                }
            }

            mesh.ComputeNormals();
            return (mesh, maxElevation, minElevation);
        }

        /// <summary>
        /// Average the vertex placement along the specified edge
        /// of this topography with the vertex placement along the 
        /// corresponding edge of a target topography.
        /// </summary>
        /// <param name="target"></param>
        /// <param name="edgeToAverage"></param>
        public void AverageEdges(Topography target, Units.CardinalDirection edgeToAverage)
        {
            if (this.RowWidth != target.RowWidth)
            {
                throw new ArgumentException("The specified topographies do not have the same number of vertices.");
            }

            Vertex[] e1 = null;
            Vertex[] e2 = null;

            switch (edgeToAverage)
            {
                case Units.CardinalDirection.North:
                    e1 = this.GetEdgeVertices(Units.CardinalDirection.North);
                    e2 = target.GetEdgeVertices(Units.CardinalDirection.South);
                    break;
                case Units.CardinalDirection.South:
                    e1 = this.GetEdgeVertices(Units.CardinalDirection.South);
                    e2 = target.GetEdgeVertices(Units.CardinalDirection.North);
                    break;
                case Units.CardinalDirection.East:
                    e1 = this.GetEdgeVertices(Units.CardinalDirection.East);
                    e2 = target.GetEdgeVertices(Units.CardinalDirection.West);
                    break;
                case Units.CardinalDirection.West:
                    e1 = this.GetEdgeVertices(Units.CardinalDirection.West);
                    e2 = target.GetEdgeVertices(Units.CardinalDirection.East);
                    break;
            }

            for (var i = 0; i < e1.Length; i++)
            {
                var pos = e1[i].Position.Average(e2[i].Position);
                e1[i].Position = pos;
                e2[i].Position = pos;
            }
        }

        /// <summary>
        /// Get the vertices along the specified edge of a square topography.
        /// </summary>
        /// <param name="direction">The edge of vertices to return.</param>
        /// <returns>A collection of vertices.</returns>
        public Vertex[] GetEdgeVertices(Units.CardinalDirection direction)
        {
            var range = Enumerable.Range(0, this.RowWidth);
            var start = 0;
            var increment = 1;
            switch (direction)
            {
                case Units.CardinalDirection.North:
                    start = this.Mesh.Vertices.Count - this.RowWidth;
                    break;
                case Units.CardinalDirection.South:
                    start = 0;
                    break;
                case Units.CardinalDirection.East:
                    start = this.RowWidth - 1;
                    increment = this.RowWidth;
                    break;
                case Units.CardinalDirection.West:
                    start = 0;
                    increment = this.RowWidth;
                    break;
                default:
                    return null;
            }
            return range.Select(i => this.Mesh.Vertices[start + i * increment]).ToArray();
        }

        /// <summary>
        /// Subtract the provided solid from this topography.
        /// </summary>
        /// <param name="solid">The solid to subtract.</param>
        /// <param name="transform">A transform applied to the solid before intersection.</param>
        /// <returns>An array of triangles that have at least one vertex inside the solid.</returns>
        private void Subtract(Solid solid, Transform transform = null)
        {
            var intersects = new List<Triangle>();
            for (var i = this.Mesh.Triangles.Count - 1; i >= 0; i--)
            {
                var t = this.Mesh.Triangles[i];
                var xsect = GetIntersectionType(t, solid, transform);
                if (xsect == IntersectionType.Intersect)
                {
                    intersects.Add(t);
                    this.Mesh.Triangles.RemoveAt(i);
                }
                else if (xsect == IntersectionType.Inside)
                {
                    this.Mesh.Triangles.RemoveAt(i);
                }
            }

            foreach (var t in intersects)
            {
                var input = new List<Vector3>();
                foreach (var v in t.Vertices)
                {
                    input.Add(v.Position);
                }

                foreach (var f in solid.Faces.Values)
                {
                    var fp = f.Plane();
                    if (transform != null)
                    {
                        fp = transform.OfPlane(fp);
                    }
                    input = SutherlandHodgman(input, fp, transform);
                }

                var contour1 = t.ToContourVertexArray();
                input.Reverse();
                var contour2 = input.ToContourVertexArray();

                var tess = new Tess();
                tess.NoEmptyPolygons = true;
                tess.AddContour(contour1);
                tess.AddContour(contour2);
                tess.Tessellate(WindingRule.Positive, LibTessDotNet.Double.ElementType.Polygons, 3);

                try
                {
                    for (var i = 0; i < tess.ElementCount; i++)
                    {
                        var a = MergeOrReturnNew(tess.Vertices[tess.Elements[i * 3]].Position.ToVector3());
                        var b = MergeOrReturnNew(tess.Vertices[tess.Elements[i * 3 + 1]].Position.ToVector3());
                        var c = MergeOrReturnNew(tess.Vertices[tess.Elements[i * 3 + 2]].Position.ToVector3());
                        this.Mesh.AddTriangle(a, b, c);
                    }
                }
                catch (ArgumentOutOfRangeException ex)
                {
                    Console.WriteLine(ex.Message);
                }
            }

            for (var i = this.Mesh.Triangles.Count - 1; i >= 0; i--)
            {
                var t = this.Mesh.Triangles[i];
                var area = t.Area();
                if (area == 0.0 ||
                    double.IsNaN(area) ||
                    area < Vector3.EPSILON ||
                    t.Normal.IsNaN())
                {
                    this.Mesh.Triangles.RemoveAt(i);
                }
            }
        }

        private enum IntersectionType
        {
            Inside, Outside, Intersect, Unknown
        }

        private Vertex MergeOrReturnNew(Vector3 v)
        {
            foreach (var vx in this.Mesh.Vertices)
            {
                if (vx.Position.IsAlmostEqualTo(v))
                {
                    return vx;
                }
            }
            var newVtx = this.Mesh.AddVertex(v, new UV());
            return newVtx;
        }

        private IntersectionType GetIntersectionType(Triangle t, Solid s, Transform trans = null)
        {
            var fCount = s.Faces.Count;
            var inside = 0;
            var outside = 0;
            var xsect = 0;
            foreach (var f in s.Faces.Values)
            {
                var p = f.Plane();
                if (trans != null)
                {
                    p = trans.OfPlane(p);
                }
                var d1 = p.Normal.Dot(t.Vertices[0].Position - p.Origin);
                var d2 = p.Normal.Dot(t.Vertices[1].Position - p.Origin);
                var d3 = p.Normal.Dot(t.Vertices[2].Position - p.Origin);

                if (d1 < 0 && d2 < 0 && d3 < 0)
                {
                    inside++;
                }
                else if (d1 > 0 && d2 > 0 && d3 > 0)
                {
                    outside++;
                }
                else
                {
                    xsect++;
                }
            }

            if (outside == fCount)
            {
                return IntersectionType.Outside;
            }
            else if (inside == fCount)
            {
                return IntersectionType.Inside;
            }
            else if (inside + xsect == fCount)
            {
                return IntersectionType.Intersect;
            }

            return IntersectionType.Unknown;
        }

        private List<Vector3> SutherlandHodgman(List<Vector3> input, Plane p, Transform trans)
        {
            // Implement Sutherland-Hodgman clipping
            // https://www.cs.drexel.edu/~david/Classes/CS430/Lectures/L-05_Polygons.6.pdf
            var output = new List<Vector3>();

            for (var j = 0; j < input.Count; j++)
            {
                // edge of the triangle
                var start = input[j];
                var end = input[j == input.Count - 1 ? 0 : j + 1];
                var d1 = p.Normal.Dot(start - p.Origin);
                var d2 = p.Normal.Dot(end - p.Origin);

                if (d1 < 0 && d2 < 0)
                {
                    //both inside
                    output.Add(start);
                }
                else if (d1 < 0 && d2 > 0)
                {
                    //start inside
                    //end outside
                    //add intersection
                    if (new Line(start, end).Intersects(p, out Vector3 result))
                    {
                        output.Add(start);
                        output.Add(result);
                    }
                }
                else if (d1 > 0 && d2 > 0)
                {
                    //both outside
                }
                else if (d1 > 0 && d2 < 0)
                {
                    //start outside
                    //end inside
                    //add intersection
                    if (new Line(start, end).Intersects(p, out Vector3 result))
                    {
                        output.Add(result);
                    }
                }
            }

            return output;
        }

    }

    internal static class TopographyExtensions
    {
        internal static ContourVertex[] ToContourVertexArray(this List<Vector3> pts)
        {
            var contour = new ContourVertex[pts.Count];
            for (var i = 0; i < contour.Length; i++)
            {
                var v = pts[i];
                contour[i] = new ContourVertex();
                contour[i].Position = new Vec3 { X = v.X, Y = v.Y, Z = v.Z };
            }
            return contour;
        }
    }
}