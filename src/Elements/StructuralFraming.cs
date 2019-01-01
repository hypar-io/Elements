using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using Elements.Geometry;
using Elements.Geometry.Interfaces;
using Elements.Interfaces;
using Elements.Serialization;

namespace Elements
{
    /// <summary>
    /// A linear structural element with a cross section.
    /// </summary>
    public abstract class StructuralFraming : Element, IExtrudeAlongCurve
    {
        private Vector3 _up;

        /// <summary>
        /// The cross-section profile of the framing element.
        /// </summary>
        [JsonProperty("profile")]
        public IProfile Profile{get;}

        /// <summary>
        /// The cross-section profile of the framing element transformed by the Element's Transform.
        /// </summary>
        [JsonIgnore]
        public IProfile ProfileTransformed
        {
            get{return this.Transform != null ? this.Transform.OfProfile(this.Profile) : this.Profile;}
        }

        /// <summary>
        /// The center line of the framing element.
        /// </summary>
        [JsonProperty("curve")]
        [JsonConverter(typeof(ICurveConverter))]
        public ICurve Curve{get;}

        /// <summary>
        /// The setback of the beam's extrusion at the start.
        /// </summary>
        [JsonProperty("start_setback")]
        public double StartSetback{get;}

        /// <summary>
        /// The setback of the Beam's extrusion at the end.
        /// </summary>
        [JsonProperty("end_setback")]
        public double EndSetback{get;}

        /// <summary>
        /// The Material of the StructuralFramingElement
        /// </summary>
        [JsonProperty("material")]
        public Material Material{get;}

        /// <summary>
        /// Construct a beam.
        /// </summary>
        /// <param name="curve">The center line of the Beam.</param>
        /// <param name="profile">The structural Profile of the Beam.</param>
        /// <param name="material">The Beam's material.</param>
        /// <param name="up">The up axis of the Beam.</param>
        /// <param name="startSetback">The setback of the framing's extrusion at its start.</param>
        /// <param name="endSetback">The setback of the framing's extrusion at its end.</param>
        [JsonConstructor]
        public StructuralFraming(ICurve curve, IProfile profile, Material material = null, Vector3 up = null, double startSetback = 0.0, double endSetback = 0.0)
        {
            this.Profile = profile;
            this.Curve = curve;
            this.Material = material == null ? BuiltInMaterials.Steel : material;
            var t = this.Curve.TransformAt(0.0, up);
            this._up = t.YAxis;

            var l = this.Curve.Length();
            if(startSetback > l || endSetback > l)
            {
                throw new ArgumentOutOfRangeException($"The start and end setbacks ({startSetback},{endSetback}) must be less than the length of the beam ({l}).");
            }
            this.StartSetback = startSetback;
            this.EndSetback = endSetback;
        }

        /// <summary>
        /// Calculate the volume of the element.
        /// </summary>
        public double Volume()
        {
            return this.Profile.Area() * this.Curve.Length();   
        }

        /// <summary>
        /// A collection of Faces which comprise this StructuralFraming.
        /// </summary>
        public IFace[] Faces()
        {
            return Extrusions.ExtrudeAlongCurve(this.Profile, this.Curve, true, this.StartSetback, this.EndSetback);
        }
    }
}