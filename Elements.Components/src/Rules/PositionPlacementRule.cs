using System;
using System.Collections.Generic;
using System.Linq;
using Elements.Geometry;

namespace Elements.Components
{

    public class PositionPlacementRule : IComponentPlacementRule
    {
        public PositionPlacementRule(string name, int anchorIndex, GeometricElement definition, Transform anchorOffset = null)
        {
            Name = name;
            AnchorIndex = anchorIndex;
            AnchorTransform = anchorOffset ?? new Transform();
            Definition = definition;
        }
        public string Name { get; set; }

        public int AnchorIndex { get; set; }
        public Transform AnchorTransform { get; set; }
        public GeometricElement Definition { get; set; }

        public static IList<IComponentPlacementRule> FromClosestPoints(IEnumerable<GeometricElement> elements, IList<Vector3> Anchors)
        {
            var rules = new List<IComponentPlacementRule>();
            foreach (var element in elements)
            {
                var origin = element.Transform.Origin;
                var closestPtIndex = Enumerable.Range(0, Anchors.Count).OrderBy(a => Anchors[a].DistanceTo(origin)).First();
                var closestAnchor = Anchors[closestPtIndex];
                var offsetXForm = new Transform(origin - closestAnchor);
                rules.Add(new PositionPlacementRule(Guid.NewGuid().ToString(), closestPtIndex, element, offsetXForm));

            }

            return rules;
        }

        public Transform GenerateTransform(Transform orientationGuide, IList<Vector3> referenceAnchors, IList<Vector3> anchorDisplacements)
        {
            // offset relative to reference anchor
            var transform = new Transform(AnchorTransform);

            //orientation correction
            transform.Concatenate(orientationGuide);

            //translate to reference anchor
            transform.Concatenate(new Transform(referenceAnchors[AnchorIndex]));

            // from reference anchor to target anchor
            transform.Concatenate(new Transform(anchorDisplacements[AnchorIndex]));
            return transform;
        }
        public List<Element> Instantiate(ComponentDefinition definition)
        {
            var t = GenerateTransform(definition.OrientationGuide, definition.ReferenceAnchors, definition.AnchorDisplacements);
            List<Element> instances = new List<Element>();
            Definition.IsElementDefinition = true;

            instances.Add(Definition.CreateInstance(t, this.Name));

            return instances;
        }
    }
}