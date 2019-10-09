using Elements.Serialization.JSON;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Elements.Geometry
{
    /// <summary>
    /// The vertical alignment of a profile.
    /// </summary>
    public enum VerticalAlignment
    {
        /// <summary>
        /// Align the profile along its top.
        /// </summary>
        Top,
        /// <summary>
        /// Align the profile along its center.
        /// </summary>
        Center,
        /// <summary>
        /// Align the profile along its bottom.
        /// </summary>
        Bottom
    }

    /// <summary>
    /// The horizontal alignment of a profile.
    /// </summary>
    public enum HorizontalAlignment
    {
        /// <summary>
        /// Align the profile along its left edge.
        /// </summary>
        Left,
        /// <summary>
        /// Align the profile along its center.
        /// </summary>
        Center,
        /// <summary>
        /// Align the profile along its right edge.
        /// </summary>
        Right
    }

    /// <summary>
    /// A polygonal perimeter with zero or more polygonal voids.
    /// </summary>
    public partial class Profile : Identifiable
    {
        internal Profile(Guid id, string name): base(id, name){}
        
        /// <summary>
        /// Construct a profile.
        /// </summary>
        /// <param name="id">The id of the profile.</param>
        /// <param name="name">The name of the profile.</param>
        /// <param name="perimeter">The perimeter of the profile.</param>
        /// <param name="voids">A collection of Polygons representing voids in the profile.</param>
        [JsonConstructor]
        public Profile(Polygon perimeter, Polygon[] voids = null, Guid id = default(Guid), string name = null): base(id, name)
        {
            this.Perimeter = perimeter;
            this.Voids = voids;

            if (!IsPlanar())
            {
                throw new Exception("To construct a profile, all points must line in the same plane.");
            }
            if(this.Voids != null)
            {
                this.Clip();
            }
        }

        /// <summary>
        /// Construct a profile.
        /// </summary>
        /// <param name="name">The name of the profile.</param>
        /// <param name="perimeter">The perimeter of the profile.</param>
        /// <param name="singleVoid">A void in the profile.</param>
        /// <param name="id">The id of the profile.</param>
        public Profile(Polygon perimeter, Polygon singleVoid, Guid id = default(Guid), string name = null):
            this(perimeter, new[] { singleVoid }, id, name){}

        /// <summary>
        /// Get a new profile which is the reverse of this profile.
        /// </summary>
        public Profile Reversed()
        {
            Polygon[] voids = null;
            if (this.Voids != null)
            {
                voids = new Polygon[this.Voids.Count];
                for (var i = 0; i < this.Voids.Count; i++)
                {
                    voids[i] = this.Voids[i].Reversed();
                }
            }
            return new Profile(this.Perimeter.Reversed(), voids);
        }

        /// <summary>
        /// The area of the profile.
        /// </summary>
        public double Area()
        {
            return ClippedArea();
        }

        /// <summary>
        /// Transform this profile in place.
        /// </summary>
        /// <param name="t">The transform.</param>
        public void Transform(Transform t)
        {
            this.Perimeter.Transform(t);
            if(this.Voids == null)
            {
                return;
            }
            
            for(var i=0; i<this.Voids.Count; i++)
            {
                this.Voids[i].Transform(t);
            }
        }

        /// <summary>
        /// Default constructor for profile.
        /// </summary>
        protected Profile(string name): base(Guid.NewGuid(), name){}

        /// <summary>
        ///  Conduct a clip operation on this profile.
        /// </summary>
        private void Clip()
        {
            var clipper = new ClipperLib.Clipper();
            clipper.AddPath(this.Perimeter.ToClipperPath(), ClipperLib.PolyType.ptSubject, true);
            if (this.Voids != null)
            {
                clipper.AddPaths(this.Voids.Select(p => p.ToClipperPath()).ToList(), ClipperLib.PolyType.ptClip, true);
            }
            var solution = new List<List<ClipperLib.IntPoint>>();
            var result = clipper.Execute(ClipperLib.ClipType.ctDifference, solution, ClipperLib.PolyFillType.pftEvenOdd);

            // Completely disjoint polygons like a circular pipe
            // profile will result in an empty solution.
            if (solution.Count > 0)
            {
                var polys = solution.Select(s => s.ToPolygon()).ToArray();
                this.Perimeter = polys[0];
                this.Voids = polys.Skip(1).ToArray();
            }
        }

        private double ClippedArea()
        {
            if (this.Voids == null || this.Voids.Count == 0)
            {
                return this.Perimeter.Area();
            }

            var clipper = new ClipperLib.Clipper();
            clipper.AddPath(this.Perimeter.ToClipperPath(), ClipperLib.PolyType.ptSubject, true);
            clipper.AddPaths(this.Voids.Select(p => p.ToClipperPath()).ToList(), ClipperLib.PolyType.ptClip, true);
            var solution = new List<List<ClipperLib.IntPoint>>();
            clipper.Execute(ClipperLib.ClipType.ctDifference, solution, ClipperLib.PolyFillType.pftEvenOdd);
            return solution.Sum(s => ClipperLib.Clipper.Area(s)) / Math.Pow(1024.0, 2);
        }

        private Transform ComputeTransform()
        {
            var v = this.Perimeter.Vertices.ToList();
            var x = (v[0] - v[1]).Unit();
            var i = 2;
            var b = (v[i] - v[1]).Unit();

            // Solve for parallel vectors
            while(b.IsAlmostEqualTo(x) || b.IsAlmostEqualTo(x.Negate()))
            {
                i++;
                b = (v[i] - v[1]).Unit();
            } 
            var z = x.Cross(b);
            return new Transform(v[0], x, z);
        }
        
        private bool IsPlanar()
        {
            var t = ComputeTransform();
            var vertices = this.Perimeter.Vertices;
            var p = t.XY();
            foreach (var v in vertices)
            {
                var d = v.DistanceTo(p);
                if (Math.Abs(d) > Vector3.Tolerance)
                {
                    Console.WriteLine($"Out of plane distance: {d}.");
                    return false;
                }
            }
            return true;
        }
    }
}