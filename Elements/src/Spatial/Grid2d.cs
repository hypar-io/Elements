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
    [JsonConverter(typeof(Elements.Serialization.JSON.JsonInheritanceConverter), "discriminator")]
    public class Grid2d
    {
        #region Properties

        /// <summary>
        /// Returns true if this 2D Grid has no subdivisions / sub-grids.
        /// </summary>
        [JsonIgnore]
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
        [JsonProperty("FromGrid")]
        internal Transform fromGrid = new Transform();

        /// <summary>
        /// A transform from world space to grid space
        /// </summary>
        [JsonProperty("ToGrid")]
        internal Transform toGrid = new Transform();

        [JsonProperty("UDomainInternal")]
        private Domain1d UDomainInternal = new Domain1d(0, 0);

        [JsonProperty("VDomainInternal")]
        private Domain1d VDomainInternal = new Domain1d(0, 0);

        /// <summary>
        /// Any boundary curves, transformed to grid space.
        /// </summary>
        [JsonProperty("BoundariesInGridSpace", NullValueHandling = NullValueHandling.Ignore)]
        private IList<Polygon> boundariesInGridSpace;
        private List<List<Grid2d>> cells;

        [JsonProperty("ModifiedChildCells", NullValueHandling = NullValueHandling.Ignore)]
        internal List<IndexedCell> ModifiedChildCells => GetModifiedChildCells();

        // for serialization purposes, we store only those cells that are not a natural consequence of the U and V 1d grids composing this grid.
        private List<IndexedCell> GetModifiedChildCells()
        {
            var indexedCellList = new List<IndexedCell>();
            if (this.cells == null)
            {
                return null;
            }
            for (int i = 0; i < cells.Count; i++)
            {
                List<Grid2d> row = (List<Grid2d>)cells[i];
                for (int j = 0; j < row.Count; j++)
                {
                    Grid2d cell = row[j];
                    if (cell.IsSingleCell)
                    {
                        continue;
                    }
                    else
                    {
                        indexedCellList.Add(new IndexedCell(i, j, cell));
                    }
                }
            }
            return indexedCellList;
        }

        /// <summary>
        /// Represents a subcell at a position in a parent grid
        /// </summary>
        [System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
        public class IndexedCell
        {
            /// <summary>
            /// Make a new indexed cell
            /// </summary>
            /// <param name="i"></param>
            /// <param name="j"></param>
            /// <param name="grid"></param>
            [JsonConstructor]
            public IndexedCell(int i, int j, Grid2d grid)
            {
                this.I = i;
                this.J = j;
                this.Grid = grid;
            }
            /// <summary>
            /// The i Index
            /// </summary>
            public int I { get; set; }

            /// <summary>
            /// The j Index
            /// </summary>
            public int J { get; set; }
            /// <summary>
            /// The grid cell
            /// </summary>
            public Grid2d Grid { get; set; }
        }
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
        /// Do not use this constructor — it is only for serialization purposes.
        /// </summary>
        /// <param name="fromGrid"></param>
        /// <param name="toGrid"></param>
        /// <param name="uDomainInternal"></param>
        /// <param name="vDomainInternal"></param>
        /// <param name="boundariesInGridSpace"></param>
        /// <param name="u"></param>
        /// <param name="v"></param>
        /// <param name="type"></param>
        /// <param name="modifiedChildCells"></param>
        /// <returns></returns>
        [JsonConstructor]
        [System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
        public Grid2d(Transform fromGrid, Transform toGrid, Domain1d uDomainInternal, Domain1d vDomainInternal, List<Polygon> boundariesInGridSpace, Grid1d u, Grid1d v, string type, List<IndexedCell> modifiedChildCells)
        {
            this.fromGrid = fromGrid;
            this.toGrid = toGrid;
            this.UDomainInternal = uDomainInternal;
            this.VDomainInternal = uDomainInternal;
            this.boundariesInGridSpace = boundariesInGridSpace;
            this.U = u;
            this.V = v;
            this.Type = type;
            this.U.SetParent(this);
            this.V.SetParent(this);
            if (modifiedChildCells != null)
            {
                this.cells = GetTopLevelCells();
                foreach (var c in modifiedChildCells)
                {
                    this.Cells[c.I][c.J] = c.Grid;
                }
            }
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

            var bbox = this.SetBoundaries(transformedBoundaries);

            InitializeUV(new Domain1d(bbox.Min.X, bbox.Max.X), new Domain1d(bbox.Min.Y, bbox.Max.Y));
        }

        /// <summary>
        /// Create a grid from a single boundary, an origin, and its U and V directions
        /// </summary>
        /// <param name="boundary"></param>
        /// <param name="origin"></param>
        /// <param name="uDirection"></param>
        /// <param name="vDirection"></param>
        /// <returns></returns>
        public Grid2d(Polygon boundary, Vector3 origin, Vector3 uDirection, Vector3 vDirection) : this(new Polygon[] { boundary }, origin, uDirection, vDirection)
        {

        }

        /// <summary>
        /// Create a grid from a list of boundaries, an origin, and its U and V directions
        /// </summary>
        /// <param name="boundaries"></param>
        /// <param name="origin"></param>
        /// <param name="uDirection"></param>
        /// <param name="vDirection"></param>
        public Grid2d(IList<Polygon> boundaries, Vector3 origin, Vector3 uDirection, Vector3 vDirection)
        {
            var bbox = this.SetBoundaries(boundaries);
            var lines = new List<Line>() {
                 new Line(origin, origin + uDirection),
                 new Line(origin, origin + vDirection)
            };
            ExpandLinesToBounds(bbox, lines);
            this.U = new Grid1d(lines[0]);
            this.V = new Grid1d(lines[1]);
            this.U.SetParent(this);
            this.V.SetParent(this);
        }

        /// <summary>
        /// Create a grid from a boundary and custom U and V grids
        /// </summary>
        /// <param name="boundary"></param>
        /// <param name="u"></param>
        /// <param name="v"></param>
        /// <returns></returns>
        public Grid2d(Polygon boundary, Grid1d u, Grid1d v) : this(new Polygon[] { boundary }, u, v)
        {

        }

        /// <summary>
        /// Create a grid from a list of boundaries and custom U and V grids
        /// </summary>
        /// <param name="boundaries"></param>
        /// <param name="u"></param>
        /// <param name="v"></param>
        public Grid2d(IList<Polygon> boundaries, Grid1d u, Grid1d v)
        {
            this.SetBoundaries(boundaries);
            this.U = u;
            this.V = v;
            this.U.SetParent(this);
            this.V.SetParent(this);
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
            U.SplitAtPoint(AxisTransformPoint(GridDirection.U, point));
            V.SplitAtPoint(AxisTransformPoint(GridDirection.V, point));
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
        [JsonIgnore]
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
        [JsonIgnore]
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
            internal set => cells = value;
        }

        /// <summary>
        /// A flat list of all the top-level cells in this grid. To get child cells as well, use Grid2d.GetCells() instead.
        /// </summary>
        [JsonIgnore]
        public List<Grid2d> CellsFlat
        {
            get
            {
                if (Cells == null) return new List<Grid2d>();
                return Cells.SelectMany(c => c).ToList();
            }
        }

        /// <summary>
        /// Get the points at the corners of all grid cells.
        /// /// </summary>
        /// <returns></returns>
        public List<Vector3> GetCellNodes()
        {
            var points = new List<Vector3>();
            var origin = GetTransformedOrigin();
            var vToOrigin = origin - V.StartPoint();
            var uToOrigin = origin - U.StartPoint();
            var uPoints = U.GetCellSeparators(true);
            var vPoints = V.GetCellSeparators(true);

            foreach (var vpt in vPoints)
            {
                var displacement = new Transform(vpt - uToOrigin);
                // points.AddRange(uPoints.Select(u => fromGrid.OfPoint(displacement.OfPoint(u))));
                points.AddRange(uPoints.Select(u => fromGrid.OfPoint(u + vpt)));
            }
            return points;
        }

        /// <summary>
        /// Get the top-level lines separating cells from one another.
        /// </summary>
        /// <param name="direction">The grid direction in which you want to get separators. </param>
        /// <param name="trim">Whether or not to trim cell separators with the trimmed cell boundary</param>
        /// <returns>The lines between cells, running parallel to the grid direction selected. </returns>
        public List<ICurve> GetCellSeparators(GridDirection direction, bool trim = false)
        {
            var curves = new List<ICurve>();
            var points = new List<Vector3>();
            BoundedCurve otherDirection = null;
            Vector3 toOrigin = new Vector3();
            switch (direction)
            {
                case GridDirection.U:
                    points = V.GetCellSeparators(true);
                    otherDirection = U.Curve;
                    toOrigin = GetTransformedOrigin() - V.StartPoint();
                    break;
                case GridDirection.V:
                    points = U.GetCellSeparators(true);
                    otherDirection = V.Curve;
                    toOrigin = GetTransformedOrigin() - U.StartPoint();
                    break;
            }
            var originVec = otherDirection.Start - toOrigin;
            foreach (var point in points)
            {
                var displacement = new Transform(point - originVec);
                curves.Add(otherDirection.Transformed(displacement).Transformed(fromGrid));
            }


            if (trim && IsTrimmed())
            {
                List<ICurve> trimmedCurves = new List<ICurve>();
                var trimmedCellGeometry = GetTrimmedCellGeometry().OfType<Polygon>();
                // TODO: support keeping polylines joined when trimming. This would depend on an implementation of Polyline.Trim(Polygon p)
                // Currently we simply return the "shattered" lines that result. Since most grids are constructed from linear
                // axes, this is fine most of the time.
                if (trimmedCellGeometry.Count() == 1)
                {
                    var boundary = trimmedCellGeometry.First();
                    var lines = curves.OfType<Line>().Union(curves.OfType<Polyline>().SelectMany(c => c.Segments()));
                    trimmedCurves.AddRange(lines.SelectMany(l => l.Trim(boundary, out var _)));
                }
                else
                {
                    // If we potentially have nested polygons, assume clockwise winding indicates a hole — trim with all the outer polygons first,
                    // and then trim out anything inside the holes.
                    // TODO: get smarter about complex nesting scenarios taking advantage of clipper's PolyTree structure.
                    var outerPolygons = trimmedCellGeometry.Where(p => !p.IsClockWise());
                    var innerPolygons = trimmedCellGeometry.Where(p => p.IsClockWise());
                    foreach (var outerPoly in outerPolygons)
                    {
                        var lines = curves.OfType<Line>().Union(curves.OfType<Polyline>().SelectMany(c => c.Segments()));
                        var intermediateResults = lines.SelectMany(l => l.Trim(outerPoly, out var _));
                        var innerPolygonsWithinOuterPolygon = innerPolygons.Where(i => outerPoly.Contains(i.Vertices.First()));
                        foreach (var ip in innerPolygonsWithinOuterPolygon)
                        {
                            var linesOutsideHole = intermediateResults.SelectMany(ir =>
                            {
                                ir.Trim(ip, out var outsideLines);
                                return outsideLines;
                            });
                            intermediateResults = linesOutsideHole;
                        }
                        trimmedCurves.AddRange(intermediateResults);
                    }
                }

                return trimmedCurves;
            }

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
        public BoundedCurve GetCellGeometry()
        {
            var baseRect = GetBaseRectangleTransformed();
            return baseRect.TransformedPolygon(fromGrid);
        }

        /// <summary>
        /// Get a list of polygons representing this cell boundary, trimmed by any polygon boundary.
        /// If the cell falls completely outside of the boundary, an empty array will be returned.
        /// </summary>
        /// <returns>Curves representing this cell in world coordinates.</returns>
        public BoundedCurve[] GetTrimmedCellGeometry()
        {
            if (boundariesInGridSpace == null || boundariesInGridSpace.Count == 0)
            {
                return new[] { GetCellGeometry() };
            }
            Polygon baseRect = GetBaseRectangleTransformed();
            var trimmedRect = Polygon.Intersection(new[] { baseRect },
                boundariesInGridSpace, PolygonIntersectionTolerance);
            if (trimmedRect != null && trimmedRect.Count() > 0)
            {
                return fromGrid.OfPolygons(trimmedRect);

            }
            return new BoundedCurve[0];
        }

        /// <summary>
        /// Get a list of profiles representing this cell boundary, trimmed by any polygon boundary.
        /// Internal polygons that are completely inside the cell and are clockwise, will be added as profile voids.
        /// If the cell falls completely outside of the boundary, an empty array will be returned.
        /// </summary>
        /// <returns>Curves representing this cell in world coordinates.</returns>
        public IEnumerable<Profile> GetTrimmedCellProfiles()
        {
            Polygon baseRect = GetBaseRectangleTransformed();

            if (boundariesInGridSpace == null || boundariesInGridSpace.Count == 0)
            {
                return new[] { new Profile(baseRect.TransformedPolygon(fromGrid)) };
            }
            var trimmedRect = Polygon.Intersection(new[] { baseRect }, boundariesInGridSpace);
            if (trimmedRect != null && trimmedRect.Count() > 0)
            {
                var profiles = new List<Profile>();
                var outerLoops = trimmedRect.Where(loop => !loop.IsClockWise());
                var innerLoops = trimmedRect.Where(loop => loop.IsClockWise());

                foreach (var item in outerLoops)
                {
                    var inner = innerLoops.Where(loop => loop.Intersects(item)).ToList();
                    profiles.Add(new Profile(item.TransformedPolygon(fromGrid), fromGrid.OfPolygons(inner)));
                }

                return profiles;
            }
            return new Profile[0];
        }

        #endregion

        #region Other Public Methods

        /// <summary>
        /// Test if the cell is trimmed by a boundary.
        /// </summary>
        /// <param name="treatFullyOutsideAsTrimmed">Should cells that fall entirely outside of the boundary be treated as trimmed? True by default.</param>
        /// <returns>True if the cell is trimmed by the grid boundary.</returns>
        public bool IsTrimmed(bool treatFullyOutsideAsTrimmed = true)
        {
            if (boundariesInGridSpace == null || boundariesInGridSpace.Count == 0)
            {
                return false;
            }

            var baseRect = GetBaseRectangleTransformed();
            if (treatFullyOutsideAsTrimmed && IsOutside(baseRect))
            {
                return true;
            }
            var trimmedRect = Polygon.Intersection(new[] { baseRect },
                boundariesInGridSpace, PolygonIntersectionTolerance);
            if (trimmedRect == null || trimmedRect.Count < 1) { return false; }
            if (trimmedRect.Count > 1) { return true; }
            // we have to up the tolerance slightly for this to work consistently.
            return !trimmedRect[0].IsAlmostEqualTo(baseRect, Vector3.EPSILON * 2, true);
        }

        /// <summary>
        /// Test if the cell is fully outside the boundary.
        /// </summary>
        /// <returns>True if the grid cell is totally outside the boundary.</returns>
        public bool IsOutside()
        {
            if (boundariesInGridSpace == null || boundariesInGridSpace.Count == 0)
            {
                return false;
            }
            var baseRect = GetBaseRectangleTransformed();
            return IsOutside(baseRect);
        }

        #endregion

        #region Private/Internal Methods
        /// <summary>
        /// Invalidate the `Cells` property, used by child (axis) 1d grids to tell the parent that they've been updated.
        /// This sort of change is not allowed if the sub-cells already have been further subdivided, as regenerating them
        /// would wipe out these subcells, so in this case an error is thrown.
        /// </summary>
        internal void TryInvalidateGrid()
        {
            if (this.cells != null && this.CellsFlat.Any(c => !c.IsSingleCell))
            {
                throw new NotSupportedException("An attempt was made to modify the underlying U or V grid of a 2D Grid, after some of its cells had already been further subdivided.");
            }
            this.cells = null;
        }

        private bool IsOutside(Polygon baseRect)
        {
            var perimeter = boundariesInGridSpace.First();
            // if any vertex of the rect is fully outside, we are trimmed
            perimeter.Contains(baseRect.Vertices.First(), out var containment);
            if (containment == Containment.Outside)
            {
                return true;
            }
            if (boundariesInGridSpace.Count() > 1)
            {
                return boundariesInGridSpace.Skip(1).Any(boundary => boundary.Covers(baseRect));
            }
            return false;
        }

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
            var transformedOrigin = GetTransformedOrigin();
            var A = GetTransformedPoint(U.Domain.Min, V.Domain.Min, transformedOrigin);
            var B = GetTransformedPoint(U.Domain.Max, V.Domain.Min, transformedOrigin);
            var C = GetTransformedPoint(U.Domain.Max, V.Domain.Max, transformedOrigin);
            var D = GetTransformedPoint(U.Domain.Min, V.Domain.Max, transformedOrigin);
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
            var end1 = uStart + vVec.Unitized();
            var end2 = vStart + uVec.Unitized();
            Line.Intersects(uStart, end1, vStart, end2, out Vector3 intersection, true);
            return intersection;
        }

        private Vector3 GetTransformedPoint(double u, double v, Vector3 origin)
        {
            var uPt = U.Evaluate(u) - U.StartPoint();
            var vPt = V.Evaluate(v) - V.StartPoint();
            return uPt + vPt + origin;
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
            var uCells = U.IsSingleCell ? new List<Grid1d> { U } : U.GetCells();
            var vCells = V.IsSingleCell ? new List<Grid1d> { V } : V.GetCells();
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

        /// <summary>
        /// Sets the clipping boundaries of this grid2d
        /// </summary>
        /// <param name="boundaries"></param>
        /// <returns></returns>
        private BBox3 SetBoundaries(IList<Polygon> boundaries)
        {
            // verify that all boundaries are in XY plane after transform
            foreach (var boundary in boundaries)
            {
                foreach (var vertex in boundary.Vertices)
                {
                    if (!vertex.Z.ApproximatelyEquals(0))
                    {
                        throw new Exception("The Grid2d could not be constructed. After transform, this polygon was not in the XY Plane. Please ensure that all your geometry as well as any provided transform all lie in the same plane.");
                    }
                }
            }

            this.boundariesInGridSpace = boundaries;

            return new BBox3(boundaries);
        }

        /// <summary>
        /// The default extension tolerances of Vector3.EPSILON were creating funny conditions
        /// when gridlines were close to but not quite parallel to a polygon edge.
        /// We pass along a much smaller tolerance when we run our line extensions for Grid2d.
        /// </summary>
        private const double ExtensionTolerance = Vector3.EPSILON * Vector3.EPSILON;

        /// <summary>
        /// Intersection returns quantized distances with step of tolerance.
        /// If intersection region is one EPSILON wide it's highly unstable:
        /// 4.00001 - 4 = 9.9999999996214228E-06 but 4 - 3.99999 = 1.0000000000065512E-05.
        /// And so, such region may be cut off by Intersection - in this case the cell is just skipped.
        /// But if number passed the tolerance test - chances are high it will fail after fromGrid
        /// transformation is applied and then whole trimming process will fail.
        /// To avoid unpredictable behavior double tolerance is used.
        /// </summary>
        private const double PolygonIntersectionTolerance = Vector3.EPSILON * 2;

        /// <summary>
        /// Modifies a list of lines intended to represent uv guides in place to hit the bounds.
        /// Accounts for skewed, parallel lists of 2. If list contains more lines, those will be ignored.
        /// </summary>
        /// <param name="bounds"></param>
        /// <param name="lines"></param>
        /// <returns></returns>
        private static List<Line> ExpandLinesToBounds(BBox3 bounds, List<Line> lines)
        {
            var boundary = Polygon.Rectangle(bounds.Min, bounds.Max);

            for (var i = 0; i < lines.Count(); i++)
            {
                lines[i] = lines[i].ExtendTo(boundary, true, true, ExtensionTolerance);
            }

            var new1 = ExtendLineSkewed(bounds, lines[0], lines[1]);
            var new2 = ExtendLineSkewed(bounds, lines[1], new1);

            new1 = ExtendLineSkewed(bounds, new1, new2);
            new2 = ExtendLineSkewed(bounds, new2, new1);

            lines[0] = new1;
            lines[1] = new2;

            return lines;
        }

        /// <summary>
        /// Extend a line to a bounding box along a second line,
        /// making sure this line extends to the boundary at the endpoints of second line to account for its skew.
        /// Used to make sure U and V guide lines extend out far enough to encompass a boundary.
        /// </summary>
        /// <param name="bounds"></param>
        /// <param name="line"></param>
        /// <param name="possiblySkewedLine"></param>
        /// <returns></returns>
        private static Line ExtendLineSkewed(BBox3 bounds, Line line, Line possiblySkewedLine)
        {
            var boundary = Polygon.Rectangle(bounds.Min, bounds.Max);

            var newLine = new Line(line.Start, line.End);

            if (newLine.Intersects(possiblySkewedLine, out var intersection))
            {
                // move to start and extend
                var toStart = possiblySkewedLine.Start - intersection;
                newLine = newLine.TransformedLine(new Transform(toStart));
                newLine = newLine.ExtendTo(boundary, true, true, ExtensionTolerance);

                // move to end and extend
                var toEnd = possiblySkewedLine.End - possiblySkewedLine.Start;
                newLine = newLine.TransformedLine(new Transform(toEnd));
                newLine = newLine.ExtendTo(boundary, true, true, ExtensionTolerance);

                // move back to original
                var toBeginning = intersection - possiblySkewedLine.End;
                newLine = newLine.TransformedLine(new Transform(toBeginning));
            }

            return newLine;
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
