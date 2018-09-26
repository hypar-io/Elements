using Hypar.Geometry;
using Newtonsoft.Json;
using System;
using System.Collections;
using System.Linq;
using System.Collections.Generic;

namespace Hypar.Elements
{
    /// <summary>
    /// A Floor is a horizontal element defined by a perimeter and one or several voids.
    /// </summary>
    public class Floor : Element, ITessellate<Mesh>
    {
        private readonly Profile _profile;

        /// <summary>
        /// The type of the element.
        /// </summary>
        public override string Type
        {
            get { return "floor"; }
        }

        /// <summary>
        /// The Profile of the Floor.
        /// </summary>
        [JsonProperty("profile")]
        public Profile Profile
        {
            get{return this.Transform != null ? this.Transform.OfProfile(this._profile) : this._profile;}
        }

        /// <summary>
        /// The elevation from which the Floor is extruded.
        /// </summary>
        [JsonProperty("elevation")]
        public double Elevation { get; }

        /// <summary>
        /// The thickness of the Floor.
        /// </summary>
        [JsonProperty("thickness")]
        public double Thickness { get; }

        /// <summary>
        /// Construct a Floor.
        /// </summary>
        /// <param name="profile">The Profile of the Floor.</param>
        /// <param name="elevation">The elevation of the Floor.</param>
        /// <param name="thickness">The thickness of the Floor.</param>
        /// <param name="material">The Floor's material.</param>
        /// <exception cref="System.ArgumentOutOfRangeException">Thrown when the slab's thickness is less than or equal to 0.0.</exception>
        [JsonConstructor]
        public Floor(Profile profile, double elevation = 0.0, double thickness = 0.1, Material material = null)
        {
            if (thickness <= 0.0)
            {
                throw new ArgumentOutOfRangeException("thickness", "The slab thickness must be greater than 0.0.");
            }

            this._profile = profile;
            this.Elevation = elevation;
            this.Thickness = thickness;
            this.Transform = new Transform(new Vector3(0, 0, elevation));
            this.Material = material == null ? BuiltInMaterials.Concrete : material;
        }

        /// <summary>
        /// Tessellate the Floor.
        /// </summary>
        /// <returns></returns>
        public Mesh Tessellate()
        {
            var clipper = new ClipperLib.Clipper();
            clipper.AddPath(this._profile.Perimeter.ToClipperPath(), ClipperLib.PolyType.ptSubject, true);
            clipper.AddPaths(this._profile.Voids.Select(p => p.ToClipperPath()).ToList(), ClipperLib.PolyType.ptClip, true);
            var solution = new List<List<ClipperLib.IntPoint>>();
            var result = clipper.Execute(ClipperLib.ClipType.ctDifference, solution, ClipperLib.PolyFillType.pftEvenOdd);
            var polys = solution.Select(s => s.ToPolygon()).ToList();

            if(polys.Count > 1)
            {
                return Mesh.Extrude(polys.First(), this.Thickness, polys.Skip(1).ToList(), true);
            } 
            else 
            {
                return Mesh.Extrude(polys.First(), this.Thickness, null, true);
            }
        }

        /// <summary>
        /// The area of the Floor.
        /// Overlapping openings and openings which are outside of the Floor's perimeter,
        /// will result in incorrect area results.
        /// </summary>
        public double Area()
        {
            return this._profile.Area;
        }
    }

    /// <summary>
    /// Extension methods for Floors.
    /// </summary>
    public static class FloorExtensions
    {
        /// <summary>
        /// Create Floors at the specified elevations within a mass.
        /// </summary>
        /// <param name="mass"></param>
        /// <param name="elevations">A collection of elevations at which Floors will be created within the mass.</param>
        /// <param name="thickness">The thickness of the Floors.</param>
        /// <param name="material">The Floor material.</param>
        public static IList<Floor> Floors(this Mass mass, IList<double> elevations, double thickness, Material material)
        {
            var Floors = new List<Floor>();
            foreach(var e in elevations)
            {
                if (e >= mass.Elevation && e <= mass.Elevation + mass.Height)
                {
                    var f = new Floor(mass.Profile, e, thickness, material);
                    Floors.Add(f);
                }
            }
            return Floors;
        }
    }
}