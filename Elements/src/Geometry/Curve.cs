using System;
using System.Collections.Generic;
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
        public static double MinimumChordLength = 0.1;

        /// <summary>
        /// Get a point along the curve at parameter u.
        /// </summary>
        /// <param name="u"></param>
        /// <returns>A point on the curve at parameter u.</returns>
        public abstract Vector3 PointAt(double u);

        /// <summary>
        /// Get a transform whose XY plane is perpendicular to the curve, and whose
        /// positive Z axis points along the curve.
        /// </summary>
        /// <param name="u">The parameter along the Line, between 0.0 and 1.0, at which to calculate the Transform.</param>
        /// <returns>A transform.</returns>
        public abstract Transform TransformAt(double u);

        /// <summary>
        /// Create a transformed copy of this Curve.
        /// </summary>
        /// <param name="transform">The transform to apply.</param>
        public abstract Curve Transformed(Transform transform);

        /// <summary>
        /// Get the parameter at a distance from the start parameter along the curve.
        /// </summary>
        /// <param name="distance">The distance from the start parameter.</param>
        /// <param name="start">The parameter from which to measure the distance.</param>
        /// <param name="reversed">Should the distance be calculated in the opposite direction of the curve?</param>
        public virtual double ParameterAtDistanceFromParameter(double distance, double start, bool reversed = false)
        {
            throw new NotImplementedException($"This method is not supported for curves of type {GetType().Name}.");
        }
    }
}