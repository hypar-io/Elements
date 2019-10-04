using Elements.ElementTypes;
using Elements.Geometry;
using Elements.Geometry.Interfaces;
using Elements.Geometry.Solids;
using Elements.Interfaces;
using Newtonsoft.Json;
using System;

namespace Elements
{
    /// <summary>
    /// A wall defined by a planar profile extruded to a height.
    /// </summary>
    /// <example>
    /// [!code-csharp[Main](../../test/Examples/WallExample.cs?name=example)]
    /// </example>
    public class Wall : Element, IElementType<WallType>, IExtrude
    {
        /// <summary>
        /// The element type id.
        /// </summary>
        protected Guid _elementTypeId;
        
        private Guid _profileId;

        /// <summary>
        /// The height of the wall.
        /// </summary>
        public double Height{get; protected set;}

        /// <summary>
        /// The WallType of the Wall.
        /// </summary>
        [JsonIgnore]
        public WallType ElementType { get; protected set;}

        /// <summary>
        /// The element type of the wall.
        /// </summary>
        public Guid ElementTypeId
        {
            get
            {
                return this.ElementType != null ? this.ElementType.Id : this._elementTypeId;
            }
        }

        /// <summary>
        /// The wall's geometry.
        /// </summary>
        [JsonIgnore]
        public Solid Geometry { get; protected set;}

        /// <summary>
        /// The extruded direction of the wall.
        /// </summary>
        public Vector3 ExtrudeDirection{get; protected set;}

        /// <summary>
        /// The extruded depth of the wall.
        /// </summary>
        public double ExtrudeDepth{get; protected set;}

        /// <summary>
        /// The extruded area of the wall.
        /// </summary>
        [JsonIgnore]
        public Profile Profile{get; protected set;}

        /// <summary>
        /// The profile id of the wall.
        /// </summary>
        public Guid ProfileId
        {
            get
            {
                return this.Profile != null ? this.Profile.Id : this._profileId;
            }
        }

        /// <summary>
        /// Extrude to both sides?
        /// </summary>
        public virtual bool BothSides => false;

        internal Wall(){}

        /// <summary>
        /// Construct a wall by extruding a profile.
        /// </summary>
        /// <param name="profile">The plan profile of the wall.</param>
        /// <param name="elementType">The wall type of the wall.</param>
        /// <param name="height">The height of the wall.</param>
        /// <param name="transform">An option transform for the wall.</param>
        internal Wall(Profile profile, WallType elementType, double height, Transform transform = null)
        {
            if (height <= 0.0)
            {
                throw new ArgumentOutOfRangeException("The wall could not be created. The height of the wall must be greater than 0.0.");
            }
            
            if(transform != null)
            {
                this.Transform = transform;
            }
            this.ElementType = elementType;
            this.Profile = profile;
            this.ExtrudeDirection = Vector3.ZAxis;
            this.ExtrudeDepth = height;
            this.Height = height;
        }

        [JsonConstructor]
        internal Wall(Guid profileId, Guid elementTypeId, double height, Transform transform = null)
        {
            if (height <= 0.0)
            {
                throw new ArgumentOutOfRangeException("The wall could not be created. The height of the wall must be greater than 0.0.");
            }
            
            if(transform != null)
            {
                this.Transform = transform;
            }
            this._elementTypeId = elementTypeId;
            this._profileId = profileId;
            this.ExtrudeDirection = Vector3.ZAxis;
            this.ExtrudeDepth = height;
            this.Height = height;
        }

        /// <summary>
        /// Construct a wall by extruding a profile.
        /// </summary>
        /// <param name="profile">The plan profile of the wall.</param>
        /// <param name="elementType">The wall type of the wall.</param>
        /// <param name="height">The height of the wall.</param>
        /// <param name="transform">An option transform for the wall.</param>
        public Wall(Polygon profile, WallType elementType, double height, Transform transform = null)
        {
            if (height <= 0.0)
            {
                throw new ArgumentOutOfRangeException("The wall could not be created. The height of the wall must be greater than 0.0.");
            }
            
            if(transform != null)
            {
                this.Transform = transform;
            }
            this.ElementType = elementType;
            this.Profile = new Profile(profile);
            this.ExtrudeDirection = Vector3.ZAxis;
            this.ExtrudeDepth = height;
            this.Height = height;
        }

        /// <summary>
        /// Construct a wall from geometry.
        /// </summary>
        /// <param name="geometry">The geometry of the wall.</param>
        /// <param name="wallType">The wall type of the wall.</param>
        /// <param name="transform">The wall's Transform.</param>
        internal Wall(Solid geometry, WallType wallType, Transform transform = null)
        {
            if (geometry == null)
            {
                throw new ArgumentOutOfRangeException("You must supply one solid to construct a Wall.");
            }
            
            this.ElementType = wallType;
            if(transform != null)
            {
                this.Transform = transform;
            }
            this.Geometry = geometry;
        }

        /// <summary>
        /// Calculate the thickness of the wall's extrusion from its wall type.
        /// </summary>
        public double Thickness()
        {
            return this.ElementType.Thickness();
        }

        /// <summary>
        /// Get the updated solid representation of a wall.
        /// </summary>
        public Solid GetUpdatedSolid()
        {
            return Kernel.Instance.CreateExtrude(this);
        }

        /// <summary>
        /// Set the wall type.
        /// </summary>
        public virtual void SetReference(WallType type)
        {
            this.ElementType = type;
            this._elementTypeId = this.ElementType.Id;
        }

        /// <summary>
        /// Set the profile;
        /// </summary>
        /// <param name="obj"></param>
        public void SetReference(Profile obj)
        {
            this.Profile = obj;
            this._profileId = obj.Id;
        }
    }
}