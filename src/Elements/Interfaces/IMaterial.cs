using System.Collections.Generic;

namespace Elements.Interfaces
{
    /// <summary>
    /// A material.
    /// </summary>
    public interface IMaterial
    {
        /// <summary>
        /// The object's material.
        /// </summary>
        Material Material{get;}
    }
    
    /// <summary>
    /// A layered material.
    /// </summary>
    public interface ILayeredMaterial
    {
        /// <summary>
        /// A collection of material layers.
        /// </summary>
        List<MaterialLayer> MaterialLayers{get;}
    }
}