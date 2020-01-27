using System;
using System.Collections.Generic;
using Elements.Geometry;
using Elements.MathUtils;

namespace Elements.Spatial
{
    public class Grid2d
    {

        private void InitializeUV(Domain1d uDomain, Domain1d vDomain)
        {
            U = new Grid1d(uDomain);
            U.TopLevelGridChange += TopLevelGridChange;
            V = new Grid1d(vDomain);
            V.TopLevelGridChange += TopLevelGridChange;

        }

        private void TopLevelGridChange(Grid1d sender, EventArgs e)
        {
            Cells = new List<Grid2d>();
            var uCells = U.IsSingleCell ? new List<Grid1d> { U } : U.Cells;
            var vCells = V.IsSingleCell ? new List<Grid1d> { V } : V.Cells;
            foreach (var vCell in vCells)
            {
                foreach (var uCell in uCells)
                {
                    var newCell = new Grid2d(uCell.Domain, vCell.Domain);
                    newCell.fromGrid = fromGrid;
                    newCell.toGrid = toGrid;
                    newCell.boundariesInGridSpace = boundariesInGridSpace;
                    Cells.Add(newCell);
                }
            }
        }

        public Grid2d()
        {
            InitializeUV(new Domain1d(), new Domain1d());
        }

        private Grid2d(Domain1d uDomain, Domain1d vDomain)
        {
            InitializeUV(uDomain, vDomain);
        }

        public Grid2d(double uDimension, double vDimension)
        {
            InitializeUV(new Domain1d(0, uDimension), new Domain1d(0, vDimension));
        }

        /// <summary>
        /// Create a Grid2d from a polygon and optional Transform. If the plane is null or not supplied, the identity transform will be used for the grid origin and orientation.
        /// Currently only transforms parallel to the world XY are supported. 
        /// </summary>
        /// <param name="boundary"></param>
        /// <param name="t">A transform representing the </param>
        public Grid2d(Polygon boundary, Transform t = null) : this(new Polygon[] { boundary }, t)
        {

        }

        public Grid2d(IList<Polygon> boundaries, Transform t = null)
        {
            if (!t.ZAxis.IsParallelTo(Vector3.ZAxis))
            {
                throw new Exception("Currently transforms that are not parallel to the XY Plane are not supported.");
            }

            if (t == null)
            {
                t = new Transform();
            }

            toGrid = new Transform(t);
            t.Invert();
            fromGrid = new Transform(t);

            var transformedBoundaries = toGrid.OfPolygons(boundaries);
            boundariesInGridSpace = transformedBoundaries;
            var bbox = new BBox3(transformedBoundaries);

            InitializeUV(new Domain1d(bbox.Min.X, bbox.Max.X), new Domain1d(bbox.Min.Y, bbox.Max.Y));
        }

        /// <summary>
        /// Retrieve the grid cell (as a Grid1d) at a length along the U and V domains. 
        /// </summary>
        /// <param name="uPos">U position</param>
        /// <param name="vPos">V Position</param>
        /// <returns></returns>
        public Grid2d FindCellAtPosition(double uPos, double vPos)
        {
            var index = FindCellIndexAtPosition(uPos, vPos);
            if (index < 0) return this;
            return Cells[index];
        }

        private int FindCellIndexAtPosition(double uPos, double vPos)
        {

            //TODO: Optimize for smarter retrieval — via 2d indexing or something
            for (int i = 0; i < Cells.Count; i++)
            {
                var cell = Cells[i];
                if (cell.U.Domain.Includes(uPos) && cell.V.Domain.Includes(vPos))
                {
                    return i;
                }
            }
            return -1;
        }


        /// <summary>
        /// Child cells of this Grid. If null, this Grid is a complete cell with no subdivisions.
        /// </summary>
        public List<Grid2d> Cells { get; private set; }


        public List<Grid2d> GetCells()
        {
            List<Grid2d> resultCells = new List<Grid2d>();
            foreach (var cell in Cells)
            {
                if (cell.IsSingleCell)
                {
                    resultCells.Add(cell);
                }
                else
                {
                    resultCells.AddRange(cell.GetCells());
                }
            }
            return resultCells;
        }

        public Curve GetCellGeometry()
        {
            var baseRect = Polygon.Rectangle(new Vector3(U.Domain.Min, V.Domain.Min), new Vector3(U.Domain.Max, V.Domain.Max));
            return fromGrid.OfPolygon(baseRect);
        }

        /// <summary>
        /// Returns true if this 1D Grid has no subdivisions / sub-grids. 
        /// </summary>
        public bool IsSingleCell => Cells == null || Cells.Count == 0;


        /// <summary>
        /// The 1d Grid along the U dimension
        /// </summary>
        public Grid1d U { get; private set; }

        /// <summary>
        /// The 1d grid along the V dimension
        /// </summary>
        public Grid1d V { get; private set; }


        private Transform fromGrid = new Transform();
        private Transform toGrid = new Transform();
        private IList<Polygon> boundariesInGridSpace;
    }
}
