using Hypar.Geometry;
using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Hypar.Elements
{
    /// <summary>
    /// A zero-thickness planar Panel defined by 3 or 4 points.
    /// </summary>
    public class Panel : Element, ITessellate<Mesh>
    {
        private IList<Vector3> _perimeter;

        /// <summary>
        /// The type of the element.
        /// </summary>
        public override string Type
        {
            get { return "panel"; }
        }

        /// <summary>
        /// A CCW collection of points defining the corners of the Panel.
        /// </summary>
        [JsonProperty("perimeter")]
        public IList<Vector3> Perimeter 
        { 
            get
            {
                return this.Transform != null ? this._perimeter.Select(v=>this.Transform.OfPoint(v)).ToList()  : this._perimeter;
            }
        }

        /// <summary>
        /// Construct a Panel.
        /// </summary>
        /// <param name="perimeter">The perimeter of the Panel.</param>
        /// <param name="material">The Panel's material</param>
        /// <exception cref="System.ArgumentException">Thrown when the number of perimeter points is less than 3 or greater than 4.</exception>
        [JsonConstructor]
        public Panel(IList<Vector3> perimeter, Material material = null)
        {
            var vCount = perimeter.Count();
            if (vCount > 4 || vCount < 3)
            {
                throw new ArgumentException("Panels can only be constructed currently using perimeters with 3 or 4 vertices.", "perimeter");
            }
            this._perimeter = perimeter;
            this.Material = material == null ? BuiltInMaterials.Default : material;
        }

        /// <summary>
        /// The edges of the Panel.
        /// </summary>
        public IList<Line> Edges()
        {
            var p = this.Perimeter;
            var result = new Line[p.Count];
            for (var i = 0; i < p.Count - 1; i++)
            {
                result[i] = new Line(p[i], p[i + 1]);
            }
            return result;
        }

        /// <summary>
        /// The normal of the Panel, defined using the first 3 vertices in the location.
        /// </summary>
        public Vector3 Normal()
        {
            var verts = this.Perimeter;
            return new Plane(verts[0], this.Perimeter).Normal;
        }

        /// <summary>
        /// Tessellate the Panel.
        /// </summary>
        /// <returns>A mesh representing the tessellated Panel.</returns>
        public Mesh Tessellate()
        {
            var mesh = new Mesh();
            var vCount = this._perimeter.Count();

            if (vCount == 3)
            {
                mesh.AddTriangle(this._perimeter);
            }
            else if (vCount == 4)
            {
                mesh.AddQuad(this._perimeter);
            }
            return mesh;
        }
    }
}