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
        public string Name { get; set; }

        public IList<int> AnchorIndices { get; set; }
        public IList<Vector3> AnchorDisplacements { get; set; }

        public Polyline Curve { get; set; }

        public GeometricElement ElementDefinition { get; set; }

        public bool IsClosed { get; set; }

        public SpacingConfiguration SpacingRule { get; set; }

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