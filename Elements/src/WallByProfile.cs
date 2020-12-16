using System;
using System.Collections.Generic;
using System.Linq;
using Elements.Geometry;
using Elements.Geometry.Solids;

namespace Elements
{
    /// <summary>
    /// A wall drawn using the elevation profile.
    /// </summary>
    public partial class WallByProfile : GeometricElement
    {
        /// <summary>
        /// The profile of the wall.
        /// </summary>
        public Profile Profile { get; set; }

        /// <summary>
        /// The wall's thickness.
        /// </summary>
        /// <value></value>
        public double Thickness { get; set; }

        /// <summary>
        /// The wall's center line.
        /// </summary>
        /// <value></value>
        public Line Centerline { get; set}

        /// <summary>
        /// Create a wall requiring only the profile, thickness and centerline.
        /// </summary>
        public WallByProfile(Profile @profile,
                             double @thickness,
                             Line @centerline,
                             Transform @transform = null,
                             Material @material = null,
                             IList<Representation> @representations = null,
                             bool @isElementDefinition = false)
            : base(transform != null ? transform : new Transform(),
                   representations != null ? representations : new[] { new SolidRepresentation(material != null ? material : BuiltInMaterials.Concrete) },
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

            var rep = (SolidRepresentation)this.Representations[0];
            rep.SolidOperations.Clear();

            // to ensure the correct direction, we find the direction form a point on the polygon to the vertical plane of the centerline
            var point = Profile.Perimeter.Vertices.First();
            var centerPlane = new Plane(Centerline.Start, Centerline.End, Centerline.End + Vector3.ZAxis);
            var direction = new Line(point, point.Project(centerPlane)).Direction();

            rep.SolidOperations.Add(new Extrude(this.Profile, this.Thickness, direction, false));
        }
    }
}