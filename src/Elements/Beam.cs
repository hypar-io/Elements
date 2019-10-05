using System;
using Elements.ElementTypes;
using Elements.Geometry;
using Newtonsoft.Json;

namespace Elements
{
    /// <summary>
    /// A structural framing element defined by a center line curve and a profile.
    /// </summary>
    /// <example>
    /// [!code-csharp[Main](../../test/Examples/BeamExample.cs?name=example)]
    /// </example>
    [UserElement]
    public class Beam : StructuralFraming
    {
        /// <summary>
        /// Construct a beam.
        /// </summary>
        /// <param name="curve">The beam's center line.</param>
        /// <param name="elementType">The beam's structural framing type.</param>
        /// <param name="startSetback">The setback of the beam's geometry at the start.</param>
        /// <param name="endSetback">The setback of the beam's geometry at the end.</param>
        /// <param name="transform">The beam's transform.</param>
        public Beam(Curve curve, StructuralFramingType elementType, 
            double startSetback = 0.0, double endSetback = 0.0, Transform transform = null) 
            : base(curve, elementType, startSetback, endSetback, transform) { }

        [JsonConstructor]
        internal Beam(Curve curve, Guid elementTypeId, 
            double startSetback = 0.0, double endSetback = 0.0, Transform transform = null) 
            : base(curve, elementTypeId, startSetback, endSetback, transform) { }
    }
}