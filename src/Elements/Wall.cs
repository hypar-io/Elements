using Elements.Geometry;
using Elements.Geometry.Interfaces;
using Elements.Interfaces;
using Newtonsoft.Json;
using System;

namespace Elements
{
    /// <summary>
    /// A wall is a building element which is used to enclose space.
    /// </summary>
    public class Wall : Element, IElementTypeProvider<WallType>, IGeometry3D, IProfileProvider
    {
        /// <summary>
        /// The Profile of the Wall.
        /// </summary>
        [JsonProperty("profile")]
        public IProfile Profile { get; }

        /// <summary>
        /// The transformed Profile of the Wall.
        /// </summary>
        [JsonIgnore]
        public IProfile ProfileTransformed
        {
            get { return this.Transform != null ? this.Transform.OfProfile(this.Profile) : this.Profile; }
        }

        /// <summary>
        /// The center line of the wall.
        /// </summary>
        [JsonProperty("center_line")]
        public Line CenterLine { get; }

        /// <summary>
        /// The height of the wall.
        /// </summary>
        [JsonProperty("height")]
        public double Height { get; }

        /// <summary>
        /// The WallType of the Wall.
        /// </summary>
        [JsonProperty("element_type")]
        public WallType ElementType { get; }

        /// <summary>
        /// The thickness of the Wall's extrusion.
        /// </summary>
        /// <value></value>
        [JsonIgnore]
        public double Thickness
        {
            get { return this.ElementType.Thickness; }
        }

        /// <summary>
        /// The Wall's geometry.
        /// </summary>
        [JsonProperty("geometry")]
        public IBRep[] Geometry { get; }

        /// <summary>
        /// Construct a wall by extruding a profile.
        /// </summary>
        /// <param name="profile"></param>
        /// <param name="height"></param>
        /// <param name="material"></param>
        /// <param name="transform"></param>
        public Wall(Profile profile, double height, Material material = null, Transform transform = null)
        {
            this.Profile = profile;
            this.Height = height;
            this.Transform = transform;
            this.Geometry = new []{new Extrude(this.Profile, this.Height, material == null ? BuiltInMaterials.Concrete : material)};
        }

        /// <summary>
        /// Construct a wall along a line.
        /// </summary>
        /// <param name="centerLine">The center line of the Wall.</param>
        /// <param name="elementType">The WallType of the Wall.</param>
        /// <param name="height">The height of the Wall.</param>
        /// <param name="openings">A collection of Openings in the Wall.</param>
        /// <param name="material">The Wall's material.</param>
        /// <exception cref="System.ArgumentOutOfRangeException">Thrown when the thickness of the Wall is less than or equal to zero.</exception>
        /// <exception cref="System.ArgumentOutOfRangeException">Thrown when the height of the Wall is less than or equal to zero.</exception>
        /// <exception cref="System.ArgumentOutOfRangeException">Thrown when the Z components of Wall's start and end points are not the same.</exception>
        [JsonConstructor]
        public Wall(Line centerLine, WallType elementType, double height, Opening[] openings = null, Material material = null)
        {
            if (height <= 0.0)
            {
                throw new ArgumentOutOfRangeException("The wall could not be constructed. The height of the wall must be greater than 0.0.");
            }

            if (centerLine.Start.Z != centerLine.End.Z)
            {
                throw new ArgumentException("The wall could not be constructed. The Z component of the start and end points of the wall's center line must be the same.");
            }

            this.CenterLine = centerLine;
            this.Height = height;
            this.ElementType = elementType;

            if (openings != null && openings.Length > 0)
            {
                var voids = new Polygon[openings.Length];
                for (var i = 0; i < voids.Length; i++)
                {
                    var o = openings[i];
                    voids[i] = Polygon.Rectangle(new Vector3(o.DistanceAlongWall, o.BaseHeight), new Vector3(o.DistanceAlongWall + o.Width, o.BaseHeight + o.Height));
                }
                this.Profile = new Profile(Polygon.Rectangle(Vector3.Origin, new Vector3(centerLine.Length(), height)), voids);
            }
            else
            {
                this.Profile = new Profile(Polygon.Rectangle(Vector3.Origin, new Vector3(centerLine.Length(), height)));
            }

            // Construct a transform whose X axis is the centerline of the wall.
            var z = centerLine.Direction.Cross(Vector3.ZAxis);
            this.Transform = new Transform(centerLine.Start, centerLine.Direction, z);
            this.Geometry = new []{new Extrude(this.Profile, this.Thickness, material == null ? BuiltInMaterials.Concrete : material)};
        }
    }
}