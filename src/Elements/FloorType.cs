using Elements.Interfaces;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace Elements.ElementTypes
{
    /// <summary>
    /// A container for properties common to floors.
    /// </summary>
    public partial class FloorType : ElementType, ILayeredMaterial
    {
        /// <summary>
        /// Construct a floor type.
        /// </summary>
        /// <param name="name">The name of the floor type.</param>
        /// <param name="thickness">The thickness of the associated floor.</param>
        public FloorType(string name, double thickness) : base(name)
        {
            if (thickness <= 0.0)
            {
                throw new ArgumentOutOfRangeException("thickness", "The floor type thickness must be greater than 0.0.");
            }

            this.MaterialLayers = new List<MaterialLayer>(){new MaterialLayer(BuiltInMaterials.Default, thickness)};
        }

        /// <summary>
        /// Construct a floor type.
        /// </summary>
        /// <param name="name">The name of the floor type.</param>
        /// <param name="materialLayers">A collection of material layers.</param>
        [JsonConstructor]
        public FloorType(string name, List<MaterialLayer> materialLayers) : base(name)
        {
            this.MaterialLayers = materialLayers;
        }
    
        /// <summary>
        /// Calculate the thickness of the floor by summing the thicknesses of its material layers.
        /// </summary>
        public double Thickness()
        {
            var thickness = 0.0;
            foreach(var l in this.MaterialLayers)
            {
                thickness += l.Thickness;
            }
            return thickness;
        }
    }
}