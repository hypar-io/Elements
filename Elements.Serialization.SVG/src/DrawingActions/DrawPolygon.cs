using Elements.Geometry;

namespace Elements.Serialization.SVG
{
    public class DrawPolygon : DrawingAction
    {
        public DrawPolygon(Polygon polygon, SvgContext context, string text = "")
        {
            _polygon = polygon;
            _text = text;
            _context = context;
        }

        internal override void Draw(BaseDrawingTool drawinTool)
        {
            drawinTool.DrawPolygon(_polygon, _context);
        }

        private Polygon _polygon;
        private string _text;
        private SvgContext _context;
    }
}