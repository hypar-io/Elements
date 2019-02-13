using System;
using System.Collections;
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
    /// A mesh triangle.
    /// </summary>
    public class Triangle
    {
        /// <summary>
        /// The triangle's vertices.
        /// </summary>
        [JsonProperty("vertices")]
        public Vertex[] Vertices{get;}

        /// <summary>
        /// The triangle's normal.
        /// </summary>
        public Vector3 Normal{get;}

        /// <summary>
        /// Create a triangle.
        /// </summary>
        /// <param name="a">The index of the first vertex of the triangle.</param>
        /// <param name="b">The index of the second vertex of the triangle.</param>
        /// <param name="c">The index of the third vertex of the triangle.</param>
        public Triangle(Vertex a, Vertex b, Vertex c)
        {
            this.Vertices = new[]{a,b,c};

            // Bend the normals for the associated vertices.
            var p1 = new Plane(a.Position, b.Position, c.Position);
            a.Normal = ((a.Normal + p1.Normal) / 2.0).Normalized();
            b.Normal = ((b.Normal + p1.Normal) / 2.0).Normalized();
            c.Normal = ((c.Normal + p1.Normal) / 2.0).Normalized();
            
            this.Normal = p1.Normal;
        }

        [JsonConstructor]
        internal Triangle(Vertex[] vertices)
        {
            this.Vertices = vertices;

            var a = this.Vertices[0];
            var b = this.Vertices[1];
            var c = this.Vertices[2];

            // Bend the normals for the associated vertices.
            var p1 = new Plane(a.Position, b.Position, c.Position);
            a.Normal = ((a.Normal + p1.Normal) / 2.0).Normalized();
            b.Normal = ((b.Normal + p1.Normal) / 2.0).Normalized();
            c.Normal = ((c.Normal + p1.Normal) / 2.0).Normalized();
            
            this.Normal = p1.Normal;
        }

        /// <summary>
        /// The area of the triangle.
        /// </summary>
        public double Area()
        {
            var a = this.Vertices[0].Position;
            var b = this.Vertices[1].Position;
            var c = this.Vertices[2].Position;

            // Heron's formula
            var l1 = a.DistanceTo(b);
            var l2 = b.DistanceTo(c);
            var l3 = c.DistanceTo(a);

            var s = (l1 + l2 + l3)/2;
            return Math.Sqrt(s*(s-l1)*(s-l2)*(s-l3));
        }
        internal Polygon ToPolygon()
        {
            return new Polygon(new[]{this.Vertices[0].Position, this.Vertices[1].Position, this.Vertices[2].Position});
        }

        internal ContourVertex[] ToContourVertexArray()
        {
            var contour = new ContourVertex[this.Vertices.Length];
            for(var i=0; i < this.Vertices.Length; i++)
            {
                var v = this.Vertices[i];
                contour[i] = new ContourVertex();
                contour[i].Position = new Vec3 { X = v.Position.X, Y = v.Position.Y, Z = v.Position.Z };
            }
            return contour;
        }
    }

    /// <summary>
    /// A mesh vertex.
    /// </summary>
    public class Vertex
    {
        /// <summary>
        /// The position of the vertex.
        /// </summary>
        [JsonProperty("position")]
        public Vector3 Position { get; }

        /// <summary>
        /// The vertex's normal.
        /// </summary>
        [JsonIgnore]
        public Vector3 Normal { get; set; }

        /// <summary>
        /// Create a vertex.
        /// </summary>
        /// <param name="position">The position of the vertex.</param>
        public Vertex(Vector3 position)
        {
            this.Position = position;
            this.Normal = Vector3.Origin;
        }
    }

    /// <summary>
    /// A topographic mesh defined by an array of elevation values.
    /// </summary>
    public class Topography : Element, ITessellate
    {
        private List<Vertex> _vertices;
        private List<Triangle> _triangles;
        private Func<Triangle, Vector3, Color> _colorizer;

        private double _minElevation = double.PositiveInfinity;

        private double _maxElevation = double.NegativeInfinity;

        /// <summary>
        /// The maximum elevation of the topography.
        /// </summary>
        [JsonProperty("max_elevation")]
        public double MaxElevation => _maxElevation;

        /// <summary>
        /// The minimum elevation of the topography.
        /// </summary>
        [JsonProperty("min_elevation")]
        public double MinElevation => _minElevation;

        /// <summary>
        /// The topography's vertices.
        /// </summary>
        [JsonProperty("vertices")]
        public List<Vertex> Vertices => _vertices;

        /// <summary>
        /// The topography's triangles.
        /// </summary>
        [JsonProperty("triangles")]
        public List<Triangle> Triangles => _triangles;

        /// <summary>
        /// The material of the topography.
        /// </summary>
        [JsonProperty("material")]
        public Material Material { get; }

        /// <summary>
        /// Create a topography.
        /// </summary>
        /// <param name="origin">The origin of the topography.</param>
        /// <param name="cellWidth">The width of each square "cell" of the topography.</param>
        /// <param name="cellHeight">The height of each square "cell" of the topography.</param>
        /// <param name="elevations">An array of elevation samples which will be converted to a square array of width.</param>
        /// <param name="width"></param>
        /// <param name="colorizer">A function which uses the normal of a facet to determine a color for that facet.</param>
        public Topography(Vector3 origin, double cellWidth, double cellHeight, double[] elevations, int width, Func<Triangle, Vector3, Color> colorizer)
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

            this._vertices = new List<Vertex>(elevations.Length);
            var triangles = (Math.Sqrt(elevations.Length) - 1) * width * 2;
            this._triangles = new List<Triangle>((int)triangles);

            var x = 0;
            var y = 0;
            for (var i = 0; i < elevations.Length; i++)
            {
                var el = elevations[i];
                this._vertices.Add(new Vertex(origin + new Vector3(x * cellWidth, y * cellHeight, el)));

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
                var v = this._vertices[i];
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
                        var a = this._vertices[i];
                        var b = this._vertices[i - width];
                        var c = this._vertices[i - (width + 1)];
                        this._triangles.Add(new Triangle(c, b, a));
                        
                        // Bottom triangle
                        var d = this._vertices[i];
                        var e = this._vertices[i + 1];
                        var f = this._vertices[i - width];
                        this._triangles.Add(new Triangle(f, e, d));
                    }
                    x++;
                }
            }
        }
        
        [JsonConstructor]
        internal Topography(List<Vertex> vertices, List<Triangle> triangles)
        {
            this._vertices = vertices;
            this._triangles = triangles;
        }

        /// <summary>
        /// Subtract the provided mass from this topography.
        /// </summary>
        /// <param name="mass">The mass to subtract.</param>
        public void Subtract(Mass mass)
        {
            foreach(var g in mass.Geometry)
            {
                Subtract(g, mass.Transform);
            }
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
            for(var i=this._triangles.Count - 1; i>=0; i--)
            {
                var t = this._triangles[i];
                var xsect = GetIntersectionType(t, solid, transform);
                if(xsect == IntersectionType.Intersect)
                {
                    intersects.Add(t);
                    this._triangles.RemoveAt(i);
                }
                else if(xsect == IntersectionType.Inside)
                {
                    this._triangles.RemoveAt(i);
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

                for (var i = 0; i < tess.ElementCount; i++)
                {
                    var a = MergeOrReturnNew(tess.Vertices[tess.Elements[i * 3]].Position.ToVector3());
                    var b = MergeOrReturnNew(tess.Vertices[tess.Elements[i * 3 + 1]].Position.ToVector3());
                    var c = MergeOrReturnNew(tess.Vertices[tess.Elements[i * 3 + 2]].Position.ToVector3());
                    var clipTriangle = new Triangle(a,b,c);
                    this._triangles.Add(clipTriangle);
                }
            }

            for(var i=this._triangles.Count - 1; i>=0; i--)
            {
                var t = this._triangles[i];
                var area = t.Area();
                if(area == 0.0 || 
                    double.IsNaN(area) || 
                    area < Vector3.Tolerance ||
                    t.Normal.IsNaN())
                {
                    Console.WriteLine("Found one.");
                    this._triangles.RemoveAt(i);
                    continue;
                }
            }
        }

        private enum IntersectionType
        {
            Inside, Outside, Intersect, Unknown
        }

        private Vertex MergeOrReturnNew(Vector3 v)
        {
            foreach(var vx in this._vertices)
            {
                if(vx.Position.IsAlmostEqualTo(v))
                {
                    return vx;
                }
            }
            var newVx = new Vertex(v);
            this._vertices.Add(newVx);
            return newVx;
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

        private bool IsInvalid(Vertex v)
        {
            if(v.Position.IsNaN())
            {
                return true;
            }

            if(v.Normal.IsNaN())
            {
                return true;
            }

            return false;
        }
        
        /// <summary>
        /// Tessellate the topography.
        /// </summary>
        /// <param name="mesh">The mesh into which the topography's facets will be added.</param>
        public void Tessellate(ref Mesh mesh)
        {
            for (var i = 0; i < this._triangles.Count; i++)
            {
                var t = this._triangles[i];
                var a = t.Vertices[0];
                var b = t.Vertices[1];
                var c = t.Vertices[2];

                if(IsInvalid(a))
                {
                    Console.WriteLine($"NaN triangle position found at {a.Position}");
                    continue;
                }

                if(IsInvalid(b))
                {
                    Console.WriteLine($"NaN triangle position found at {b.Position}");
                    continue;
                }

                if(IsInvalid(c))
                {
                    Console.WriteLine($"NaN triangle position found at {c.Position}");
                    continue;
                }

                var ac = _colorizer(t, a.Normal);
                var bc = _colorizer(t, b.Normal);
                var cc = _colorizer(t, c.Normal);

                mesh.AddTriangle(a.Position, b.Position, c.Position, a.Normal, b.Normal, c.Normal, ac, bc, cc);
            }
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