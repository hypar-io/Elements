using Svg;

namespace Elements.Serialization.SVG
{
    /// <summary>
    /// Contextual data for the SVG serialization process.
    /// </summary>
    public class SvgContext
    {
        /// <summary>
        /// The fill color.
        /// </summary>
        public SvgColourServer? Fill { get; set; }

        /// <summary>
        /// The stroke color.
        /// </summary>
        public SvgColourServer? Stroke { get; set; }

        /// <summary>
        /// The stroke width.
        /// </summary>
        public SvgUnit StrokeWidth { get; set; } = new SvgUnit(SvgUnitType.User, 0.01f);

        /// <summary>
        /// The line end cap style.
        /// </summary>
        public SvgStrokeLineCap? StrokeLineCap { get; set; } = SvgStrokeLineCap.Butt;

        /// <summary>
        /// The pattern of dashes and gaps used to stroke paths.
        /// </summary>
        public SvgUnitCollection? StrokeDashArray { get; set; }
    }
}