using System;
using Elements.ElementTypes;
using Elements.Geometry;
using Newtonsoft.Json;

namespace Elements
{
    /// <summary>
    /// A Brace is a structural framing element which is often diagonal.
    /// </summary>
    [UserElement]
    public class Brace : StructuralFraming
    {
        /// <summary>
        /// Construct a Brace.
        /// </summary>
        /// <param name="curve">The brace's center line.</param>
        /// <param name="elementType">The structural framing type of the brace.</param>
        /// <param name="startSetback">The setback of the brace's geometry at the start.</param>
        /// <param name="endSetback">The setback of the brace's geometry at the end.</param>
        public Brace(Curve curve, StructuralFramingType elementType, 
            double startSetback = 0.0, double endSetback = 0.0) 
            : base(curve, elementType, startSetback, endSetback){}
    
        [JsonConstructor]
        internal Brace(Curve curve, Guid elementTypeId, 
            double startSetback = 0.0, double endSetback = 0.0) 
            : base(curve, elementTypeId, startSetback, endSetback){}
    }
}