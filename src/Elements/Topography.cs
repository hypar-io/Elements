using System;
using System.Collections.Generic;
using Elements.Geometry;
using Elements.Geometry.Interfaces;
using Elements.Geometry.Solids;
using Elements.Interfaces;
using LibTessDotNet.Double;
using Newtonsoft.Json;

namespace Elements
{
    /// <summary>
    /// A topographic mesh defined by an array of elevation values.
    /// </summary>
    /// <example>
    /// [!code-csharp[Main](../../test/Examples/TopographyExample.cs?name=example)]
    /// </example>
    public class Topography : Element, ITessellate, IMaterial
    {
        private Func<Triangle, Color> _colorizer;

        private Mesh _mesh = new Mesh();

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
        /// The material of the topography.
        /// </summary>
        public Material Material { get; private set;}

        /// <summary>
        /// The material id of the topography.
        /// </summary>
        public Guid MaterialId { get; private set; }

        /// <summary>
        /// Create a topography.
        /// </summary>
        /// <param name="origin">The origin of the topography.</param>
        /// <param name="cellWidth">The width of each square "cell" of the topography.</param>
        /// <param name="cellHeight">The height of each square "cell" of the topography.</param>
        /// <param name="elevations">An array of elevation samples which will be converted to a square array of width.</param>
        /// <param name="width"></param>
        /// <param name="colorizer">A function which produces a color for a triangle.</param>
        public Topography(Vector3 origin, double cellWidth, double cellHeight, double[] elevations, int width, Func<Triangle, Color> colorizer)
        {
            // Elevations a represented by *
            // *-*-*-*
            // |/|/|/|
            // *-*-*-*
            // |/|/|/|
            // *-*-*-*

            if (elevations.Length % (width + 1) != 0)
            {
                throw new ArgumentException($"The topography could not be created. The length of the elevations array, {elevations.Length}, must be equally divisible by the width plus one, {width}.");
            }
            if(colorizer == null)
            {
                throw new ArgumentNullException("The topography could not be created. You must supply a colorizer function.");
            }
            
            this.Material = BuiltInMaterials.Topography;
            this._colorizer = colorizer;

            var triangles = (Math.Sqrt(elevations.Length) - 1) * width * 2;

            var x = 0;
            var y = 0;
            for (var i = 0; i < elevations.Length; i++)
            {
                var el = elevations[i];
                this._mesh.AddVertex(origin + new Vector3(x * cellWidth, y * cellHeight, el));
                _minElevation = Math.Min(_minElevation, el);
                _maxElevation = Math.Max(_maxElevation, el);

                if (x == width)
                {
                    x = 0;
                    y++;
                }
                else
                {
                    x++;
                }
            }

            x = 0;
            y = 0;
            for (var i = 0; i < elevations.Length; i++)
            {
                var v = this._mesh.Vertices[i];
                if (x == width)
                {
                    x = 0;
                    y++;
                }
                else
                {
                    if (y > 0)
                    {
                        // Top triangle
                        var a = this._mesh.Vertices[i];
                        var b = this._mesh.Vertices[i - width];
                        var c = this._mesh.Vertices[i - (width + 1)];
                        this._mesh.AddTriangle(c,b,a);
                        
                        // Bottom triangle
                        var d = this._mesh.Vertices[i];
                        var e = this._mesh.Vertices[i + 1];
                        var f = this._mesh.Vertices[i - width];
                        this._mesh.AddTriangle(f, e, d);
                    }
                    x++;
                }
            }
        }
        
        /// <summary>
        /// Tessellate the topography.
        /// </summary>
        /// <param name="mesh">The mesh into which the topography's facets will be added.</param>
        public void Tessellate(ref Mesh mesh)
        {
            foreach(var t in this._mesh.Triangles)
            {
                var c = this._colorizer(t);
                t.Vertices[0].Color = c;
                t.Vertices[1].Color = c;
                t.Vertices[2].Color = c;
            }

            mesh.AddMesh(this._mesh);
        }

        [JsonConstructor]
        internal Topography(List<Elements.Geometry.Vertex> vertices, List<Triangle> triangles){}

        /// <summary>
        /// Subtract the provided mass from this topography.
        /// </summary>
        /// <param name="mass">The mass to subtract.</param>
        internal void Subtract(Mass mass)
        {
            Subtract(mass.Geometry, mass.Transform);
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
            for(var i=this._mesh.Triangles.Count - 1; i>=0; i--)
            {
                var t = this._mesh.Triangles[i];
                var xsect = GetIntersectionType(t, solid, transform);
                if(xsect == IntersectionType.Intersect)
                {
                    intersects.Add(t);
                    this._mesh.Triangles.RemoveAt(i);
                }
                else if(xsect == IntersectionType.Inside)
                {
                    this._mesh.Triangles.RemoveAt(i);
                }
            }

            foreach(var t in intersects)
            {
                var input = new List<Vector3>();
                foreach(var v in t.Vertices)
                {
                    input.Add(v.Position);
                }

                foreach(var f in solid.Faces.Values)
                {
                    var fp = f.Plane();
                    if(transform != null)
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
                        this._mesh.AddTriangle(a,b,c);
                    }
                }
                catch(ArgumentOutOfRangeException ex)
                {
                    Console.WriteLine(ex.Message);
                }
            }

            for(var i=this._mesh.Triangles.Count - 1; i>=0; i--)
            {
                var t = this._mesh.Triangles[i];
                var area = t.Area();
                if(area == 0.0 || 
                    double.IsNaN(area) || 
                    area < Vector3.Tolerance ||
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
            foreach(var vx in this._mesh.Vertices)
            {
                if(vx.Position.IsAlmostEqualTo(v))
                {
                    return vx;
                }
            }
            var newVtx = this._mesh.AddVertex(v);
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
                if(trans != null)
                {
                    p = trans.OfPlane(p);
                }
                var d1 = p.Normal.Dot(t.Vertices[0].Position - p.Origin);
                var d2 = p.Normal.Dot(t.Vertices[1].Position - p.Origin);
                var d3 = p.Normal.Dot(t.Vertices[2].Position - p.Origin);

                if(d1 < 0 && d2 < 0 && d3 < 0)
                {
                    inside++;
                }
                else if(d1 > 0 && d2 > 0 && d3 > 0)
                {
                    outside++;
                }
                else
                {
                    xsect++;
                }
            }
            
            if(outside == fCount)
            {
                return IntersectionType.Outside;
            }
            else if(inside == fCount)
            {
                return IntersectionType.Inside;
            }
            else if(inside + xsect == fCount)
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

            for(var j=0; j<input.Count; j++)
            {
                // edge of the triangle
                var start = input[j];
                var end = input[j == input.Count - 1 ? 0 : j+1];
                var d1 = p.Normal.Dot(start - p.Origin);
                var d2 = p.Normal.Dot(end - p.Origin);

                if(d1 < 0 && d2 < 0)
                {
                    //both inside
                    output.Add(start);
                }
                else if(d1 < 0 && d2 > 0)
                {
                    //start inside
                    //end outside
                    //add intersection
                    var xsect = new Line(start, end).Intersect(p);
                    output.Add(start);
                    output.Add(xsect);
                }
                else if(d1 > 0 && d2 > 0)
                {
                    //both outside
                }
                else if(d1 > 0 && d2 < 0)
                {
                    //start outside
                    //end inside
                    //add intersection
                    var xsect =new Line(start, end).Intersect(p);
                    output.Add(xsect);
                }
            }

            return output;
        }

        /// <summary>
        /// Set the material;
        /// </summary>
        public void SetReference(Material obj)
        {
            this.Material = obj;
            this.MaterialId = obj.Id;
        }
    }

    internal static class TopographyExtensions
    {
        internal static ContourVertex[] ToContourVertexArray(this List<Vector3> pts)
        {
            var contour = new ContourVertex[pts.Count];
            for(var i=0; i < contour.Length; i++)
            {
                var v = pts[i];
                contour[i] = new ContourVertex();
                contour[i].Position = new Vec3 { X = v.X, Y = v.Y, Z = v.Z };
            }
            return contour;
        }
    }
}