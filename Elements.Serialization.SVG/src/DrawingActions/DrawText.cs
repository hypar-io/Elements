using Elements.Geometry;

namespace Elements.Serialization.SVG
{
    /// <summary>
    /// Draw text action
    /// </summary>
    internal class DrawText : DrawingAction
    {
        /// <summary>
        /// Initializes a new instance for DrawText class.
        /// </summary>
        /// <param name="text">The text.</param>
        /// <param name="transform">The transformation of the text.</param>
        /// <param name="context">The svg context that can be used to draw the text (color, line thickness, etc.)</param>
        public DrawText(string text, Transform transform, SvgContext context)
        {
            _text = text;
            _transform = transform;
            _context = context;
        }

        /// <summary>
        /// Draws the text.
        /// </summary>
        /// <param name="canvas">The canvas where the text will be added.</param>
        public override void Draw(BaseSvgCanvas canvas)
        {
            canvas.DrawText(_text, _transform, _context);
        }

        private string _text;
        private Transform _transform;
        private SvgContext _context;
    }
}