using Newtonsoft.Json;
using System;
using Elements.Geometry;
using Elements.Geometry.Interfaces;
using Elements.Geometry.Solids;
using Elements.Serialization.JSON;
using Elements.Interfaces;

namespace Elements
{
    /// <summary>
    /// A structural element with a Profile swept along a curve or extruded.
    /// </summary>
    public abstract class StructuralFraming : Element, IGeometry3D, IElementType<StructuralFramingType>
    {
        /// <summary>
        /// The cross-section profile of the framing element transformed by the element's transform.
        /// </summary>
        [JsonIgnore]
        public Profile ProfileTransformed
        {
            get { return this.Transform != null ? this.Transform.OfProfile(this.ElementType.Profile) : this.ElementType.Profile; }
        }

        /// <summary>
        /// The center line of the framing element.
        /// </summary>
        [JsonProperty("curve")]
        [JsonConverter(typeof(ICurveConverter))]
        public ICurve Curve { get; }

        /// <summary>
        /// The setback of the beam's extrusion at the start.
        /// </summary>
        [JsonProperty("start_setback")]
        public double StartSetback { get; }

        /// <summary>
        /// The setback of the beam's extrusion at the end.
        /// </summary>
        [JsonProperty("end_setback")]
        public double EndSetback { get; }

        /// <summary>
        /// The geometry of the StructuralFraming.
        /// </summary>
        [JsonIgnore]
        public Solid[] Geometry { get; }

        /// <summary>
        /// The element type of the structural framing.
        /// </summary>
        [JsonProperty("element_type")]
        public StructuralFramingType ElementType {get;}

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
            this.Geometry = new[]{Solid.SweepFaceAlongCurve(this.ElementType.Profile.Perimeter, 
                this.ElementType.Profile.Voids, this.Curve, this.ElementType.Material, this.StartSetback, this.EndSetback)};
        }

        /// <summary>
        /// Calculate the volume of the element.
        /// </summary>
        public double Volume()
        {
            return this.ElementType.Profile.Area() * this.Curve.Length();
        }
    }
}