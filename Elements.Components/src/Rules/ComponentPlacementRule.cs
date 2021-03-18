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
        /// <summary>
        /// Construct a new ComponentPlacementRule from scratch
        /// </summary>
        /// <param name="component"></param>
        /// <param name="p"></param>
        /// <param name="anchorIndices"></param>
        /// <param name="anchorDisplacements"></param>
        /// <param name="name"></param>
        /// <param name="anchorTransformer"></param>
        public ComponentPlacementRule(ComponentDefinition component, Polyline p, IList<int> anchorIndices, IList<Vector3> anchorDisplacements, string name, Func<List<Vector3>, List<Vector3>> anchorTransformer = null)
        {
            Component = component;
            Curve = p;
            AnchorIndices = anchorIndices;
            AnchorDisplacements = anchorDisplacements;
            Name = name;
            AnchorTransformer = anchorTransformer;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="component"></param>
        /// <param name="p"></param>
        /// <param name="Anchors"></param>
        /// <param name="name"></param>
        /// <param name="anchorTransformer"></param>
        /// <returns></returns>
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

        /// <summary>
        /// The component definition to place
        /// </summary>

        public ComponentDefinition Component { get; set; }

        /// <summary>
        /// The guide curve acting as the component's boundary
        /// </summary>

        public Polyline Curve { get; set; }
        /// <summary>
        /// The indices of the source anchors corresponding to each displacement
        /// </summary>
        public IList<int> AnchorIndices { get; set; }

        /// <summary>
        /// The displacement from each anchor
        /// </summary>
        public IList<Vector3> AnchorDisplacements { get; set; }

        /// <summary>
        /// An optional function applied to the definition's anchors before calling instantiate on the child component.
        /// </summary>

        public Func<List<Vector3>, List<Vector3>> AnchorTransformer { get; set; }
        /// <summary>
        /// The name of this rule.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Construct a set of elements from this rule for a given definition.
        /// </summary>
        /// <param name="definition"></param>
        public List<Element> Instantiate(ComponentDefinition definition)
        {
            var transformedVertices = PolylinePlacementRule.TransformPolyline(this, definition);
            var newVertices = AnchorTransformer == null ? transformedVertices : AnchorTransformer(transformedVertices);
            return Component.Instantiate(newVertices).Instances;
        }
    }
}