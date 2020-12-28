using System;
using System.Collections.Generic;
using Elements.Geometry;
using Elements.Geometry.Solids;

namespace Elements
{
    /// <summary>
    /// A structural element with a profile swept along a curve.
    /// </summary>
    public abstract class StructuralFraming : GeometricElement
    {
        private Curve curve;
        private double startSetback;
        private double endSetback;
        private Profile profile;
        private double rotation;

        /// <summary>
        /// The center line of the framing element.
        /// </summary>
        public Curve Curve
        {
            get => curve;
            set
            {
                if (curve != value)
                {
                    curve = value;
                    RaisePropertyChanged();
                }
            }
        }

        /// <summary>
        /// The setback of the framing's extrusion at the start.
        /// </summary>
        public double StartSetback
        {
            get => startSetback;
            set
            {
                if (startSetback != value)
                {
                    startSetback = value;
                    RaisePropertyChanged();
                }
            }
        }

        /// <summary>
        /// The setback of the framing's extrusion at the end.
        /// </summary>
        public double EndSetback
        {
            get => endSetback;
            set
            {
                if (endSetback != value)
                {
                    endSetback = value;
                    RaisePropertyChanged();
                }
            }
        }

        /// <summary>
        /// The structural framing's profile.
        /// </summary>
        public Profile Profile
        {
            get => profile;
            set
            {
                if (profile != value)
                {
                    profile = value;
                    RaisePropertyChanged();
                }
            }
        }

        /// <summary>
        /// The profile rotation around the center curve of the beam in degrees.
        /// </summary>
        public double Rotation
        {
            get => rotation;
            set
            {
                if (rotation != value)
                {
                    rotation = value;
                    RaisePropertyChanged();
                }
            }
        }

        /// <summary>
        /// Construct a beam.
        /// </summary>
        /// <param name="curve">The center line of the beam.</param>
        /// <param name="profile">The structural framing's profile.</param>
        /// <param name="material">The structural framing's material.</param>
        /// <param name="startSetback">The setback distance of the beam's extrusion at its start.</param>
        /// <param name="endSetback">The setback distance of the beam's extrusion at its end.</param>
        /// <param name="rotation">An optional rotation in degrees of the transform around its z axis.</param>
        /// <param name="transform">The element's Transform.</param>
        /// <param name="representations">The structural framing's representation.</param>
        /// <param name="isElementDefinition">Is this an element definition?</param>
        /// <param name="id">The structural framing's id.</param>
        /// <param name="name">The structural framing's name.</param>
        public StructuralFraming(Curve curve,
                                 Profile profile,
                                 Material material = null,
                                 double startSetback = 0.0,
                                 double endSetback = 0.0,
                                 double rotation = 0.0,
                                 Transform transform = null,
                                 IList<Representation> representations = null,
                                 bool isElementDefinition = false,
                                 Guid id = default(Guid),
                                 string name = null) : base(
                                     transform != null ? transform : new Transform(),
                                     representations != null ? representations : new Representation[] { new CurveRepresentation(curve, BuiltInMaterials.Edges), new SolidRepresentation(material != null ? material : BuiltInMaterials.Steel) },
                                     isElementDefinition,
                                     id != default(Guid) ? id : Guid.NewGuid(),
                                     name)
        {
            SetProperties(curve, profile, startSetback, endSetback, rotation);

            var rep = this.FirstRepresentationOfType<SolidRepresentation>();
            rep.SolidOperations.Add(new Sweep(this.Profile,
                                              this.Curve,
                                              this.StartSetback,
                                              this.EndSetback,
                                              this.Rotation,
                                              false));
        }

        private void SetProperties(Curve curve,
                                   Profile profile,
                                   double startSetback,
                                   double endSetback,
                                   double rotation)
        {
            this.Curve = curve;
            var l = this.Curve.Length();
            if (startSetback > l || endSetback > l)
            {
                throw new ArgumentOutOfRangeException($"The start and end setbacks ({startSetback},{endSetback}) must be less than the length of the beam ({l}).");
            }
            this.StartSetback = startSetback;
            this.EndSetback = endSetback;
            this.Profile = profile;
            this.Rotation = rotation;
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
            return Math.Abs(this.Profile.Area()) * this.Curve.Length();
        }

        /// <summary>
        /// Get the cross-section profile of the framing element transformed by the element's transform.
        /// </summary>
        public Profile ProfileTransformed()
        {
            return this.Transform != null ? this.Transform.OfProfile(this.Profile) : this.Profile;
        }

        /// <summary>
        /// Update the representations.
        /// </summary>
        public override void UpdateRepresentations()
        {
            var rep = this.FirstRepresentationOfType<SolidRepresentation>();
            var sweep = (Sweep)rep.SolidOperations[0];
            sweep.Curve = this.Curve;
            sweep.StartSetback = this.StartSetback;
            sweep.EndSetback = this.EndSetback;
            sweep.ProfileRotation = this.Rotation;
        }
    }
}