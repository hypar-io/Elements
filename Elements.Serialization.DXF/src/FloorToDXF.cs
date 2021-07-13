using Elements.Serialization.DXF.Extensions;
using IxMilia.Dxf.Entities;

namespace Elements.Serialization.DXF
{
    public class FloorToDXF : IRenderDxf
    {
        public bool TryToCreateDxfEntity(Element element, DxfRenderContext context, out DxfEntity perimeter)
        {
            var floor = element as Floor;

            var polyline = floor.Profile.Perimeter.ToDxf();
            perimeter = polyline;
            return true;
        }
    }
}