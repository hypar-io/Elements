using Hypar.Geometry;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Hypar.Elements
{
    /// <summary>
    /// A zero-thickness planar panel with an arbitrary outline.
    /// </summary>
    public class Panel : Element, ITessellate<Mesh>
    {
        private Polyline _perimeter;

        /// <summary>
        /// A polyline which defines the perimeter of the panel.
        /// </summary>
        /// <returns></returns>
        public Polyline Perimeter => _perimeter;

        /// <summary>
        /// The normal of the panel, derived from the normal of the perimeter.
        /// </summary>
        /// <returns></returns>
        public Vector3 Normal
        {
            get{return this._perimeter.Normal();}
        }

        /// <summary>
        /// Set the perimeter of the panel.
        /// </summary>
        /// <param name="perimeter">The perimeter.</param>
        /// <returns>The panel.</returns>
        public static Panel WithinPerimeter(Polyline perimeter)
        {
            var panel = new Panel();
            panel._perimeter = perimeter;
            return panel;
        }

        /// <summary>
        /// Construct a default panel.
        /// </summary>
        /// <param name="material">The panel's material.</param>
        /// <param name="transform"></param>
        /// <returns></returns>
        public Panel(Material material = null, Transform transform = null) : base(material, transform)
        {
            this._perimeter = Profiles.Rectangular(new Vector3(), 10, 10);
        }

        /// <summary>
        /// Construct a Panel.
        /// </summary>
        /// <param name="perimeter">The perimeter of the panel.</param>
        /// <param name="material"></param>
        /// <param name="transform"></param>
        /// <returns></returns>
        public Panel(Polyline perimeter, Material material, Transform transform = null) : base(material, transform)
        {
            var vCount = perimeter.Vertices.Count();
            if (vCount > 4 || vCount < 3)
            {
                throw new ArgumentException("Panels can only be constructed currently using perimeters with 3 or 4 vertices.", "perimeter");
            }
            this._perimeter = perimeter;
        }
        
        /// <summary>
        /// Tessellate the panel.
        /// </summary>
        /// <returns>A mesh representing the tessellated panel.</returns>
        public Mesh Tessellate()
        {
            var mesh = new Mesh();
            var vCount = this.Perimeter.Count();

            if (vCount == 3)
            {
                mesh.AddTriangle(this.Perimeter.ToArray());
            }
            else if (vCount == 4)
            {
                mesh.AddQuad(this.Perimeter.ToArray());
            }
            return mesh;
        }

        /// <summary>
        /// Set the material of the panel.
        /// </summary>
        /// <param name="material">The material.</param>
        /// <returns>The panel.</returns>
        public Panel OfMaterial(Material material)
        {
            this.Material = material;
            return this;
        }
    }

    /// <summary>
    /// Extension methods for panels.
    /// </summary>
    public static class PanelExtensions
    {
        /// <summary>
        /// Set the material of a collection of panels.
        /// </summary>
        /// <param name="panels"></param>
        /// <param name="m"></param>
        /// <returns></returns>
        public static IEnumerable<Panel> OfMaterial(this IEnumerable<Panel> panels, Material m)
        {
            foreach(var p in panels)
            {
                p.OfMaterial(m);
            }
            return panels;
        }
    }
}