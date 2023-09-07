using System.Collections.Generic;

namespace Elements.Geometry.Interfaces
{
    /// <summary>
    /// An infinite curve.
    /// </summary>
    public interface ICurve
    {
        /// <summary>
        /// Get a point along the curve at parameter u.
        /// </summary>
        /// <param name="u">A parameter on the curve.</param>
        /// <returns>The point on the curve.</returns>
        Vector3 PointAt(double u);

        /// <summary>
        /// Get the frame from the curve at parameter u.
        /// </summary>
        /// <param name="u">A parameter on the curve.</param>
        /// <returns>The transform of the curve at parameter u, with the transform's Z axis tangent to the curve.</returns>
        Transform TransformAt(double u);

        /// <summary>
        /// Get the parameter at a distance from the specified parameter along the curve.
        /// </summary>
        /// <param name="distance">The distance from the start parameter.</param>
        /// <param name="parameter">The parameter from which to measure the distance.</param>
        double ParameterAtDistanceFromParameter(double distance, double parameter);

        /// <summary>
        /// Does this curve intersect the provided curve?
        /// </summary>
        /// <param name="curve">Curve to intersect.</param>
        /// <param name="results">List of intersection points, empty if there is no intersection.</param>
        /// <returns>True if any intersections exist, otherwise false.</returns>
        bool Intersects(ICurve curve, out List<Vector3> results);
    }
}