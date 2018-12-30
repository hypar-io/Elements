using System;
using System.Collections.Generic;
using System.Linq;
using Elements.Geometry.Interfaces;

namespace Elements.Geometry
{
    /// <summary>
    /// A PlanarFace bound by 4 edges.
    /// </summary>
    public class QuadFace : PlanarFace
    {
        /// <summary>
        /// Construct a QuadFace.
        /// </summary>
        /// <param name="edges"></param>
        // public QuadFace(IList<ICurve> edges):base(edges)
        // {
        //     if(edges.Count > 4)
        //     {
        //         throw new Exception("A QuadFace can only have 4 edges.");
        //     }
        // }

        /// <summary>
        /// Construct a QuadFace.
        /// </summary>
        /// <param name="vertices"></param>
        public QuadFace(IList<Vector3> vertices):base(vertices){}

        /// <summary>
        /// Compute the Mesh for the QuadFace.
        /// </summary>
        /// <param name="mesh"></param>
        public override void Tessellate(Mesh mesh)
        {
            mesh.AddQuad(this.Vertices.ToList());
        }
    }
}