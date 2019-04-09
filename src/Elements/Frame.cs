using Elements.Interfaces;
using Elements.Geometry.Interfaces;
using Elements.Geometry;
using Elements.Geometry.Solids;
using Newtonsoft.Json;

namespace Elements
{
    /// <summary>
    /// An element defined by a perimeter and a cross section swept along that perimeter.
    /// </summary>
    public class Frame : Element, IGeometry3D, IProfile
    {
        /// <summary>
        /// The perimeter of the frame.
        /// </summary>
        public Polygon Perimeter{get;}

        /// <summary>
        /// The frame's profile.
        /// </summary>
        public Profile Profile{get;}

        /// <summary>
        /// The frame's geometry.
        /// </summary>
        /// <value></value>
        public Solid[] Geometry{get;}

        /// <summary>
        /// Create a frame.
        /// </summary>
        /// <param name="perimeter">The frame's perimeter.</param>
        /// <param name="profile">The frame's profile.</param>
        /// <param name="offset">The amount which the perimeter will be offset internally.</param>
        /// <param name="material">The frame's material.</param>
        /// <param name="transform">The frame's transform.</param>
        public Frame(Polygon perimeter, Profile profile, double offset = 0.0, Material material = null, Transform transform = null)
        {
            this.Perimeter = perimeter.Offset(-offset)[0];
            this.Profile = profile;
            this.Transform = transform;
            this.Geometry = new[]{Solid.SweepFaceAlongCurve(profile.Perimeter, profile.Voids, this.Perimeter)};
        }
    }
}