using Elements.Geometry;
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
    [UserElement]
    public class Wall : Element, IGeometry, IMaterial
    {
        /// <summary>
        /// The height of the wall.
        /// </summary>
        public double Height { get; protected set; }

        /// <summary>
        /// The wall's geometry.
        /// </summary>
        public Elements.Geometry.Geometry Geometry { get; } = new Geometry.Geometry();

        /// <summary>
        /// The profile of the wall.
        /// </summary>
        public Profile Profile { get; protected set; }

        /// <summary>
        /// The material of the wall.
        /// </summary>
        public Material Material { get; protected set; }

        /// <summary>
        /// Construct a wall by extruding a profile.
        /// </summary>
        /// <param name="profile">The plan profile of the wall.</param>
        /// <param name="height">The height of the wall.</param>
        /// <param name="material">The material of the wall.</param>
        /// <param name="transform">An option transform for the wall.</param>
        /// <param name="id">The id of the wall.</param>
        /// <param name="name">The name of the wall.</param>
        [JsonConstructor]
        public Wall(Profile profile,
                      double height,
                      Material material = null,
                      Transform transform = null,
                      Guid id = default(Guid),
                      string name = null) : base(id, name, transform)
        {
            if (height <= 0.0)
            {
                throw new ArgumentOutOfRangeException("The wall could not be created. The height of the wall must be greater than 0.0.");
            }

            this.Profile = profile;
            this.Height = height;
            this.Material = material != null ? material : BuiltInMaterials.Concrete;
            this.Geometry.SolidOperations.Add(new Extrude(this.Profile, this.Height, Vector3.ZAxis));
        }

        /// <summary>
        /// A pass-through constructor to set the id, name, and transform.
        /// </summary>
        /// <param name="id"></param>
        /// <param name="name"></param>
        /// <param name="transform"></param>
        /// <returns></returns>
        protected Wall(Guid id, string name, Transform transform) : base(id, name, transform){}

        /*
        /// <summary>
        /// Construct a wall by extruding a profile.
        /// </summary>
        /// <param name="profile">The plan profile of the wall.</param>
        /// <param name="height">The height of the wall.</param>
        /// <param name="material">The material of the wall.</param>
        /// <param name="transform">An option transform for the wall.</param>
        public Wall(Polygon profile, double height, Material material = null, Transform transform = null)
        {
            if (height <= 0.0)
            {
                throw new ArgumentOutOfRangeException("The wall could not be created. The height of the wall must be greater than 0.0.");
            }

            if (transform != null)
            {
                this.Transform = transform;
            }
            this.Profile = new Profile(profile);
            this.Height = height;
            this.Material = material != null ? material : BuiltInMaterials.Concrete;
            this.Geometry.SolidOperations.Add(new Extrude(this.Profile, height, Vector3.ZAxis));
        }*/

        /// <summary>
        /// Construct a wall from geometry.
        /// </summary>
        /// <param name="geometry">The geometry of the wall.</param>
        /// <param name="transform">The wall's Transform.</param>
        internal Wall(Solid geometry, Transform transform = null)
        {
            if (geometry == null)
            {
                throw new ArgumentOutOfRangeException("You must supply one solid to construct a Wall.");
            }

            if (transform != null)
            {
                this.Transform = transform;
            }
            this.Geometry.SolidOperations.Add(new Import(geometry));
        }
    }
}