using System.Collections.Generic;
using System.Linq;
using Elements.Geometry;
using System.Text.Json.Serialization;
using System;

namespace Elements.Spatial.CellComplex.Interfaces
{
    /// <summary>
    /// An interface for children of cell complex that have easily defined neighbors of the same class.
    /// </summary>
    /// <typeparam name="ChildClass"></typeparam>
    /// <typeparam name="GeometryType"></typeparam>
    public interface IHasNeighbors<ChildClass, GeometryType>
    {
        /// <summary>
        /// Traverse the neighbors of this element toward the target point.
        /// </summary>
        /// <param name="target"></param>
        /// <param name="completedRadius"></param>
        /// <returns></returns>
        List<ChildClass> TraverseNeighbors(Vector3 target, double completedRadius = 0);

        /// <summary>
        /// Get the closest associated neighbor of this element to the target point.
        /// </summary>
        /// <param name="target"></param>
        /// <returns></returns>
        ChildClass GetClosestNeighbor(Vector3 target);

        /// <summary>
        /// Get all associated neighbors of this element.
        /// </summary>
        /// <returns></returns>
        List<ChildClass> GetNeighbors();
    }
}