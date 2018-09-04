using System;
using Hypar.Geometry;

namespace Hypar.Elements
{
    /// <summary>
    /// A material with red, green, blue, alpha, and metallic factor components.
    /// </summary>
    public class Material
    {   
        /// <summary>
        /// The RGBA Color of the Material.
        /// </summary>
        /// <value></value>
        public Color Color{get;}

        /// <summary>
        /// The specular factor.
        /// </summary>
        /// <returns></returns>
        public float SpecularFactor{get;}

        /// <summary>
        /// The glossiness factor.
        /// </summary>
        /// <returns></returns>
        public float GlossinessFactor{get;}

        /// <summary>
        /// The name of the material.
        /// </summary>
        /// <returns></returns>
        public string Name {get; internal set;}

        /// <summary>
        /// A flag indicating whether the material uses per-vertex coloring.
        /// </summary>
        /// <returns></returns>
        public bool UsesPerVertexColoring{get;set;}
        
        /// <summary>
        /// Construct a material.
        /// </summary>
        /// <param name="name">The identifier of the material. Identifiers should be unique within a model.</param>
        /// <param name="color">The RGBA color of the material.</param>
        /// <param name="specularFactor">The specular component of the color. Between 0.0 and 1.0.</param>
        /// <param name="glossinessFactor">The glossiness component of the color. Between 0.0 and 1.0.</param>
        public Material(string name, Color color, float specularFactor, float glossinessFactor)
        {
            if(specularFactor < 0.0 || glossinessFactor < 0.0)
            {
                throw new ArgumentOutOfRangeException("Specular, and glossiness values must be less greater than 0.0.");
            }

            if(specularFactor > 1.0 || glossinessFactor > 1.0)
            {
                throw new ArgumentOutOfRangeException("Color, specular, and glossiness values must be less than 1.0.");
            }
            
            this.Name = name;
            this.Color = color;
            this.SpecularFactor = specularFactor;
            this.GlossinessFactor = glossinessFactor;
        }
    }
}