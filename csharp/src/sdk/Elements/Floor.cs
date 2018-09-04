using Hypar.Geometry;
using Newtonsoft.Json;
using System;
using System.Linq;
using System.Collections.Generic;

namespace Hypar.Elements
{
    /// <summary>
    /// A Floor is a horizontal element defined by an outer boundary and one or several holes.
    /// </summary>
    public class Floor : Element, ILocateable<Polyline>, ITessellate<Mesh>, ITransformable, IMaterialize
    {
        /// <summary>
        /// The boundary of the floor.
        /// </summary>
        /// <value></value>
        [JsonProperty("location")]
        public Polyline Location{get;}

        /// <summary>
        /// The transform of the floor 
        /// </summary>
        /// <value></value>
        [JsonProperty("transform")]
        public Transform Transform{get;}

        /// <summary>
        /// The openings in the slab.
        /// </summary>
        /// <returns></returns>
        [JsonProperty("openings")]
        public IEnumerable<Polyline> Openings{get;}

        /// <summary>
        /// The elevation from which the floor is extruded.
        /// </summary>
        /// <returns></returns>
        [JsonProperty("elevation")]
        public double Elevation{get;}

        /// <summary>
        /// The thickness of the floor.
        /// </summary>
        [JsonProperty("thickness")]
        public double Thickness{get;}
        
        /// <summary>
        /// The floor's material.
        /// </summary>
        /// <value></value>
        [JsonIgnore]
        public Material Material{get;set;}

        /// <summary>
        /// Construct a default floor.
        /// </summary>
        public Floor() : base()
        {
            this.Location = Profiles.Rectangular();
            this.Elevation = 0.0;
            this.Thickness = 0.2;
            this.Material = BuiltInMaterials.Concrete;
        }
        
        /// <summary>
        /// Construct a floor without penetrations.
        /// </summary>
        /// <param name="profile">The profile of the floor.</param>
        /// <param name="thickness">The thickness of the floor</param>
        public Floor(Polyline profile, double thickness)
        {
            if (thickness <= 0.0)
            {
                throw new ArgumentOutOfRangeException("thickness", "The slab thickness must be greater than 0.0.");
            }
            
            this.Location = profile;
            this.Elevation = 0.0;
            this.Thickness = thickness;
            this.Transform = new Transform(new Vector3(0, 0, this.Elevation), new Vector3(1, 0, 0), new Vector3(0, 0, 1));
            this.Material = BuiltInMaterials.Concrete;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="profile">The profile of the floor.</param>
        /// <param name="openings">The openings in the floor.</param>
        /// <param name="elevation">The elevation of the floor.</param>
        /// <param name="thickness">The thickness of the floor.</param>
        /// <param name="material">The floor's material.</param>
        public Floor(Polyline profile, IEnumerable<Polyline> openings, double elevation, double thickness, Material material)
        {
            if (thickness <= 0.0)
            {
                throw new ArgumentOutOfRangeException("thickness", "The slab thickness must be greater than 0.0.");
            }

            this.Location = profile;
            this.Openings = openings;
            this.Elevation = elevation;
            this.Thickness = thickness;
            this.Transform = new Transform(new Vector3(0, 0, elevation), new Vector3(1, 0, 0), new Vector3(0, 0, 1));
            this.Material = material;
        }
        
        /// <summary>
        /// Tessellate the slab.
        /// </summary>
        /// <returns></returns>
        public Mesh Tessellate()
        {
            var polys = new List<Polyline>();
            polys.Add(this.Location);
            if(this.Openings != null)
            {
                polys.AddRange(this.Openings);
            }
            
            return Mesh.Extrude(polys, this.Thickness, true);
        }
    }
}