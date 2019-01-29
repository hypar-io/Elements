using Elements.Geometry;
using Elements.Geometry.Interfaces;
using Elements.Geometry.Solids;
using Elements.Interfaces;
using Hypar.Elements.Interfaces;
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
        /// The profile of the wall.
        /// </summary>
        [JsonProperty("profile")]
        public Profile Profile { get; }

        /// <summary>
        /// The transformed profile of the wall.
        /// </summary>
        [JsonIgnore]
        public Profile ProfileTransformed
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
        /// The thickness of the wall's extrusion.
        /// </summary>
        [JsonIgnore]
        public double Thickness
        {
            get { return this.ElementType.Thickness; }
        }

        /// <summary>
        /// The wall's geometry.
        /// </summary>
        [JsonProperty("geometry")]
        public Solid[] Geometry { get; }

        /// <summary>
        /// An array of Openings in the wall.
        /// </summary>
        [JsonProperty("openings")]
        public Opening[] Openings{ get; }

        /// <summary>
        /// Construct a wall by extruding a profile.
        /// </summary>
        /// <param name="profile"></param>
        /// <param name="height"></param>
        /// <param name="material"></param>
        /// <param name="transform"></param>
        public Wall(Profile profile, double height, Material material = null, Transform transform = null)
        {
            if (height <= 0.0)
            {
                throw new ArgumentOutOfRangeException("The wall could not be created. The height of the wall must be greater than 0.0.");
            }
            
            this.Profile = profile;
            this.Height = height;
            this.Transform = transform;
            this.Geometry = new []{new SweptSolid(this.Profile.Perimeter, this.Profile.Voids, this.Height, material == null ? BuiltInMaterials.Concrete : material)};
        }

        /// <summary>
        /// Construct a wall along a line.
        /// </summary>
        /// <param name="center_line">The center line of the wall.</param>
        /// <param name="element_type">The wall type of the wall.</param>
        /// <param name="height">The height of the wall.</param>
        /// <param name="openings">A collection of Openings in the wall.</param>
        /// <param name="material">The wall's material.</param>
        /// <exception cref="System.ArgumentOutOfRangeException">Thrown when the height of the wall is less than or equal to zero.</exception>
        /// <exception cref="System.ArgumentOutOfRangeException">Thrown when the Z components of wall's start and end points are not the same.</exception>
        public Wall(Line center_line, WallType element_type, double height, Material material = null, Opening[] openings = null)
        {
            if (height <= 0.0)
            {
                throw new ArgumentOutOfRangeException($"The wall could not be created. The height of the wall provided, {height}, must be greater than 0.0.");
            }

            if (center_line.Start.Z != center_line.End.Z)
            {
                throw new ArgumentException("The wall could not be created. The Z component of the start and end points of the wall's center line must be the same.");
            }

            this.CenterLine = center_line;
            this.Height = height;
            this.ElementType = element_type;
            this.Openings = openings;

            if (openings != null && openings.Length > 0)
            {
                var voids = new Polygon[openings.Length];
                for (var i = 0; i < voids.Length; i++)
                {
                    var o = openings[i];
                    voids[i] = o.Perimeter;
                }
                this.Profile = new Profile(Polygon.Rectangle(Vector3.Origin, new Vector3(center_line.Length(), height)), voids);
            }
            else
            {
                this.Profile = new Profile(Polygon.Rectangle(Vector3.Origin, new Vector3(center_line.Length(), height)));
            }

            // Construct a transform whose X axis is the centerline of the wall.
            var z = center_line.Direction.Cross(Vector3.ZAxis);
            this.Transform = new Transform(center_line.Start, center_line.Direction, z);
            this.Geometry = new []{new SweptSolid(this.Profile.Perimeter, this.Profile.Voids, this.Thickness, material == null ? BuiltInMaterials.Concrete : material)};
        }

        /// <summary>
        /// Construct a wall from a collection of geometry.
        /// </summary>
        /// <param name="geometry">The geometry of the wall.</param>
        /// <param name="center_line">The center line of the wall.</param>
        /// <param name="element_type">The wall type of the wall.</param>
        /// <param name="height">The height of the wall.</param>
        /// <param name="transform">The wall's Transform.</param>
        [JsonConstructor]
        public Wall(Solid[] geometry, WallType element_type, double height = 0.0, Line center_line = null, Transform transform = null)
        {
            if (geometry == null || geometry.Length == 0)
            {
                throw new ArgumentOutOfRangeException("You must supply at least one IBRep to construct a Wall.");
            }
            
            // TODO: Remove this when the Profile is no longer available
            // as a property on the Element. 
            // foreach(var g in geometry)
            // {
            //     var extrude = g as Extrude;
            //     if(extrude != null)
            //     {
            //         this.Profile = extrude.Profile;
            //     }
            // }

            this.Height = height;
            this.ElementType = element_type;
            this.Transform = transform;
            this.Geometry = geometry;
            this.CenterLine = center_line;
        }
    }
}