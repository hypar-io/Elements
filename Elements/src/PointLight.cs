using System;
using Elements.Geometry;

namespace Elements
{
    /// <summary>
    /// A point light.
    /// </summary>
    /// <example>
    /// [!code-csharp[Main](../../Elements/test/LightTests.cs?name=point_example)]
    /// </example>
    public class PointLight : Light
    {
        /// <summary>
        /// A point light.
        /// </summary>
        /// <param name="intensity">The light's intensity measured in candela.</param>
        /// <param name="color">The light's color.</param>
        /// <param name="transform">The light's transform.</param>
        /// <param name="id">The light's unique id.</param>
        /// <param name="name">The light's name.</param>
        /// <returns></returns>
        public PointLight(Color color,
                          Transform transform,
                          double intensity = 1.0,
                          Guid id = default(Guid),
                          string name = null) : base(intensity,
                                                     color,
                                                     transform == null ? new Transform() : transform,
                                                     id == default(Guid) ? Guid.NewGuid() : id,
                                                     name)
        {
            this.LightType = LightType.Point;
        }
    }
}