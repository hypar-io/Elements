using Elements.Geometry;
using Elements.Interfaces;
using Newtonsoft.Json;
using System;
using System.Collections;
using System.Linq;
using System.Collections.Generic;

namespace Elements
{
    /// <summary>
    /// A Floor is a horizontal element defined by a perimeter and one or several voids.
    /// </summary>
    public class Floor : Element, IElementTypeProvider<FloorType>, IExtrude
    {
        /// <summary>
        /// The elevation from which the Floor is extruded.
        /// </summary>
        [JsonProperty("elevation")]
        public double Elevation { get; }

        /// <summary>
        /// The FloorType of the Floor.
        /// </summary>
        [JsonProperty("element_type")]
        public FloorType ElementType{get;}

        /// <summary>
        /// The Material of the Floor.
        /// </summary>
        [JsonProperty("material")]
        public Material Material{get;}

        /// <summary>
        /// The Profile of the Floor.
        /// </summary>
        [JsonProperty("profile")]
        public Profile Profile{get;}

        /// <summary>
        /// The transformed Profile of the Floor.
        /// </summary>
        [JsonIgnore]
        public Profile ProfileTransformed
        {
            get{return this.Transform != null ? this.Transform.OfProfile(this.Profile) : this.Profile;}
        }

        /// <summary>
        /// The thickness of the Floor's extrusion.
        /// </summary>
        [JsonIgnore]
        public double Thickness
        {
            get{return this.ElementType.Thickness;}
        }

        /// <summary>
        /// Construct a Floor.
        /// </summary>
        /// <param name="profile">The <see cref="Elements.Geometry.Profile"/>of the Floor.</param>
        /// <param name="elevation">The elevation of the Floor.</param>
        /// <param name="elementType">The <see cref="Elements.ElementType"/> of the Floor.</param>
        /// <param name="material">The Floor's <see cref="Elements.Material"/>.</param>
        /// <exception cref="System.ArgumentOutOfRangeException">Thrown when the slab's thickness is less than or equal to 0.0.</exception>
        [JsonConstructor]
        public Floor(Profile profile, FloorType elementType, double elevation = 0.0, Material material = null)
        {
            this.Profile = profile;
            this.Elevation = elevation;
            this.ElementType = elementType;
            this.Transform = new Transform(new Vector3(0, 0, elevation));
            this.Material = material == null ? BuiltInMaterials.Concrete : material;
        }

        /// <summary>
        /// The area of the Floor.
        /// Overlapping openings and openings which are outside of the Floor's perimeter,
        /// will result in incorrect area results.
        /// </summary>
        public double Area()
        {
            return this.Profile.Area();
        }
    }

    /// <summary>
    /// Extension methods for Floors.
    /// </summary>
    public static class FloorExtensions
    {
        /// <summary>
        /// Create Floors at the specified elevations within a mass.
        /// </summary>
        /// <param name="mass"></param>
        /// <param name="elevations">A collection of elevations at which Floors will be created within the mass.</param>
        /// <param name="floorType">The FloorType of the Floors.</param>
        /// <param name="material">The Floor material.</param>
        public static IList<Floor> Floors(this Mass mass, IList<double> elevations, FloorType floorType, Material material)
        {
            var Floors = new List<Floor>();
            foreach(var e in elevations)
            {
                if (e >= mass.Elevation && e <= mass.Elevation + mass.Height)
                {
                    var f = new Floor(mass.Profile, floorType, e, material);
                    Floors.Add(f);
                }
            }
            return Floors;
        }
    }
}