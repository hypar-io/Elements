using System;
using Elements.Geometry;

namespace Elements
{
    /// <summary>
    /// A light type.
    /// </summary>
    public enum LightType
    {
        /// <summary>
        /// A directional light.
        /// </summary>
        Directional,
        /// <summary>
        /// A point light.
        /// </summary>
        Point,
        /// <summary>
        /// A spot light.
        /// </summary>
        Spot,
        /// <summary>
        /// An undefined light type.
        /// </summary>
        Undefined
    }

    /// <summary>
    /// Base class for all lights.
    /// </summary>
    public abstract class Light : Element
    {
        /// <summary>
        /// The type of the light.
        /// </summary>
        public LightType LightType { get; set; } = LightType.Undefined;

        /// <summary>
        /// The intensity of the light measured in lux.
        /// </summary>
        public double Intensity { get; set; }

        /// <summary>
        /// The color of the light.
        /// The color's alpha value will be ignored.
        /// </summary>
        public Color Color { get; set; }

        /// <summary>
        /// The light's transform.
        /// The light will be aimed along the transform's -Z axis.
        /// </summary>
        public Transform Transform { get; set; }

        /// <summary>
        /// Construct a light.
        /// </summary>
        /// <param name="intensity">The intensity of the light.</param>
        /// <param name="color">The color of the light.</param>
        /// <param name="transform">The transform of the light.</param>
        /// <param name="id">The unique identifier of the light.</param>
        /// <param name="name">The name of the light.</param>
        public Light(double intensity,
                     Color color,
                     Transform transform,
                     Guid id,
                     string name) : base(id != default(Guid) ? id : Guid.NewGuid(),
                                         name)
        {
            this.Intensity = intensity;
            this.Color = color;
            this.Transform = transform;
        }
    }
}