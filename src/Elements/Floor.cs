using Elements.Geometry;
using Elements.Interfaces;
using Elements.Geometry.Interfaces;
using Newtonsoft.Json;
using Elements.Geometry.Solids;
using System;
using System.Collections.Generic;
using Elements.ElementTypes;

namespace Elements
{
    /// <summary>
    /// A floor is a horizontal element defined by a profile.
    /// </summary>
    /// <example>
    /// [!code-csharp[Main](../../test/Examples/FloorExample.cs?name=example)]
    /// </example>
    [UserElement]
    public class Floor : Element, IElementType<FloorType>, ISolid, IExtrude, IHasOpenings
    {
        private List<Opening> _openings = new List<Opening>();
        private Guid _elementTypeId;

        /// <summary>
        /// The elevation from which the floor is extruded.
        /// </summary>
        public double Elevation { get; }

        /// <summary>
        /// The floor type of the floor.
        /// </summary>
        [JsonIgnore]
        [ReferencedByProperty("ElementTypeId")]
        public FloorType ElementType { get; private set;}

        /// <summary>
        /// The element type of the floor.
        /// </summary>
        public Guid ElementTypeId {
            get
            {
                return this.ElementType != null ? this.ElementType.Id : this._elementTypeId;
            }
        }

        /// <summary>
        /// The untransformed profile of the floor.
        /// </summary>
        [JsonIgnore]
        [ReferencedByProperty("ProfileId")]
        public Profile Profile { get; private set; }

        /// <summary>
        /// The untransformed profile of the floor.
        /// </summary>
        public Guid ProfileId { get; private set; }

        /// <summary>
        /// The floor's geometry.
        /// </summary>
        public Solid Geometry { get; }

        /// <summary>
        /// The openings in the floor.
        /// </summary>
        public List<Opening> Openings
        {
            get{return _openings;}
            set{_openings = value;}
        }

        /// <summary>
        /// The extrude direction of the floor.
        /// </summary>
        public Vector3 ExtrudeDirection {get;}

        /// <summary>
        /// The extrude depth of the floor.
        /// </summary>
        public double ExtrudeDepth => this.Thickness();

        /// <summary>
        /// Extrude to both sides?
        /// </summary>
        public bool BothSides => false;

        /// <summary>
        /// Create a floor.
        /// </summary>
        /// <param name="profile">The profile of the floor.</param>
        /// <param name="elementType">The floor type of the floor.</param>
        /// <param name="elevation">The elevation of the top of the floor.</param>
        /// <param name="transform">The floor's transform. If set, this will override the floor's elevation.</param>
        /// <param name="openings">An array of openings in the floor.</param>
        public Floor(Polygon profile, FloorType elementType, double elevation = 0.0, Transform transform = null, List<Opening> openings = null)
        {
            this.Profile = new Profile(profile);
            if(openings != null)
            {
                this._openings = openings;
            }
            this.Elevation = elevation;
            this.ElementType = elementType;
            var thickness = elementType.Thickness();
            this.Transform = transform != null ? transform : new Transform(new Vector3(0, 0, elevation));
            this.ExtrudeDirection = Vector3.ZAxis;
        }

        /// <summary>
        /// Create a floor.
        /// </summary>
        /// <param name="profile">The profile of the floor.</param>
        /// <param name="start">A tranform used to pre-transform the profile and direction vector before sweeping the geometry.</param>
        /// <param name="direction">The direction of the floor's sweep.</param>
        /// <param name="elementType">The floor type of the floor.</param>
        /// <param name="elevation">The elevation of the floor.</param>
        /// <param name="transform">The floor's transform. If set, this will override the elevation.</param>
        public Floor(Profile profile, Transform start, Vector3 direction, FloorType elementType, double elevation = 0.0, Transform transform = null)
        {
            this.Elevation = elevation;
            this.ElementType = elementType;
            this.Transform = transform != null ? transform : new Transform(new Vector3(0, 0, elevation));
            this.Profile = start.OfProfile(profile);
            this.ExtrudeDirection = start.OfVector(direction);
        }

        [JsonConstructor]
        internal Floor(Guid profileId, Guid elementTypeId, double elevation = 0.0, Transform transform = null, List<Opening> openings = null)
        {
            this.ProfileId = profileId;
            if(openings != null)
            {
                this._openings = openings;
            }
            this.Elevation = elevation;
            this._elementTypeId = elementTypeId;
            
            this.Transform = transform != null ? transform : new Transform(new Vector3(0, 0, elevation));
            this.ExtrudeDirection = Vector3.ZAxis;
        }

        /// <summary>
        /// Get the profile of the floor transformed by the floor's transform.
        /// </summary>
        public Profile ProfileTransformed()
        {
            return this.Transform != null ? this.Transform.OfProfile(this.Profile) : this.Profile;
        }

        /// <summary>
        /// Calculate thickness of the floor's extrusion.
        /// </summary>
        public double Thickness()
        {
            return this.ElementType.Thickness();
        }

        /// <summary>
        /// Get the updated solid representation of the floor.
        /// </summary>
        /// <returns></returns>
        public Solid GetUpdatedSolid()
        {
            return Kernel.Instance.CreateExtrude(this);
        }

        /// <summary>
        /// The area of the floor.
        /// </summary>
        /// <returns>The area of the floor, not including the area of openings.</returns>
        public double Area()
        {
            return Math.Abs(this.Profile.Area());
        }

        /// <summary>
        /// The area of the floor.
        /// </summary>
        /// <returns>The area of the floor, not including the area of openings.</returns>
        public double Volume()
        {
            return Math.Abs(this.Profile.Area()) * this.Thickness();
        }

        /// <summary>
        /// Set the floor type.
        /// </summary>
        public void SetReference(FloorType type)
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
            this.ProfileId = profile.Id;
        }
    }
}