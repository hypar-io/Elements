using System;
using System.Collections.Generic;
using Elements.Geometry;
using Elements.Geometry.Interfaces;
using Elements.Interfaces;

namespace Elements
{
    internal class Triangle
    {
        public int A { get; }
        public int B { get; }
        public int C { get; }
        public Triangle(int a, int b, int c)
        {
            this.A = a;
            this.B = b;
            this.C = c;
        }
    }

    internal class Vertex
    {
        public Vector3 Point{get;}
        public Vector3 Normal{get;set;}

        public Vertex(Vector3 point)
        {
            this.Point = point;
            this.Normal = Vector3.Origin;
        }
    }

    /// <summary>
    /// A topographic mesh defined by an array of elevation values.
    /// </summary>
    public class Topography : Element, ITessellate
    {
        private Vertex[] _vertices;
        private Triangle[] _triangles;
        private Func<Vector3, Color> _colorizer;

        /// <summary>
        /// The material of the topography.
        /// </summary>
        public Material Material{get;}

        /// <summary>
        /// Create a topography.
        /// </summary>
        /// <param name="origin">The origin of the topography.</param>
        /// <param name="cellWidth">The width of each square "cell" of the topography.</param>
        /// <param name="cellHeight">The height of each square "cell" of the topography.</param>
        /// <param name="elevations">An array of elevation samples which will be converted to a square array of width.</param>
        /// <param name="width"></param>
        /// <param name="colorizer">A function which uses the normal of a facet to determine a color for that facet.</param>
        public Topography(Vector3 origin, double cellWidth, double cellHeight, double[] elevations, int width, Func<Vector3,Color> colorizer)
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
            this.Material = BuiltInMaterials.Topography;
            this._colorizer = colorizer;

            this._vertices = new Vertex[elevations.Length];
            var triangles = (Math.Sqrt(elevations.Length) - 1) * width * 2;
            this._triangles = new Triangle[(int)triangles];

            var x = 0;
            var y = 0;
            var t = 0;
            for (var i = 0; i < elevations.Length; i++)
            {
                this._vertices[i] = new Vertex(origin + new Vector3(x * cellWidth, y * cellHeight, elevations[i]));
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
                        var a = i;
                        var b = i - width;
                        var c = i - (width + 1);
                        this._triangles[t] = new Triangle(c, b, a);
                        t++;

                        // Bottom triangle
                        var d = i;
                        var e = i + 1;
                        var f = i - width;
                        this._triangles[t] = new Triangle(f, e, d);
                        t++;
                    }
                    x++;
                }
            }

            x=0;
            y=0;
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
                        var p1 = new Plane(c.Point, b.Point, a.Point);
                        a.Normal = ((a.Normal + p1.Normal)/2.0).Normalized();
                        b.Normal = ((b.Normal + p1.Normal)/2.0).Normalized();
                        c.Normal = ((c.Normal + p1.Normal)/2.0).Normalized();

                        // Bottom triangle
                        var d = this._vertices[i];
                        var e = this._vertices[i + 1];
                        var f = this._vertices[i - width];
                        var p2 = new Plane(f.Point, e.Point, d.Point);
                        d.Normal = ((d.Normal + p2.Normal)/2.0).Normalized();
                        e.Normal = ((e.Normal + p2.Normal)/2.0).Normalized();
                        f.Normal = ((f.Normal + p2.Normal)/2.0).Normalized();
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
            for (var i = 0; i < this._triangles.Length; i++)
            {
                var t = this._triangles[i];
                var a = this._vertices[t.A];
                var b = this._vertices[t.B];
                var c = this._vertices[t.C];
                
                mesh.AddTriangle(a.Point, b.Point, c.Point, a.Normal, b.Normal, c.Normal, _colorizer);
            }
        }
    }
}