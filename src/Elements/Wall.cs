using Elements.Geometry;
using Elements.Geometry.Interfaces;
using Elements.Geometry.Solids;
using Elements.Interfaces;
using Newtonsoft.Json;
using System;

namespace Elements
{
    /// <summary>
    /// A wall is a building element which is used to enclose space.
    /// </summary>
    public class Wall : Element, IElementType<WallType>, ISolid, IProfile
    {
        /// <summary>
        /// The profile of the wall.
        /// </summary>
        public Profile Profile { get; }

        /// <summary>
        /// The center line of the wall.
        /// </summary>
        public Line CenterLine { get; }

        /// <summary>
        /// The height of the wall.
        /// </summary>
        public double Height { get; }

        /// <summary>
        /// The WallType of the Wall.
        /// </summary>
        public WallType ElementType { get; }

        /// <summary>
        /// The wall's geometry.
        /// </summary>
        public Solid Geometry { get; }

        /// <summary>
        /// An array of Openings in the wall.
        /// </summary>
        public Opening[] Openings{ get; }

        /// <summary>
        /// Construct a wall by extruding a profile.
        /// </summary>
        /// <param name="profile">The plan profile of the wall.</param>
        /// <param name="wallType">The wall type of the wall.</param>
        /// <param name="height">The height of the wall.</param>
        /// <param name="transform">An option transform for the wall.</param>
        public Wall(Profile profile, WallType wallType, double height, Transform transform = null)
        {
            if (height <= 0.0)
            {
                throw new ArgumentOutOfRangeException("The wall could not be created. The height of the wall must be greater than 0.0.");
            }
            
            this.Profile = profile;
            this.Height = height;
            this.Transform = transform;
            this.ElementType = wallType;
            this.Geometry = Solid.SweepFace(this.Profile.Perimeter, this.Profile.Voids, this.Height);
        }

        /// <summary>
        /// Construct a wall along a line.
        /// </summary>
        /// <param name="centerLine">The center line of the wall.</param>
        /// <param name="wallType">The wall type of the wall.</param>
        /// <param name="height">The height of the wall.</param>
        /// <param name="openings">A collection of Openings in the wall.</param>
        /// <param name="transform">The transform of the wall.
        /// This transform will be concatenated to the transform created to describe the wall in 2D.</param>
        /// <exception cref="System.ArgumentOutOfRangeException">Thrown when the height of the wall is less than or equal to zero.</exception>
        /// <exception cref="System.ArgumentOutOfRangeException">Thrown when the Z components of wall's start and end points are not the same.</exception>
        public Wall(Line centerLine, WallType wallType, double height, Opening[] openings = null, Transform transform = null)
        {
            if (height <= 0.0)
            {
                throw new ArgumentOutOfRangeException($"The wall could not be created. The height of the wall provided, {height}, must be greater than 0.0.");
            }

            if (centerLine.Start.Z != centerLine.End.Z)
            {
                throw new ArgumentException("The wall could not be created. The Z component of the start and end points of the wall's center line must be the same.");
            }

            this.CenterLine = centerLine;
            this.Height = height;
            this.ElementType = wallType;
            this.Openings = openings;

            // Construct a transform whose X axis is the centerline of the wall.
            // The wall is described as if it's lying flat in the XY plane of that Transform.
            var d = centerLine.Direction();
            var z = d.Cross(Vector3.ZAxis);
            var wallTransform = new Transform(centerLine.Start, d, z);
            this.Transform = wallTransform;
            if(transform != null) 
            {
                wallTransform.Concatenate(transform);
            }

            if (openings != null && openings.Length > 0)
            {
                var voids = new Polygon[openings.Length];
                for (var i = 0; i < voids.Length; i++)
                {
                    var o = openings[i];
                    voids[i] = o.Perimeter;
                }
                this.Profile = new Profile(Polygon.Rectangle(Vector3.Origin, new Vector3(centerLine.Length(), height)), voids);
            }
            else
            {
                this.Profile = new Profile(Polygon.Rectangle(Vector3.Origin, new Vector3(centerLine.Length(), height)));
            }

            this.Geometry = Solid.SweepFace(this.Profile.Perimeter, 
                this.Profile.Voids, this.Thickness(), true);
        }

        /// <summary>
        /// Construct a wall from a collection of geometry.
        /// </summary>
        /// <param name="geometry">The geometry of the wall.</param>
        /// <param name="centerLine">The center line of the wall.</param>
        /// <param name="wallType">The wall type of the wall.</param>
        /// <param name="height">The height of the wall.</param>
        /// <param name="transform">The wall's Transform.</param>
        [JsonConstructor]
        public Wall(Solid geometry, WallType wallType, double height = 0.0, Line centerLine = null, Transform transform = null)
        {
            if (geometry == null)
            {
                throw new ArgumentOutOfRangeException("You must supply one solid to construct a Wall.");
            }
            
            // TODO: Remove this when the Profile is no longer available
            // as a property on the Element. 
            // foreach(var g in geometry)
            // {
            //     var extrude = g as Extrude;
            //     if(extrude != null)
            //     {
            //         this.Profile = extrude.Profile;
            //     }
            // }

            this.Height = height;
            this.ElementType = wallType;
            this.Transform = transform;
            this.Geometry = geometry;
            this.CenterLine = centerLine;
        }

        /// <summary>
        /// Calculate the thickness of the wall's extrusion from its wall type.
        /// </summary>
        public double Thickness()
        {
            return this.ElementType.Thickness();
        }

        /// <summary>
        /// Get the transformed profile of the wall transformed by the wall's transform.
        /// </summary>
        public Profile ProfileTransformed()
        {
            return this.Transform != null ? this.Transform.OfProfile(this.Profile) : this.Profile;
        }
    }
}