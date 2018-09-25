using Hypar.Geometry;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Hypar.Geometry
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
        public Polygon Perimeter{get; protected set;}

        /// <summary>
        /// A collection of Polygons representing voids in the Profile.
        /// </summary>
        [JsonProperty("voids")]
        public IList<Polygon> Voids{get; protected set;}
        
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

        protected Profile(){}

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
            if(this.Voids == null || this.Voids.Count == 0)
            {
                return this.Perimeter.Area;
            }

            var clipper = new ClipperLib.Clipper();
            clipper.AddPath(this.Perimeter.ToClipperPath(), ClipperLib.PolyType.ptSubject, true);
            clipper.AddPaths(this.Voids.Select(p => p.ToClipperPath()).ToList(), ClipperLib.PolyType.ptClip, true);
            var solution = new List<List<ClipperLib.IntPoint>>();
            clipper.Execute(ClipperLib.ClipType.ctDifference, solution, ClipperLib.PolyFillType.pftEvenOdd);
            return solution.Sum(s=>ClipperLib.Clipper.Area(s))/Math.Pow(1024.0, 2);
        }
    }
}