using System;
using System.Collections.Generic;
using System.Linq;
using Elements.Geometry;
using Elements.Spatial;

namespace Elements.Components
{
    /// <summary>
    /// A rule for placing instances of a component definition.
    /// </summary>
    public class ComponentPlacementRule : ICurveBasedComponentPlacementRule
    {
        public ComponentPlacementRule(ComponentDefinition component, Polyline p, IList<int> anchorIndices, IList<Vector3> anchorDisplacements, string name, Func<List<Vector3>, List<Vector3>> anchorTransformer = null)
        {
            Component = component;
            Curve = p;
            AnchorIndices = anchorIndices;
            AnchorDisplacements = anchorDisplacements;
            Name = name;
            AnchorTransformer = anchorTransformer;
        }

        public static IComponentPlacementRule FromClosestPoints(ComponentDefinition component, Polyline p, IList<Vector3> Anchors, string name, Func<List<Vector3>, List<Vector3>> anchorTransformer = null)
        {
            var anchorIndices = new List<int>();
            var anchorDisplacements = new List<Vector3>();
            foreach (var v in p.Vertices)
            {
                var closestAnchorIndex = Enumerable.Range(0, Anchors.Count).OrderBy(a => Anchors[a].DistanceTo(v)).First();
                anchorIndices.Add(closestAnchorIndex);
                var closestAnchor = Anchors[closestAnchorIndex];
                anchorDisplacements.Add(v - closestAnchor);
            }
            return new ComponentPlacementRule(component, p, anchorIndices, anchorDisplacements, name, anchorTransformer);
        }
        public ComponentDefinition Component { get; set; }
        public Polyline Curve { get; set; }
        public IList<int> AnchorIndices { get; set; }
        public IList<Vector3> AnchorDisplacements { get; set; }

        public Func<List<Vector3>, List<Vector3>> AnchorTransformer { get; set; }
        public string Name { get; set; }

        public List<Element> Instantiate(ComponentDefinition definition)
        {
            var transformedVertices = PolylinePlacementRule.TransformPolyline(this, definition);
            var newVertices = AnchorTransformer == null ? transformedVertices : AnchorTransformer(transformedVertices);
            return Component.Instantiate(newVertices).Instances;
        }
    }
}