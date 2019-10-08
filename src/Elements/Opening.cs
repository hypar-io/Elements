using Elements.Interfaces;
using System;
using Elements.Geometry;
using Elements.Geometry.Solids;
using Newtonsoft.Json;

namespace Elements
{
    /// <summary>
    /// A rectangular opening in a wall or floor.
    /// </summary>
    [UserElement]
    public class Opening : Element, IGeometry
    {
        /// <summary>
        /// The perimeter of the opening.
        /// </summary>
        /// <value>A polygon of Width and Height translated by X and Y.</value>
        public Profile Profile { get; private set; }

        /// <summary>
        /// The extrude direction of the opening.
        /// </summary>     
        public Vector3 Direction => Vector3.ZAxis;

        /// <summary>
        /// The depth of the opening's extrusion.
        /// </summary>
        public double Depth { get; }

        /// <summary>
        /// The opening's geometry.
        /// </summary>
        public Geometry.Geometry Geometry { get; }

        /// <summary>
        /// Create a rectangular opening.
        /// </summary>
        /// <param name="x">The distance along the X axis of the transform of the host element to the center of the opening.</param>
        /// <param name="y">The distance along the Y axis of the transform of the host element to the center of the opening.</param>
        /// <param name="width">The width of the opening.</param>
        /// <param name="height">The height of the opening.</param>
        /// <param name="depth">The depth of the opening's extrusion.</param>
        /// <param name="id"></param>
        /// <param name="name"></param>
        public Opening(double x,
                       double y,
                       double width,
                       double height,
                       double depth = 5.0,
                       Guid id = default(Guid),
                       string name = null) : base(id, name)
        {
            this.Profile = new Profile(Polygon.Rectangle(width, height, new Vector3(x, y)));
            this.Depth = depth;
            this.Geometry.SolidOperations.Add(new Extrude(this.Profile, this.Depth, this.Direction));
        }

        /// <summary>
        /// Create a polygonal opening.
        /// </summary>
        /// <param name="perimeter">A polygon representing the perimeter of the opening.</param>
        /// <param name="x">The distance along the X axis of the transform of the host element to transform the profile.</param>
        /// <param name="y">The distance along the Y axis of the transform of the host element to transform the profile.</param>
        /// <param name="depth">The depth of the opening's extrusion.</param>
        /// <param name="id">The id of the opening.</param>
        /// <param name="name">The name of the opening.</param>
        public Opening(Polygon perimeter,
                       double x = 0.0,
                       double y = 0.0,
                       double depth = 5.0,
                       Guid id = default(Guid),
                       string name = null) : base(id, name)
        {
            var t = new Transform(x, y, 0.0);
            this.Depth = depth;
            this.Profile = t.OfProfile(new Profile(perimeter));
            this.Geometry.SolidOperations.Add(new Extrude(this.Profile, this.Depth, this.Direction));
        }

        /// <summary>
        /// Create an opening.
        /// </summary>
        /// <param name="perimeter">A polygon representing the perimeter of the opening.</param>
        /// <param name="depth">The depth of the opening's extrusion.</param>
        /// <param name="transform">An additional transform applied to the opening.</param>
        /// <param name="id">The id of the opening.</param>
        /// <param name="name">The name of the opening.</param>
        public Opening(Polygon perimeter,
                       double depth,
                       Transform transform = null,
                       Guid id = default(Guid),
                       string name = null) : base(id, name, transform)
        {
            this.Profile = new Profile(perimeter);
            this.Depth = depth;
            this.Geometry.SolidOperations.Add(new Extrude(this.Profile, this.Depth, this.Direction));
        }

        /// <summary>
        /// Create an opening.
        /// </summary>
        /// <param name="profile">A polygon representing the profile of the opening.</param>
        /// <param name="extrudeDepth">The depth of the opening's extrusion.</param>
        /// <param name="transform">An additional transform applied to the opening.</param>
        /// <param name="id">The id of the opening.</param>
        /// <param name="name">The name of the opening.</param>
        [JsonConstructor]
        internal Opening(Profile profile,
                         double extrudeDepth,
                         Transform transform = null,
                         Guid id = default(Guid),
                         string name = null) : base(id, name, transform)
        {
            this.Profile = profile;
            this.Depth = extrudeDepth;
            this.Geometry.SolidOperations.Add(new Extrude(this.Profile, this.Depth, this.Direction));
        }
    }
}