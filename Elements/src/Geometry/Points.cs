using System.Collections.Generic;
using Elements.Interfaces;

namespace Elements.Geometry
{
    /// <summary>
    /// A collection of points.
    /// </summary>
    public class Points : List<Vector3>, IRenderable
    {
        /// <summary>
        /// Create a point collection
        /// </summary>
        /// <param name="points"></param>
        /// <returns></returns>
        public Points(IList<Vector3> points) : base(points) { }

        /// <summary>
        /// Render the point collection.
        /// </summary>
        /// <param name="renderer"></param>
        public void Render(IRenderer renderer)
        {
            renderer.Render(this);
        }
    }
}