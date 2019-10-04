using Newtonsoft.Json;
using System;
using Elements.Geometry;
using Elements.Geometry.Interfaces;
using Elements.Interfaces;
using Elements.Geometry.Solids;
using Elements.ElementTypes;

namespace Elements
{
    /// <summary>
    /// A structural element with a profile swept along a curve.
    /// </summary>
    public abstract class StructuralFraming : Element, IElementType<StructuralFramingType>, ISweepAlongCurve
    {
        private Guid _elementTypeId;
        private Guid _profileId;

        /// <summary>
        /// The center line of the framing element.
        /// </summary>
        public Curve Curve { get; }

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
        [JsonIgnore]
        public StructuralFramingType ElementType { get; private set;}

        /// <summary>
        /// The id of the element type.
        /// </summary>
        public Guid ElementTypeId
        {
            get
            {
                return this.ElementType != null ? this.ElementType.Id : this._elementTypeId;
            }
        }

        /// <summary>
        /// The extrusion's profile.
        /// </summary>
        [JsonIgnore]
        public Profile Profile {
            get
            {
                return this.ElementType.Profile;
            }
            set
            {
                // Do nothing. The profile is now set
                // through the element type.
            }
        }
        
        /// <summary>
        /// The extrusion's profile id.
        /// </summary>
        public Guid ProfileId
        {
            get
            {
                return this.Profile != null ? this.Profile.Id : this._profileId;
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
        public StructuralFraming(Curve curve, StructuralFramingType elementType, double startSetback = 0.0, double endSetback = 0.0, Transform transform = null)
        {
            this.Curve = curve;
            var l = this.Curve.Length();
            if (startSetback > l || endSetback > l)
            {
                throw new ArgumentOutOfRangeException($"The start and end setbacks ({startSetback},{endSetback}) must be less than the length of the beam ({l}).");
            }
            this.StartSetback = startSetback;
            this.EndSetback = endSetback;
            this.ElementType = elementType;

            if(transform != null)
            {
                this.Transform = transform;
            }
        }

        [JsonConstructor]
        internal StructuralFraming(Curve curve, Guid elementTypeId, double startSetback = 0.0, double endSetback = 0.0, Transform transform = null)
        {
            this.Curve = curve;
            var l = this.Curve.Length();
            if (startSetback > l || endSetback > l)
            {
                throw new ArgumentOutOfRangeException($"The start and end setbacks ({startSetback},{endSetback}) must be less than the length of the beam ({l}).");
            }
            this.StartSetback = startSetback;
            this.EndSetback = endSetback;
            this._elementTypeId = elementTypeId;

            if(transform != null)
            {
                this.Transform = transform;
            }
        }

        /// <summary>
        /// Calculate the volume of the element.
        /// </summary>
        public double Volume()
        {
            if (this.Curve.GetType() != typeof(Line))
            {
                throw new InvalidOperationException("Volume calculation for non-linear elements is not yet supported");
            }
            //TODO: Support all curve / profile calculations.
            return Math.Abs(this.ElementType.Profile.Area()) * this.Curve.Length();
        }
    
        /// <summary>
        /// Get the cross-section profile of the framing element transformed by the element's transform.
        /// </summary>
        public Profile ProfileTransformed()
        {
            return this.Transform != null ? this.Transform.OfProfile(this.ElementType.Profile) : this.ElementType.Profile;
        }

        /// <summary>
        /// Get the updated solid representation of the framing element.
        /// </summary>
        public Solid GetUpdatedSolid()
        {
            return Kernel.Instance.CreateSweepAlongCurve(this);
        }

        /// <summary>
        /// Set the structural framing type.
        /// </summary>
        public void SetReference(StructuralFramingType type)
        {
            this.ElementType = type;
            this._elementTypeId = type.Id;
        }

        /// <summary>
        /// Set the profile.
        /// </summary>
        public void SetReference(Profile profile)
        {
            this.Profile = profile;
            this._profileId = profile.Id;
        }
    }
}