using System;
using Elements.Geometry;
using Elements.Geometry.Solids;

namespace Elements
{
    /// <summary>
    /// A structural element with a profile swept along a curve.
    /// </summary>
    public abstract class StructuralFraming : GeometricElement
    {
        private double _rotation = 0.0;

        /// <summary>
        /// The center line of the framing element.
        /// </summary>
        public Curve Curve { get; private set; }

        /// <summary>
        /// The setback of the framing's extrusion at the start.
        /// </summary>
        public double StartSetback { get; private set; }

        /// <summary>
        /// The setback of the framing's extrusion at the end.
        /// </summary>
        public double EndSetback { get; private set; }

        /// <summary>
        /// The structural framing's profile.
        /// </summary>
        public Profile Profile { get; set; }

        /// <summary>
        /// Construct a beam.
        /// </summary>
        /// <param name="curve">The center line of the beam.</param>
        /// <param name="profile">The structural framing's profile.</param>
        /// <param name="material">The structural framing's material.</param>
        /// <param name="startSetback">The setback of the beam's extrusion at its start.</param>
        /// <param name="endSetback">The setback of the beam's extrusion at its end.</param>
        /// <param name="rotation">An optional rotation in degrees of the transform around its z axis.</param>
        /// <param name="transform">The element's Transform.</param>
        /// <param name="id">The structural framing's id.</param>
        /// <param name="name">The structural framing's name.</param>
        public StructuralFraming(Curve curve,
                                 Profile profile,
                                 Material material = null,
                                 double startSetback = 0.0,
                                 double endSetback = 0.0,
                                 double rotation = 0.0,
                                 Transform transform = null,
                                 Guid id = default(Guid),
                                 string name = null) : base(material, transform, id, name)
        {
            SetProperties(curve, profile, material, transform, startSetback, endSetback, rotation);
        }

        private void SetProperties(Curve curve,
                                   Profile profile,
                                   Material material,
                                   Transform transform,
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
            this.Material = material != null ? material : BuiltInMaterials.Steel;
            this._rotation = rotation;
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
            if(this.Representation.SolidOperations.Count > 0)
            {
                return;
            }

            this.Representation.SolidOperations.Add(new Sweep(this.Profile, this.Curve, this.StartSetback, this.EndSetback, this._rotation));

            // TODO(Ian): Remove this code if we are able to make sweeps work
            // for all curves. Otherwise, we will need to use this code to 
            // segment curves.

            // var ct = this.Curve.GetType();
            // if(ct == typeof(Line))
            // {
            //     var l = (Line)this.Curve;
            //     this.Geometry.SolidOperations.Add(new Extrude(this.Profile, l.Length(), this._rotation));
            // }
            // else if(ct == typeof(Arc))
            // {
            //     var arc = (Arc)this.Curve;
            //     var seg = 5;
            //     var div = 1.0/seg;
            //     for(var i=0; i<seg-1; i++)
            //     {
            //         var a = arc.PointAt(i * div);
            //         var b = arc.PointAt((i+1) * div);
            //         var d = (b-a).Normalized();
            //         var l = a.DistanceTo(b);
            //         this.Geometry.SolidOperations.Add(new Extrude(this.Profile, l, this._rotation));
            //     }
            // }
            // else if(typeof(Polyline).IsAssignableFrom(ct))
            // {
            //     var p = (Polyline)this.Curve;
            //     foreach(var s in p.Segments())
            //     {
            //         this.Geometry.SolidOperations.Add(new Extrude(this.Profile, s.Length(), this._rotation));
            //     }
            // }
        }
    }
}