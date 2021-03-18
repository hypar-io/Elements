using System;
using System.Collections.Generic;
using System.Linq;
using Elements.Geometry;

namespace Elements.Components
{
    /// <summary>
    /// A rule for transforming a polyline or polygon
    /// </summary>
    public class PolylinePlacementRule : Element, ICurveBasedComponentPlacementRule
    {
        /// <summary>
        /// Construct a new Polyline Placement rule
        /// </summary>
        /// <param name="p"></param>
        /// <param name="anchorIndices"></param>
        /// <param name="anchorDisplacements"></param>
        /// <param name="name"></param>
        /// <returns></returns>
        public PolylinePlacementRule(Polyline p, IList<int> anchorIndices, IList<Vector3> anchorDisplacements, string name) : base(Guid.NewGuid(), name)
        {
            Curve = p;
            AnchorIndices = anchorIndices;
            AnchorDisplacements = anchorDisplacements;
            Name = name;
            IsPolygon = p is Polygon;
        }
        /// <summary>
        /// The indices of the source anchors corresponding to each displacement
        /// </summary>
        public IList<int> AnchorIndices { get; set; }

        /// <summary>
        /// The displacement from each anchor
        /// </summary>
        public IList<Vector3> AnchorDisplacements { get; set; }

        /// <summary>
        /// The curve being deformed by this rule
        /// </summary>
        public Polyline Curve { get; set; }

        /// <summary>
        /// Should the curve be treated as a closed polygon?
        /// </summary>
        public bool IsPolygon { get; set; }

        /// <summary>
        /// Construct a PolylinePlacementRule from closest points using a set of reference anchors. Each polyline vertex will be associated with its closest anchor.
        /// </summary>
        /// <param name="p"></param>
        /// <param name="Anchors"></param>
        /// <param name="name"></param>
        /// <returns></returns>
        public static PolylinePlacementRule FromClosestPoints(Polyline p, IList<Vector3> Anchors, string name)
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
            return new PolylinePlacementRule(p, anchorIndices, anchorDisplacements, name);
        }

        /// <summary>
        /// Construct a set of elements from this rule for a given definition.
        /// </summary>
        /// <param name="definition"></param>
        public List<Element> Instantiate(ComponentDefinition definition)
        {
            var transformedCurves = new List<Element>();
            List<Vector3> newVertices = TransformPolyline(this, definition);

            transformedCurves.Add(new ModelCurve(IsPolygon ? new Polygon(newVertices) : new Polyline(newVertices)));
            return transformedCurves;
        }

        internal static List<Vector3> TransformPolyline(ICurveBasedComponentPlacementRule rule, ComponentDefinition definition)
        {
            var newVertices = new List<Vector3>();
            for (int i = 0; i < rule.Curve.Vertices.Count; i++)
            {
                var anchorIndex = rule.AnchorIndices[i];

                var anchorForVertex = definition.ReferenceAnchors[anchorIndex];
                var displacementForVertex = definition.AnchorDisplacements[anchorIndex];

                // transform from reference anchor to polyline vertex
                var transform = new Transform(definition.OrientationGuide.OfVector(rule.AnchorDisplacements[i]));

                transform.Concatenate(new Transform(displacementForVertex));

                newVertices.Add(transform.OfPoint(anchorForVertex));
            }

            return newVertices;
        }
    }
}