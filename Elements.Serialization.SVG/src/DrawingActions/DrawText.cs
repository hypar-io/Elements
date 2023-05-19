using Elements.Geometry;

namespace Elements.Serialization.SVG
{
    public class DrawText : DrawingAction
    {
        public DrawText(string text, Transform transform, SvgContext context)
        {
            _text = text;
            _transform = transform;
            _context = context;
        }

        internal override void Draw(BaseDrawingTool drawingTool)
        {
            drawingTool.DrawText(_text, _transform, _context);
        }

        private string _text;
        private Transform _transform;
        private SvgContext _context;
    }
}