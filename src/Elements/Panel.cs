using Elements.Geometry;
using Elements.Geometry.Interfaces;
using Elements.Serialization;
using Elements.Interfaces;
using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Elements
{
    /// <summary>
    /// A zero-thickness planar Panel defined by 3 or 4 points.
    /// </summary>
    public class Panel : Element, IGeometry3D
    {
        /// <summary>
        /// The vertices forming the perimeter of the panel.
        /// </summary>
        [JsonProperty("perimeter")]
        public Vector3[] Perimeter { get; }

        /// <summary>
        /// The Panel's geometry.
        /// </summary>
        [JsonProperty("geometry")]
        public IBRep[] Geometry { get; }

        /// <summary>
        /// Construct a Panel.
        /// </summary>
        /// <param name="perimeter">The perimeter of the Panel.</param>
        /// <param name="material">The Panel's material</param>
        /// <exception cref="System.ArgumentException">Thrown when the number of perimeter points is less than 3 or greater than 4.</exception>
        [JsonConstructor]
        public Panel(Vector3[] perimeter, Material material = null)
        {
            var vCount = perimeter.Count();
            if (vCount > 4 || vCount < 3)
            {
                throw new ArgumentException("Panels can only be constructed currently using perimeters with 3 or 4 vertices.", "perimeter");
            }
            this.Perimeter = perimeter;
            this.Geometry = new []{new FacetedBRep(new[]{new PlanarFace(this.Perimeter)}, material == null ? BuiltInMaterials.Default : material)};
        }

        /// <summary>
        /// The normal of the Panel, defined using the first 3 vertices in the location.
        /// </summary>
        public Vector3 Normal()
        {
            return new Plane(this.Perimeter[0], this.Perimeter).Normal;
        }
    }
}