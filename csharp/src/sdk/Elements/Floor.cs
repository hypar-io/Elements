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
        private readonly Polygon _perimeter;

        /// <summary>
        /// The type of the element.
        /// </summary>
        public override string Type
        {
            get { return "floor"; }
        }

        /// <summary>
        /// The boundary of the Floor.
        /// </summary>
        [JsonProperty("perimeter")]
        public Polygon Perimeter
        {
            get{return this.Transform != null ? this.Transform.OfPolygon(this._perimeter) : this._perimeter;}
        }

        /// <summary>
        /// The voids in the Floor.
        /// </summary>
        [JsonProperty("voids")]
        public IList<Polygon> Voids { get; }

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
        /// <param name="perimeter">The perimeter of the Floor.</param>
        /// <param name="elevation">The elevation of the Floor.</param>
        /// <param name="thickness">The thickness of the Floor.</param>
        /// <param name="voids">The voids in the Floor.</param>
        /// <param name="material">The Floor's material.</param>
        [JsonConstructor]
        public Floor(Polygon perimeter, double elevation = 0.0, double thickness = 0.1, IList<Polygon> voids = null, Material material = null)
        {
            if (thickness <= 0.0)
            {
                throw new ArgumentOutOfRangeException("thickness", "The slab thickness must be greater than 0.0.");
            }

            this._perimeter = perimeter;
            this.Voids = voids == null ? new List<Polygon>() : voids.Select(v => v.Reversed()).ToList();
            this.Elevation = elevation;
            this.Thickness = thickness;
            this.Transform = new Transform(new Vector3(0, 0, elevation), new Vector3(1, 0, 0), new Vector3(0, 0, 1));
            this.Material = material == null ? BuiltInMaterials.Concrete : material;
        }

        /// <summary>
        /// Tessellate the Floor.
        /// </summary>
        /// <returns></returns>
        public Mesh Tessellate()
        {
            var clipper = new ClipperLib.Clipper();
            clipper.AddPath(this._perimeter.ToClipperPath(), ClipperLib.PolyType.ptSubject, true);
            clipper.AddPaths(this.Voids.Select(p => p.ToClipperPath()).ToList(), ClipperLib.PolyType.ptClip, true);
            var solution = new List<List<ClipperLib.IntPoint>>();
            var result = clipper.Execute(ClipperLib.ClipType.ctDifference, solution, ClipperLib.PolyFillType.pftEvenOdd);
            var polys = solution.Select(s => s.ToPolygon());

            return Mesh.Extrude(polys, this.Thickness, true);
        }

        /// <summary>
        /// The area of the Floor.
        /// Overlapping openings and openings which are outside of the Floor's perimeter,
        /// will result in incorrect area results.
        /// </summary>
        public double Area()
        {
            return this._perimeter.Area + this.Voids.Sum(o => o.Area);
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
                    var f = new Floor(mass.Perimeter, e, thickness, new Polygon[]{}, material);
                    Floors.Add(f);
                }
            }
            return Floors;
        }
    }
}