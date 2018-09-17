using Hypar.Geometry;
using Newtonsoft.Json;
using System;
using System.Linq;
using System.Collections.Generic;

namespace Hypar.Elements
{
    /// <summary>
    /// A Floor is a horizontal element defined by an outer boundary and one or several holes.
    /// </summary>
    public class Floor : Element, ITessellate<Mesh>
    {
        /// <summary>
        /// The boundary of the floor.
        /// </summary>
        [JsonProperty("location")]
        public Polygon Perimeter{get;}

        /// <summary>
        /// The voids in the slab.
        /// </summary>
        [JsonProperty("voids")]
        public IEnumerable<Polygon> Voids{get;}

        /// <summary>
        /// The elevation from which the floor is extruded.
        /// </summary>
        [JsonProperty("elevation")]
        public double Elevation{get;}

        /// <summary>
        /// The thickness of the floor.
        /// </summary>
        [JsonProperty("thickness")]
        public double Thickness{get;}
        
        /// <summary>
        /// The area of the floor.
        /// Overlapping openings and openings which are outside of the floor's perimeter,
        /// will result in incorrect area results.
        /// </summary>
        [JsonProperty("area")]
        public double Area
        {
            get{return this.Perimeter.Area + this.Voids.Sum(o=>o.Area);}
        }

        /// <summary>
        /// Construct a floor.
        /// </summary>
        public Floor() : base()
        {
            this.Perimeter = Profiles.Rectangular();
            this.Elevation = 0.0;
            this.Thickness = 0.2;
            this.Material = BuiltInMaterials.Concrete;
            this.Voids = new List<Polygon>();
        }
        
        /// <summary>
        /// Construct a floor.
        /// </summary>
        /// <param name="perimeter">The perimeter of the floor.</param>
        /// <param name="elevation">The elevation of the floor.</param>
        /// <param name="thickness">The thickness of the floor.</param>
        /// <param name="voids">The voids in the floor.</param>
        /// <param name="material">The floor's material.</param>
        public Floor(Polygon perimeter, double elevation, double thickness, IList<Polygon> voids = null, Material material = null)
        {
            if (thickness <= 0.0)
            {
                throw new ArgumentOutOfRangeException("thickness", "The slab thickness must be greater than 0.0.");
            }

            this.Perimeter = perimeter;
            this.Voids = voids == null ? new List<Polygon>() : voids;
            this.Elevation = elevation;
            this.Thickness = thickness;
            this.Transform = new Transform(new Vector3(0, 0, elevation), new Vector3(1, 0, 0), new Vector3(0, 0, 1));
            this.Material = material == null ? BuiltInMaterials.Concrete : material;
        }

        /// <summary>
        /// Tessellate the floor.
        /// </summary>
        /// <returns></returns>
        public Mesh Tessellate()
        {
            var clipper = new ClipperLib.Clipper();
            clipper.AddPath(this.Perimeter.ToClipperPath(), ClipperLib.PolyType.ptSubject, true);
            clipper.AddPaths(this.Voids.Select(p=>p.ToClipperPath()).ToList(), ClipperLib.PolyType.ptClip, true);
            List<List<ClipperLib.IntPoint>> solution = new List<List<ClipperLib.IntPoint>>();
            var result = clipper.Execute(ClipperLib.ClipType.ctDifference, solution, ClipperLib.PolyFillType.pftEvenOdd);
            var polys = solution.Select(s=>s.ToPolygon());

            return Mesh.Extrude(polys, this.Thickness, true);
        }
    }

    public static class FloorExtensions
    {
        /// <summary>
        /// Create floors at the specified elevations within a mass.
        /// </summary>
        /// <param name="elevations">A collection of elevations at which floors will be created within the mass.</param>
        /// <param name="thickness">The thickness of the floors.</param>
        /// <param name="material">The floor material.</param>
        public static IEnumerable<Floor> FromMass(this Mass mass, IList<double> elevations, double thickness, Material material)
        {
            var floors = new List<Floor>();
            foreach(var e in elevations)
            {
                if (e >= mass.Elevation && e <= mass.Elevation + mass.Height)
                {
                    var f = new Floor(mass.Perimeter, e, thickness, new Polygon[]{}, material);
                    floors.Add(f);
                }
            }
            return floors;
        }
    }
}