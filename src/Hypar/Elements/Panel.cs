using Hypar.Geometry;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Hypar.Elements
{
    /// <summary>
    /// A zero-thickness planar panel with an arbitrary outline.
    /// </summary>
    public class Panel : Element, IMeshProvider
    {
        private Polyline _perimeter;

        /// <summary>
        /// A Polygon3 which defines the perimeter of the panel.
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
        /// Construct a panel within a perimeter.
        /// </summary>
        /// <param name="perimeter"></param>
        /// <returns></returns>
        public static Panel WithinPerimeter(Polyline perimeter)
        {
            var p = new Panel();
            p._perimeter = perimeter;
            return p;
        }

        /// <summary>
        /// Construct multiple panels from a collection of perimeters.
        /// </summary>
        /// <param name="perimeters"></param>
        /// <returns></returns>
        public static IEnumerable<Panel> WithinPerimeters(IEnumerable<Polyline> perimeters)
        {
            var panels = new List<Panel>();
            foreach(var p in perimeters)
            {
                var panel = Panel.WithinPerimeter(p);
                panels.Add(panel);
            }
            return panels;
        }

        /// <summary>
        /// Construct multiple panels from a collection of perimeters.
        /// </summary>
        /// <param name="perimeters"></param>
        /// <returns></returns>
        public static IEnumerable<Panel> WithinPerimeters(params Polyline[] perimeters)
        {
            var panels = new List<Panel>();
            foreach(var p in perimeters)
            {
                var panel = Panel.WithinPerimeter(p);
                panels.Add(panel);
            }
            return panels;
        }

        public Panel(Material material = null, Transform transform = null) : base(material, transform)
        {
            this._perimeter = Profiles.Square(new Vector3(), 10, 10);
        }

        /// <summary>
        /// Construct a Panel.
        /// </summary>
        /// <param name="perimeter"></param>
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
        
        public Mesh Tessellate()
        {
            var mesh = new Mesh();
            var vCount = this.Perimeter.Count();

            if (vCount == 3)
            {
                mesh.AddTri(this.Perimeter.ToArray());
            }
            else if (vCount == 4)
            {
                mesh.AddQuad(this.Perimeter.ToArray());
            }
            return mesh;
        }

        public Panel OfMaterial(Material m)
        {
            this._material = m;
            return this;
        }
    }

    public static class PanelExtensions
    {
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