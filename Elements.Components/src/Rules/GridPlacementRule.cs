using System;
using System.Collections.Generic;
using System.Linq;
using Elements.Geometry;
using Elements.Spatial;

namespace Elements.Components
{
    public struct GridCellDefinition
    {
        public GridCellDefinition(IEnumerable<GeometricElement> elements, double cellWidth, double cellLength)
        {
            Elements = elements;
            CellWidth = cellWidth;
            CellLength = cellLength;
        }
        public IEnumerable<GeometricElement> Elements { get; set; }
        public double CellWidth { get; set; }
        public double CellLength { get; set; }

    }
    /// <summary>
    /// A rule for placing elements in a grid.
    /// </summary>
    public class GridPlacementRule : ICurveBasedComponentPlacementRule
    {
        public GridPlacementRule(GridCellDefinition definition, Polygon gridArea, IList<int> anchorIndices, IList<Vector3> anchorDisplacements, string name, Action<Grid2d> gridCreationRule = null)
        {
            Curve = gridArea;
            AnchorIndices = anchorIndices;
            AnchorDisplacements = anchorDisplacements;
            Name = name;
            CellDefinition = definition;
            if (gridCreationRule == null)
            {
                GridCreationRule = (Grid2d g) =>
                {
                    g.U.DivideByFixedLength(CellDefinition.CellLength, FixedDivisionMode.RemainderAtBothEnds);
                    g.V.DivideByFixedLength(CellDefinition.CellWidth, FixedDivisionMode.RemainderAtBothEnds);
                };
            }
            else
            {
                GridCreationRule = gridCreationRule;
            }
        }
        public string Name { get; set; }

        public IList<int> AnchorIndices { get; set; }
        public IList<Vector3> AnchorDisplacements { get; set; }

        public Polyline Curve { get; set; }

        public GridCellDefinition CellDefinition { get; set; }

        public Action<Grid2d> GridCreationRule { get; set; }

        public static GridPlacementRule FromClosestPoints(GridCellDefinition cell, Polygon gridArea, IList<Vector3> Anchors, string name, Action<Grid2d> gridCreationRule = null)
        {
            var anchorIndices = new List<int>();
            var anchorDisplacements = new List<Vector3>();
            foreach (var v in gridArea.Vertices)
            {
                var closestAnchorIndex = Enumerable.Range(0, Anchors.Count).OrderBy(a => Anchors[a].DistanceTo(v)).First();
                anchorIndices.Add(closestAnchorIndex);
                var closestAnchor = Anchors[closestAnchorIndex];
                anchorDisplacements.Add(v - closestAnchor);
            }
            return new GridPlacementRule(cell, gridArea, anchorIndices, anchorDisplacements, name, gridCreationRule);
        }

        public List<Element> Instantiate(ComponentDefinition definition)
        {
            var arrayElements = new List<Element>();
            var newVertices = PolylinePlacementRule.TransformPolyline(this, definition);

            var path = new Polygon(newVertices);

            var grid2d = new Grid2d(path, definition.OrientationGuide);
            GridCreationRule(grid2d);

            var cells = grid2d.GetCells().Where(c => !c.IsTrimmed()).SelectMany(c => c.GetTrimmedCellGeometry()).OfType<Polygon>().Where(c => c.Area().ApproximatelyEquals(CellDefinition.CellLength * CellDefinition.CellWidth));

            foreach (var element in CellDefinition.Elements)
            {

                foreach (var cell in cells)
                {
                    var transform = new Transform(element.Transform);
                    transform.Concatenate(definition.OrientationGuide);
                    transform.Concatenate(new Transform(cell.Vertices[2]));
                    element.IsElementDefinition = true;
                    var instance = element.CreateInstance(transform, null);
                    arrayElements.Add(instance);

                }
            }

            arrayElements.AddRange(cells.Select(c => new ModelCurve(c)));
            return arrayElements;
        }
    }
}