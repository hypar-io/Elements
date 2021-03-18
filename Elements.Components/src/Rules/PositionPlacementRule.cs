using System;
using System.Collections.Generic;
using System.Linq;
using Elements.Geometry;

namespace Elements.Components
{
    /// <summary>
    /// A rule for placing a single element with a fixed displacement from an anchor.
    /// </summary>
    public class PositionPlacementRule : IComponentPlacementRule
    {
        /// <summary>
        /// Construct a new position placement rule from scratch.
        /// </summary>
        /// <param name="name"></param>
        /// <param name="anchorIndex"></param>
        /// <param name="definition"></param>
        /// <param name="anchorOffset"></param>
        public PositionPlacementRule(string name, int anchorIndex, GeometricElement definition, Transform anchorOffset = null)
        {
            Name = name;
            AnchorIndex = anchorIndex;
            AnchorTransform = anchorOffset ?? new Transform();
            Definition = definition;
        }
        /// <summary>
        /// The name of this rule.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// The index of the anchor this element transforms with
        /// </summary>
        public int AnchorIndex { get; set; }

        /// <summary>
        /// The transform for this item, relative to its anchor
        /// </summary>
        public Transform AnchorTransform { get; set; }

        /// <summary>
        /// The element placed by this rule.
        /// </summary>
        public GeometricElement Definition { get; set; }

        /// <summary>
        /// Construct a position placement rule by closest point.
        /// </summary>
        /// <param name="elements"></param>
        /// <param name="Anchors"></param>
        /// <returns></returns>
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

        /// <summary>
        /// Calculate the appropriate transform to instantiate this element relative to an anchor.
        /// </summary>
        /// <param name="orientationGuide"></param>
        /// <param name="referenceAnchors"></param>
        /// <param name="anchorDisplacements"></param>
        /// <returns></returns>

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
        /// <summary>
        /// Construct a set of elements from this rule for a given definition.
        /// </summary>
        /// <param name="definition"></param>
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