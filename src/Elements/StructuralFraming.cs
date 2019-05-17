using Newtonsoft.Json;
using System;
using Elements.Geometry;
using Elements.Geometry.Interfaces;
using Elements.Serialization.JSON;
using Elements.Interfaces;

namespace Elements
{
    /// <summary>
    /// A structural element with a Profile swept along a curve or extruded.
    /// </summary>
    public abstract class StructuralFraming : Element, IElementType<StructuralFramingType>, ISweepAlongCurve
    {
        /// <summary>
        /// The center line of the framing element.
        /// </summary>
        [JsonConverter(typeof(ICurveConverter))]
        public ICurve Curve { get; }

        /// <summary>
        /// The setback of the framing's extrusion at the start.
        /// </summary>
        public double StartSetback { get; }

        /// <summary>
        /// The setback of the framing's extrusion at the end.
        /// </summary>
        public double EndSetback { get; }

        /// <summary>
        /// The element type of the structural framing.
        /// </summary>
        public StructuralFramingType ElementType {get;}

        /// <summary>
        /// The extrusion's profile.
        /// </summary>
        [JsonIgnore]
        public Profile Profile {
            get
            {
                return this.ElementType.Profile;
            }
        }


        /// <summary>
        /// Construct a beam.
        /// </summary>
        /// <param name="curve">The center line of the beam.</param>
        /// <param name="elementType">The structural framing type.</param>
        /// <param name="startSetback">The setback of the beam's extrusion at its start.</param>
        /// <param name="endSetback">The setback of the beam's extrusion at its end.</param>
        /// <param name="transform">The element's Transform.</param>
        [JsonConstructor]
        public StructuralFraming(ICurve curve, StructuralFramingType elementType, double startSetback = 0.0, double endSetback = 0.0, Transform transform = null)
        {
            this.Curve = curve;
            var l = this.Curve.Length();
            if (startSetback > l || endSetback > l)
            {
                throw new ArgumentOutOfRangeException($"The start and end setbacks ({startSetback},{endSetback}) must be less than the length of the beam ({l}).");
            }
            this.StartSetback = startSetback;
            this.EndSetback = endSetback;
            this.Transform = transform;
            this.ElementType = elementType;
        }

        /// <summary>
        /// Calculate the volume of the element.
        /// </summary>
        public double Volume()
        {
            return this.ElementType.Profile.Area() * this.Curve.Length();
        }
    
        /// <summary>
        /// Get the cross-section profile of the framing element transformed by the element's transform.
        /// </summary>
        public Profile ProfileTransformed()
        {
            return this.Transform != null ? this.Transform.OfProfile(this.ElementType.Profile) : this.ElementType.Profile;
        }
    }
}