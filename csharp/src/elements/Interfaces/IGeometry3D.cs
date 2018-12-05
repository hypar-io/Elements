using Hypar.Elements;
using Hypar.Geometry;
using System.Collections.Generic;

namespace Hypar.Interfaces
{
    /// <summary>
    /// ITessellateMesh is implemented by all types which provide a Mesh for visualization.
    /// </summary>
    public interface IGeometry3D
    {
        /// <summary>
        /// The Material which is used to visualize this geometry.
        /// </summary>
        /// <value></value>
        Material Material{get;}
    }
}