using Hypar.Geometry;
using Newtonsoft.Json;
using System;

namespace Hypar.Elements
{
    /// <summary>
    /// A wall is a building element which is used to enclose space.
    /// </summary>
    public class Wall: Element, ILocateable<Line>, IMaterialize, ITessellate<Mesh>
    {
        /// <summary>
        /// The center line of the wall.
        /// </summary>
        /// <value></value>
        [JsonProperty("Line")]
        public Line Location{get;}

        /// <summary>
        /// The wall's material.
        /// </summary>
        /// <value></value>
        [JsonIgnore]
        public Material Material {get;set;}

        /// <summary>
        /// The thickness of the wall.
        /// </summary>
        /// <value></value>
        [JsonProperty("thickness")]
        public double Thickness{get;set;}

        /// <summary>
        /// The height of the wall.
        /// </summary>
        /// <value></value>
        [JsonProperty("height")]
        public double Height{get;}

        /// <summary>
        /// Construct a wall along a line.
        /// </summary>
        /// <param name="centerLine">The center line of the wall.</param>
        /// <param name="thickness">The thickness of the wall.</param>
        /// <param name="height">The height of the wall.</param>
        /// <param name="material">The wall's material.</param>
        public Wall(Line centerLine, double thickness, double height, Material material)
        {
            if(centerLine.Start.Z != centerLine.End.Z)
            {
                throw new ArgumentException("The start and end points of the wall's centerline must be in the same plane.");
            }

            if(height <= 0.0)
            {
                throw new ArgumentOutOfRangeException("The height of the wall must be greater than 0.0.");
            }

            this.Location = centerLine;
            this.Thickness = thickness;
            this.Material = material;
            this.Height = height;
        }

        /// <summary>
        /// Generate a mesh of the wall.
        /// </summary>
        /// <returns></returns>
        public Mesh Tessellate()
        {
            return Mesh.Extrude(new[] { this.Location.Thicken(this.Thickness) }, this.Height);
        }
    }
}