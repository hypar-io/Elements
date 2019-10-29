using System;
using System.Collections.Generic;
using System.Linq;

namespace Elements.Geometry
{
    /// <summary>
    /// A polygonal perimeter with zero or more polygonal voids.
    /// </summary>
    public partial class Profile : Identifiable
    {
        /// <summary>
        /// Construct a profile.
        /// </summary>
        /// <param name="perimeter">The perimeter of the profile.</param>
        public Profile(Polygon perimeter): base(Guid.NewGuid(), null)
        {
            this.Perimeter = perimeter;
        }

        /// <summary>
        /// Construct a profile.
        /// </summary>
        /// <param name="perimeter">The perimeter of the profile.</param>
        /// <param name="singleVoid">A void in the profile.</param>
        public Profile(Polygon perimeter,
                       Polygon singleVoid) :
            this(perimeter, new[] { singleVoid }, Guid.NewGuid(), null){}

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
            return new Profile(this.Perimeter.Reversed(), voids, Guid.NewGuid(), null);
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

        internal static void ValidateConstructorParameters(Polygon @perimeter, IList<Polygon> @voids, System.Guid @id, string @name)
        {
            if (perimeter != null && !perimeter.Vertices.AreCoplanar())
            {
                throw new Exception("To construct a profile, all points must line in the same plane.");
            }
        }

        /// <summary>
        ///  Conduct a clip operation on this profile.
        /// </summary>
        internal void Clip(IEnumerable<Profile> additionalHoles = null)
        {
            var clipper = new ClipperLib.Clipper();
            clipper.AddPath(this.Perimeter.ToClipperPath(), ClipperLib.PolyType.ptSubject, true);
            if (this.Voids != null)
            {
                clipper.AddPaths(this.Voids.Select(p => p.ToClipperPath()).ToList(), ClipperLib.PolyType.ptClip, true);
            }
            if(additionalHoles != null)
            {
                clipper.AddPaths(additionalHoles.Select(h=>h.Perimeter.ToClipperPath()).ToList(), ClipperLib.PolyType.ptClip, true);
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
            var x = (v[0] - v[1]).Normalized();
            var i = 2;
            var b = (v[i] - v[1]).Normalized();

            // Solve for parallel vectors
            while(b.IsAlmostEqualTo(x) || b.IsAlmostEqualTo(x.Negate()))
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