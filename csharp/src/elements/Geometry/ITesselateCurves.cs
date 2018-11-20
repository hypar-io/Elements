using System.Collections.Generic;

namespace Hypar.Geometry
{
    /// <summary>
    /// Interface implemented by classes which return curves for rendering.
    /// </summary>
    public interface ITessellateCurves
    {
        /// <summary>
        /// A collection of curves for rendering, specified as collections of Vector3.
        /// </summary>
        /// <returns></returns>
        IList<IList<Vector3>> Curves();
    }
}