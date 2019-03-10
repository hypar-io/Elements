using Newtonsoft.Json;
using System;
using System.Collections;
using Elements.Geometry;
using Elements.Interfaces;
using Elements.Serialization;

namespace Elements
{
    /// <summary>
    /// A material with red, green, blue, alpha, and metallic factor components.
    /// </summary>
    public class Material: IIdentifiable
    {   
        /// <summary>
        /// The unique identifier of the material.
        /// </summary>
        [JsonProperty("id")]
        public long Id{get; internal set;}

        /// <summary>
        /// The RGBA Color of the material.
        /// </summary>
        [JsonProperty("color")]
        public Color Color{get;}

        /// <summary>
        /// The specular factor.
        /// </summary>
        [JsonProperty("specular_factor")]
        public float SpecularFactor{get;}

        /// <summary>
        /// The glossiness factor.
        /// </summary>
        [JsonProperty("glossiness_factor")]
        public float GlossinessFactor{get;}

        /// <summary>
        /// The name of the material.
        /// </summary>
        [JsonProperty("name")]
        public string Name {get; internal set;}

        /// <summary>
        /// Is the material double sided?
        /// </summary>
        [JsonProperty("double_sided")]
        public bool DoubleSided{get;}

        /// <summary>
        /// Construct a material.
        /// </summary>
        /// <param name="name">The identifier of the material. Identifiers should be unique within a model.</param>
        /// <param name="color">The RGBA color of the material.</param>
        /// <param name="specularFactor">The specular component of the color. Between 0.0 and 1.0.</param>
        /// <param name="glossinessFactor">The glossiness component of the color. Between 0.0 and 1.0.</param>
        /// <param name="doubleSided">Is the material double sided?</param>
        /// <exception cref="System.ArgumentOutOfRangeException">Thrown when the specular or glossiness value is less than 0.0.</exception>
        /// <exception cref="System.ArgumentOutOfRangeException">Thrown when the specular or glossiness value is greater than 1.0.</exception>
        [JsonConstructor]
        public Material(string name, Color color, float specularFactor = 0.1f, float glossinessFactor = 0.1f, bool doubleSided = false)
        {
            if(specularFactor < 0.0 || glossinessFactor < 0.0)
            {
                throw new ArgumentOutOfRangeException("The material could not be created. Specular and glossiness values must be less greater than 0.0.");
            }

            if(specularFactor > 1.0 || glossinessFactor > 1.0)
            {
                throw new ArgumentOutOfRangeException("The material could not be created. Color, specular, and glossiness values must be less than 1.0.");
            }
            
            this.Id = IdProvider.Instance.GetNextId();
            this.Name = name;
            this.Color = color;
            this.SpecularFactor = specularFactor;
            this.GlossinessFactor = glossinessFactor;
            this.DoubleSided = doubleSided;
        }

        /// <summary>
        /// Is this material equal to the provided material?
        /// </summary>
        /// <param name="obj"></param>
        public override bool Equals(object obj)
        {
            var m = obj as Material;
            if(m == null)
            {
                return false;
            }
            return this.Color.Equals(m.Color) && this.SpecularFactor == m.SpecularFactor && this.GlossinessFactor == m.GlossinessFactor && this.Name == m.Name;
        }

        /// <summary>
        /// Get the hash code for the material.
        /// </summary>
        /// <returns></returns>
        public override int GetHashCode()
        {
            return new ArrayList(){this.Name, this.Color, this.SpecularFactor, this.GlossinessFactor}.GetHashCode();
        }
    }
}