using System;
using Elements.Geometry;
using Elements.Geometry.Interfaces;
using Elements.Geometry.Solids;
using Newtonsoft.Json;

namespace Elements
{
    /// <summary>
    /// A rectangular opening in a wall or floor.
    /// </summary>
    public class Opening : Element, IExtrude
    {
        Guid _profileId;

        /// <summary>
        /// The perimeter of the opening.
        /// </summary>
        /// <value>A polygon of Width and Height translated by X and Y.</value>
        [JsonIgnore]
        public Profile Profile { get; internal set; }

        /// <summary>
        /// The profile id.
        /// </summary>
        public Guid ProfileId
        {
            get
            {
                return this.Profile != null ? this.Profile.Id : this._profileId;
            }
        }

        /// <summary>
        /// The extrude direction of the opening.
        /// </summary>     
        public Vector3 ExtrudeDirection => Vector3.ZAxis;

        /// <summary>
        /// The depth of the opening's extrusion.
        /// </summary>
        public double ExtrudeDepth { get; }

        /// <summary>
        /// Extrude to both sides?
        /// </summary>
        public bool BothSides => true;

        /// <summary>
        /// Create a rectangular opening.
        /// </summary>
        /// <param name="x">The distance along the X axis of the transform of the host element to the center of the opening.</param>
        /// <param name="y">The distance along the Y axis of the transform of the host element to the center of the opening.</param>
        /// <param name="width">The width of the opening.</param>
        /// <param name="height">The height of the opening.</param>
        /// <param name="depth">The depth of the opening's extrusion.</param>
        public Opening(double x, double y, double width, double height, double depth = 5.0)
        {
            this.Profile = new Profile(Polygon.Rectangle(width, height, new Vector3(x, y)));
            this.ExtrudeDepth = depth;
        }

        /// <summary>
        /// Create a polygonal opening.
        /// </summary>
        /// <param name="profile">A polygon representing the profile of the opening.</param>
        /// <param name="x">The distance along the X axis of the transform of the host element to transform the profile.</param>
        /// <param name="y">The distance along the Y axis of the transform of the host element to transform the profile.</param>
        /// <param name="depth">The depth of the opening's extrusion.</param>
        public Opening(Polygon profile, double x = 0.0, double y = 0.0, double depth = 5.0)
        {
            var t = new Transform(x, y, 0.0);
            this.ExtrudeDepth = depth;
            this.Profile = t.OfProfile(new Profile(profile));
        }

        /// <summary>
        /// Create an opening.
        /// </summary>
        /// <param name="profile">A polygon representing the profile of the opening.</param>
        /// <param name="depth">The depth of the opening's extrusion.</param>
        /// <param name="transform">An additional transform applied to the opening.</param>
        public Opening(Polygon profile, double depth, Transform transform = null)
        {
            this.Profile = new Profile(profile);
            this.Transform = transform;
            this.ExtrudeDepth = depth;
        }

        /// <summary>
        /// Create an opening.
        /// </summary>
        /// <param name="profile">A polygon representing the profile of the opening.</param>
        /// <param name="extrudeDepth">The depth of the opening's extrusion.</param>
        /// <param name="transform">An additional transform applied to the opening.</param>
        internal Opening(Profile profile, double extrudeDepth, Transform transform = null)
        {
            this.Profile = profile;
            this.Transform = transform;
            this.ExtrudeDepth = extrudeDepth;
        }

        [JsonConstructor]
        internal Opening(Guid profileId, double extrudeDepth, Transform transform = null)
        {
            this._profileId = profileId;
            this.Transform = transform;
            this.ExtrudeDepth = extrudeDepth;
        }

        /// <summary>
        /// Get the updated solid representation of the opening.
        /// </summary>
        public Solid GetUpdatedSolid()
        {
            return Kernel.Instance.CreateExtrude(this);
        }

        /// <summary>
        /// Set the profile.
        /// </summary>
        public void SetReference(Profile profile)
        {
            this.Profile = profile;
            this._profileId = profile.Id;
        }
    }
}