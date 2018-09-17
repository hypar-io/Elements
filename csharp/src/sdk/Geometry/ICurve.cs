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
        /// The start of the curve.
        /// </summary>
        Vector3 Start{get;}

        /// <summary>
        /// The end of the curve.
        /// </summary>
        Vector3 End{get;}

        /// <summary>
        /// Get a point along the curve at parameter u.
        /// </summary>
        /// <param name="u">A parameter on the curve between 0.0 and 1.0.</param>
        /// <returns>The point on the curve.</returns>
        Vector3 PointAt(double u);
    }
}