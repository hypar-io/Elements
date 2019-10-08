using Elements.Geometry;
using Elements.Interfaces;
using System;
using System.Collections.Generic;
using Elements.Geometry.Solids;

namespace Elements
{
    /// <summary>
    /// A floor is a horizontal element defined by a profile.
    /// </summary>
    /// <example>
    /// [!code-csharp[Main](../../test/Examples/FloorExample.cs?name=example)]
    /// </example>
    [UserElement]
    public class Floor : Element, IMaterial, IGeometry
    {
        private List<Opening> _openings = new List<Opening>();

        /// <summary>
        /// The elevation from which the floor is extruded.
        /// </summary>
        public double Elevation { get; private set; }

        /// <summary>
        /// The thickness of the floor.
        /// </summary>
        public double Thickness { get; private set;}

        /// <summary>
        /// The untransformed profile of the floor.
        /// </summary>
        public Profile Profile { get; private set; }

        /// <summary>
        /// The floor's geometry.
        /// </summary>
        public Elements.Geometry.Geometry Geometry { get; } = new Geometry.Geometry();

        /// <summary>
        /// The openings in the floor.
        /// </summary>
        public List<Opening> Openings
        {
            get{return _openings;}
            set{_openings = value;}
        }

        /// <summary>
        /// The floor's material.
        /// </summary>
        public Material Material{ get; private set; }

        /// <summary>
        /// Create a floor.
        /// </summary>
        /// <param name="perimeter">The profile of the floor.</param>
        /// <param name="thickness">The thickness of the floor.</param>
        /// <param name="elevation">The elevation of the top of the floor.</param>
        /// <param name="transform">The floor's transform. If set, this will override the floor's elevation.</param>
        /// <param name="openings">An array of openings in the floor.</param>
        /// <param name="material">The floor's material.</param>
        /// <param name="id">The floor's id.</param>
        /// <param name="name">The floor's name.</param>
        public Floor(Polygon perimeter, double thickness, double elevation = 0.0, Transform transform = null, 
            List<Opening> openings = null, Material material = null, Guid id = default(Guid), string name = null): base(id, name, transform)
        {
            SetProperties(new Profile(perimeter), openings, elevation, thickness, material, transform);
        }

        /* 
        /// <summary>
        /// Create a floor.
        /// </summary>
        /// <param name="profile">The profile of the floor.</param>
        /// <param name="start">A tranform used to pre-transform the profile and direction vector before sweeping the geometry.</param>
        /// <param name="direction">The direction of the floor's sweep.</param>
        /// <param name="thickness">The thickness of the floor.</param>
        /// <param name="elevation">The elevation of the floor.</param>
        /// <param name="transform">The floor's transform. If set, this will override the elevation.</param>
        /// <param name="material">The floor's material.</param>
        public Floor(Profile profile, double thickness, Transform start, Vector3 direction, double elevation = 0.0, Transform transform = null, Material material = null)
        {
            this.Elevation = elevation;
            this.Transform = transform != null ? transform : new Transform(new Vector3(0, 0, elevation));
            this.Profile = start.OfProfile(profile);
            this.Material = material != null ? material : BuiltInMaterials.Concrete;
            this.Geometry.SolidOperations.Add(new Extrude(this.Profile, this.Thickness, start.OfVector(direction)));
        }*/

        private void SetProperties(Profile profile, List<Opening> openings, double elevation, double thickness, Material material, Transform transform)
        {
            this.Profile = profile;
            if(openings != null)
            {
                this._openings = openings;
            }
            this.Elevation = elevation;
            this.Thickness = thickness;
            this.Transform = transform != null ? transform : new Transform(new Vector3(0, 0, elevation));
            this.Material = material != null ? material : BuiltInMaterials.Concrete;
            this.Geometry.SolidOperations.Add(new Extrude(this.Profile, this.Thickness, Vector3.ZAxis));
        }

        /// <summary>
        /// Get the profile of the floor transformed by the floor's transform.
        /// </summary>
        public Profile ProfileTransformed()
        {
            return this.Transform != null ? this.Transform.OfProfile(this.Profile) : this.Profile;
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
            return Math.Abs(this.Profile.Area()) * this.Thickness;
        }
    }
}