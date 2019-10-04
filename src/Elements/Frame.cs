using Elements.Interfaces;
using Elements.Geometry.Interfaces;
using Elements.Geometry;
using Elements.Geometry.Solids;

namespace Elements
{
    /// <summary>
    /// An element defined by a perimeter and a cross section swept along that perimeter.
    /// </summary>
    public class Frame : Element, IProfile, IMaterial, ISweepAlongCurve
    {
        /// <summary>
        /// The frame's profile.
        /// </summary>
        public Profile Profile { get; }

        /// <summary>
        /// The frame's material.
        /// </summary>
        public Material Material { get; }

        /// <summary>
        /// The perimeter of the frame.
        /// </summary>
        public Curve Curve { get; }

        /// <summary>
        /// The start setback of the sweep along the curve.
        /// </summary>
        public double StartSetback => 0.0;

        /// <summary>
        /// The end setback of the sweep along the curve.
        /// </summary>
        public double EndSetback => 0.0;

        /// <summary>
        /// Create a frame.
        /// </summary>
        /// <param name="curve">The frame's perimeter.</param>
        /// <param name="profile">The frame's profile.</param>
        /// <param name="offset">The amount which the perimeter will be offset internally.</param>
        /// <param name="material">The frame's material.</param>
        /// <param name="transform">The frame's transform.</param>
        public Frame(Polygon curve, Profile profile, double offset = 0.0, Material material = null, Transform transform = null)
        {
            this.Curve = curve.Offset(-offset)[0];
            this.Profile = profile;
            this.Transform = transform;
            this.Material = material != null ? material : BuiltInMaterials.Default;
        }

        /// <summary>
        /// Get the updated solid representation of the frame.
        /// </summary>
        /// <returns></returns>
        public Solid GetUpdatedSolid()
        {
            return Kernel.Instance.CreateSweepAlongCurve(this);
        }
    }
}