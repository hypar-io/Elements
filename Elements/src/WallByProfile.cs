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
        public new Profile Profile { get; set; }

        /// <summary>The overall thickness of the wall</summary>
        [Newtonsoft.Json.JsonProperty("Thickness", Required = Newtonsoft.Json.Required.DisallowNull, NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore)]
        public double Thickness { get; set; }

        /// <summary>The Centerline of the wall</summary>
        [Newtonsoft.Json.JsonProperty("Centerline", Required = Newtonsoft.Json.Required.Default, NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore)]
        public Line Centerline { get; set; }

        /// <summary>
        /// Construct a wall by profile.
        /// </summary>
        /// <param name="profile">The elevation of the wall.</param>
        /// <param name="thickness">The thickness of the wall.</param>
        /// <param name="centerline">The centerline of the wall.</param>
        /// <param name="transform">The transform of the wall.</param>
        /// <param name="material">The material of the wall.</param>
        /// <param name="representation">The representation of the wall.</param>
        /// <param name="isElementDefinition">Is the wall an element definition?</param>
        /// <param name="id">The id of the wall.</param>
        /// <param name="name">The name of the wall.</param>
        [Newtonsoft.Json.JsonConstructor]
        public WallByProfile(Profile @profile,
                             double @thickness,
                             Line @centerline,
                             Transform @transform,
                             Material @material,
                             Representation @representation,
                             bool @isElementDefinition,
                             System.Guid @id,
                             string @name)
            : base(transform, material, representation, isElementDefinition, id, name)
        {
            this.Profile = @profile;
            this.Thickness = @thickness;
            this.Centerline = @centerline;
        }

        /// <summary>
        /// Create a wall requiring only the profile, thickness and centerline.
        /// </summary>
        public WallByProfile(Profile @profile,
                             double @thickness,
                             Line @centerline,
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

            this.Profile = @profile;
            this.Thickness = @thickness;
            this.Centerline = @centerline;
        }

        /// <summary>Update the geometric representation of this wall.</summary>
        public override void UpdateRepresentations()
        {
            this.Representation.SolidOperations.Clear();

            // to ensure the correct direction, we find the direction form a point on the polygon to the vertical plane of the centerline
            var point = Profile.Perimeter.Vertices.First();
            var centerPlane = new Plane(Centerline.Start, Centerline.End, Centerline.End + Vector3.ZAxis);
            var direction = new Line(point, point.Project(centerPlane)).Direction();

            this.Representation.SolidOperations.Add(new Extrude(this.Profile, this.Thickness, direction, false));
        }
    }
}