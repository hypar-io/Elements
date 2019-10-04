using System;
using System.Collections.Generic;

namespace Elements.Interfaces
{
    /// <summary>
    /// A material.
    /// </summary>
    public interface IMaterial: IReference<Material>
    {
        /// <summary>
        /// The object's material.
        /// </summary>
        Material Material{get;}

        /// <summary>
        /// The object's material id.
        /// </summary>
        Guid MaterialId { get; }
    }
    
    /// <summary>
    /// A layered material.
    /// </summary>
    public interface ILayeredMaterial
    {
        /// <summary>
        /// A collection of material layers.
        /// </summary>
        IList<MaterialLayer> MaterialLayers{get;}
    }
}