using Elements.Interfaces;
using Elements.Geometry.Interfaces;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Elements.Geometry
{
    /// <summary>
    /// The vertical alignment of the profile.
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
    /// The horizontal alignment of the profile.
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
    /// A Profile describes a Polygonal perimeter
    /// with zero or many Polygonal voids.
    /// </summary>
    public class Profile : IIdentifiable
    {
        private Polygon _perimeter;
        private Polygon[] _voids;

        /// <summary>
        /// The identifier of the Profile.
        /// </summary>
        [JsonProperty("id")]
        public long Id { get; internal set; }

        /// <summary>
        /// The name of the Profile.
        /// </summary>
        [JsonProperty("name")]
        public string Name { get; }

        /// <summary>
        /// The perimeter of the Profile.
        /// </summary>
        [JsonProperty("perimeter")]
        public Polygon Perimeter { get => _perimeter; protected set => _perimeter = value; }

        /// <summary>
        /// A collection of Polygons representing voids in the Profile.
        /// </summary>
        [JsonProperty("voids")]
        public Polygon[] Voids { get => _voids; protected set => _voids = value; }

        /// <summary>
        /// Construct a Profile.
        /// </summary>
        /// <param name="name">The name of the Profile.</param>
        /// <param name="perimeter">The perimeter of the Profile.</param>
        /// <param name="voids">A collection of Polygons representing voids in the Profile.</param>
        [JsonConstructor]
        public Profile(Polygon perimeter, Polygon[] voids, string name = null)
        {
            this.Id = IdProvider.Instance.GetNextId();
            this.Perimeter = perimeter;
            this.Voids = voids;

            this.Name = name;
            if (!IsPlanar())
            {
                throw new Exception("To construct a Profile, all points must line in the same Plane.");
            }
            if(this.Voids != null)
            {
                this.Clip();
            }
        }

        /// <summary>
        /// Construct a Profile.
        /// </summary>
        /// <param name="name">The name of the Profile.</param>
        /// <param name="perimeter">The perimeter of the Profile</param>
        public Profile(Polygon perimeter, string name = null)
        {
            this.Id = IdProvider.Instance.GetNextId();
            this.Perimeter = perimeter;
            this.Name = name;
            if (!IsPlanar())
            {
                throw new Exception("To construct a Profile, all points must line in the same Plane.");
            }
        }

        /// <summary>
        /// Construct a Profile.
        /// </summary>
        /// <param name="name">The name of the Profile.</param>
        /// <param name="perimeter">The perimeter of the Profile.</param>
        /// <param name="singleVoid">A void in the Profile.</param>
        public Profile(Polygon perimeter, Polygon singleVoid, string name = null)
        {
            this.Id = IdProvider.Instance.GetNextId();
            this.Perimeter = perimeter;
            this.Voids = new[] { singleVoid };
            this.Name = name;
            if (!IsPlanar())
            {
                throw new Exception("To construct a Profile, all points must line in the same Plane.");
            }
            if(singleVoid != null)
            {
                this.Clip();
            }
        }

        /// <summary>
        /// Get a new Profile which is the reverse of this Profile.
        /// </summary>
        public Profile Reversed()
        {
            Polygon[] voids = null;
            if (this.Voids != null)
            {
                voids = new Polygon[this.Voids.Length];
                for (var i = 0; i < this.Voids.Length; i++)
                {
                    voids[i] = this.Voids[i].Reversed();
                }
            }
            return new Profile(this.Perimeter.Reversed(), voids);
        }

        /// <summary>
        /// The area of the Profile.
        /// </summary>
        public double Area()
        {
            return ClippedArea();
        }

        /// <summary>
        /// Default constructor for Profile.
        /// </summary>
        protected Profile(string name)
        {
            this.Id = IdProvider.Instance.GetNextId();
            this.Name = name;
        }

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
            if (this.Voids == null || this.Voids.Length == 0)
            {
                return this.Perimeter.Area;
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
            var x = (v[0] - v[1]).Normalized();
            var i = 2;
            var b = (v[i] - v[1]).Normalized();

            // Solve for parallel vectors
            while(b.IsAlmostEqualTo(x) || b.IsAlmostEqualTo(x.Negated()))
            {
                i++;
                b = (v[i] - v[1]).Normalized();
            } 
            var z = x.Cross(b);
            return new Transform(v[0], x, z);
        }
        
        private bool IsPlanar()
        {
            var t = ComputeTransform();
            var vertices = this.Perimeter.Vertices;
            var p = t.XY;
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