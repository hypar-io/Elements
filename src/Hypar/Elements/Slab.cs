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

        /// <summary>
        /// Construct one slab given a perimeter.
        /// </summary>
        /// <param name="perimeter"></param>
        /// <returns></returns>
        public static Slab WithinPerimeter(Polyline perimeter)
        {
            var slab = new Slab();
            slab._perimeter = perimeter;
            return slab;
        }

        /// <summary>
        /// Construct many slabs given a collection of perimeters.
        /// </summary>
        /// <param name="perimeters"></param>
        /// <returns></returns>
        public static IEnumerable<Slab> WithinPerimeters(IEnumerable<Polyline> perimeters)
        {
            var slabs = new List<Slab>();
            foreach(var p in perimeters)
            {
                var s = Slab.WithinPerimeter(p);
                slabs.Add(s);
            }
            return slabs;
        }

        public static IEnumerable<Slab> WithinPerimeters(params Polyline[] perimeters)
        {
            var slabs = new List<Slab>();
            foreach(var p in perimeters)
            {
                var s = Slab.WithinPerimeter(p);
                slabs.Add(s);
            }
            return slabs;
        }

        internal Slab()
        {
            this._perimeter = Profiles.Square(new Vector3(), 10,10);
            this._elevation = 0.0;
            this._thickness = 0.2;
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
        /// The the elevation of the Slab.
        /// </summary>
        /// <param name="elevation"></param>
        /// <returns></returns>
        public Slab AtElevation(double elevation)
        {
            this._elevation = elevation;
            return this;
        }

        /// <summary>
        /// Set the thickness of the Slab.
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
        /// Set the penetrations of the the Slab.
        /// </summary>
        /// <param name="holes"></param>
        /// <returns></returns>
        public Slab WithHoles(IEnumerable<Polyline> holes)
        {
            this._holes = holes;
            return this;
        }

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
        public static IEnumerable<Slab> AtElevations(this IEnumerable<Slab> slabs, IEnumerable<double> elevation)
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
    }
}