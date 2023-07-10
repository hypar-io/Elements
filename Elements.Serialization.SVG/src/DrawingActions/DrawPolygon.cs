using Elements.Geometry;

namespace Elements.Serialization.SVG
{
    /// <summary>
    /// Draw polygon action
    /// </summary>
    public class DrawPolygon : DrawingAction
    {
        /// <summary>
        /// Initializes a new instance for DrawPolygon class.
        /// </summary>
        /// <param name="polygon">The polygon.</param>
        /// <param name="context">The svg context that can be used to draw the polygon (color, line thickness, etc.)</param>
        /// <param name="text">The text inside the polygon.</param>
        public DrawPolygon(Polygon polygon, SvgContext context, string text = "")
        {
            _polygon = polygon;
            _text = text;
            _context = context;
        }

        /// <summary>
        /// Draws the polygon.
        /// </summary>
        /// <param name="canvas">The canvas where the polygon will be added.</param>
        public override void Draw(BaseSvgCanvas canvas)
        {
            canvas.DrawPolygon(_polygon, _context);
        }

        private Polygon _polygon;
        private string _text;
        private SvgContext _context;
    }
}