using System;
using System.Collections.Generic;
using System.Linq;
using Elements.Geometry;
using Elements.Geometry.Solids;

namespace Elements
{
    /// <summary>
    /// A wall drawn using the elevation profile
    /// </summary>
    [Newtonsoft.Json.JsonConverter(typeof(Elements.Serialization.JSON.JsonInheritanceConverter), "discriminator")]
    public class WallByProfile : Wall
    {
        /// <summary>The profile, which includes openings that will be extruded.</summary>
        [Newtonsoft.Json.JsonProperty("Profile", Required = Newtonsoft.Json.Required.Default, NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore)]
        [Obsolete("The Profile property is obsolete, use the GetProfile method to access a profile created from the perimeter and the openings.")]
        public new Profile Profile { get; set; }

        /// <summary>The overall thickness of the wall</summary>
        [Newtonsoft.Json.JsonProperty("Thickness", Required = Newtonsoft.Json.Required.DisallowNull, NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore)]
        public double Thickness { get; set; }

        /// <summary>The perimeter of the wall's profile.  It is assumed to be in the same plane as the centerline, and will often be projected to that plane during internal operations.</summary>
        [Newtonsoft.Json.JsonProperty("Perimeter", Required = Newtonsoft.Json.Required.DisallowNull, NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore)]
        public Polygon Perimeter { get; set; }

        /// <summary>The Centerline of the wall</summary>
        [Newtonsoft.Json.JsonProperty("Centerline", Required = Newtonsoft.Json.Required.Default, NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore)]
        public Line Centerline { get; set; }

        /// <summary>
        /// Construct a wall by profile.
        /// </summary>
        /// <param name="perimeter">The perimeter of the wall elevation.</param>
        /// <param name="profile">This value should be left null.</param>
        /// <param name="thickness">The thickness of the wall.</param>
        /// <param name="centerline">The centerline of the wall.</param>
        /// <param name="transform">The transform of the wall.</param>
        /// <param name="material">The material of the wall.</param>
        /// <param name="representation">The representation of the wall.</param>
        /// <param name="isElementDefinition">Is the wall an element definition?</param>
        /// <param name="id">The id of the wall.</param>
        /// <param name="name">The name of the wall.</param>
        [Newtonsoft.Json.JsonConstructor]
        public WallByProfile(Polygon @perimeter,
                             Profile @profile,
                             double @thickness,
                             Line @centerline,
                             Transform @transform,
                             Material @material,
                             List<Opening> @openings,
                             Representation @representation,
                             bool @isElementDefinition,
                             System.Guid @id,
                             string @name)
            : base(transform, material, representation, isElementDefinition, id, name)
        {
            this.Perimeter = @perimeter;
            this.Profile = @profile;
            this.Thickness = @thickness;
            this.Centerline = @centerline;
            this.Openings.AddRange(@openings);
        }

        /// <summary>
        /// The profile of the wall computed from it's Perimeter and the openings.
        /// </summary>
        /// <returns></returns>
        public Profile GetProfile()
        {
            return new Profile(Perimeter, Openings.Select(o => o.Perimeter).ToList());
        }

        /// <summary>
        /// The computed height of the wall.
        /// </summary>
        public double GetHeight()
        {
            var bottom = Math.Min(Centerline.Start.Z, Centerline.End.Z);
            var top = Perimeter.Vertices.Max(v => v.Z);
            return top - bottom;
        }

        /// <summary>
        /// Create a wall from a profile and thickness.  If a centerline is not include it will be
        /// computed from the profile.  The profile will be projected to the
        /// centerline plane, and used to find openings of the Wall.
        /// </summary>
        public WallByProfile(Profile @profile,
                             double @thickness,
                             Line @centerline = null,
                             Transform @transform = null,
                             Material @material = null,
                             Representation @representation = null,
                             bool @isElementDefinition = false)
            : base(transform != null ? transform : new Transform(),
                   material != null ? material : BuiltInMaterials.Concrete,
                   representation != null ? representation : new Representation(new List<SolidOperation>()),
                   isElementDefinition,
                   Guid.NewGuid(),
                   "Wall by Profile")
        {
            var point = profile.Perimeter.Vertices.First();
            var centerPlane = new Plane(centerline.Start, centerline.End, centerline.End + Vector3.ZAxis);
            this.Perimeter = profile.Perimeter.Project(centerPlane);

            var perpendicularToWall = centerline.Direction().Cross(Vector3.ZAxis);
            foreach (var v in profile.Voids)
            {
                var opening = new Opening(v, 1.1 * thickness, 1.1 * thickness, normal: perpendicularToWall);
                this.Openings.Add(opening);
            }

            this.Thickness = @thickness;
            this.Centerline = @centerline;
        }

        /// <summary>Update the geometric representation of this wall.</summary>
        public override void UpdateRepresentations()
        {
            this.Representation.SolidOperations.Clear();

            if (this.Profile != null)
            {
                //TODO remove this geometry path once we completely delete the obsolete Profile property.
                // to ensure the correct direction, we find the direction form a point on the polygon to the vertical plane of the centerline
                var point = Profile.Perimeter.Vertices.First();
                var centerPlane = new Plane(Centerline.Start, Centerline.End, Centerline.End + Vector3.ZAxis);
                var direction = new Line(point, point.Project(centerPlane)).Direction();

                this.Representation.SolidOperations.Add(new Extrude(this.Profile, this.Thickness, direction, false));
            }
            else
            {
                var direction = Centerline.Direction().Cross(Vector3.ZAxis);
                var shiftedProfile = GetProfile().Transformed(new Transform(direction.Negate() * Thickness / 2));

                this.Representation.SolidOperations.Add(new Extrude(shiftedProfile, this.Thickness, direction, false));
            }
        }
    }
}