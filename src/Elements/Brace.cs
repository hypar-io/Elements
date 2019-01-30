using Elements.Geometry;
using Elements.Geometry.Interfaces;
using Newtonsoft.Json;
using System.Collections.Generic;

namespace Elements
{
    /// <summary>
    /// A Brace is a structural framing element which is often diagonal.
    /// </summary>
    public class Brace : StructuralFraming
    {
        /// <summary>
        /// Construct a Brace.
        /// </summary>
        /// <param name="curve">The Brace's center line.</param>
        /// <param name="profile">The Brace's profile.</param>
        /// <param name="material">The Brace's material.</param>
        /// <param name="startSetback">The setback of the Brace's geometry at the start.</param>
        /// <param name="endSetback">The setback of the Brace's geometry at the end.</param>
        /// [JsonConstructor]
        public Brace(ICurve curve, Profile profile, Material material = null, double startSetback = 0.0, double endSetback = 0.0) : base(curve, profile, material, startSetback, endSetback){}
    }
}