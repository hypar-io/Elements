using System;
using System.Collections.Generic;
using System.Linq;
using Elements.Geometry;
using Elements.Geometry.Interfaces;
using Newtonsoft.Json;

namespace Elements.Spatial
{
    /// <summary>
    /// Represents a 2-dimensional grid which can be subdivided 
    /// </summary>
    /// <example>
    /// [!code-csharp[Main](../../Elements/test/Grid2dTests.cs?name=example)]
    /// </example>
    public class Grid2d
    {
        #region Properties

        /// <summary>
        /// Returns true if this 2D Grid has no subdivisions / sub-grids. 
        /// </summary>
        public bool IsSingleCell => U.IsSingleCell && V.IsSingleCell;

        /// <summary>
        /// The 1d Grid along the U dimension
        /// </summary>
        public Grid1d U { get; private set; }

        /// <summary>
        /// The 1d grid along the V dimension
        /// </summary>
        public Grid1d V { get; private set; }

        /// <summary>
        /// An optional type designation for this cell.  
        /// </summary>
        public string Type
        {
            get; set;
        }

        #endregion

        #region Private fields

        /// <summary>
        /// A transform from grid space to world space
        /// </summary>
        private Transform fromGrid = new Transform();

        /// <summary>
        /// A transform from world space to grid space
        /// </summary>
        private Transform toGrid = new Transform();

        private Domain1d UDomainInternal = new Domain1d(0, 0);
        private Domain1d VDomainInternal = new Domain1d(0, 0);

        /// <summary>
        /// Any boundary curves, transformed to grid space. 
        /// </summary>
        private IList<Polygon> boundariesInGridSpace;
        private List<List<Grid2d>> cells;

        #endregion

        #region Constructors

        /// <summary>
        /// Construct a Grid2d with default domain (0,0) to (1,1)
        /// </summary>
        public Grid2d()
        {
            InitializeUV(new Domain1d(), new Domain1d());
        }

        /// <summary>
        /// Construct a Grid2d from another Grid2d
        /// </summary>
        /// <param name="other"></param>
        public Grid2d(Grid2d other)
        {
            this.U = new Grid1d(other.U);
            this.U.SetParent(this);
            this.UDomainInternal = other.UDomainInternal;
            this.V = new Grid1d(other.V);
            this.V.SetParent(this);
            this.VDomainInternal = other.VDomainInternal;
            this.Type = other.Type;
            this.boundariesInGridSpace = other.boundariesInGridSpace;
            this.fromGrid = other.fromGrid;
            this.toGrid = other.toGrid;
            if (!other.IsSingleCell)
            {
                this.Cells = new List<List<Grid2d>>(
                    other.Cells.Select(
                        col => col.Select(
                        cell => new Grid2d(cell)).ToList()
                        ).ToList());
            }
        }

        /// <summary>
        /// Construct a Grid2d using another Grid2d as the base, but with different Grid1ds as its axes. 
        /// </summary>
        /// <param name="other">The Grid2d to base this one on.</param>
        /// <param name="u">The Grid1d representing the U Axis.</param>
        /// <param name="v">The Grid1d representing the V Axis.</param>
        public Grid2d(Grid2d other, Grid1d u, Grid1d v)
        {
            this.U = u;
            this.U.SetParent(this);
            this.UDomainInternal = other.UDomainInternal;
            this.V = v;
            this.V.SetParent(this);
            this.VDomainInternal = other.VDomainInternal;
            this.Type = other.Type;
            this.boundariesInGridSpace = other.boundariesInGridSpace;
            this.fromGrid = other.fromGrid;
            this.toGrid = other.toGrid;
        }

        /// <summary>
        /// Construct a Grid2d from two Grid1ds in the U and V directions
        /// </summary>
        /// <param name="u"></param>1
        /// <param name="v"></param>
        public Grid2d(Grid1d u, Grid1d v)
        {
            this.U = u;
            this.V = v;
            this.U.SetParent(this);
            this.V.SetParent(this);
        }

        /// <summary>
        /// Construct a 2d grid with two 1d domains
        /// </summary>
        /// <param name="uDomain">The domain along the U axis</param>
        /// <param name="vDomain">The domain along the V axis</param>
        private Grid2d(Domain1d uDomain, Domain1d vDomain)
        {
            InitializeUV(uDomain, vDomain);
        }


        /// <summary>
        /// Construct a Grid2d with specified dimensions for the U and V direction.
        /// </summary>
        /// <param name="uDimension">The size along the U axis</param>
        /// <param name="vDimension">The size along the V axis</param>
        public Grid2d(double uDimension, double vDimension)
        {
            InitializeUV(new Domain1d(0, uDimension), new Domain1d(0, vDimension));
        }

        /// <summary>
        /// Create a Grid2d from a polygon and optional Transform.
        /// If the plane is null or not supplied, the identity transform will be used for the grid origin and orientation.
        /// Currently only transforms parallel to the supplied polygons are supported.
        /// The polygon's bounding box parallel to the supplied transform will be
        /// used as the grid extents. 
        /// </summary>
        /// <param name="boundary">The external boundary of this grid system.</param>
        /// <param name="transform">A transform representing the alignment of the grid.</param>
        public Grid2d(Polygon boundary, Transform transform = null) : this(new Polygon[] { boundary }, transform)
        {

        }

        /// <summary>
        /// Create a Grid2d from a list of boundary polygons and an optional transform.
        /// If the transform is null or not supplied, a transform will be generated automatically from the boundaries' normal.
        /// Currently only transforms parallel to the supplied polygons are supported.
        /// The polygons' bounding box parallel to the supplied transform will be
        /// used as the grid extents.
        /// </summary>
        /// <param name="boundaries">The external boundaries of this grid system.</param>
        /// <param name="transform">A transform representing the alignment of the grid.</param>
        public Grid2d(IList<Polygon> boundaries, Transform transform = null)
        {
            if (transform == null)
            {
                //if no transform is supplied, calculate one from the normal. 
                var planeTransform = boundaries.First().Vertices.ToTransform();
                // If we are calculating the transform automatically, then the user has not
                // supplied any rotational orientation information; we only care about
                // direction. So if the polygon is nearly XY-parallel, let's just use
                // the XY plane at the boundary's location to be consistent with default behavior. 
                transform = Math.Abs(planeTransform.ZAxis.Dot(Vector3.ZAxis)).ApproximatelyEquals(1) ?
                    new Transform(planeTransform.Origin) :
                    planeTransform;
            }

            fromGrid = new Transform(transform);
            toGrid = new Transform(transform);
            toGrid.Invert();

            var transformedBoundaries = toGrid.OfPolygons(boundaries);

            // verify that all boundaries are in XY plane after transform
            foreach (var boundary in transformedBoundaries)
            {
                foreach (var vertex in boundary.Vertices)
                {
                    if (!vertex.Z.ApproximatelyEquals(0))
                    {
                        throw new Exception("The Grid2d could not be constructed. After transform, this polygon was not in the XY Plane. Please ensure that all your geometry as well as any provided transform all lie in the same plane.");
                    }
                }
            }
            var bbox = new BBox3(transformedBoundaries);

            boundariesInGridSpace = transformedBoundaries;

            InitializeUV(new Domain1d(bbox.Min.X, bbox.Max.X), new Domain1d(bbox.Min.Y, bbox.Max.Y));
        }

        #endregion

        #region Split Methods

        /// <summary>
        /// Split the grid at points in world space
        /// </summary>
        /// <param name="points">The points at which to split</param>
        public void SplitAtPoints(IEnumerable<Vector3> points)
        {
            foreach (var point in points)
            {
                SplitAtPoint(point);
            }
        }

        /// <summary>
        /// Split the grid at positions in the grid's coordinate system
        /// </summary>
        /// <param name="positions">The positions at which to split, with X = U and Y = V. </param>
        public void SplitAtPositions(IEnumerable<Vector3> positions)
        {
            foreach (var pos in positions)
            {
                SplitAtPosition(pos);
            }
        }

        /// <summary>
        /// Split the grid at a point in world space
        /// </summary>
        /// <param name="point">The point at which to split.</param>
        public void SplitAtPoint(Vector3 point)
        {
            var ptTransformed = toGrid.OfPoint(point);
            U.SplitAtPoint(AxisTransformPoint(GridDirection.U, ptTransformed));
            V.SplitAtPoint(AxisTransformPoint(GridDirection.V, ptTransformed));
        }

        /// <summary>
        /// Split the grid at a position in the grid's coordinate system
        /// </summary>
        /// <param name="position">The position at which to split, with X = U and Y = V.</param>
        public void SplitAtPosition(Vector3 position)
        {
            SplitAtPosition(position.X, position.Y);
        }

        /// <summary>
        /// Split the grid at a position in the grid's coordinate system
        /// </summary>
        /// <param name="uPosition">The U position</param>
        /// <param name="vPosition">The V position</param>
        public void SplitAtPosition(double uPosition, double vPosition)
        {
            if (UDomainInternal.Length > 0)
            {
                uPosition = uPosition.MapBetweenDomains(UDomainInternal, U.Domain);
            }
            if (VDomainInternal.Length > 0)
            {
                vPosition = vPosition.MapBetweenDomains(VDomainInternal, V.Domain);
            }
            U.SplitAtPosition(uPosition);
            V.SplitAtPosition(vPosition);
        }

        #endregion

        #region Cell Retrieval

        /// <summary>
        /// Retrieve the grid cell (as a Grid1d) at a length along the U and V domains. 
        /// </summary>
        /// <param name="uPosition">U Position</param>
        /// <param name="vPosition">V Position</param>
        /// <returns></returns>
        public Grid2d FindCellAtPosition(double uPosition, double vPosition)
        {
            (var uIndex, var vIndex) = FindCellIndexAtPosition(uPosition, vPosition);
            if (uIndex < 0 || vIndex < 0) return this;
            return Cells[uIndex][vIndex];
        }

        /// <summary>
        /// Retrieve the U and V indices of a given cell at a position in grid space.
        /// This is used to map between position and indices.
        /// </summary>
        /// <param name="uPosition"></param>
        /// <param name="vPosition"></param>
        /// <returns></returns>
        private (int u, int v) FindCellIndexAtPosition(double uPosition, double vPosition)
        {

            // TODO: Optimize for smarter retrieval — via indexing or something.
            // Consider https://docs.microsoft.com/en-us/dotnet/api/system.collections.generic.list-1.binarysearch?view=netframework-4.8
            for (int u = 0; u < Cells.Count; u++)
            {
                if (Cells[u][0].U.Domain.Includes(uPosition))
                {
                    for (int v = 0; v < Cells[u].Count; v++)
                    {
                        var cell = Cells[u][v];
                        if (cell.V.Domain.Includes(vPosition))
                        {
                            return (u, v);
                        }

                    }

                }
            }
            return (-1, -1);
        }

        /// <summary>
        /// Get a list of all the top-level cells at a given u index.
        /// </summary>
        /// <param name="u">The u index</param>
        /// <returns>A list of the column of all cells with this u index.</returns>
        public List<Grid2d> GetColumnAtIndex(int u)
        {
            return Cells[u];
        }

        /// <summary>
        /// Get a list of all the top-level cells at a given v index.
        /// </summary>
        /// <param name="v">The v index</param>
        /// <returns>A list of the row of all cells with this v index.</returns>
        public List<Grid2d> GetRowAtIndex(int v)
        {
            return Cells.Select(c => c[v]).ToList();
        }

        /// <summary>
        /// Retrieve a single top-level cell at the specified [u,v] indices.
        /// </summary>
        /// <param name="u">The U index</param>
        /// <param name="v">The V index</param>
        /// <returns>The cell at these indices</returns>
        public Grid2d GetCellAtIndices(int u, int v)
        {
            return Cells[u][v];
        }

        /// <summary>
        /// Retrieve a single top-level cell at the specified [u,v] indices.
        /// </summary>
        /// <param name="u">The U index</param>
        /// <param name="v">The V index</param>
        /// <returns>The cell at these indices</returns>
        public Grid2d this[int u, int v]
        {
            get
            {
                return Cells[u][v];
            }
        }


        /// <summary>
        /// Child cells of this Grid. If null, this Grid is a complete cell with no subdivisions.
        /// </summary>
        public List<List<Grid2d>> Cells
        {
            get
            {
                if (cells == null)
                {
                    cells = GetTopLevelCells();
                }
                return cells;
            }
            private set => cells = value;
        }

        /// <summary>
        /// A flat list of all the top-level cells in this grid. To get child cells as well, use Grid2d.GetCells() instead.
        /// </summary>
        [JsonIgnoreAttribute]
        public List<Grid2d> CellsFlat
        {
            get
            {
                if (Cells == null) return new List<Grid2d>();
                return Cells.SelectMany(c => c).ToList();
            }
        }

        /// <summary>
        /// Get the top-level lines separating cells from one another.
        /// </summary>
        /// <param name="direction">The grid direction in which you want to get separators. </param>
        /// <returns>The lines between cells, running parallel to the grid direction selected. </returns>
        public List<ICurve> GetCellSeparators(GridDirection direction)
        {
            var curves = new List<ICurve>();
            var points = new List<Vector3>();
            Curve otherDirection = null;
            Vector3 toOrigin = new Vector3();
            switch (direction)
            {
                case GridDirection.U:
                    points = V.GetCellSeparators(true);
                    otherDirection = U.curve;
                    toOrigin = GetTransformedOrigin() - V.StartPoint();
                    break;
                case GridDirection.V:
                    points = U.GetCellSeparators(true);
                    otherDirection = V.curve;
                    toOrigin = GetTransformedOrigin() - U.StartPoint();
                    break;
            }
            var originVec = otherDirection.PointAt(0) - toOrigin;
            foreach (var point in points)
            {
                var displacement = new Transform(point - originVec);
                curves.Add(otherDirection.Transformed(displacement).Transformed(fromGrid));
            }

            //TODO: add support for trimmed lines

            return curves;

        }

        /// <summary>
        /// Recursively retrieve all bottom-level cells from this grid.
        /// </summary>
        /// <returns>A list of all bottom-level cells in the grid.</returns>
        public List<Grid2d> GetCells()
        {
            if (IsSingleCell)
            {
                return new List<Grid2d> { this };
            }
            List<Grid2d> resultCells = new List<Grid2d>();
            foreach (var cell in Cells.SelectMany(c => c))
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

        /// <summary>
        /// Get a rectangular polygon representing this untrimmed cell boundary.
        /// </summary>
        /// <returns>A rectangle representing this cell in world coordinates.</returns>
        public Curve GetCellGeometry()
        {
            var baseRect = GetBaseRectangleTransformed();
            return baseRect.TransformedPolygon(fromGrid);
        }

        /// <summary>
        /// Get a list of polygons representing this cell boundary, trimmed by any polygon boundary.
        /// If the cell falls completely outside of the boundary, an empty array will be returned.
        /// </summary>
        /// <returns>Curves representing this cell in world coordinates.</returns>
        public Curve[] GetTrimmedCellGeometry()
        {
            if (boundariesInGridSpace == null || boundariesInGridSpace.Count == 0)
            {
                return new[] { GetCellGeometry() };
            }
            Polygon baseRect = GetBaseRectangleTransformed();
            var trimmedRect = Polygon.Intersection(new[] { baseRect }, boundariesInGridSpace);
            if (trimmedRect != null && trimmedRect.Count() > 0)
            {
                return fromGrid.OfPolygons(trimmedRect);

            }
            return new Curve[0];
        }

        #endregion

        #region Other Public Methods

        /// <summary>
        /// Test if the cell is trimmed by a boundary.
        /// </summary>
        /// <returns>True if the cell is trimmed by the grid boundary.</returns>
        public bool IsTrimmed()
        {
            if (boundariesInGridSpace == null || boundariesInGridSpace.Count == 0)
            {
                return false;
            }

            var baseRect = GetBaseRectangleTransformed();

            var trimmedRect = Polygon.Intersection(new[] { baseRect }, boundariesInGridSpace);
            if (trimmedRect == null || trimmedRect.Count > 1 || trimmedRect.Count < 1) return false;
            return !trimmedRect[0].IsAlmostEqualTo(baseRect, Vector3.EPSILON);
        }

        #endregion

        #region Private Methods
        private void InitializeUV(Domain1d uDomain, Domain1d vDomain)
        {
            var uCrv = new Line(new Vector3(uDomain.Min, 0, 0), new Vector3(uDomain.Max, 0, 0));
            var vCrv = new Line(new Vector3(0, vDomain.Min, 0), new Vector3(0, vDomain.Max, 0));
            UDomainInternal = uDomain;
            VDomainInternal = vDomain;
            U = new Grid1d(uCrv);
            U.SetParent(this);
            V = new Grid1d(vCrv);
            V.SetParent(this);
        }

        /// <summary>
        /// Get the base rectangle of this cell in grid coordinates.
        /// </summary>
        /// <returns></returns>
        private Polygon GetBaseRectangle()
        {
            return Polygon.Rectangle(new Vector3(U.Domain.Min, V.Domain.Min), new Vector3(U.Domain.Max, V.Domain.Max));
        }

        /// <summary>
        /// This method returns the "rectangle" of the cell transformed into the grid's
        /// distorted coordinate space. The result may be a parallelogram rather than a rectangle
        /// depending on the shape of the axis curves. 
        /// </summary>
        /// <returns></returns>
        private Polygon GetBaseRectangleTransformed()
        {
            var A = GetTransformedPoint(U.Domain.Min, V.Domain.Min);
            var B = GetTransformedPoint(U.Domain.Max, V.Domain.Min);
            var C = GetTransformedPoint(U.Domain.Max, V.Domain.Max);
            var D = GetTransformedPoint(U.Domain.Min, V.Domain.Max);
            return new Polygon(new[] { A, B, C, D });
        }

        /// <summary>
        /// This method finds the origin of the transformed 2d grid. Since the axes
        /// may not be perpendicular or intersect at all, the point is located
        /// at the intersection of two lines: one extending in the V direction from the start
        /// of the U axis, and one extending in the U direction from the start of the V axis.
        /// </summary>
        /// <returns></returns>
        private Vector3 GetTransformedOrigin()
        {
            var uVec = U.Direction();
            var uStart = U.StartPoint();
            var vVec = V.Direction();
            var vStart = V.StartPoint();
            var vAxis = new Line(uStart, vVec, 1.0);
            var uAxis = new Line(vStart, uVec, 1.0);
            vAxis.Intersects(uAxis, out Vector3 intersection, true);
            return intersection;
        }

        private Vector3 GetTransformedPoint(double u, double v)
        {
            var uPt = U.Evaluate(u) - U.StartPoint();
            var vPt = V.Evaluate(v) - V.StartPoint();
            return uPt + vPt + GetTransformedOrigin();
        }

        private Vector3 AxisTransformPoint(GridDirection direction, Vector3 point)
        {
            if (direction == GridDirection.U)
            {
                var projVec = V.Direction();
                var vLine = new Line(point, projVec, 1.0);
                var uLine = new Line(U.StartPoint(), U.Direction(), 1.0);
                vLine.Intersects(uLine, out Vector3 result, true);
                return result;
            }
            else
            {
                var projVec = U.Direction();
                var uLine = new Line(point, projVec, 1.0);
                var vLine = new Line(V.StartPoint(), V.Direction(), 1.0);
                uLine.Intersects(vLine, out Vector3 result, true);
                return result;
            }
        }

        private List<List<Grid2d>> GetTopLevelCells()
        {
            if (U.IsSingleCell && V.IsSingleCell)
            {
                return new List<List<Grid2d>> { new List<Grid2d> { this } };
            }
            var cells = new List<List<Grid2d>>();
            var uCells = U.IsSingleCell ? new List<Grid1d> { U } : U.Cells;
            var vCells = V.IsSingleCell ? new List<Grid1d> { V } : V.Cells;
            foreach (var uCell in uCells)
            {
                var column = new List<Grid2d>();
                foreach (var vCell in vCells)
                {
                    Grid2d newCell = SpawnSubGrid(uCell, vCell);
                    column.Add(newCell);
                }
                cells.Add(column);
            }
            return cells;
        }

        private Grid2d SpawnSubGrid(Grid1d uCell, Grid1d vCell)
        {
            var u = U.SpawnSubGrid(uCell.Domain);
            var v = V.SpawnSubGrid(vCell.Domain);
            var newCell = new Grid2d(u, v);
            // Map type name from U and V type names. In most cases this
            // should only be one direction, so we inherit directly.
            if (uCell.Type != null && vCell.Type != null)
            {
                newCell.Type = $"{uCell.Type} / {vCell.Type}";
            }
            else if (uCell.Type != null)
            {
                newCell.Type = uCell.Type;
            }
            else if (vCell.Type != null)
            {
                newCell.Type = vCell.Type;
            }

            newCell.fromGrid = fromGrid;
            newCell.toGrid = toGrid;
            newCell.boundariesInGridSpace = boundariesInGridSpace;

            return newCell;
        }

        #endregion

    }

    /// <summary>
    /// A direction/dimension on a 2d grid.
    /// </summary>
    public enum GridDirection
    {
        /// <summary>
        /// The U Direction
        /// </summary>
        U,
        /// <summary>
        /// The V Direction
        /// </summary>
        V
    }
}
