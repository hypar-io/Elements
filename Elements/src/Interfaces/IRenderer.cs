using System.Collections.Generic;
using Elements.Geometry;
using Elements.Geometry.Solids;

namespace Elements.Interfaces
{
    /// <summary>
    /// A renderer.
    /// </summary>
    public interface IRenderer
    {
        /// <summary>
        /// Render a mesh.
        /// </summary>
        /// <param name="mesh"></param>
        void Render(Mesh mesh);

        /// <summary>
        /// Render a line.
        /// </summary>
        /// <param name="line"></param>
        void Render(Line line);

        /// <summary>
        /// Render a polyline.
        /// </summary>
        /// <param name="polyline"></param>
        void Render(Polyline polyline);

        /// <summary>
        /// Render a polygon.
        /// </summary>
        /// <param name="polygon"></param>
        void Render(Polygon polygon);

        /// <summary>
        /// Render a collection of solids.
        /// </summary>
        /// <param name="solids"></param>
        void Render(List<SolidOperation> solids);

        /// <summary>
        /// Render a bezier.
        /// </summary>
        /// <param name="bezier"></param>
        void Render(Bezier bezier);

        /// <summary>
        /// Render an arc.
        /// </summary>
        /// <param name="arc"></param>
        void Render(Arc arc);
    }
}