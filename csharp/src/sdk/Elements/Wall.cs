using Hypar.Geometry;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Hypar.Elements
{
    /// <summary>
    /// A wall is a building element which is used to enclose space.
    /// </summary>
    public class Wall : Element, ITessellate<Mesh>
    {
        private readonly Line _centerLine;

        private readonly Profile _profile;

        /// <summary>
        /// The Profile of the wall.
        /// </summary>
        [JsonProperty("profile")]
        public Profile Profile
        {
            get{return this.Transform != null ? this.Transform.OfProfile(this._profile) : this._profile;}
        }

        /// <summary>
        /// The center line of the wall.
        /// </summary>
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
        /// <param name="openings">A collection of Openings in the Wall.</param>
        /// <param name="material">The wall's material.</param>
        /// <exception cref="System.ArgumentOutOfRangeException">Thrown when the thickness of the Wall is less than or equal to zero.</exception>
        /// <exception cref="System.ArgumentOutOfRangeException">Thrown when the height of the Wall is less than or equal to zero.</exception>
        /// <exception cref="System.ArgumentOutOfRangeException">Thrown when the Z components of Wall's start and end points are not the same.</exception>
        public Wall(Line centerLine, double thickness, double height, IList<Opening> openings = null, Material material = null)
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

            if(openings != null && openings.Count > 0)
            {
                var voids = openings.Select(o=>Polygon.Rectangle(new Vector3(o.DistanceAlongWall, o.BaseHeight), new Vector3(o.DistanceAlongWall + o.Width, o.BaseHeight + o.Height))).ToList();
                this._profile = new Profile(Polygon.Rectangle(Vector3.Origin, new Vector3(centerLine.Length, height)), voids);
            }
            else
            {
                this._profile = new Profile(Polygon.Rectangle(Vector3.Origin, new Vector3(centerLine.Length, height)));
            }
            
            // Construct a transform whose X axis is the centerline of the wall.
            var z = centerLine.Direction.Cross(Vector3.ZAxis);
            this.Transform = new Transform(centerLine.Start, centerLine.Direction, z);
        }

        /// <summary>
        /// Generate a mesh of the wall.
        /// </summary>
        public Mesh Tessellate()
        {
            return Mesh.Extrude(this._profile.Perimeter, this.Thickness, this._profile.Voids, true);
        }
    }
}