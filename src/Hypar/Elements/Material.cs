using System;

namespace Hypar.Elements
{
    /// <summary>
    /// A material with red, green, blue, alpha, and metallic factor components.
    /// </summary>
    public class Material
    {
        public float Red{get;}
        public float Green{get;}
        public float Blue{get;}
        public float Alpha{get;}

        public float SpecularFactor{get;}

        public float GlossinessFactor{get;}
        public string Id {get; internal set;}

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