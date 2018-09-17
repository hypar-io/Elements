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
    public class Space : Element, ILocateable<Polygon>, ITessellate<Mesh>
    {
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
        [JsonProperty("location")]
        public Polygon Location{get;}

        /// <summary>
        /// The voids within the Space.
        /// </summary>
        /// <value></value>
        [JsonProperty("voids")]
        public IEnumerable<Polygon> Voids { get; }

        /// <summary>
        /// Construct a space.
        /// </summary>
        public Space()
        {
            this.Elevation = 0.0;
            this.Height = 1.0;
            this.Location = Profiles.Rectangular();
            this.Material = BuiltInMaterials.Default;
            this.Transform = new Transform(new Vector3(0, 0, this.Elevation), new Vector3(1, 0, 0), new Vector3(0, 0, 1));
        }

        /// <summary>
        /// Construct a space.
        /// </summary>
        /// <param name="perimeter">The perimeter of the space.</param>
        /// <param name="voids">A list of perimeters as vertical voids the same height as the space.</param>
        /// <param name="elevation">The elevation of the perimeter.</param>
        /// <param name="height">The height of the space above the lower elevation.</param>
        /// <returns></returns>
        public Space(Polygon perimeter, IEnumerable<Polygon> voids, double elevation, double height)
        {
            if (height <= 0.0)
            {
                throw new ArgumentOutOfRangeException(Messages.HEIGHT_EXCEPTION, "height");
            }

            // TODO: Test that voids are within perimeter. Add appropriate exception message.

            this.Location = perimeter;
            this.Voids = voids;
            this.Elevation = elevation;
            this.Height = height;
            
            this.Material = BuiltInMaterials.Default;
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
                this.Location
            };
            if (this.Voids != null)
            {
                polys.AddRange(this.Voids);
            }
            return Mesh.Extrude(polys, this.Height, true);
        }
    }
}