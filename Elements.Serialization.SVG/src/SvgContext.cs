using Svg;

namespace Elements.Serialization.SVG
{
    public class SvgContext
    {
        public SvgColourServer? Fill { get; set; }
        public SvgColourServer? Stroke { get; set; }
        public SvgUnit StrokeWidth { get; set; } = new SvgUnit(SvgUnitType.User, 0.01f);
        public SvgStrokeLineCap? StrokeLineCap { get; set; } = SvgStrokeLineCap.Butt;
        public SvgUnitCollection? StrokeDashArray { get; set; }
    }
}