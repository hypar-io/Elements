using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using Elements.Geometry;
using Elements.Geometry.Interfaces;
using Elements.Interfaces;
using Elements.Serialization;
using Elements.Geometry.Solids;

namespace Elements
{
    /// <summary>
    /// A structural element with a Profile swept along a curve or extruded.
    /// </summary>
    public abstract class StructuralFraming : Element, IGeometry3D, IProfileProvider
    {
        private Vector3 _up;

        /// <summary>
        /// The cross-section profile of the framing element.
        /// </summary>
        [JsonProperty("profile")]
        public Profile Profile { get; }

        /// <summary>
        /// The cross-section profile of the framing element transformed by the element's transform.
        /// </summary>
        [JsonIgnore]
        public Profile ProfileTransformed
        {
            get { return this.Transform != null ? this.Transform.OfProfile(this.Profile) : this.Profile; }
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
        [JsonProperty("geometry")]
        public Solid[] Geometry { get; }

        /// <summary>
        /// Construct a beam.
        /// </summary>
        /// <param name="curve">The center line of the beam.</param>
        /// <param name="profile">The structural Profile of the beam.</param>
        /// <param name="material">The beam's material.</param>
        /// <param name="up">The up axis of the beam.</param>
        /// <param name="startSetback">The setback of the beam's extrusion at its start.</param>
        /// <param name="endSetback">The setback of the beam's extrusion at its end.</param>
        /// <param name="transform">The element's Transform.</param>
        [JsonConstructor]
        public StructuralFraming(ICurve curve, Profile profile, Material material = null, Vector3 up = null, double startSetback = 0.0, double endSetback = 0.0, Transform transform = null)
        {
            this.Profile = profile;
            this.Curve = curve;
            var t = this.Curve.TransformAt(0.0, up);
            this._up = t.YAxis;

            var l = this.Curve.Length();
            if (startSetback > l || endSetback > l)
            {
                throw new ArgumentOutOfRangeException($"The start and end setbacks ({startSetback},{endSetback}) must be less than the length of the beam ({l}).");
            }
            this.StartSetback = startSetback;
            this.EndSetback = endSetback;
            this.Transform = transform;
            this.Geometry = new[]{Solid.SweepFaceAlongCurve(this.Profile.Perimeter, this.Profile.Voids, this.Curve, material == null ? BuiltInMaterials.Steel : material, this.StartSetback, this.EndSetback)};
        }

        /// <summary>
        /// Calculate the volume of the element.
        /// </summary>
        public double Volume()
        {
            return this.Profile.Area() * this.Curve.Length();
        }
    }
}