using System.Linq;
using Elements.Geometry;

namespace Elements.Serialization.SVG
{
    /// <summary>
    /// The SVG drawing tool
    /// </summary>
    public abstract class BaseSvgCanvas
    {
        /// <summary>
        /// The bounds of the scene.
        /// </summary>
        protected BBox3 SceneBounds { get; set; }
        /// <summary>
        /// The SVG document
        /// </summary>
        protected SvgBaseDrawing Document { get; }

        /// <summary>
        /// The height of the page.
        /// </summary>
        public double PageHeight { get; set; }
        /// <summary>
        /// The width of the page.
        /// </summary>
        public double PageWidth { get; set; }

        /// <summary>
        /// Initializes a new instance of BaseDrawingTool.
        /// </summary>
        /// <param name="document">The SVG document.</param>
        public BaseSvgCanvas(SvgBaseDrawing document)
        {
            Document = document;
        }

        /// <summary>
        /// Sets the scene bounds
        /// </summary>
        /// <param name="sceneBounds">The scene bounding box.</param>
        /// <param name="rotation">The plan rotation angle (in degrees).</param>
        public virtual void SetBounds(BBox3 sceneBounds, double rotation)
        {
            SceneBounds = sceneBounds;
            var transform = new Transform(Vector3.Origin);
            transform.Rotate(rotation);
            var bounds = new BBox3(sceneBounds.Corners().Select(v => transform.OfPoint(v)));
            var viewBoxWidth = (float)(bounds.Max.X - bounds.Min.X);
            var viewBoxHeight = (float)(bounds.Max.Y - bounds.Min.Y);

            PageHeight = viewBoxHeight;
            PageWidth = viewBoxWidth;
        }

        /// <summary>
        /// Draw polygon logic.
        /// </summary>
        /// <param name="polygon">The polygon.</param>
        /// <param name="context">The svg context that can be used to draw the polygon (color, line thickness, etc.)</param>
        public abstract void DrawPolygon(Polygon polygon, SvgContext context);

        /// <summary>
        /// Draw text logic.
        /// </summary>
        /// <param name="text">The text.</param>
        /// <param name="transform">The text transformation matrix.</param>
        /// <param name="context">The svg context that can be used to draw the text (color, line thickness, etc.)</param>
        public abstract void DrawText(string text, Transform transform, SvgContext context);

        /// <summary>
        /// Draw line logic.
        /// </summary>
        /// <param name="line">The line.</param>
        /// <param name="context"></param>
        public abstract void DrawLine(Line line, SvgContext context);

        /// <summary>
        /// Draw circle logic.
        /// </summary>
        /// <param name="center">The circle center.</param>
        /// <param name="radius">The circle radius.</param>
        /// <param name="context">The svg context that can be used to draw the circle (color, line thickness, etc.)</param>
        public abstract void DrawCircle(Vector3 center, double radius, SvgContext context);

        /// <summary>
        /// Close the document.
        /// </summary>
        public abstract void Close();
    }
}