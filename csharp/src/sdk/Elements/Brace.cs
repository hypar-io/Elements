using Hypar.Geometry;
using Newtonsoft.Json;
using System.Collections.Generic;

namespace Hypar.Elements
{
    /// <summary>
    /// A Brace is a structural framing element which is often diagonal.
    /// </summary>
    public class Brace : StructuralFraming
    {
        /// <summary>
        /// The type of the element.
        /// </summary>
        public override string Type
        {
            get{return "brace";}
        }

        /// <summary>
        /// Construct a Brace.
        /// </summary>
        /// <param name="centerLine">The Brace's center line.</param>
        /// <param name="profile">The Brace's profile.</param>
        /// <param name="material">The Brace's material.</param>
        /// <param name="up">The Brace's up axis.</param>
        /// <param name="startSetback">The setback of the Brace's geometry at the start.</param>
        /// <param name="endSetback">The setback of the Brace's geometry at the end.</param>
        /// [JsonConstructor]
        public Brace(ICurve centerLine, Profile profile, Material material = null, Vector3 up = null, double startSetback = 0.0, double endSetback = 0.0) : base(centerLine, profile, material, up, startSetback, endSetback){}
    }
}