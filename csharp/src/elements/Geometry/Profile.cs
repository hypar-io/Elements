using Elements.Interfaces;
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
    /// A Profile describes 
    /// </summary>
    public class Profile : IIdentifiable
    {
        /// <summary>
        /// The identifier of the Profile.
        /// </summary>
        [JsonProperty("id")]
        public long Id{get; internal set;}

        /// <summary>
        /// The name of the Profile.
        /// </summary>
        [JsonProperty("name")]
        public string Name{get;}

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
        /// <param name="name">The name of the Profile.</param>
        /// <param name="perimeter">The perimeter of the Profile.</param>
        /// <param name="voids">A collection of Polygons representing voids in the Profile.</param>
        [JsonConstructor]
        public Profile(Polygon perimeter, IList<Polygon> voids, string name = null)
        {
            this.Id = IdProvider.Instance.GetNextId();
            this.Perimeter = perimeter;
            this.Voids = voids;
            this.Name = name;
        }

        /// <summary>
        /// Default constructor for Profile.
        /// </summary>
        protected Profile(string name){
            this.Id = IdProvider.Instance.GetNextId();
            this.Name = name;
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
            this.Voids = new []{singleVoid};
            this.Name = name;
        }

        /// <summary>
        /// The area of the Profile.
        /// </summary>
        public double Area()
        {
            return ClippedArea();
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