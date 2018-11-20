using Hypar.Geometry;
using Newtonsoft.Json;
using System.Collections.Generic;

namespace Hypar.Elements
{
    /// <summary>
    /// A Beam is a structural framing element which is often horizontal.
    /// </summary>
    public class Beam : StructuralFraming
    {
        /// <summary>
        /// The type of the element.
        /// </summary>
        public override string Type
        {
            get{return "beam";}
        }

        /// <summary>
        /// Construct a Beam.
        /// </summary>
        /// <param name="centerLine">The Beam's center line.</param>
        /// <param name="profile">The Beam's profile.</param>
        /// <param name="material">The Beam's material.</param>
        /// <param name="up">The Beam's up axis.</param>
        /// <param name="startSetback">The setback of the Beam's geometry at the start.</param>
        /// <param name="endSetback">The setback of the Beam's geometry at the end.</param>
        [JsonConstructor]
        public Beam(ICurve centerLine, Profile profile, Material material = null, Vector3 up = null, double startSetback = 0.0, double endSetback = 0.0) : base(centerLine, profile, material, up, startSetback, endSetback){}
    }
}