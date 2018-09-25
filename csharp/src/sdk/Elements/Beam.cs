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
        [JsonConstructor]
        public Beam(Line centerLine, Profile profile, Material material = null, Vector3 up = null) : base(centerLine, profile, material, up){}
    }
}