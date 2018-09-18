using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Hypar.Geometry;

namespace Hypar.Elements
{
    /// <summary>
    /// A space represents the extruded boundary of an occupiable region.
    /// </summary>
    public class Space : Element, ITessellate<Mesh>
    {
        /// <summary>
        /// The type of the element.
        /// </summary>
        public override string Type
        {
            get{return "space";}
        }

        /// <summary>
        /// The elevation of the lower Space perimeter.
        /// </summary>
        [JsonProperty("elevation")]
        public double Elevation { get; }

        /// <summary>
        /// The height of the Space above its perimeter elevation.
        /// </summary>
        [JsonProperty("height")]
        public double Height { get; }

        /// <summary>
        /// The lower perimeter of the Space.
        /// </summary>
        /// <returns></returns>
        [JsonProperty("perimeter")]
        public Polygon Perimeter{get;}

        /// <summary>
        /// The voids within the Space.
        /// </summary>
        /// <value></value>
        [JsonProperty("voids")]
        public IList<Polygon> Voids { get; }

        /// <summary>
        /// Construct a space.
        /// </summary>
        /// <param name="perimeter">The perimeter of the space.</param>
        /// <param name="voids">A list of perimeters as vertical voids the same height as the space.</param>
        /// <param name="elevation">The elevation of the perimeter.</param>
        /// <param name="height">The height of the space above the lower elevation.</param>
        /// <param name="material">The space's material.</param>
        [JsonConstructor]
        public Space(Polygon perimeter, IList<Polygon> voids = null, double elevation = 0.0, double height = 1.0, Material material = null)
        {
            if (height <= 0.0)
            {
                throw new ArgumentOutOfRangeException(Messages.HEIGHT_EXCEPTION, "height");
            }

            this.Perimeter = perimeter;
            this.Voids = voids != null? voids : new List<Polygon>();
            this.Elevation = elevation;
            this.Height = height;
            
            this.Material = material != null? material : BuiltInMaterials.Default;
            this.Transform =  new Transform(new Vector3(0, 0, this.Elevation), new Vector3(1, 0, 0), new Vector3(0, 0, 1));
        }

        /// <summary>
        /// Tessellate the Space.
        /// </summary>
        /// <returns></returns>
        public Mesh Tessellate()
        {
            var polys = new List<Polygon>
            {
                this.Perimeter
            };
            if (this.Voids != null)
            {
                polys.AddRange(this.Voids);
            }
            return Mesh.Extrude(polys, this.Height, true);
        }
    }
}