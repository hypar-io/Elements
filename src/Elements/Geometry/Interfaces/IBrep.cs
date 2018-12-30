using System.Collections.Generic;

namespace Elements.Geometry.Interfaces
{
    /// <summary>
    /// A boundary representation containing a collection of
    /// bounded faces.
    /// </summary>
    public interface IBRep: IGeometry3D
    {
        /// <summary>
        /// A collection of Faces.
        /// </summary>
        IFace[] Faces();
    }
}