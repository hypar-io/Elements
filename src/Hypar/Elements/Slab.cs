using Hypar.Geometry;
using System;
using System.Linq;
using System.Collections.Generic;

namespace Hypar.Elements
{
    /// <summary>
    /// A slab is a horizontal element defined by an outer boundary and one or several holes.
    /// </summary>
    public class Slab : Element, IMeshProvider
    {
        private Polyline _perimeter;
        private IEnumerable<Polyline> _holes;
        private double _elevation;
        private double _thickness;

        /// <summary>
        /// The perimeter of the slab.
        /// </summary>
        /// <returns></returns>
        public Polyline Perimeter => _perimeter;

        /// <summary>
        /// The holes in the slab.
        /// </summary>
        /// <returns></returns>
        public IEnumerable<Polyline> Holes => _holes;

        /// <summary>
        /// The elevation from which the slab is extruded.
        /// </summary>
        /// <returns></returns>
        public double Elevation => _elevation;

        public double Thickness => _thickness;

        public Slab()
        {
            this._perimeter = Profiles.Rectangular();
            this._elevation = 0.0;
            this._thickness = 0.2;
        }
        
        internal Slab(Polyline profile)
        {
            this._perimeter = profile;
            this._elevation = 0.0;
            this._thickness = 0.2;
            this._transform = new Transform(new Vector3(0, 0, this._elevation), new Vector3(1, 0, 0), new Vector3(0, 0, 1));
        }

        internal Slab(Polyline perimeter, IEnumerable<Polyline> holes, double elevation, double thickness, Material material = null, Transform transform = null) : base(material, transform)
        {
            if (thickness <= 0.0)
            {
                throw new ArgumentOutOfRangeException("thickness", "The slab thickness must be greater than 0.0.");
            }

            this._perimeter = perimeter;
            this._holes = holes;
            this._elevation = elevation;
            this._thickness = thickness;
            this._transform = new Transform(new Vector3(0, 0, elevation), new Vector3(1, 0, 0), new Vector3(0, 0, 1));
        }

        public Mesh Tessellate()
        {
            var polys = new List<Polyline>();
            polys.Add(this._perimeter);
            if(this._holes != null)
            {
                polys.AddRange(this._holes);
            }
            
            return Mesh.Extrude(polys, this.Thickness, true);
        }

        /// <summary>
        /// Create a slab within a perimeter.
        /// </summary>
        /// <param name="perimeter"></param>
        /// <returns></returns>
        public static Slab WithinPerimeter(Polyline perimeter)
        {
            var slab = new Slab(perimeter);
            return slab;
        }

        /// <summary>
        /// Set the elevation of the slab.
        /// </summary>
        /// <param name="elevation"></param>
        /// <returns></returns>
        public Slab AtElevation(double elevation)
        {
            this._elevation = elevation;
            this._transform = new Transform(new Vector3(0, 0, this._elevation), new Vector3(1, 0, 0), new Vector3(0, 0, 1));
            return this;
        }

        /// <summary>
        /// Set the thickness of the slab.
        /// </summary>
        /// <param name="thickness"></param>
        /// <returns></returns>
        public Slab WithThickness(double thickness)
        {
            if(thickness == 0.0)
            {
                throw new ArgumentOutOfRangeException("thickness", Messages.ZERO_THICKNESS_EXCEPTION);
            }
            this._thickness = thickness;
            return this;
        }

        /// <summary>
        /// Set the penetrations of the the slab.
        /// </summary>
        /// <param name="holes"></param>
        /// <returns></returns>
        public Slab WithHoles(IEnumerable<Polyline> holes)
        {
            this._holes = holes;
            return this;
        }

        /// <summary>
        /// Set the material of the slab.
        /// </summary>
        /// <param name="m"></param>
        /// <returns></returns>
        public Slab OfMaterial(Material m)
        {
            this._material = m;
            return this;
        }
    }

    public static class SlabExtensions
    {
        /// <summary>
        /// Set the elevation of multiple slabs from a collection of elevations.
        /// </summary>
        /// <param name="slabs"></param>
        /// <param name="elevation"></param>
        /// <returns></returns>
        public static IEnumerable<Slab> AtElevation(this IEnumerable<Slab> slabs, IEnumerable<double> elevation)
        {
            var slabArr = slabs.ToArray();
            var elevArr = elevation.ToArray();

            for(var i=0; i<slabArr.Length; i++)
            {
                slabArr[i].AtElevation(elevArr[i]);
            }
            return slabs;
        }

        /// <summary>
        /// Set the thickness of multiple slabs to the same value.
        /// </summary>
        /// <param name="slabs"></param>
        /// <param name="thickness"></param>
        /// <returns></returns>
        public static IEnumerable<Slab> WithThickness(this IEnumerable<Slab> slabs, double thickness)
        {
            foreach(var s in slabs)
            {
                s.WithThickness(thickness);
            }
            return slabs;
        }

        /// <summary>
        /// Set the holes in multiple slabs from a collection of collections of Polylines.
        /// </summary>
        /// <param name="slabs"></param>
        /// <param name="holes"></param>
        /// <returns></returns>
        public static IEnumerable<Slab> WithHoles(this IEnumerable<Slab> slabs, IEnumerable<IEnumerable<Polyline>> holes)
        {
            var slabArr = slabs.ToArray();
            var holesArr = holes.ToArray();

            for(var i=0; i<slabArr.Length; i++)
            {
                slabArr[i].WithHoles(holesArr[i]);
            }
            return slabs;
        }

        public static IEnumerable<Slab> OfMaterial(this IEnumerable<Slab> slabs, Material m)
        {
            foreach(var s in slabs)
            {
                s.OfMaterial(m);
            }
            return slabs;
        }
    }
}