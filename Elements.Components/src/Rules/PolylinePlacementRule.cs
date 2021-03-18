using System;
using System.Collections.Generic;
using System.Linq;
using Elements.Geometry;

namespace Elements.Components
{
    public class PolylinePlacementRule : Element, ICurveBasedComponentPlacementRule
    {
        public PolylinePlacementRule(Polyline p, IList<int> anchorIndices, IList<Vector3> anchorDisplacements, string name) : base(Guid.NewGuid(), name)
        {
            Curve = p;
            AnchorIndices = anchorIndices;
            AnchorDisplacements = anchorDisplacements;
            Name = name;
            IsPolygon = p is Polygon;
        }
        public IList<int> AnchorIndices { get; set; }
        public IList<Vector3> AnchorDisplacements { get; set; }
        public Polyline Curve { get; set; }

        public bool IsPolygon { get; set; }

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