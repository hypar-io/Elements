using Elements.Geometry;
using Elements.Interfaces;
using System;
using Elements.Geometry.Solids;
using Newtonsoft.Json;

namespace Elements
{
    /// <summary>
    /// A floor is a horizontal element defined by a profile.
    /// </summary>
    /// <example>
    /// [!code-csharp[Main](../../test/Examples/FloorExample.cs?name=example)]
    /// </example>
    [UserElement]
    public class Floor : Element, IMaterial, IGeometry
    {
        /// <summary>
        /// The elevation from which the floor is extruded.
        /// </summary>
        public double Elevation { get; private set; }

        /// <summary>
        /// The thickness of the floor.
        /// </summary>
        public double Thickness { get; private set;}

        /// <summary>
        /// The untransformed profile of the floor.
        /// </summary>
        public Profile Profile { get; private set; }

        /// <summary>
        /// The floor's geometry.
        /// </summary>
        public Elements.Geometry.Geometry Geometry { get; } = new Geometry.Geometry();

        /// <summary>
        /// The floor's material.
        /// </summary>
        public Material Material{ get; private set; }

        /// <summary>
        /// Create a floor.
        /// </summary>
        /// <param name="profile">The perimeter of the floor.</param>
        /// <param name="thickness">The thickness of the floor.</param>
        /// <param name="elevation">The elevation of the top of the floor.</param>
        /// <param name="transform">The floor's transform. If set, this will override the floor's elevation.</param>
        /// <param name="material">The floor's material.</param>
        /// <param name="id">The floor's id.</param>
        /// <param name="name">The floor's name.</param>
        public Floor(Polygon profile,
                     double thickness,
                     double elevation = 0.0,
                     Transform transform = null,
                     Material material = null,
                     Guid id = default(Guid),
                     string name = null) : base(id, name, transform)
        {
            SetProperties(new Profile(profile), elevation, thickness, material, transform);
        }

        /// <summary>
        /// Create a floor.
        /// </summary>
        /// <param name="profile">The perimeter of the floor.</param>
        /// <param name="thickness">The thickness of the floor.</param>
        /// <param name="elevation">The elevation of the top of the floor.</param>
        /// <param name="transform">The floor's transform. If set, this will override the floor's elevation.</param>
        /// <param name="material">The floor's material.</param>
        /// <param name="id">The floor's id.</param>
        /// <param name="name">The floor's name.</param>
        [JsonConstructor]
        public Floor(Profile profile,
                     double thickness,
                     double elevation = 0.0,
                     Transform transform = null,
                     Material material = null,
                     Guid id = default(Guid),
                     string name = null) : base(id, name, transform)
        {
            SetProperties(profile, elevation, thickness, material, transform);
        }

        private void SetProperties(Profile profile, double elevation, double thickness, Material material, Transform transform)
        {
            this.Profile = profile;
            this.Elevation = elevation;

            if(thickness <= 0.0) 
            {
                throw new ArgumentOutOfRangeException($"The floor could not be created. The provided thickness ({thickness}) was less than or equal to zero.");
            }

            this.Thickness = thickness;
            this.Transform = transform != null ? new Transform(transform) : new Transform();
            this.Transform.Move(new Vector3(0, 0, elevation));
            this.Material = material != null ? material : BuiltInMaterials.Concrete;
            this.Geometry.SolidOperations.Add(new Extrude(this.Profile, this.Thickness, Vector3.ZAxis));
        }

        /// <summary>
        /// Get the profile of the floor transformed by the floor's transform.
        /// </summary>
        public Profile ProfileTransformed()
        {
            return this.Transform != null ? this.Transform.OfProfile(this.Profile) : this.Profile;
        }

        /// <summary>
        /// The area of the floor.
        /// </summary>
        /// <returns>The area of the floor, not including the area of openings.</returns>
        public double Area()
        {
            return Math.Abs(this.Profile.Area());
        }

        /// <summary>
        /// The area of the floor.
        /// </summary>
        /// <returns>The area of the floor, not including the area of openings.</returns>
        public double Volume()
        {
            return Math.Abs(this.Profile.Area()) * this.Thickness;
        }
    }
}