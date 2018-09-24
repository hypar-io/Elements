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
        private readonly Polygon _perimeter;
        private readonly IList<Polygon> _voids;

        /// <summary>
        /// The type of the element.
        /// </summary>
        public override string Type
        {
            get { return "space"; }
        }

        /// <summary>
        /// The elevation of the Space perimeter.
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
        [JsonProperty("perimeter")]
        public Polygon Perimeter
        {
            get { return this.Transform != null? this.Transform.OfPolygon(_perimeter) : this._perimeter; }
        }

        /// <summary>
        /// The voids with the Space.
        /// </summary>
        [JsonProperty("voids")]
        public IList<Polygon> Voids
        {
            get{return this._voids.Select(v=>this.Transform.OfPolygon(v)).ToList();}
        }

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

            this._perimeter = perimeter;
            this._voids = voids != null ? voids : new List<Polygon>();
            this.Elevation = elevation;
            this.Height = height;

            this.Material = material != null ? material : BuiltInMaterials.Default;
            this.Transform = new Transform(new Vector3(0, 0, this.Elevation), new Vector3(1, 0, 0), new Vector3(0, 0, 1));
        }

        /// <summary>
        /// Tessellate the Space.
        /// </summary>
        /// <returns>The Mesh representing the Space.</returns>
        public Mesh Tessellate()
        {
            var polys = new List<Polygon>
            {
                this._perimeter
            };
            if (this._voids != null)
            {
                polys.AddRange(this.Voids);
            }
            return Mesh.Extrude(polys, this.Height, true);
        }
    }
}