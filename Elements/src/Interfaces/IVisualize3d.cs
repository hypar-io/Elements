using Elements.Geometry;

namespace Elements.Interfaces
{
    /// <summary>
    /// Elements which visualize solid and mesh geometry.
    /// </summary>
    public interface IVisualize3d
    {
        /// <summary>
        /// The 3d element's material.
        /// </summary>
        Material Material { get; set; }

        /// <summary>
        /// The 3d element's transform.
        /// </summary>
        Transform Transform { get; set; }

        /// <summary>
        /// Visualize
        /// </summary>
        GraphicsBuffers Visualize3d();
    }

    /// <summary>
    /// Elements which visualize 3d curves.
    /// </summary>
    public interface IVisualizeCurves3d
    {
        /// <summary>
        /// The curve's material.
        /// </summary>
        Material Material { get; set; }

        /// <summary>
        /// The curve's transform.
        /// </summary>
        Transform Transform { get; set; }

        /// <summary>
        /// Visualize
        /// </summary>
        /// <param name="lineLoop">Does the graphics buffer represent a line loop?</param>
        GraphicsBuffers VisualizeCurves3d(bool lineLoop);
    }

    /// <summary>
    /// Elements which visualize points.
    /// </summary>
    public interface IVisualizePoints3d
    {
        /// <summary>
        /// The points' material.
        /// </summary>
        Material Material { get; set; }

        /// <summary>
        /// The points' transform.
        /// </summary>
        Transform Transform { get; set; }

        /// <summary>
        /// Visualize
        /// </summary>
        GraphicsBuffers VisualizePoints3d();
    }
}