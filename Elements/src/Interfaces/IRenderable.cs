namespace Elements.Interfaces
{
    /// <summary>
    /// A rendereable entity.
    /// </summary>
    public interface IRenderable
    {
        /// <summary>
        /// Render.
        /// </summary>
        /// <param name="renderer"></param>
        void Render(IRenderer renderer);
    }
}