using System;
using System.Collections.Generic;
using System.Linq;
using Elements.Geometry;
using Elements.Spatial;

namespace Elements.Components
{
    /// <summary>
    /// Define a grid cell subdivision.
    /// </summary>
    public struct GridCellDefinition
    {
        /// <summary>
        /// Construct a new GridCellDefinition
        /// </summary>
        /// <param name="elements">The elements to place in the grid cells</param>
        /// <param name="cellWidth">The cell width</param>
        /// <param name="cellLength">The cell length</param>
        public GridCellDefinition(IEnumerable<GeometricElement> elements, double cellWidth, double cellLength)
        {
            Elements = elements;
            CellWidth = cellWidth;
            CellLength = cellLength;
        }
        /// <summary>
        /// The elements to place
        /// </summary>

        public IEnumerable<GeometricElement> Elements { get; set; }

        /// <summary>
        /// The width of the grid cell
        /// </summary>
        public double CellWidth { get; set; }

        /// <summary>
        /// The length of the grid cell
        /// </summary>

        public double CellLength { get; set; }

    }
    /// <summary>
    /// A rule for placing elements in a grid.
    /// </summary>
    public class GridPlacementRule : ICurveBasedComponentPlacementRule
    {
        /// <summary>
        /// Construct a new grid placement rule from scratch.
        /// </summary>
        /// <param name="definition">The grid cell definition</param>
        /// <param name="gridArea"></param>
        /// <param name="anchorIndices"></param>
        /// <param name="anchorDisplacements"></param>
        /// <param name="name"></param>
        /// <param name="gridCreationRule">An optional action that handles subdividing the created Grid2d.</param>
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
        /// <summary>
        /// The name of this rule.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// The indices of the source anchors corresponding to each displacement
        /// </summary>
        public IList<int> AnchorIndices { get; set; }

        /// <summary>
        /// The displacement from each anchor
        /// </summary>
        public IList<Vector3> AnchorDisplacements { get; set; }

        /// <summary>
        /// The boundary curve in which to construct the grid
        /// </summary>

        public Polyline Curve { get; set; }

        /// <summary>
        /// The cell definition for this grid rule.
        /// </summary>

        public GridCellDefinition CellDefinition { get; set; }

        /// <summary>
        /// An optional function to apply to the grid to override its default subdivision.
        /// </summary>

        public Action<Grid2d> GridCreationRule { get; set; }

        /// <summary>
        /// Construct an ArrayPlacementRule from closest points using a set of reference anchors. Each polyline vertex will be associated with its closest anchor.
        /// </summary>
        /// <param name="cell"></param>
        /// <param name="gridArea"></param>
        /// <param name="Anchors"></param>
        /// <param name="name"></param>
        /// <param name="gridCreationRule"></param>
        /// <returns></returns>
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

        /// <summary>
        /// Construct a set of elements from this rule for a given definition.
        /// </summary>
        /// <param name="definition"></param>
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