using System;
using Elements.Geometry;
using Elements.Geometry.Solids;
using Elements.Interfaces;
using Newtonsoft.Json;

namespace Elements
{
    /// <summary>
    /// A polygonal opening.
    /// Openings have a local placement defined by the x and y coordinates as well as a transform. 
    /// </summary>
    [UserElement]
    public class Opening : Element, IGeometry
    {
        /// <summary>
        /// The profile of the opening.
        /// </summary>
        public Profile Profile { get; private set; }

        /// <summary>
        /// The opening's geometry.
        /// </summary>
        public Geometry.Geometry Geometry { get; } = new Geometry.Geometry();

        /// <summary>
        /// Create a rectangular opening.
        /// </summary>
        /// <param name="x">The distance along the x axis to the center of the opening.</param>
        /// <param name="y">The distance along the y axis to the center of the opening.</param>
        /// <param name="width">The width of the opening.</param>
        /// <param name="height">The height of the opening.</param>
        /// <param name="transform">The opening's transform.</param>
        /// <param name="id">The id of the opening..</param>
        /// <param name="name">The name of the opening.</param>
        public Opening(double x,
                       double y,
                       double width,
                       double height,
                       Transform transform = null,
                       Guid id = default(Guid),
                       string name = null) : base(id, name)
        {
            this.Profile = new Profile(Polygon.Rectangle(width, height));
            this.Transform = transform != null ? transform : new Transform();
            this.Profile.Transform(new Transform(new Vector3(x, y)));
        }

        /// <summary>
        /// Create a polygonal opening.
        /// </summary>
        /// <param name="perimeter">A polygon representing the perimeter of the opening.</param>
        /// <param name="x">The distance along the x to transform the profile.</param>
        /// <param name="y">The distance along the y to transform the profile.</param>
        /// <param name="transform">The opening's transform.</param>
        /// <param name="id">The id of the opening.</param>
        /// <param name="name">The name of the opening.</param>
        public Opening(Polygon perimeter,
                       double x = 0.0,
                       double y = 0.0,
                       Transform transform = null,
                       Guid id = default(Guid),
                       string name = null) : base(id, name)
        {
            this.Transform = transform != null ? transform : new Transform();
            this.Transform.Move(new Vector3(x, y));
            this.Profile = perimeter;
            this.Profile.Transform(new Transform(new Vector3(x, y)));
        }

        /// <summary>
        /// Create an opening.
        /// </summary>
        /// <param name="profile">A polygon representing the perimeter of the opening.</param>
        /// <param name="depth">The depth of the opening's extrusion.</param>
        /// <param name="transform">An additional transform applied to the opening.</param>
        /// <param name="id">The id of the opening.</param>
        /// <param name="name">The name of the opening.</param>
        [JsonConstructor]
        public Opening(Profile profile,
                       double depth,
                       Transform transform = null,
                       Guid id = default(Guid),
                       string name = null) : base(id, name, transform)
        {
            this.Profile = profile; // Don't re-transform the profile.
        }

        /// <summary>
        /// Update solid operations.
        /// </summary>
        public void UpdateSolidOperations()
        {
            // TODO(Ian): Give this a proper depth when booleans are supported.
            this.Geometry.SolidOperations.Add(new Extrude(this.Profile, 5, this.Transform.ZAxis, 0.0, true));
        }
    }
}