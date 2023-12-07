using System;
using System.Collections.Generic;
using System.Linq;
using Elements.Geometry;
using Elements.Geometry.Solids;
using System.Text.Json.Serialization;

namespace Elements
{
    /// <summary>
    /// A wall drawn using the elevation profile
    /// </summary>
    public class WallByProfile : Wall
    {
        /// <summary>The overall thickness of the Wall</summary>
        public double Thickness { get; set; }

        /// <summary>
        /// The perimeter of the Wall's elevation.  It is assumed to be in the same Plane as the Centerline,
        /// and will often be projected to that Plane during internal operations.
        /// </summary>
        public Polygon Perimeter { get; set; }

        /// <summary>The Centerline of the wall</summary>
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
        /// <param name="openings">The openings of the wall.</param>
        /// <param name="representation">The representation of the wall.</param>
        /// <param name="isElementDefinition">Is the wall an element definition?</param>
        /// <param name="id">The id of the wall.</param>
        /// <param name="name">The name of the wall.</param>
        [JsonConstructor]
        [Obsolete("Do not use.  This constructor is only preserved to maintain backwards compatibility upon serialization/deserialization.")]
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
            if (@openings != null && @openings.Count > 0)
            {
                this.Openings.AddRange(@openings);
            }
        }

        /// <summary>
        /// Construct a wall by profile.
        /// </summary>
        /// <param name="perimeter">The perimeter of the wall elevation.</param>
        /// <param name="thickness">The thickness of the wall.</param>
        /// <param name="centerline">The centerline of the wall.</param>
        /// <param name="transform">The transform of the wall.</param>
        /// <param name="material">The material of the wall.</param>
        /// <param name="representation">The representation of the wall.</param>
        /// <param name="isElementDefinition">Is the wall an element definition?</param>
        /// <param name="name">The name of the wall.</param>
        public WallByProfile(Polygon @perimeter,
                             double @thickness,
                             Line @centerline,
                             Transform @transform = null,
                             Material @material = null,
                             Representation @representation = null,
                             bool @isElementDefinition = false,
                             string @name = "Wall by Profile")
            : base(transform, material, representation, isElementDefinition, Guid.NewGuid(), name)
        {
            this.Thickness = @thickness;
            this.Centerline = @centerline;
            this.Perimeter = @perimeter.Project(GetCenterPlane());
#pragma warning disable 612, 618
            this.Profile = GetProfile();
#pragma warning restore 612, 618
        }

        /// <summary>
        /// The Profile of the Wall computed from its Perimeter and the Openings.
        /// </summary>
        /// <returns></returns>
        public Profile GetProfile()
        {
#pragma warning disable 612, 618
            if (Perimeter == null && Profile != null) // this might be a legacy style WallByProfile, we should check for Profile directly
            {
                return Profile;
            }
#pragma warning restore 612, 618
            return new Profile(Perimeter, Openings.Select(o => o.Perimeter).ToList());
        }

        /// <summary>
        /// The computed height of the Wall.
        /// </summary>
        public double GetHeight()
        {
            var bottom = Math.Min(Centerline.Start.Z, Centerline.End.Z);
            var top = Perimeter.Vertices.Max(v => v.Z);
            return top - bottom;
        }

        /// <summary>
        /// Create a Wall from a Profile and thickness.  If centerline is not included it will be
        /// computed from the Profile.  The Profile will be projected to the
        /// centerline Plane, and used to find Openings of the Wall.
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
            this.Centerline = @centerline;
            this.Thickness = @thickness;
            this.Perimeter = profile.Perimeter.Project(GetCenterPlane());

            foreach (var aVoid in profile.Voids)
            {
                AddOpening(aVoid, thickness, thickness);
            }

            // TODO remove when we remove Profile.
            var perpendicularToWall = Centerline.Direction().Cross(Vector3.ZAxis);
#pragma warning disable 612, 618
            this.Profile = GetProfile().Transformed(new Transform(perpendicularToWall * this.Thickness / 2));
#pragma warning restore 612, 618
        }

        /// <summary>
        /// Add an Opening to the Wall.
        /// </summary>
        /// <param name="perimeter"></param>
        /// <param name="depthFront"></param>
        /// <param name="depthBack"></param>
        public void AddOpening(Polygon perimeter, double depthFront = 1, double depthBack = 1)
        {
            var perpendicularToWall = Centerline.Direction().Cross(Vector3.ZAxis);
            var voidOnCenterline = perimeter.Project(GetCenterPlane());
            var opening = new Opening(voidOnCenterline, perpendicularToWall, depthFront, depthBack);
            this.Openings.Add(opening);
        }

        /// <summary>Update the geometric representation of this Wall.</summary>
        public override void UpdateRepresentations()
        {
            if (Representation == null)
            {
                Representation = new Representation(new List<SolidOperation>());
            }
            this.Representation?.SolidOperations?.Clear();
            var direction = Centerline.Direction().Cross(Vector3.ZAxis);
            var shiftedProfile = GetProfile().Transformed(new Transform(direction.Negate() * Thickness / 2));

            this.Representation.SolidOperations.Add(new Extrude(shiftedProfile, this.Thickness, direction, false));
        }

        private Plane GetCenterPlane()
        {
            return new Plane(Centerline.Start, Centerline.End, Centerline.End + Vector3.ZAxis);
        }
    }
}