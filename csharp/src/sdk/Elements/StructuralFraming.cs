using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using Hypar.Geometry;

namespace Hypar.Elements
{
    /// <summary>
    /// A linear structural element with a cross section.
    /// </summary>
    public abstract class StructuralFraming : Element, ITessellateMesh, ITessellateCurves, IProfileProvider
    {
        private ICurve _centerLine;
        private Profile _profile;

        /// <summary>
        /// The cross-section profile of the framing element.
        /// </summary>
        [JsonProperty("profile")]
        public Profile Profile
        {
            get{return this._profile;}
        }

        /// <summary>
        /// The cross-section profile of the framing element transformed by the Element's Transform.
        /// </summary>
        /// <value></value>
        [JsonIgnore]
        public Profile ProfileTransformed
        {
            get{return this.Transform != null ? this.Transform.OfProfile(this._profile) : this._profile;}
        }

        /// <summary>
        /// The up axis of the framing element.
        /// </summary>
        [JsonIgnore]
        public Vector3 UpAxis { get; }

        /// <summary>
        /// The center line of the framing element.
        /// </summary>
        [JsonProperty("center_line")]
        public ICurve CenterLine
        {
            get { return this._centerLine; }
        }

        /// <summary>
        /// The volume of the StructuralFraming element.
        /// </summary>
        [JsonIgnore]
        public double Volume
        {
            get
            {
                return this._profile.Area * this._centerLine.Length;   
            }
        }

        /// <summary>
        /// The setback of the beam's extrusion at the start.
        /// </summary>
        [JsonProperty("start_setback")]
        public double StartSetback{get;}

        /// <summary>
        /// Thet setback of the Beam's extrusion at the end.
        /// </summary>
        [JsonProperty("end_setback")]
        public double EndSetback{get;}

        /// <summary>
        /// Construct a beam.
        /// </summary>
        /// <param name="centerLine">The center line of the Beam.</param>
        /// <param name="profile">The structural Profile of the Beam.</param>
        /// <param name="material">The Beam's material.</param>
        /// <param name="up">The up axis of the Beam.</param>
        /// <param name="startSetback">The setback of the framing's extrusion at its start.</param>
        /// <param name="endSetback">The setback of the framing's extrusion at its end.</param>
        [JsonConstructor]
        public StructuralFraming(ICurve centerLine, Profile profile, Material material = null, Vector3 up = null, double startSetback = 0.0, double endSetback = 0.0)
        {
            this._profile = profile;
            this._centerLine = centerLine;
            this.Material = material == null ? BuiltInMaterials.Steel : material;
            var t = centerLine.TransformAt(0.0, up);
            this.UpAxis = up == null ? t.YAxis : up;

            var l = centerLine.Length;
            if(startSetback > l || endSetback > l)
            {
                throw new ArgumentOutOfRangeException($"The start and end setbacks ({startSetback},{endSetback}) must be less than the length of the beam ({l}).");
            }
            this.StartSetback = startSetback;
            this.EndSetback = endSetback;
        }

        /// <summary>
        /// Tessellate the Beam.
        /// </summary>
        /// <returns>A mesh representing the tessellated Beam.</returns>
        public Mesh Mesh()
        {
            return Hypar.Geometry.Mesh.ExtrudeAlongCurve(this._centerLine, this._profile.Perimeter, this._profile.Voids, true, this.StartSetback, this.EndSetback);
        }

        /// <summary>
        /// Tessellate the Beam's Profile.
        /// </summary>
        /// <returns>A collection of curves representing the tessellated Profile.</returns>
        public IList<IList<Vector3>> Curves()
        {
            var curves = new List<IList<Vector3>>();
            curves.Add(this.CenterLine.Vertices);
            return curves;
        }
    }
}