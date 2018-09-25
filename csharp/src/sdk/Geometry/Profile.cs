using Hypar.Geometry;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Hypar.Geometry
{
    /// <summary>
    /// A Profile describes 
    /// </summary>
    public class Profile
    {
        /// <summary>
        /// The area of the Profile.
        /// </summary>
        [JsonProperty("area")]
        public double Area
        {
            get{return ClippedArea();}
        }

        /// <summary>
        /// The perimeter of the Profile.
        /// </summary>
        [JsonProperty("perimeter")]
        public Polygon Perimeter{get;}

        /// <summary>
        /// A collection of Polygons representing voids in the Profile.
        /// </summary>
        [JsonProperty("voids")]
        public IList<Polygon> Voids{get;}
        
        /// <summary>
        /// Construct a Profile.
        /// </summary>
        /// <param name="perimeter">The perimeter of the Profile.</param>
        /// <param name="voids">A collection of Polygons representing voids in the Profile.</param>
        [JsonConstructor]
        public Profile(Polygon perimeter, IList<Polygon> voids = null)
        {
            this.Perimeter = perimeter;
            this.Voids = voids;
        }

        /// <summary>
        /// Construct a Profile.
        /// </summary>
        /// <param name="perimeter">The perimeter of the Profile</param>
        public Profile(Polygon perimeter)
        {
            this.Perimeter = perimeter;
            this.Voids = new List<Polygon>();
        }
        
        /// <summary>
        /// Construct a Profile.
        /// </summary>
        /// <param name="perimeter">The perimeter of the Profile.</param>
        /// <param name="singleVoid">A void in the Profile.</param>
        public Profile(Polygon perimeter, Polygon singleVoid)
        {
            this.Perimeter = perimeter;
            this.Voids = new []{singleVoid};
        }

        private double ClippedArea()
        {
            var clipper = new ClipperLib.Clipper();
            clipper.AddPath(this.Perimeter.ToClipperPath(), ClipperLib.PolyType.ptSubject, true);
            clipper.AddPaths(this.Voids.Select(p => p.ToClipperPath()).ToList(), ClipperLib.PolyType.ptClip, true);
            var solution = new List<List<ClipperLib.IntPoint>>();
            clipper.Execute(ClipperLib.ClipType.ctDifference, solution, ClipperLib.PolyFillType.pftEvenOdd);
            return solution.Sum(s=>ClipperLib.Clipper.Area(s))/Math.Pow(1024.0, 2);
        }
    }
}