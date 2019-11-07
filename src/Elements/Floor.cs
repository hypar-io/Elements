using Elements.Geometry;
using Elements.Interfaces;
using System;
using Elements.Geometry.Solids;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;

namespace Elements
{
    /// <summary>
    /// A floor is a horizontal element defined by a profile.
    /// </summary>
    /// <example>
    /// [!code-csharp[Main](../../test/Elements.Tests/Examples/FloorExample.cs?name=example)]
    /// </example>
    [UserElement]
    public class Floor : GeometricElement, IHasOpenings
    {
        /// <summary>
        /// The elevation from which the floor is extruded.
        /// </summary>
        public double Elevation { get; set; }

        /// <summary>
        /// The thickness of the floor.
        /// </summary>
        public double Thickness { get; set;}

        /// <summary>
        /// The untransformed profile of the floor.
        /// </summary>
        public Profile Profile { get; set; }

        /// <summary>
        /// A collection of openings in the floor.
        /// </summary>
        public List<Opening> Openings{ get; } = new List<Opening>();

        /// <summary>
        /// Create a floor.
        /// </summary>
        /// <param name="profile">The perimeter of the floor.</param>
        /// <param name="thickness">The thickness of the floor.</param>
        /// <param name="elevation">The elevation of the top of the floor.
        /// If a transform has been provided, this elevation will be added to the origin of that transform.</param>
        /// <param name="transform">The floor's transform. If set, this will override the floor's elevation.</param>
        /// <param name="material">The floor's material.</param>
        /// <param name="representation">The floor's representation.</param>
        /// <param name="id">The floor's id.</param>
        /// <param name="name">The floor's name.</param>
        public Floor(Profile profile,
                     double thickness,
                     double elevation = 0.0,
                     Transform transform = null,
                     Material material = null,
                     Representation representation = null,
                     Guid id = default(Guid),
                     string name = null) : base(transform != null ? transform : new Transform(),
                                                material != null ? material : BuiltInMaterials.Concrete,
                                                representation != null ? representation : new Representation(new List<SolidOperation>()),
                                                id != default(Guid) ? id : Guid.NewGuid(),
                                                name)
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
            if(elevation != 0.0)
            {
                this.Transform.Move(new Vector3(0,0,elevation));
            }
            this.Material = material ?? BuiltInMaterials.Concrete;
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

        /// <summary>
        /// Update the representations.
        /// </summary>
        public override void UpdateRepresentations()
        {
            if(this.Openings.Count > 0)
            {
                this.Openings.ForEach(o=>o.UpdateRepresentations());

                // Find all the void ops which point in the same direction.
                var holes = this.Openings.SelectMany(o=>o.Representation.SolidOperations.
                                                        Where(op=>op is Extrude && op.IsVoid == true).
                                                        Cast<Extrude>().
                                                        Where(ex=>ex.Direction.IsAlmostEqualTo(Vector3.ZAxis)));
                if(holes.Any())
                {
                    var holeProfiles = holes.Select(ex=>ex.Profile);
                    this.Profile.Clip(holeProfiles);
                }
            }
            this.Representation.SolidOperations.Clear();
            this.Representation.SolidOperations.Add(new Extrude(this.Profile, this.Thickness, Vector3.ZAxis, 0, false));
        }
    }
}