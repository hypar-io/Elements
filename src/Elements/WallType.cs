using Elements.Interfaces;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace Elements
{
    /// <summary>
    /// A container for properties common to walls.
    /// </summary>
    public class WallType : ElementType, ILayeredMaterial
    {
        /// <summary>
        /// The thickness of the Wall.
        /// </summary>
        [JsonIgnore]
        public double Thickness
        {
            get
            {
                var thickness = 0.0;
                foreach(var l in this.MaterialLayers)
                {
                    thickness += l.Thickness;
                }
                return thickness;
            }
        }

        /// <summary>
        /// The type of the wall type.
        /// </summary>
        [JsonProperty("type")]
        public override string Type
        {
            get { return "wall_type"; }
        }

        /// <summary>
        /// The material layers of the wall.
        /// </summary>
        [JsonProperty("material_layers")]
        public List<MaterialLayer> MaterialLayers {get;}

        /// <summary>
        /// Construct a wall type.
        /// </summary>
        /// <param name="name">The name of the wall type.</param>
        /// <param name="thickness">The thickness for all walls of this wall type.</param>
        /// <returns></returns>
        public WallType(string name, double thickness) : base(name)
        {
            if (thickness <= 0.0)
            {
                throw new ArgumentOutOfRangeException("The WallType could not be created. The thickness of the WallType must be greater than 0.0.");
            }
            this.MaterialLayers = new List<MaterialLayer>(){new MaterialLayer(BuiltInMaterials.Default, thickness)};
        }

        /// <summary>
        /// Construct a wall type.
        /// </summary>
        /// <param name="name">The name of the wall type.</param>
        /// <param name="materialLayers">The material layers of the wall type.</param>
        [JsonConstructor]
        public WallType(string name, List<MaterialLayer> materialLayers) : base(name)
        {
            this.MaterialLayers = materialLayers;
        }
    }
}