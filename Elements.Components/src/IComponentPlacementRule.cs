using System.Collections.Generic;
using Elements.Geometry;

namespace Elements.Components
{
    public interface IComponentPlacementRule
    {
        string Name { get; set; }
        List<Element> Instantiate(ComponentDefinition definition);
    }

    public interface ICurveBasedComponentPlacementRule : IComponentPlacementRule
    {
        Polyline Curve {get; set;}
        IList<int> AnchorIndices { get; set; }
        IList<Vector3> AnchorDisplacements { get; set; }
    }
}