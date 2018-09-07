using System.Collections.Generic;

namespace Hypar.Geometry
{
    /// <summary>
    /// ICurve is implemented by all curve types.
    /// </summary>
    public interface ICurve: ITessellate<IEnumerable<Vector3>>
    {
        /// <summary>
        /// Get the length of the curve.
        /// </summary>
        /// <returns></returns>
        double Length { get; }

        /// <summary>
        /// Get a point along the curve at parameter u.
        /// </summary>
        /// <param name="u">A parameter on the curve between 0.0 and 1.0.</param>
        /// <returns>The point on the curve.</returns>
        Vector3 PointAt(double u);

        /// <summary>
        /// Get a transform whose XY plane is perpendicular to the curve, and whose
        /// positive Z axis points along the curve.
        /// </summary>
        /// <param name="up">The vector which will become the Y vector of the transform.</param>
        /// <returns>A transform.</returns>
        Transform GetTransform(Vector3 up = null);
    } 
}