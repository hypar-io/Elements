using Hypar.Geometry;
using System;
using System.Linq;

namespace Hypar.Elements
{
    /// <summary>
    /// A planar panel with an arbitrary outline.
    /// </summary>
    public class Panel : Element, IMeshProvider
    {   
        /// <summary>
        /// A Polygon3 which defines the perimeter of the panel.
        /// </summary>
        /// <returns></returns>
        public Polygon3 Perimeter{get;}

        /// <summary>
        /// The normal of the panel, derived from the normal of the perimeter.
        /// </summary>
        /// <returns></returns>
        public Vector3 Normal{get;}

        /// <summary>
        /// Construct a Panel.
        /// </summary>
        /// <param name="perimeter"></param>
        /// <param name="material"></param>
        /// <param name="transform"></param>
        /// <returns></returns>
        public Panel(Polygon3 perimeter, Material material, Transform transform = null):base(material,transform)
        {
            var vCount = perimeter.Vertices.Count();
            if(vCount > 4 || vCount < 3)
            {
                throw new ArgumentException("Panels can only be constructed currently using perimeters with 3 or 4 vertices.", "perimeter");
            }
            this.Perimeter = perimeter;
            this.Normal = perimeter.Normal();
        }

        public Mesh Tessellate()
        {
            var mesh = new Mesh();
            var vCount = this.Perimeter.Vertices.Count();
            if(vCount == 3)
            {
                mesh.AddTri(this.Perimeter.ToArray());
            }
            else if(vCount == 4)
            {
                mesh.AddQuad(this.Perimeter.ToArray());
            }
            return mesh;
        }
    }    
}