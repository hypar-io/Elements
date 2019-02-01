using Elements.Geometry;
using Elements.Interfaces;
using Elements.Geometry.Interfaces;
using Newtonsoft.Json;
using Hypar.Elements.Interfaces;
using Elements.Geometry.Solids;

namespace Elements
{
    /// <summary>
    /// A floor is a horizontal element defined by a perimeter and one or several voids.
    /// </summary>
    public class Floor : Element, IElementTypeProvider<FloorType>, IGeometry3D, IProfileProvider
    {
        /// <summary>
        /// The elevation from which the floor is extruded.
        /// </summary>
        [JsonProperty("elevation")]
        public double Elevation { get; }

        /// <summary>
        /// The floor type of the floor.
        /// </summary>
        [JsonProperty("element_type")]
        public FloorType ElementType { get; }

        /// <summary>
        /// The untransformed profile of the floor.
        /// </summary>
        [JsonProperty("profile")]
        public Profile Profile { get; }

        /// <summary>
        /// The transformed profile of the floor.
        /// </summary>
        [JsonIgnore]
        public Profile ProfileTransformed
        {
            get { return this.Transform != null ? this.Transform.OfProfile(this.Profile) : this.Profile; }
        }

        /// <summary>
        /// The thickness of the floor's extrusion.
        /// </summary>
        [JsonIgnore]
        public double Thickness
        {
            get { return this.ElementType.Thickness; }
        }

        /// <summary>
        /// The floor's geometry.
        /// </summary>
        [JsonProperty("geometry")]
        public Solid[] Geometry { get; }

        /// <summary>
        /// The openings in the floor.
        /// </summary>
        [JsonProperty("openings")]
        public Opening[] Openings{get;}

        /// <summary>
        /// Create a Floor.
        /// </summary>
        /// <param name="profile">The profile of the floor.</param>
        /// <param name="elementType">The floor type of the floor.</param>
        /// <param name="elevation">The elevation of the top of the floor.</param>
        /// <param name="material">The floor's material.</param>
        /// <param name="transform">The floor's transform.</param>
        /// <returns>A floor.</returns>
        [JsonConstructor]
        public Floor(Profile profile, FloorType elementType, double elevation = 0.0, Material material = null, Transform transform = null)
        {
            this.Profile = profile;
            this.Elevation = elevation;
            this.ElementType = elementType;
            this.Transform = transform != null ? transform : new Transform(new Vector3(0, 0, elevation - elementType.Thickness));
            this.Geometry = new[]{Solid.SweepFace(this.Profile.Perimeter, this.Profile.Voids, this.Thickness, material == null ? BuiltInMaterials.Concrete : material)};
        }

        /// <summary>
        /// Create a floor.
        /// </summary>
        /// <param name="profile">The profile of the floor.</param>
        /// <param name="elementType">The floor type of the floor.</param>
        /// <param name="elevation">The elevation of the top of the floor.</param>
        /// <param name="material">The floor's material.</param>
        /// <param name="transform">The floor's transform. If set, this will override the floor's elevation.</param>
        /// <param name="openings">An array of openings in the floor.</param>
        public Floor(Polygon profile, FloorType elementType, double elevation = 0.0, Material material = null, Transform transform = null, Opening[] openings = null)
        {
            if (openings != null && openings.Length > 0)
            {
                var voids = new Polygon[openings.Length];
                for (var i = 0; i < voids.Length; i++)
                {
                    var o = openings[i];
                    voids[i] = o.Perimeter;
                }
                this.Profile = new Profile(profile, voids);
            }
            else
            {
                this.Profile = new Profile(profile);
            }

            this.Openings = openings;
            this.Elevation = elevation;
            this.ElementType = elementType;
            this.Transform = transform != null ? transform : new Transform(new Vector3(0, 0, elevation - elementType.Thickness));
            this.Geometry = new[]{Solid.SweepFace(this.Profile.Perimeter, this.Profile.Voids, this.Thickness, material == null ? BuiltInMaterials.Concrete : material)};
        }

        /// <summary>
        /// Create a floor.
        /// </summary>
        /// <param name="profile">The profile of the floor.</param>
        /// <param name="start">A tranforms used to pre-transform the profile and direction vector before sweeping the geometry.</param>
        /// <param name="direction">The direction of the floor's sweep.</param>
        /// <param name="elementType">The floor type of the floor.</param>
        /// <param name="elevation">The elevation of the floor.</param>
        /// <param name="material">The floor's material.</param>
        /// <param name="transform">The floor's transform. If set, this will override the elevation.</param>
        public Floor(Profile profile, Transform start, Vector3 direction, FloorType elementType, double elevation = 0.0, Material material = null, Transform transform = null)
        {
            this.Profile = profile;
            this.Elevation = elevation;
            this.ElementType = elementType;
            this.Transform = transform != null ? transform : new Transform(new Vector3(0, 0, elevation));
            var outer = start.OfPolygon(this.Profile.Perimeter);
            var inner = this.Profile.Voids != null ? start.OfPolygons(this.Profile.Voids) : null;
            this.Geometry = new[]{Solid.SweepFace(outer, inner, start.OfVector(direction), this.Thickness, material == null ? BuiltInMaterials.Concrete : material)};
        }
        
        /// <summary>
        /// The area of the floor.
        /// Overlapping openings and openings which are outside of the floor's perimeter,
        /// will result in incorrect area results.
        /// </summary>
        /// <returns>The area of the floor.</returns>
        public double Area()
        {
            return this.Profile.Area();
        }
    }
}