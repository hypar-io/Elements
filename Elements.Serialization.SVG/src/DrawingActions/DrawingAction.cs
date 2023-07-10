namespace Elements.Serialization.SVG
{
    /// <summary>
    /// The drawing action (e.g. draw polygon, text)
    /// </summary>
    public abstract class DrawingAction
    {
        /// <summary>
        /// Draw something using input drawing tool
        /// </summary>
        /// <param name="canvas">The canvas where the element will be added</param>
        public abstract void Draw(BaseSvgCanvas canvas);
    }
}