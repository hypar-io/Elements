using Elements.Serialization.DXF.Extensions;
using netDxf.Entities;

namespace Elements.Serialization.DXF
{
    public class FloorToDXF : IRenderDxf
    {
        public bool TryToCreateDxfEntity(Element element, DxfRenderContext context, out EntityObject perimeter)
        {
            var floor = element as Floor;

            perimeter = floor.Profile.Perimeter.ToDxf();
            perimeter.Lineweight = netDxf.Lineweight.W20;

            return true;
        }
    }
}