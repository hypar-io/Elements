using Hypar.Geometry;
using Newtonsoft.Json;
using System;

namespace Hypar.Elements
{
    /// <summary>
    /// A wall is a building element which is used to enclose space.
    /// </summary>
    public class Wall : Element, ITessellate<Mesh>
    {
        private readonly Line _centerLine;

        /// <summary>
        /// The center line of the wall.
        /// </summary>
        /// <value></value>
        [JsonProperty("center_line")]
        public Line CenterLine
        {
            get{return this.Transform != null ? this.Transform.OfLine(this._centerLine) : this._centerLine;}
        }

        /// <summary>
        /// The thickness of the wall.
        /// </summary>
        /// <value></value>
        [JsonProperty("thickness")]
        public double Thickness { get; set; }

        /// <summary>
        /// The height of the wall.
        /// </summary>
        /// <value></value>
        [JsonProperty("height")]
        public double Height { get; }

        /// <summary>
        /// Construct a wall along a line.
        /// </summary>
        /// <param name="centerLine">The center line of the wall.</param>
        /// <param name="thickness">The thickness of the wall.</param>
        /// <param name="height">The height of the wall.</param>
        /// <param name="material">The wall's material.</param>
        public Wall(Line centerLine, double thickness, double height, Material material = null)
        {
            if (thickness <= 0.0)
            {
                throw new ArgumentOutOfRangeException("The wall could not be constructed. The thickness of the wall must be greater than 0.0.");
            }

            if (height <= 0.0)
            {
                throw new ArgumentOutOfRangeException("The wall could not be constructed. The height of the wall must be greater than 0.0.");
            }

            if (centerLine.Start.Z != centerLine.End.Z)
            {
                throw new ArgumentException("The wall could not be constructed. The Z component of the start and end points of the wall's center line must be the same.");
            }

            this._centerLine = centerLine;
            this.Thickness = thickness;
            this.Material = material == null ? BuiltInMaterials.Concrete : material;
            this.Height = height;
        }

        /// <summary>
        /// Generate a mesh of the wall.
        /// </summary>
        /// <returns></returns>
        public Mesh Tessellate()
        {
            return Mesh.Extrude(this._centerLine.Thicken(this.Thickness), this.Height);
        }
    }
}