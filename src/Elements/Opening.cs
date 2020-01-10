using System;
using System.Collections.Generic;
using Elements.Geometry;
using Elements.Geometry.Solids;
using Newtonsoft.Json;

namespace Elements
{
    /// <summary>
    /// A polygonal opening.
    /// An opening's placement is defined by the x and y coordinates.
    /// The direction of the opening corresponds to the +Z axis of the transform.
    /// </summary>
    [UserElement]
    public class Opening : GeometricElement
    {
        /// <summary>
        /// The profile of the opening.
        /// </summary>
        public Profile Profile { get; set; }

        /// <summary>
        /// Create a rectangular opening.
        /// </summary>
        /// <param name="x">The distance along the x axis to the center of the opening.</param>
        /// <param name="y">The distance along the y axis to the center of the opening.</param>
        /// <param name="width">The width of the opening.</param>
        /// <param name="height">The height of the opening.</param>
        /// <param name="transform">The opening's transform.</param>
        public Opening(double x,
                       double y,
                       double width,
                       double height,
                       Transform transform = null) : base(transform = transform != null ? transform : new Transform(),
                                                          BuiltInMaterials.Void,
                                                          new Representation(new List<SolidOperation>()),
                                                          Guid.NewGuid(),
                                                          null)
        {
            this.Profile = new Profile(Polygon.Rectangle(width, height));
            this.Profile.Transform(new Transform(new Vector3(x, y)));
        }

        /// <summary>
        /// Create a polygonal opening.
        /// </summary>
        /// <param name="perimeter">A polygon representing the perimeter of the opening.</param>
        /// <param name="x">The distance along the x to transform the profile.</param>
        /// <param name="y">The distance along the y to transform the profile.</param>
        /// <param name="transform">The opening's transform.</param>
        public Opening(Polygon perimeter,
                       double x = 0.0,
                       double y = 0.0,
                       Transform transform = null) : base(transform = transform != null ? transform : new Transform(),
                                                          BuiltInMaterials.Void,
                                                          new Representation(new List<SolidOperation>()),
                                                          Guid.NewGuid(),
                                                          null)
        {
            this.Profile = perimeter;
            this.Profile.Transform(new Transform(new Vector3(x, y)));
        }

        /// <summary>
        /// Create an opening.
        /// </summary>
        /// <param name="profile">A polygon representing the perimeter of the opening.</param>
        /// <param name="depth">The depth of the opening's extrusion.</param>
        /// <param name="transform">The opening's transform.</param>
        /// <param name="representation">The opening's representation.</param>
        /// <param name="id">The id of the opening.</param>
        /// <param name="name">The name of the opening.</param>
        [JsonConstructor]
        public Opening(Profile profile,
                       double depth,
                       Transform transform = null,
                       Representation representation = null,
                       Guid id = default(Guid),
                       string name = null) : base(transform != null ? transform : new Transform(),
                                                  BuiltInMaterials.Void,
                                                  representation != null ? representation : new Representation(new List<SolidOperation>()),
                                                  id != default(Guid) ? id : Guid.NewGuid(),
                                                  name)
        {
            this.Profile = profile; // Don't re-transform the profile.
        }

        /// <summary>
        /// Update representations
        /// </summary>
        public override void UpdateRepresentations()
        {
            this.Representation.SolidOperations.Clear();
            // TODO(Ian): Give this a proper depth when booleans are supported.
            this.Representation.SolidOperations.Add(new Extrude(this.Profile, 5, this.Transform.ZAxis, 0.0, true));
        }
    }
}