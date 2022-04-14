using System;
using Elements.Geometry;

namespace Elements
{
    /// <summary>
    /// A spot light.
    /// </summary>
    /// <example>
    /// [!code-csharp[Main](../../Elements/test/LightTests.cs?name=spot_example)]
    /// </example>
    public class SpotLight : Light
    {
        /// <summary>
        /// The inner cone angle in radians.
        /// </summary>
        public double InnerConeAngle { get; set; }

        /// <summary>
        /// The outer cone angle in radians.
        /// </summary>
        public double OuterConeAngle { get; set; }

        /// <summary>
        /// Construct a spot light.
        /// </summary>
        /// <param name="innerConeAngle">The light's inner cone angle.</param>
        /// <param name="outerConeAngle">The light's outer cone angle.</param>
        /// <param name="intensity">The light's intensity measured in candela.</param>
        /// <param name="color">The light's color.</param>
        /// <param name="transform">The light's transform.</param>
        /// <param name="id">The light's unique identifier.</param>
        /// <param name="name">The light's name.</param>
        /// <returns></returns>
        public SpotLight(Color color,
                         Transform transform,
                         double intensity = 1.0,
                         double innerConeAngle = 0.0,
                         double outerConeAngle = Math.PI / 4.0,
                         Guid id = default(Guid),
                         string name = null) : base(intensity,
                                                    color,
                                                    transform == null ? new Transform() : transform,
                                                    id == default(Guid) ? Guid.NewGuid() : id,
                                                    name)
        {
            if (innerConeAngle > Math.PI / 2 || innerConeAngle < 0)
            {
                throw new ArgumentException("The inner cone angle must be greater than zero and less than PI/2.");
            }

            if (outerConeAngle > Math.PI / 2 || outerConeAngle < 0)
            {
                throw new ArgumentException("The outer cone angle must be greater than zero and less than PI/2.");
            }

            if (innerConeAngle > outerConeAngle)
            {
                throw new ArgumentException("The inner cone angle must be less than the outer cone angle.");

            }
            this.InnerConeAngle = innerConeAngle;
            this.OuterConeAngle = outerConeAngle;
            this.LightType = LightType.Spot;
        }
    }
}