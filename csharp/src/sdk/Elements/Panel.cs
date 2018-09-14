using Hypar.Geometry;
using Newtonsoft.Json;
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
        /// <summary>
        /// A CCW collection of points defining the corners of the panel.
        /// </summary>
        [JsonProperty("perimeter")]
        public IList<Vector3> Perimeter{get;}

        /// <summary>
        /// The edges of the panel.
        /// </summary>
        [JsonProperty("edges")]
        public IEnumerable<Line> Edges
        {
            get
            {
                for(var i=0; i<this.Perimeter.Count-1; i++)
                {
                    yield return new Line(this.Perimeter[i], this.Perimeter[i+1]);
                }
            }
        }

        /// <summary>
        /// The normal of the panel, defined using the first 3 vertices in the location.
        /// </summary>
        /// <value></value>
        public Vector3 Normal
        {
            get
            {
                var verts = this.Perimeter.ToArray();
                return new Plane(verts[0], verts).Normal;
            }
        }

        /// <summary>
        /// Construct a panel.
        /// </summary>
        /// <param name="perimeter">The perimeter of the panel.</param>
        /// <param name="material">The panel's material</param>
        public Panel(IList<Vector3> perimeter, Material material = null)
        {
            var vCount = perimeter.Count();
            if (vCount > 4 || vCount < 3)
            {
                throw new ArgumentException("Panels can only be constructed currently using perimeters with 3 or 4 vertices.", "perimeter");
            }
            this.Perimeter = perimeter;
            this.Material = material == null ? BuiltInMaterials.Default : material;
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
                mesh.AddTriangle(this.Perimeter);
            }
            else if (vCount == 4)
            {
                mesh.AddQuad(this.Perimeter);
            }
            return mesh;
        }
    }
}