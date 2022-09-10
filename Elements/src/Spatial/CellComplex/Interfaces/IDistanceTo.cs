using System.Collections.Generic;
using System.Linq;
using Elements.Geometry;
using System.Text.Json.Serialization;
using System;

namespace Elements.Spatial.CellComplex.Interfaces
{
    /// <summary>
    /// This interface is used when we expect a class to have a `DistanceTo` method.
    /// </summary>
    public interface IDistanceTo
    {
        /// <summary>
        /// Provides the closest distance from an element to the provided point.
        /// </summary>
        /// <param name="point"></param>
        /// <returns></returns>
        double DistanceTo(Vector3 point);
    }
}