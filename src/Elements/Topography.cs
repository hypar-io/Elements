using System;
using System.Collections.Generic;
using Elements.Geometry;
using Elements.Geometry.Interfaces;
using Elements.Geometry.Solids;
using LibTessDotNet.Double;
using Newtonsoft.Json;

namespace Elements
{
    /// <summary>
    /// A topographic mesh defined by an array of elevation values.
    /// </summary>
    /// <example>
    /// [!code-csharp[Main](../../test/Elements.Tests/Examples/TopographyExample.cs?name=example)]
    /// </example>
    [UserElement]
    public class Topography : GeometricElement, ITessellate
    {
        private Mesh _mesh;

        private double _minElevation = double.PositiveInfinity;

        private double _maxElevation = double.NegativeInfinity;

        /// <summary>
        /// The topography's mesh.
        /// </summary>
        [JsonIgnore]
        public Mesh Mesh => _mesh;

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
        /// Create a topography.
        /// </summary>
        /// <param name="origin">The origin of the topography.</param>
        /// <param name="width">The width the topography. When constructed from a set of elevations, the
        /// width and the length of the topography will be the same.</param>
        /// <param name="elevations">An array of elevation samples which will be converted to a square array of width.</param>
        /// <param name="material">The topography's material.</param>
        public Topography(Vector3 origin,
                          double width,
                          double[] elevations,
                          Material material = null) : base(new Transform(),
                                                          material != null ? material : BuiltInMaterials.Topography,
                                                          null,
                                                          Guid.NewGuid(),
                                                          null)
        {
            //    0 1 2 3
            // 2  *-*-*-* width = 4
            //    |3|4|5|
            // 1  4-5-6-7 ei = x + y * (width - 1)
            //    |0|1|2|
            // 0  0-1-2-3

            this._mesh = new Mesh();

            this.Origin = origin;
            
            this.Elevations = elevations;
            
            if (Math.Sqrt(elevations.Length) % 2 != 0)
            {
                throw new ArgumentException($"The topography could not be created. The length of the elevations array, {elevations.Length}, must be a square.");
            }

            this.RowWidth = (int)Math.Sqrt(elevations.Length) + 1;
            this.CellWidth = width / (this.RowWidth - 1);
            this.CellHeight = this.CellWidth;

            var triangles = (Math.Sqrt(elevations.Length) - 1) * this.RowWidth * 2;

            for (var y = 0; y < this.RowWidth; y++)
            {
                for ( var x = 0; x < this.RowWidth; x++)
                {
                    var xShift = x == this.RowWidth - 1 ? x - 1 : x;
                    var yShift = y == this.RowWidth - 1 ? y - 1 : y;
                    var ei = xShift +  yShift * (this.RowWidth - 1);
                    var el = this.Elevations[ei];

                    var u = (double)x / (double)(this.RowWidth - 1);
                    var v = (double)y / (double)(this.RowWidth - 1);
                    var uv = new UV(u, v);
                    this._mesh.AddVertex(origin + new Vector3(x * this.CellWidth, y * this.CellHeight, el), uv: uv);
                    _minElevation = Math.Min(_minElevation, el);
                    _maxElevation = Math.Max(_maxElevation, el);

                    if (y > 0 && x > 0)
                    {
                        var i = x + y * this.RowWidth;

                        // Top triangle
                        var a = this._mesh.Vertices[i];
                        var b = this._mesh.Vertices[i - 1];
                        var c = this._mesh.Vertices[i - this.RowWidth];
                        var tt = this._mesh.AddTriangle(a, b, c);

                        // Bottom triangle
                        var d = this._mesh.Vertices[i - 1];
                        var e = this._mesh.Vertices[i - 1 - this.RowWidth];
                        var f = this._mesh.Vertices[i - this.RowWidth];
                        var tb = this._mesh.AddTriangle(d, e, f);
                    }
                }
            }
        }

        /// <summary>
        /// Tessellate the topography.
        /// </summary>
        /// <param name="mesh">The mesh into which the topography's facets will be added.</param>
        public void Tessellate(ref Mesh mesh)
        {
            mesh.AddMesh(this._mesh);
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
            for (var i = this._mesh.Triangles.Count - 1; i >= 0; i--)
            {
                var t = this._mesh.Triangles[i];
                var xsect = GetIntersectionType(t, solid, transform);
                if (xsect == IntersectionType.Intersect)
                {
                    intersects.Add(t);
                    this._mesh.Triangles.RemoveAt(i);
                }
                else if (xsect == IntersectionType.Inside)
                {
                    this._mesh.Triangles.RemoveAt(i);
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
                        this._mesh.AddTriangle(a, b, c);
                    }
                }
                catch (ArgumentOutOfRangeException ex)
                {
                    Console.WriteLine(ex.Message);
                }
            }

            for (var i = this._mesh.Triangles.Count - 1; i >= 0; i--)
            {
                var t = this._mesh.Triangles[i];
                var area = t.Area();
                if (area == 0.0 ||
                    double.IsNaN(area) ||
                    area < Vector3.Epsilon ||
                    t.Normal.IsNaN())
                {
                    this._mesh.Triangles.RemoveAt(i);
                }
            }
        }

        private enum IntersectionType
        {
            Inside, Outside, Intersect, Unknown
        }

        private Elements.Geometry.Vertex MergeOrReturnNew(Vector3 v)
        {
            foreach (var vx in this._mesh.Vertices)
            {
                if (vx.Position.IsAlmostEqualTo(v))
                {
                    return vx;
                }
            }
            var newVtx = this._mesh.AddVertex(v, new UV());
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
                    if(new Line(start, end).Intersects(p, out Vector3 result))
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
                    if(new Line(start, end).Intersects(p, out Vector3 result))
                    {
                        output.Add(result);
                    }
                }
            }

            return output;
        }

        /// <summary>
        /// Update the representations.
        /// </summary>
        public override void UpdateRepresentations()
        {
            return;
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