using System;

namespace Hypar.Elements
{
    /// <summary>
    /// A material with red, green, blue, alpha, and metallic factor components.
    /// </summary>
    public class Material
    {   
        /// <summary>
        /// The red component.
        /// </summary>
        /// <returns></returns>
        public float Red{get;}

        /// <summary>
        /// The green component.
        /// </summary>
        /// <returns></returns>
        public float Green{get;}

        /// <summary>
        /// The blue component.
        /// </summary>
        /// <returns></returns>
        public float Blue{get;}

        /// <summary>
        /// The alpha component.
        /// </summary>
        /// <returns></returns>
        public float Alpha{get;}

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
        /// The identifer of the material.
        /// </summary>
        /// <returns></returns>
        public string Id {get; internal set;}

        /// <summary>
        /// A flag indicating whether the material uses per-vertex coloring.
        /// </summary>
        /// <returns></returns>
        public bool UsesPerVertexColoring{get;set;}
        
        /// <summary>
        /// Construct a material.
        /// </summary>
        /// <param name="id">The identifier of the material. Identifiers should be unique within a model.</param>
        /// <param name="red">The red component of the color. Between 0.0 and 1.0.</param>
        /// <param name="green">The green component of the color. Between 0.0 and 1.0.</param>
        /// <param name="blue">The blue component of the color. Between 0.0 and 1.0.</param>
        /// <param name="alpha">The alpha (transparency) component of the color. Between 0.0 and 1.0.</param>
        /// <param name="specularFactor">The specular component of the color. Between 0.0 and 1.0.</param>
        /// <param name="glossinessFactor">The glossiness component of the color. Between 0.0 and 1.0.</param>
        public Material(string id, float red, float green, float blue, float alpha, float specularFactor, float glossinessFactor)
        {
            if(red < 0.0 || green < 0.0 || blue < 0.0 || alpha < 0.0 || specularFactor < 0.0 || glossinessFactor < 0.0)
            {
                throw new ArgumentOutOfRangeException("Color, specular, and glossiness values must be less greater than 0.0.");
            }

            if(red > 1.0 || green > 1.0 || blue > 1.0 || alpha > 1.0 || specularFactor > 1.0 || glossinessFactor > 1.0)
            {
                throw new ArgumentOutOfRangeException("Color, specular, and glossiness values must be less than 1.0.");
            }
            
            this.Id = id;
            this.Red = red;
            this.Green = green;
            this.Blue = blue;
            this.Alpha = alpha;
            this.SpecularFactor = specularFactor;
            this.GlossinessFactor = glossinessFactor;
        }
    }
}