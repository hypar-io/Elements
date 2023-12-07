using Elements.Geometry.Interfaces;
using System;
using System.Collections.Generic;
using Elements.Serialization.JSON;
using System.Text.Json.Serialization;

namespace Elements.Geometry
{
    /// <summary>
    /// The abstract base class for all curves.
    /// </summary>
    [JsonConverter(typeof(ElementConverter<Curve>))]
    public abstract partial class Curve : ICurve, ITransformable<Curve>
    {
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
        /// Use of non-affine transforms (i.e. scale) will
        /// result in unpredictable results for curve methods such as Length().
        /// </summary>
        /// <param name="transform">The transform to apply.</param>
        public abstract Curve Transformed(Transform transform);

        /// <summary>
        /// Get the parameter at a distance from the start parameter along the curve.
        /// </summary>
        /// <param name="distance">The distance from the start parameter.</param>
        /// <param name="start">The parameter from which to measure the distance.</param>
        public abstract double ParameterAtDistanceFromParameter(double distance, double start);

        /// <inheritdoc/>
        public abstract bool Intersects(ICurve curve, out List<Vector3> results);
    }
}