using System.Collections.Generic;

namespace Elements.Geometry.Interfaces
{
    /// <summary>
    /// A Face.
    /// </summary>
    public interface IFace
    {
        /// <summary>
        /// A collection of vertices.
        /// </summary>
        Vector3[] Vertices{get;}

        /// <summary>
        /// A collection of edges.
        /// </summary>
        /// <value></value>
        ICurve[] Edges{get;}

        /// <summary>
        /// Get the tesselated represenation of the face.
        /// </summary>
        void Tessellate(Mesh mesh);
    }
}