using netDxf;
using netDxf.Entities;
using Elements;

namespace Elements.DXF
{
    public class DrawingRange
    {
        public Vector3 Direction;
        public Vector3 Origin;
        public double Depth;
    }
    public class DxfRenderContext
    {
        public Model Model;
        public DrawingRange DrawingRange;
    }
    public interface IRenderDxf
    {
        bool TryToCreateDxfEntity(Element element, DxfRenderContext context, out EntityObject entity);

    }
}