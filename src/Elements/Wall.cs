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
    public class Wall : Element, IElementType<WallType>, ISolid, IExtrude
    {
        /// <summary>
        /// The height of the wall.
        /// </summary>
        public double Height{get; protected set;}

        /// <summary>
        /// The WallType of the Wall.
        /// </summary>
        public WallType ElementType { get; protected set;}

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
        public Profile Profile{get; protected set;}

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
        [JsonConstructor]
        internal Wall(Profile profile, WallType elementType, double height, Transform transform = null)
        {
            if (height <= 0.0)
            {
                throw new ArgumentOutOfRangeException("The wall could not be created. The height of the wall must be greater than 0.0.");
            }
            
            this.Transform = transform;
            this.ElementType = elementType;
            this.Profile = profile;
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
            
            this.Transform = transform;
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
            this.Transform = transform;
            this.Geometry = geometry;
        }

        /// <summary>
        /// Calculate the thickness of the wall's extrusion from its wall type.
        /// </summary>
        public double Thickness()
        {
            return this.ElementType.Thickness();
        }
    }
}