using System;
using Elements.Geometry;
using Elements.Geometry.Solids;
using Elements.Interfaces;
using Newtonsoft.Json;

namespace Elements
{
    /// <summary>
    /// A polygonal opening.
    /// The opening extends to 'depth' in the z direction of the supplied transform.
    /// </summary>
    [UserElement]
    public class Opening : Element, IGeometry, IMaterial
    {
        /// <summary>
        /// The profile of the opening.
        /// </summary>
        public Profile Profile { get; private set; }

        /// <summary>
        /// The depth of the opening's extrusion.
        /// </summary>
        public double Depth { get; }

        /// <summary>
        /// The opening's geometry.
        /// </summary>
        public Geometry.Geometry Geometry { get; } = new Geometry.Geometry();

        /// <summary>
        /// The default void material.
        /// </summary>
        public Material Material => BuiltInMaterials.Void;

        /// <summary>
        /// Create a rectangular opening.
        /// </summary>
        /// <param name="x">The distance along the X axis of the transform of the host element to the center of the opening.</param>
        /// <param name="y">The distance along the Y axis of the transform of the host element to the center of the opening.</param>
        /// <param name="width">The width of the opening.</param>
        /// <param name="height">The height of the opening.</param>
        /// <param name="depth">The depth of the opening's extrusion.</param>
        /// <param name="transform">The opening's transform.</param>
        /// <param name="id">The id of the opening..</param>
        /// <param name="name">The name of the opening.</param>
        public Opening(double x,
                       double y,
                       double width,
                       double height,
                       double depth = 5.0,
                       Transform transform = null,
                       Guid id = default(Guid),
                       string name = null) : base(id, name, transform)
        {
            this.Profile = new Profile(Polygon.Rectangle(width, height, new Vector3(x, y)));
            this.Profile.Transform(this.Transform);
            this.Depth = depth;
            this.Geometry.SolidOperations.Add(new Extrude(this.Profile, this.Depth, this.Transform.ZAxis, true));
        }

        /// <summary>
        /// Create a polygonal opening.
        /// </summary>
        /// <param name="perimeter">A polygon representing the perimeter of the opening.</param>
        /// <param name="x">The distance along the X axis of the transform of the host element to transform the profile.</param>
        /// <param name="y">The distance along the Y axis of the transform of the host element to transform the profile.</param>
        /// <param name="depth">The depth of the opening's extrusion.</param>
        /// <param name="transform">The opening's transform.</param>
        /// <param name="id">The id of the opening.</param>
        /// <param name="name">The name of the opening.</param>
        public Opening(Polygon perimeter,
                       double x = 0.0,
                       double y = 0.0,
                       double depth = 5.0,
                       Transform transform = null,
                       Guid id = default(Guid),
                       string name = null) : base(id, name, transform)
        {
            this.Transform = new Transform(x,y,0);
            this.Depth = depth;
            this.Profile = perimeter;
            this.Profile.Transform(this.Transform);
            this.Geometry.SolidOperations.Add(new Extrude(this.Profile, this.Depth, this.Transform.ZAxis, true));
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
            this.Profile = this.Profile; // Don't re-transform the profile.
            this.Depth = depth;
            this.Geometry.SolidOperations.Add(new Extrude(this.Profile, this.Depth, this.Transform.ZAxis, true));
        }
    }
}