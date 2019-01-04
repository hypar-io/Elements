using System.Collections.Generic;

namespace Elements.Geometry.Interfaces
{
    /// <summary>
    /// A Face.
    /// </summary>
    public interface IFace
    {
        /// <summary>
        /// A type descriptor for use in deserialization.
        /// </summary>
        string Type { get; }
        
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

        /// <summary>
        /// Intersect this face with the specified Plane.
        /// </summary>
        /// <param name="p">A Plane.</param>
        ICurve Intersect(Plane p);
    }
}