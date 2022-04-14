using System;
using Elements.Geometry;

namespace Elements
{
    /// <summary>
    /// A directional light.
    /// </summary>
    /// <example>
    /// [!code-csharp[Main](../../Elements/test/LightTests.cs?name=directional_example)]
    /// </example>
    public class DirectionalLight : Light
    {
        /// <summary>
        /// Create a directional light.
        /// </summary>
        /// <param name="color">The color of the light.</param>
        /// <param name="intensity">The intensity of the light measured in lux.</param>
        /// <param name="transform">The light's transform.</param>
        /// <param name="id">The unique identifier of the light.</param>
        /// <param name="name">The name of the light.</param>
        public DirectionalLight(Color color,
                                Transform transform,
                                double intensity = 1.0,
                                Guid id = default(Guid),
                                string name = "Sun") : base(intensity,
                                                            color,
                                                            transform == null ? new Transform() : transform,
                                                            id == default(Guid) ? Guid.NewGuid() : id,
                                                            name)
        {
            this.LightType = LightType.Directional;
        }
    }
}