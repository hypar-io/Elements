using System;
using System.Collections.Generic;
using System.Linq;
using Elements.Geometry;
using System.Text.Json.Serialization;

namespace Elements.Components
{
    /// <summary>
    /// A rule for placing elements from a collection of possible arrangements, based on available space.
    /// </summary>
    public class SizeBasedPlacementRule : Element, ICurveBasedComponentPlacementRule
    {
        /// <summary>
        /// Construct a new Size-based placement rule from scratch.
        /// </summary>
        /// <param name="elementsAndClearances">The collection of possible elements and their clearances that can be placed.</param>
        /// <param name="boundary">The boundary curve used to determine fit.</param>
        /// <param name="anchorIndices">For each vertex, the index of the corresponding anchor.</param>
        /// <param name="anchorDisplacements">For each vertex, the displacement from its anchor.</param>
        /// <param name="name">The name of the rule.</param>
        public SizeBasedPlacementRule(List<(GeometricElement elem, Polygon clearance)> elementsAndClearances, Polygon boundary, IList<int> anchorIndices, IList<Vector3> anchorDisplacements, string name) : base(Guid.NewGuid(), name)
        {
            Curve = boundary;
            AnchorIndices = anchorIndices;
            AnchorDisplacements = anchorDisplacements;
            Name = name;
            IsPolygon = true;
            ElementsAndClearances = elementsAndClearances.OrderByDescending(item => item.clearance.Area()).ToList();
        }
        /// <summary>
        /// The indices of the source anchors corresponding to each displacement.
        /// </summary>
        public IList<int> AnchorIndices { get; set; }

        /// <summary>
        /// The displacement from each anchor.
        /// </summary>
        public IList<Vector3> AnchorDisplacements { get; set; }

        /// <summary>
        /// The boundary curve used to determine fit.
        /// </summary>
        public Polyline Curve { get; set; }

        /// <summary>
        /// Is the boundary a closed shape?
        /// </summary>
        /// <value></value>

        public bool IsPolygon { get; set; }

        /// <summary>
        /// The collection of possible elements and their clearances that can be placed.
        /// </summary>

        public List<(GeometricElement element, Polygon clearance)> ElementsAndClearances { get; set; }

        /// <summary>
        /// Construct a size-based placement rule based on the closest point to the vertices of a reference polygon.
        /// </summary>
        /// <param name="elementsAndClearances">The collection of possible elements and their clearances that can be placed.</param>
        /// <param name="polygon">The polygon .</param>
        /// <param name="Anchors">The reference anchors from which to calculate the associations.</param>

        /// <param name="name">The name.</param>
        public static SizeBasedPlacementRule FromClosestPoints(List<(GeometricElement elem, Polygon clearance)> elementsAndClearances, Polygon polygon, IList<Vector3> Anchors, string name)
        {
            var anchorIndices = new List<int>();
            var anchorDisplacements = new List<Vector3>();
            foreach (var v in polygon.Vertices)
            {
                var closestAnchorIndex = Enumerable.Range(0, Anchors.Count).OrderBy(a => Anchors[a].DistanceTo(v)).First();
                anchorIndices.Add(closestAnchorIndex);
                var closestAnchor = Anchors[closestAnchorIndex];
                anchorDisplacements.Add(v - closestAnchor);
            }
            return new SizeBasedPlacementRule(elementsAndClearances, polygon, anchorIndices, anchorDisplacements, name);
        }

        /// <summary>
        /// Construct a set of elements from this rule for a given definition.
        /// </summary>
        /// <param name="definition">The boundary curve used to determine fit.</param>
        public List<Element> Instantiate(ComponentDefinition definition)
        {
            var placedElements = new List<Element>();
            List<Vector3> newVertices = TransformPolyline(this, definition);
            Polygon newBoundary = new Polygon(newVertices);
            var targetCenter = newVertices.Distinct().ToList().Average();

            foreach (var possiblePlacement in ElementsAndClearances)
            {
                var clearance = possiblePlacement.clearance.TransformedPolygon(definition.OrientationGuide);
                var clearanceCtr = clearance.Centroid();
                var displacement = new Transform(targetCenter - clearanceCtr);
                //TODO: don't assume options come in descending area order
                if (Covers(newBoundary, clearance.TransformedPolygon(displacement)))
                {
                    possiblePlacement.element.IsElementDefinition = true;
                    var transform = new Transform(definition.OrientationGuide);
                    transform.Concatenate(displacement);
                    if (possiblePlacement.element is InstanceGroup instgrp)
                    {
                        foreach (var elem in instgrp.Instances)
                        {
                            var grpElemtransform = new Transform(elem.Transform);
                            grpElemtransform.Concatenate(transform);
                            var instance = elem.BaseDefinition.CreateInstance(grpElemtransform, null);
                            placedElements.Add(instance);
                        }
                    }
                    else
                    {
                        var instance = possiblePlacement.element.CreateInstance(transform, null);
                        placedElements.Add(instance);
                    }
                    break;
                }

            }

            return placedElements;
        }

        private static bool Covers(Polygon A, Polygon B)
        {
            foreach (var v in B.Vertices)
            {
                if (!A.Covers(v))
                {
                    return false;
                }
            }
            return true;
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
                var transform = new Transform(definition.OrientationGuide.OfPoint(rule.AnchorDisplacements[i]));

                transform.Concatenate(new Transform(displacementForVertex));

                newVertices.Add(transform.OfPoint(anchorForVertex));
            }

            return newVertices;
        }
    }
}