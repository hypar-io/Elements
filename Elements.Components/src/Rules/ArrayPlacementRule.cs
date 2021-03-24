using System;
using System.Collections.Generic;
using System.Linq;
using Elements.Geometry;
using Elements.Spatial;

namespace Elements.Components
{
    /// <summary>
    /// A rule for placing elements arrayed along a path.
    /// </summary>
    public class ArrayPlacementRule : ICurveBasedComponentPlacementRule
    {
        /// <summary>
        /// Construct a new array placement rule from scratch.
        /// </summary>
        /// <param name="definition">The element to array.</param>
        /// <param name="arrayPath">The path along which to array.</param>
        /// <param name="spacingRule">The configuration for the spacing.</param>
        /// <param name="anchorIndices">For each vertex, the index of the corresponding anchor.</param>
        /// <param name="anchorDisplacements">For each vertex, the displacement from its anchor.</param>
        /// <param name="name">The name.</param>
        public ArrayPlacementRule(GeometricElement definition, Polyline arrayPath, SpacingConfiguration spacingRule, IList<int> anchorIndices, IList<Vector3> anchorDisplacements, string name)
        {
            Curve = arrayPath;
            AnchorIndices = anchorIndices;
            AnchorDisplacements = anchorDisplacements;
            Name = name;
            IsClosed = arrayPath is Polygon;
            SpacingRule = spacingRule;
            ElementDefinition = definition;
        }
        /// <summary>
        /// The name of this rule.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// The indices of the source anchors corresponding to each displacement.
        /// </summary>
        public IList<int> AnchorIndices { get; set; }

        /// <summary>
        /// The displacement from each anchor.
        /// </summary>
        public IList<Vector3> AnchorDisplacements { get; set; }

        /// <summary>
        /// The path along which the array is constructed.
        /// </summary>
        public Polyline Curve { get; set; }

        /// <summary>
        /// The element to array.
        /// </summary>
        
        public GeometricElement ElementDefinition { get; set; }

        /// <summary>
        /// Is the array path a closed shape?.
        /// </summary>

        public bool IsClosed { get; set; }

        /// <summary>
        /// The spacing configuration for the array.
        /// </summary>

        public SpacingConfiguration SpacingRule { get; set; }

        /// <summary>
        /// Construct an ArrayPlacementRule from closest points using a set of reference anchors. Each polyline vertex will be associated with its closest anchor.
        /// </summary>
        /// <param name="e">The element to array.</param>
        /// <param name="p">The array path.</param>
        /// <param name="spacingRule">The spacing configuration.</param>
        /// <param name="Anchors">The reference anchors from which to calculate the associations.</param>
        /// <param name="name">The rule name.</param>
        public static ArrayPlacementRule FromClosestPoints(GeometricElement e, Polyline p, SpacingConfiguration spacingRule, IList<Vector3> Anchors, string name)
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
            return new ArrayPlacementRule(e, p, spacingRule, anchorIndices, anchorDisplacements, name);
        }

        /// <summary>
        /// Construct a set of elements from this rule for a given definition.
        /// </summary>
        /// <param name="definition">The definition to instantiate.</param>
        public List<Element> Instantiate(ComponentDefinition definition)
        {
            var arrayElements = new List<Element>();
            var newVertices = PolylinePlacementRule.TransformPolyline(this, definition);

            var path = IsClosed ? new Polygon(newVertices) : new Polyline(newVertices);

            var grid1d = new Grid1d(path);
            switch (SpacingRule.SpacingMode)
            {
                case SpacingMode.ByLength:
                    grid1d.DivideByFixedLength(SpacingRule.Value);
                    break;
                case SpacingMode.ByApproximateLength:
                    grid1d.DivideByApproximateLength(SpacingRule.Value);
                    break;
                case SpacingMode.ByCount:
                    grid1d.DivideByCount((int)SpacingRule.Value);
                    break;
            }

            var separators = grid1d.GetCellSeparators();
            foreach (var sep in separators)
            {
                ElementDefinition.IsElementDefinition = true;
                var transform = new Transform(definition.OrientationGuide);
                transform.Concatenate(new Transform(sep));
                var instance = ElementDefinition.CreateInstance(transform, Guid.NewGuid().ToString());
                arrayElements.Add(instance);
            }
            return arrayElements;
        }
    }
}