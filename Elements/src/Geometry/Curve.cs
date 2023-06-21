using Elements.Geometry.Interfaces;
using Newtonsoft.Json;

namespace Elements.Geometry
{
    /// <summary>
    /// The abstract base class for all curves.
    /// </summary>
    [JsonConverter(typeof(Elements.Serialization.JSON.JsonInheritanceConverter), "discriminator")]
    public abstract partial class Curve : ICurve, ITransformable<Curve>
    {
        /// <summary>
        /// The minimum chord length allowed for subdivision of the curve.
        /// A lower MinimumChordLength results in smoother curves.
        /// </summary>
        public static double MinimumChordLength = 0.01;

        /// <summary>
        /// Get a point along the curve at parameter u.
        /// </summary>
        /// <param name="u">A parameter along the curve between domain.min and domain.max.</param>
        /// <returns>A point along the curve at parameter u.</returns>
        public abstract Vector3 PointAt(double u);

        /// <summary>
        /// Get a transform whose XY plane is perpendicular to the curve, and whose
        /// positive Z axis points along the curve.
        /// </summary>
        /// <param name="u">The transform at a parameter along the curve between domain.min and domain.max.</param>
        /// <returns>A transform on the curve at parameter u.</returns>
        public abstract Transform TransformAt(double u);

        /// <summary>
        /// Create a transformed copy of this curve.
        /// </summary>
        /// <param name="transform">The transform to apply.</param>
        public abstract Curve Transformed(Transform transform);

        /// <summary>
        /// Get the parameter at a distance from the start parameter along the curve.
        /// </summary>
        /// <param name="distance">The distance from the start parameter.</param>
        /// <param name="start">The parameter from which to measure the distance.</param>
        public abstract double ParameterAtDistanceFromParameter(double distance, double start);
    }
}