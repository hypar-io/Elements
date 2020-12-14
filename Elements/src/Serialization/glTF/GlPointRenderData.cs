using System.Collections.Generic;
using Elements.Geometry;

namespace Elements.Serialization.glTF
{
    /// <summary>
    /// 
    /// </summary>
    public class GlPointRenderData
    {
        /// <summary>
        /// A collection of vertices.
        /// </summary>
        public IList<Vector3> Vertices { get; set; }

        /// <summary>
        /// Construct a 
        /// </summary>
        /// <param name="vertices">A collection of vertices.</param>
        public GlPointRenderData(IList<Vector3> vertices)
        {
            this.Vertices = vertices;
        }
    }
}