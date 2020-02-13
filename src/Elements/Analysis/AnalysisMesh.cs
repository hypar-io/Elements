using System;
using System.Collections.Generic;
using Elements.Geometry;
using Elements.Geometry.Interfaces;
using Elements.Spatial;
using LibTessDotNet.Double;

namespace Elements.Analysis
{
    /// <summary>
    /// A visualization of computed values at locations in space.
    /// </summary>
    /// <example>
    /// [!code-csharp[Main](../../test/Elements.Tests/AnalysisMeshTests.cs?name=example)]
    /// </example>
    [UserElement]
    public class AnalysisMesh : GeometricElement, ITessellate
    {
        private Grid2d _grid;
        private Func<Vector3, double> _analyze;

        /// <summary>
        /// The length of the cells in the u direction.
        /// </summary>
        public double ULength { get; set; }

        /// <summary>
        /// The length of the cells in the v direction.
        /// </summary>
        public double VLength { get; set; }

        /// <summary>
        /// The perimeter of the analysis mesh.
        /// </summary>
        public Polygon Perimeter { get; set; }

        /// <summary>
        /// The color scale used to represent this analysis mesh.
        /// </summary>
        public ColorScale ColorScale { get; set; }

        /// <summary>
        /// Construct an analysis mesh.
        /// </summary>
        /// <param name="perimeter">The perimeter of the mesh.</param>
        /// <param name="uLength">The number of divisions in the u direction.</param>
        /// <param name="vLength">The number of divisions in the v direction.</param>
        /// <param name="colorScale">The color scale to be used in the visualization.</param>
        /// <param name="analyze">A function which takes a location and computes a value.</param>
        /// <param name="id">The id of the analysis mesh.</param>
        /// <param name="name">The name of the analysis mesh.</param>
        public AnalysisMesh(Polygon perimeter,
                            double uLength,
                            double vLength,
                            ColorScale colorScale,
                            Func<Vector3, double> analyze,
                            Guid id = default(Guid),
                            string name = null) : base(new Transform(),
                                                       BuiltInMaterials.Default,
                                                       null,
                                                       false,
                                                       id == default(Guid) ? Guid.NewGuid() : id,
                                                       name)
        {
            this.Perimeter = perimeter;
            this.ULength = uLength;
            this.VLength = vLength;
            this.ColorScale = colorScale;
            this._analyze = analyze;

            Line longestSegment = null;
            foreach (var s in perimeter.Segments())
            {
                if (longestSegment == null || s.Length() > longestSegment.Length())
                {
                    longestSegment = s;
                }
            }
            var l = longestSegment.Length();

            var gridTransform = new Transform(longestSegment.Start, longestSegment.Direction(), Vector3.ZAxis);
            this._grid = new Grid2d(this.Perimeter, gridTransform);

            this._grid.U.DivideByApproximateLength(this.ULength);
            this._grid.V.DivideByApproximateLength(this.VLength);
        }

        /// <summary>
        /// Tessellate the analysis mesh.
        /// </summary>
        /// <param name="mesh">The mesh into which the analytical cells will be drawn.</param>
        public void Tessellate(ref Mesh mesh)
        {
            var results = new List<double>();
            var cells = new List<Polygon>();

            var min = double.MaxValue;
            var max = double.MinValue;

            var flatCells = this._grid.CellsFlat;

            foreach (var innerGrid in flatCells)
            {
                var innerCells = innerGrid.GetTrimmedCellGeometry() as Polygon[];
                if (innerCells == null)
                {
                    continue;
                }

                foreach (var innerCell in innerCells)
                {
                    var center = innerCell.Centroid();
                    var result = this._analyze(center);
                    results.Add(result);
                    cells.Add(innerCell);
                    min = Math.Min(min, result);
                    max = Math.Max(max, result);
                }
            }

            var span = max - min;
            for (var i = 0; i < cells.Count; i++)
            {
                var cell = cells[i];
                var result = results[i];

                var tess = new Tess();
                tess.NoEmptyPolygons = true;
                tess.AddContour(cell.ToContourVertexArray());
                tess.Tessellate(WindingRule.Positive, LibTessDotNet.Double.ElementType.Polygons, 3);

                for (var j = 0; j < tess.ElementCount; j++)
                {
                    var a = tess.Vertices[tess.Elements[j * 3]].Position.ToVector3();
                    var b = tess.Vertices[tess.Elements[j * 3 + 1]].Position.ToVector3();
                    var c = tess.Vertices[tess.Elements[j * 3 + 2]].Position.ToVector3();

                    var color = this.ColorScale.GetColorForValue((result - min) / span);

                    var v1 = mesh.AddVertex(a, new UV(), color: color);
                    var v2 = mesh.AddVertex(b, new UV(), color: color);
                    var v3 = mesh.AddVertex(c, new UV(), color: color);
                    mesh.AddTriangle(v1, v2, v3);
                }
            }
        }
    }
}