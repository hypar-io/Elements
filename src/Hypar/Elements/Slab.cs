using Hypar.Geometry;
using System;
using System.Collections.Generic;

namespace Hypar.Elements
{
    /// <summary>
    /// A slab is a horizontal element defined by an outer boundary and one or several holes.
    /// </summary>
    public class Slab : Element, IMeshProvider
    {
        /// <summary>
        /// The perimeter of the slab.
        /// </summary>
        /// <returns></returns>
        public Polygon2 Perimeter{get;}

        /// <summary>
        /// The holes in the slab.
        /// </summary>
        /// <returns></returns>
        public IEnumerable<Polygon2> Holes{get;}

        /// <summary>
        /// The elevation from which the slab is extruded.
        /// </summary>
        /// <returns></returns>
        public double Elevation{get;}

        public double Thickness{get;}

        /// <summary>
        /// Construct a Slab by specifying its perimeter and holes.
        /// </summary>
        /// <param name="perimeter">The boundary </param>
        /// <param name="holes"></param>
        /// <param name="elevation"></param>
        /// <param name="thickness"></param>
        /// <param name="material"></param>
        /// <param name="transform"></param>
        /// <returns></returns>
        public Slab(Polygon2 perimeter, IEnumerable<Polygon2> holes, double elevation, double thickness, Material material, Transform transform=null): base(material, transform)
        {
            if(thickness <= 0.0)
            {
                throw new ArgumentOutOfRangeException("thickness","The slab thickness must be greater than 0.0.");
            }
            
            this.Perimeter = perimeter;
            this.Holes = holes;
            this.Elevation = elevation;
            this.Thickness = thickness;
            this.Transform = new Transform(new Vector3(0,0,elevation),new Vector3(1,0,0), new Vector3(0,0,1));
            
        }

        /// <summary>
        /// Construct from a section through a Mass.
        /// </summary>
        /// <param name="mass"></param>
        /// <param name="elevation"></param>
        /// <param name="thickness"></param>
        /// <param name="material"></param>
        /// <param name="transform"></param>
        /// <returns></returns>
        public Slab(Mass mass, double elevation, double thickness, Material material, Transform transform = null) : base(material, transform)
        {
            throw new NotImplementedException("Not implemented!");
        }

        public Mesh Tessellate()
        {
            var polys = new List<Polygon2>();
            polys.Add(this.Perimeter);
            polys.AddRange(this.Holes);
            return Mesh.Extrude(polys, this.Thickness, true);
        }
    }
}