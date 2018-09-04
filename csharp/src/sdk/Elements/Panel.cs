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
    public class Panel : Element, ILocateable<Polyline>, ITessellate<Mesh>, IMaterialize
    {
        /// <summary>
        /// The normal of the panel, derived from the normal of the perimeter.
        /// </summary>
        /// <returns></returns>
        [JsonProperty("normal")]
        public Vector3 Normal
        {
            get{return this.Location.Normal();}
        }

        /// <summary>
        /// The boundary of the panel.
        /// </summary>
        /// <value></value>
        [JsonProperty("location")]
        public Polyline Location {get;}

        /// <summary>
        /// The material of the panel.
        /// </summary>
        /// <value></value>
        [JsonIgnore]
        public Material Material{get;set;}

        /// <summary>
        /// Construct a panel.
        /// </summary>
        /// <param name="perimeter">The perimeter of the panel.</param>
        public Panel(Polyline perimeter)
        {
            var vCount = perimeter.Vertices.Count();
            if (vCount > 4 || vCount < 3)
            {
                throw new ArgumentException("Panels can only be constructed currently using perimeters with 3 or 4 vertices.", "perimeter");
            }
            this.Location = perimeter;
            this.Material = BuiltInMaterials.Default;
        }

        /// <summary>
        /// Construct a Panel.
        /// </summary>
        /// <param name="perimeter">The perimeter of the panel.</param>
        /// <param name="material">The panel's material.</param>
        /// <returns></returns>
        public Panel(Polyline perimeter, Material material)
        {
            var vCount = perimeter.Vertices.Count();
            if (vCount > 4 || vCount < 3)
            {
                throw new ArgumentException("Panels can only be constructed currently using perimeters with 3 or 4 vertices.", "perimeter");
            }
            this.Location = perimeter;
            this.Material = material;
        }
        
        /// <summary>
        /// Tessellate the panel.
        /// </summary>
        /// <returns>A mesh representing the tessellated panel.</returns>
        public Mesh Tessellate()
        {
            var mesh = new Mesh();
            var vCount = this.Location.Count();

            if (vCount == 3)
            {
                mesh.AddTriangle(this.Location.ToArray());
            }
            else if (vCount == 4)
            {
                mesh.AddQuad(this.Location.ToArray());
            }
            return mesh;
        }
    }
}