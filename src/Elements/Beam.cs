using Elements.Geometry;
using Elements.Geometry.Interfaces;
using Newtonsoft.Json;
using System.Collections.Generic;

namespace Elements
{
    /// <summary>
    /// A Beam is a structural framing element which is often horizontal.
    /// </summary>
    public class Beam : StructuralFraming
    {
        /// <summary>
        /// Construct a Beam.
        /// </summary>
        /// <param name="curve">The Beam's center line.</param>
        /// <param name="profile">The Beam's profile.</param>
        /// <param name="material">The Beam's material.</param>
        /// <param name="up">The Beam's up axis.</param>
        /// <param name="startSetback">The setback of the Beam's geometry at the start.</param>
        /// <param name="endSetback">The setback of the Beam's geometry at the end.</param>
        /// <param name="transform">The Beam's Transform.</param>
        [JsonConstructor]
        public Beam(ICurve curve, Profile profile, Material material = null, Vector3 up = null, double startSetback = 0.0, double endSetback = 0.0, Transform transform = null) : base(curve, profile, material, up, startSetback, endSetback, transform) { }
    }
}