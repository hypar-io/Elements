using System;
using System.Collections.Generic;
using System.Linq;
using Elements.Geometry;
using Newtonsoft.Json;

namespace Elements.Spatial
{
    /// <summary>
    /// Represents a "1-dimensional grid", akin to a number line that can be subdivided.
    /// </summary>
    /// <example>
    /// [!code-csharp[Main](../../Elements/test/Grid1dTests.cs?name=example)]
    /// </example>
    [JsonConverter(typeof(Elements.Serialization.JSON.JsonInheritanceConverter), "discriminator")]
    public class Grid1d
    {
        #region Properties

        /// <summary>
        /// An optional type designation for this cell.
        /// </summary>
        public string Type { get; set; }

        /// <summary>
        /// Child cells of this Grid. If null, this Grid is a complete cell with no subdivisions.
        /// </summary>
        [JsonProperty("Cells", NullValueHandling = NullValueHandling.Ignore)]
        public List<Grid1d> Cells
        {
            get => cells;
            private set
            {
                //invalidate the 2d grid this belongs to
                parent?.TryInvalidateGrid();
                cells = value;
            }
        }

        /// <summary>
        /// Numerical domain of this Grid
        /// </summary>
        public Domain1d Domain { get; }

        /// <summary>
        /// The base curve at the top level of this grid.
        /// </summary>
        /// <value></value>
        [JsonIgnore]
        public BoundedCurve Curve
        {
            get
            {
                if (this.curve != null)
                {
                    return this.curve;
                }
                else
                {
                    if (this.topLevelParentGrid != null)
                    {
                        this.curve = this.topLevelParentGrid.curve;
                        return this.curve;
                    }
                    return null;
                }
            }
        }


        /// <summary>
        /// Returns true if this 1D Grid has no subdivisions / sub-grids.
        /// </summary>
        public bool IsSingleCell => Cells == null || Cells.Count == 0;

        #endregion

        #region Private fields

        // The curve this was generated from, often a line.
        // subdivided cells maintain the complete original curve,
        // rather than a subcurve.
        internal BoundedCurve curve;

        // we have to maintain an internal curve domain because subsequent subdivisions of a grid
        // based on a curve retain the entire curve; this domain allows us to map from the subdivided
        // domain back to the original curve.
        [JsonProperty("CurveDomain")]
        internal readonly Domain1d curveDomain;

        // if this 1d grid is the axis of a 2d grid, this is where we store that reference. If not, it will be null
        private Grid2d parent;

        // if this is a cell belonging to a parent grid, this is where we store the very topmost grid. This
        // is useful in serialization so we only store the base curve once.
        private Grid1d topLevelParentGrid;

        [JsonProperty("TopLevelParentCurve", NullValueHandling = NullValueHandling.Ignore)]
        private Curve topLevelParentCurve
        {
            get
            {
                // if we ARE the top level grid, we have no top-level grid, so we return the curve
                if (this.topLevelParentGrid == null)
                {
                    return this.curve;
                }
                else
                {
                    return null;
                }
            }
        }

        private List<Grid1d> cells;

        #endregion

        #region Constructors
        /// <summary>
        /// Do not use this constructor — it is only for serialization purposes.
        /// </summary>
        /// <param name="cells"></param>
        /// <param name="type"></param>
        /// <param name="domain"></param>
        /// <param name="topLevelParentCurve"></param>
        /// <param name="curveDomain"></param>
        [JsonConstructor]
        [System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
        public Grid1d(List<Grid1d> cells,
                      string type,
                      Domain1d domain,
                      BoundedCurve topLevelParentCurve,
                      Domain1d curveDomain)
        {
            if (topLevelParentCurve != null)
            {
                // we're deserializing the toplevel grid
                this.curve = topLevelParentCurve;
            }

            this.curveDomain = curveDomain;
            this.Type = type;
            this.Domain = domain;
            this.Cells = cells;
        }

        /// <summary>
        /// Default constructor with optional length parameter
        /// </summary>
        /// <param name="length">Length of the grid domain</param>
        public Grid1d(double length = 1.0) : this(new Domain1d(0, length))
        {

        }

        /// <summary>
        /// Construct a 1D Grid from another 1D Grid
        /// </summary>
        /// <param name="other"></param>
        public Grid1d(Grid1d other)
        {
            this.curve = other.curve;
            this.curveDomain = other.curveDomain;
            this.Domain = other.Domain;
            if (other.Cells != null)
            {
                this.Cells = other.Cells.Select(c => new Grid1d(c)).ToList();
            }
            this.Type = other.Type;
        }

        /// <summary>
        /// Construct a 1D grid from a numerical domain. The geometry will be assumed to lie along the X axis.
        /// </summary>
        /// <param name="domain">The 1-dimensional domain for the grid extents.</param>
        public Grid1d(Domain1d domain)
        {
            Domain = domain;
            curve = new Line(new Vector3(domain.Min, 0, 0), new Vector3(Domain.Max, 0, 0));
            curveDomain = domain;
        }

        /// <summary>
        /// Construct a 1D grid from a curve.
        /// </summary>
        /// <param name="curve">The curve from which to generate the grid.</param>
        public Grid1d(BoundedCurve curve)
        {
            this.curve = curve;
            Domain = new Domain1d(0, curve.Length());
            curveDomain = Domain;
        }

        /// <summary>
        /// This constructor is only for internal use by subdivision / split methods.
        /// </summary>
        /// <param name="topLevelParent">The top level grid1d, containing the base curve</param>
        /// <param name="domain">The domain of the new subdivided segment</param>
        /// <param name="curveDomain">The entire domain of the parent grid's curve</param>
        private Grid1d(Grid1d topLevelParent, Domain1d domain, Domain1d curveDomain)
        {
            this.topLevelParentGrid = topLevelParent;
            this.curve = topLevelParent.curve;
            this.curveDomain = curveDomain;
            this.parent = topLevelParent.parent;
            Domain = domain;
        }

        #endregion

        #region Split Methods


        /// <summary>
        /// Split the grid at a normalized parameter from 0 to 1 along its domain.
        /// </summary>
        /// <param name="t">The parameter at which to split.</param>
        public void SplitAtParameter(double t)
        {
            var pos = t.MapToDomain(Domain);
            SplitAtPosition(pos);
        }

        /// <summary>
        /// Split the grid at a list of normalized parameters from 0 to 1 along its domain.
        /// </summary>
        /// <param name="parameters">A list of parameters at which to split the grid.</param>
        public void SplitAtParameters(IEnumerable<double> parameters)
        {
            foreach (var t in parameters)
            {
                SplitAtParameter(t);
            }
        }

        /// <summary>
        /// Split the grid at a fixed position from the start or end
        /// </summary>
        /// <param name="position">The length along the grid at which to split.</param>
        public void SplitAtPosition(double position)
        {

            if (PositionIsAtCellEdge(position)) // already split at this location
            {
                return; // swallow silently.
            }
            if (!Domain.Includes(position))
            {
                throw new ArgumentException("Cannot split at position outside of cell domain.");
            }
            if (IsSingleCell) // simple single split
            {
                var newDomains = Domain.SplitAt(position);
                Cells = new List<Grid1d>
                {
                    new Grid1d(this, newDomains[0], curveDomain),
                    new Grid1d(this, newDomains[1], curveDomain)
                };
            }
            else
            {
                // find this-level cell to split
                var index = FindCellIndexAtPosition(position);
                var cellToSplit = Cells[index];
                var isSingleCell = cellToSplit.IsSingleCell;
                cellToSplit.SplitAtPosition(position);

                if (isSingleCell) // if we're splitting a cell with no children, we replace it directly with the split cells
                {
                    var replacementCells = cellToSplit.Cells;
                    if (replacementCells == null) return; // if we tried to split but hit an edge, for instance
                    Cells.RemoveAt(index);
                    Cells.InsertRange(index, replacementCells);
                }
                else // otherwise, we split it AND split its parent
                {
                    if (cellToSplit.PositionIsAtCellEdge(position))
                    {
                        return; //swallow silently
                    }
                    var newDomains = cellToSplit.Domain.SplitAt(position);
                    var cellsToInsert = new List<Grid1d>
                    {
                        new Grid1d(this, newDomains[0], curveDomain),
                        new Grid1d(this, newDomains[1], curveDomain)
                    };
                    Cells.RemoveAt(index);
                    cellsToInsert.ForEach(c => c.Cells = new List<Grid1d>());
                    Cells.InsertRange(index, cellsToInsert);
                    // The split of "cellToSplit" could have resulted in any number of new cells;
                    // these need to be reallocated to the correct parent.
                    var childrenToReallocate = cellToSplit.Cells;
                    foreach (var child in childrenToReallocate)
                    {
                        if (child.Domain.Max <= position) // left of position
                        {
                            cellsToInsert[0].Cells.Add(child);
                        }
                        else // right of position
                        {
                            cellsToInsert[1].Cells.Add(child);
                        }
                    }
                }

                // Direct `set` to the `Cells` property will trigger this
                // automatically, but in this pathway we also use `Insert`,
                // `RemoveAt`, `AddRange`, etc, which won't cause the parent to
                // update, so we call it explicitly here.
                parent?.TryInvalidateGrid();
            }
        }

        /// <summary>
        /// Split a cell at a relative position measured from its domain start or end.
        /// </summary>
        /// <param name="position">The relative position at which to split.</param>
        /// <param name="fromEnd">If true, measure the position from the end rather than the start</param>
        /// <param name="ignoreOutsideDomain">If true, splits at offsets outside the domain will be silently ignored.</param>
        public void SplitAtOffset(double position, bool fromEnd = false, bool ignoreOutsideDomain = false)
        {
            InternalSplitAtOffset(position, fromEnd, ignoreOutsideDomain, false);
        }

        /// <summary>
        /// This private method is called by public SplitAtOffset, as well as by SplitAtPoint, which calculates its position relative to the
        /// overall curve domain, rather than relative to the grid's own (possibly different) subdomain.
        /// </summary>
        /// <param name="position">The relative position at which to split.</param>
        /// <param name="fromEnd">If true, measure the position from the end rather than the start</param>
        /// <param name="ignoreOutsideDomain">If true, splits at offsets outside the domain will be silently ignored.</param>
        /// <param name="useCurveDomain">If true, the position is measured relative to the top-level curve domain, not the subdomain. </param>
        private void InternalSplitAtOffset(double position, bool fromEnd = false, bool ignoreOutsideDomain = false, bool useCurveDomain = false)
        {
            var domain = useCurveDomain ? curveDomain : Domain;
            position = fromEnd ? domain.Max - position : domain.Min + position;
            if (PositionIsAtCellEdge(position))
            {
                return; // this should be swallowed silently rather than left for Domain.Includes to handle.
            }
            if (!domain.Includes(position))
            {
                if (ignoreOutsideDomain)
                {
                    return;
                }
                else
                {
                    throw new Exception("Offset position was beyond the grid's domain.");
                }
            }
            SplitAtPosition(position);
        }

        /// <summary>
        /// Split a cell at a list of relative positions measured from its domain start or end.
        /// </summary>
        /// <param name="positions">The relative positions at which to split.</param>
        /// <param name="fromEnd">If true, measure the position from the end rather than the start</param>
        public void SplitAtOffsets(IEnumerable<double> positions, bool fromEnd = false)
        {
            foreach (var position in positions)
            {
                SplitAtOffset(position, fromEnd);
            }
        }

        /// <summary>
        /// Split the grid at a list of fixed positions from the start or end
        /// </summary>
        /// <param name="positions">The lengths along the grid at which to split.</param>
        public void SplitAtPositions(IEnumerable<double> positions)
        {
            foreach (var pos in positions)
            {
                SplitAtPosition(pos);
            }
        }

        /// <summary>
        /// Split the grid at a point in world space. Note that for curved grids an approximate
        /// point will be used.
        /// </summary>
        /// <param name="point"></param>
        public void SplitAtPoint(Vector3 point)
        {
            double posAlongCurve = ClosestPosition(point);
            InternalSplitAtOffset(posAlongCurve, false, true, true);
        }

        /// <summary>
        /// Get the position along the grid's domain closest to a supplied point.
        /// </summary>
        /// <param name="point"></param>
        public double ClosestPosition(Vector3 point)
        {
            // If we have a 2d parent, it's most intuitive to transform the point into grid space before doing this calculation.
            if (parent != null)
            {
                point = parent.toGrid.OfPoint(point);
            }
            if (Curve is Polyline pl && pl.Segments().Count() > 1)
            {
                var minDist = Double.MaxValue;
                Line[] segments = pl.Segments();
                int closestSegment = -1;
                for (int i = 0; i < segments.Length; i++)
                {
                    Line seg = segments[i];
                    var cp = point.ClosestPointOn(seg);
                    var dist = cp.DistanceTo(point);
                    if (dist < minDist)
                    {
                        minDist = dist;
                        closestSegment = i;
                    }
                }
                double curvePosition = 0.0;
                for (int i = 0; i < closestSegment; i++)
                {
                    curvePosition += segments[i].Length();
                }
                curvePosition += segments[closestSegment].Start.DistanceTo(point);
                return curvePosition;
            }
            var A = Curve.Start;
            var B = Curve.End;
            var C = point;
            var AB = B - A;
            AB = AB.Unitized();
            var AC = C - A;
            var posAlongCurve = AC.Dot(AB);
            return posAlongCurve;
        }


        /// <summary>
        /// Split the grid at points in world space. Note that for curved grids an approximate
        /// point will be used.
        /// </summary>
        /// <param name="points">The points at which to split.</param>
        public void SplitAtPoints(IEnumerable<Vector3> points)
        {
            foreach (var pos in points)
            {
                SplitAtPoint(pos);
            }
        }

        #endregion

        #region Divide Methods

        /// <summary>
        /// Divide the grid into N even subdivisions. Grids that are already subdivided will fail.
        /// </summary>
        /// <param name="n">Number of subdivisions</param>
        public void DivideByCount(int n)
        {
            if (n <= 0)
            {
                throw new ArgumentException($"Unable to divide by {n}.");
            }
            if (!IsSingleCell)
            {
                throw new Exception("This grid already has subdivisions. Maybe you meant to select a subgrid to divide?");
            }

            var newDomains = Domain.DivideByCount(n);
            Cells = new List<Grid1d>(newDomains.Select(d => new Grid1d(this, d, curveDomain)));
        }

        /// <summary>
        /// Divide a grid by an approximate length. The length will be adjusted to generate whole-number
        /// subdivisions, governed by an optional DivisionMode.
        /// </summary>
        /// <param name="targetLength">The approximate length by which to divide the grid.</param>
        /// <param name="divisionMode">Whether to permit any size cell, or only larger or smaller cells by rounding up or down.</param>
        public void DivideByApproximateLength(double targetLength, EvenDivisionMode divisionMode = EvenDivisionMode.Nearest)
        {
            if (targetLength <= Vector3.EPSILON)
            {
                throw new ArgumentException($"Unable to divide. Target Length {targetLength} is too small.");
            }
            var numDivisions = Math.Max(1, Domain.Length / targetLength);
            int roundedDivisions;
            switch (divisionMode)
            {
                case EvenDivisionMode.RoundUp:
                    roundedDivisions = (int)Math.Ceiling(numDivisions);
                    break;
                case EvenDivisionMode.RoundDown:
                    roundedDivisions = (int)Math.Floor(numDivisions);
                    break;
                case EvenDivisionMode.Nearest:
                default:
                    roundedDivisions = (int)Math.Round(numDivisions);
                    break;
            }
            DivideByCount(roundedDivisions);
        }

        /// <summary>
        /// Divide a grid by constant length subdivisions, starting from a point location.
        /// </summary>
        /// <param name="length">The length of subdivisions</param>
        /// <param name="point">The point at which to begin subdividing.</param>
        public void DivideByFixedLengthFromPoint(double length, Vector3 point)
        {
            var position = ClosestPosition(point);
            DivideByFixedLengthFromPosition(length, position);
        }

        /// <summary>
        /// Divide a grid by constant length subdivisions, starting from a position.
        /// </summary>
        /// <param name="length">The length of subdivisions</param>
        /// <param name="position">The position along the domain at which to begin subdividing.</param>
        public void DivideByFixedLengthFromPosition(double length, double position)
        {
            if (length < Vector3.EPSILON)
            {
                throw new ArgumentException($"Length {length} is smaller than tolerance.");
            }

            if (!Domain.Includes(position))
            {
                // attempt to pick a position that is within the domain
                if (position <= Domain.Min)
                {
                    var diffFromMin = Domain.Min - position;
                    var lengthsAway = Math.Ceiling(diffFromMin / length);
                    position = position + lengthsAway * length;
                    if (position >= Domain.Max)
                    {
                        // no multiple of the length measured from the position intersects the domain. Leave undivided.
                        return;
                    }
                }
                else if (position >= Domain.Max)
                {
                    var diffFromMax = position - Domain.Max;
                    var lengthsAway = Math.Ceiling(diffFromMax / length);
                    position = position - lengthsAway * length;
                    if (position <= Domain.Min)
                    {
                        // no multiple of the length measured from the position intersects the domain. Leave undivided.
                        return;
                    }
                }
            }


            for (double p = position; Domain.Includes(p); p += length)
            {
                SplitAtPosition(p);
            }

            if (!Domain.Includes(position - length)) return;

            for (double p = position - length; Domain.Includes(p); p -= length)
            {
                SplitAtPosition(p);
            }
        }

        /// <summary>
        /// Divide a grid by constant length subdivisions, with a variable division mode to control how leftover
        /// space is handled.
        /// </summary>
        /// <param name="length">The division length</param>
        /// <param name="divisionMode">How to handle leftover / partial remainder panels </param>
        /// <param name="sacrificialPanels">How many full length panels to sacrifice to make remainder panels longer.</param>
        public void DivideByFixedLength(double length, FixedDivisionMode divisionMode = FixedDivisionMode.RemainderAtEnd, int sacrificialPanels = 0)
        {
            if (length <= Vector3.EPSILON)
            {
                throw new ArgumentException($"Unable to divide by length {length}: smaller than tolerance.");
            }
            var lengthToFill = Domain.Length;
            var maxPanelCount = (int)Math.Floor(lengthToFill / length) - sacrificialPanels;
            if (maxPanelCount < 1) return;


            var remainderSize = lengthToFill - maxPanelCount * length;
            if (remainderSize < 0.01)
            {
                DivideByCount(maxPanelCount);
                return;
            }
            switch (divisionMode)
            {
                case FixedDivisionMode.RemainderAtBothEnds:
                    for (double i = remainderSize / 2.0; i < lengthToFill; i += length)
                    {
                        SplitAtOffset(i);
                    }
                    break;
                case FixedDivisionMode.RemainderAtStart:
                    for (double i = remainderSize; i < lengthToFill; i += length)
                    {
                        SplitAtOffset(i);
                    }
                    break;
                case FixedDivisionMode.RemainderAtEnd:
                    for (int i = 1; i < maxPanelCount + 1; i++)
                    {
                        SplitAtOffset(i * length);
                    }
                    break;
                case FixedDivisionMode.RemainderNearMiddle:
                    // assumes we must have at least 2 full-size panels
                    int panelsOnLeft = maxPanelCount / 2; //integer division, on purpose
                    //make left panels
                    for (int i = 1; i <= panelsOnLeft; i++)
                    {
                        SplitAtOffset(i * length);
                    }
                    //make middle + right panels
                    for (double i = panelsOnLeft * length + remainderSize; i < lengthToFill; i += length)
                    {
                        SplitAtOffset(i);
                    }

                    break;
            }
        }

        /// <summary>
        /// Divide a grid by a pattern of lengths. Type names will be automatically generated, repetition will be governed by PatternMode,
        /// and remainder handling will be governed by DivisionMode.
        /// </summary>
        /// <param name="lengthPattern">A pattern of lengths to apply to the grid</param>
        /// <param name="patternMode">How to apply/repeat the pattern</param>
        /// <param name="divisionMode">How to handle leftover/remainder length</param>
        public void DivideByPattern(IList<double> lengthPattern, PatternMode patternMode = PatternMode.Cycle, FixedDivisionMode divisionMode = FixedDivisionMode.RemainderAtEnd)
        {
            var patternwithNames = new List<(string typeName, double length)>();
            for (int i = 0; i < lengthPattern.Count; i++)
            {
                patternwithNames.Add((StringExtensions.NumberToString(i), lengthPattern[i]));
            }
            DivideByPattern(patternwithNames, patternMode, divisionMode);
        }

        /// <summary>
        /// Divide a grid by a pattern of named lengths. Repetition will be governed by PatternMode,
        /// and remainder handling will be governed by DivisionMode.
        /// </summary>
        /// <param name="lengthPattern">A pattern of lengths to apply to the grid</param>
        /// <param name="patternMode">How to apply/repeat the pattern</param>
        /// <param name="divisionMode">How to handle leftover/remainder length</param>
        public void DivideByPattern(IList<(string typeName, double length)> lengthPattern, PatternMode patternMode = PatternMode.Cycle, FixedDivisionMode divisionMode = FixedDivisionMode.RemainderAtEnd)
        {
            if (lengthPattern.Any(p => p.length <= Vector3.EPSILON))
            {
                throw new ArgumentException("One or more of the pattern segments is too small.");
            }
            //a list of all the segments that fit in the grid
            IList<(string typeName, double length)> patternSegments = new List<(string, double)>();
            switch (patternMode)
            {
                case PatternMode.None:
                    patternSegments = lengthPattern;
                    break;
                case PatternMode.Cycle:
                    Cycle(lengthPattern, patternSegments);
                    break;
                case PatternMode.Flip:
                    if (lengthPattern.Count < 3)
                    {
                        Cycle(lengthPattern, patternSegments);
                        break;
                    }
                    var flippedLengthPattern = new List<(string, double)>(lengthPattern);
                    for (int i = lengthPattern.Count - 2; i > 0; i--)
                    {
                        flippedLengthPattern.Add(lengthPattern[i]);
                    }
                    Cycle(flippedLengthPattern, patternSegments);
                    break;
            }

            var totalPatternLength = patternSegments.Select(s => s.length).Sum();
            if (totalPatternLength > Domain.Length + Vector3.EPSILON)
            {
                throw new ArgumentException("The grid could not be constructed. Pattern length exceeds grid length.");
            }

            var remainderSize = totalPatternLength.ApproximatelyEquals(Domain.Length) ? 0 : Domain.Length - totalPatternLength;

            switch (divisionMode)
            {
                case FixedDivisionMode.RemainderAtBothEnds:
                    DivideWithPatternAndOffset(patternSegments, remainderSize / 2.0);
                    break;
                case FixedDivisionMode.RemainderAtStart:
                    DivideWithPatternAndOffset(patternSegments, remainderSize);
                    break;
                case FixedDivisionMode.RemainderAtEnd:
                    DivideWithPatternAndOffset(patternSegments, 0);
                    break;
                case FixedDivisionMode.RemainderNearMiddle:
                    throw new Exception("Remainder Near Middle is not supported for Pattern-based subdivision.");
            }

        }

        internal Vector3 Evaluate(double t)
        {
            if (Curve != null)
            {
                var tNormalized = t.MapFromDomain(curveDomain);
                if (tNormalized > 1 || tNormalized < 0)
                {
                    throw new Exception("t must be in the curve domain.");
                }
                return Curve.PointAt(Curve.Domain.Min + Curve.Domain.Length * tNormalized);
            }
            else
            {
                return new Vector3(t, 0, 0);
            }
        }

        internal void SetParent(Grid2d grid2d)
        {
            this.parent = grid2d;
        }


        /// <summary>
        /// Divide by a list of named lengths and an offset from start, used by the DivideByPattern function.
        /// </summary>
        /// <param name="patternSegments"></param>
        /// <param name="offset"></param>
        private void DivideWithPatternAndOffset(IList<(string typeName, double length)> patternSegments, double offset)
        {
            double runningPosition = offset;
            if (offset > 0)
            {
                SplitAtOffset(runningPosition);
            }
            for (int i = 0; i < patternSegments.Count; i++)
            {
                runningPosition += patternSegments[i].length;
                SplitAtOffset(runningPosition);
            }
            if (Cells == null && patternSegments.Count == 1)
            {
                Type = patternSegments[0].typeName;
                return;
            }
            for (int i = 0; i < patternSegments.Count; i++)
            {
                var cellOffset = offset > 0 ? 1 : 0;
                Cells[i + cellOffset].Type = patternSegments[i].typeName;
            }

        }


        /// <summary>
        /// Populate a list of pattern segments by repeating a pattern up to the length of the grid domain.
        /// </summary>
        /// <param name="lengthPattern"></param>
        /// <param name="patternSegments"></param>
        private void Cycle(IList<(string typeName, double length)> lengthPattern, IList<(string, double)> patternSegments)
        {
            var runningLength = 0.0;
            int i = 0;
            while (true)
            {
                var segmentToAdd = lengthPattern[i % lengthPattern.Count];
                runningLength += segmentToAdd.length;
                if (runningLength <= Domain.Length)
                {
                    patternSegments.Add(segmentToAdd);

                }
                else
                {
                    break;
                }
                i++;
            }
        }

        internal Vector3 Direction()
        {
            if (Curve != null)
            {
                return (Curve.End - Curve.Start).Unitized();
            }
            else
            {
                return Vector3.XAxis;
            }
        }

        internal Vector3 StartPoint()
        {
            return Curve.PointAt(curveDomain.Min);
        }

        #endregion

        #region Cell Retrieval

        /// <summary>
        /// Retrieve a cell by index
        /// </summary>
        /// <param name="i">The index</param>
        /// <returns>A Grid1d representing the selected cell/segment.</returns>
        [JsonIgnore]
        public Grid1d this[int i]
        {
            get
            {
                if (Cells.Count <= i)
                {
                    return null;
                }
                else
                {
                    return Cells[i];
                }
            }
        }

        /// <summary>
        /// Retrieve the grid cell (as a Grid1d) at a length along the domain.
        /// </summary>
        /// <param name="pos">The position in the grid's domain to find</param>
        /// <returns>The cell at this position, if found, or this grid if it is a single cell.</returns>
        public Grid1d FindCellAtPosition(double pos)
        {
            var index = FindCellIndexAtPosition(pos);
            if (index < 0) return this;
            return Cells[index];
        }

        /// <summary>
        /// Retrieve the index of the grid cell at a length along the domain. If
        /// position is exactly on the edge, it returns the righthand cell index.
        /// </summary>
        /// <param name="position"></param>
        /// <returns>Returns the index of the first cell. </returns>
        private int FindCellIndexAtPosition(double position)
        {
            if (!Domain.Includes(position))
            {
                throw new Exception("Position was outside the domain of the grid");
            }

            if (IsSingleCell)
            {
                return -1;
            }

            else
            {
                var domainsSequence = DomainsToSequence();
                for (int i = 0; i < domainsSequence.Count - 1; i++)
                {
                    if (position <= domainsSequence[i + 1])
                    {
                        return i;
                    }
                }
                return Cells.Count - 1; // default to last cell
            }

        }


        private List<double> DomainsToSequence(bool recursive = false)
        {
            if (IsSingleCell)
            {
                return new List<double> { Domain.Min, Domain.Max };
            }
            var cells = recursive ? GetCells() : Cells;
            return cells.Select(c => c.Domain.Min).Union(new[] { Cells.Last().Domain.Max }).ToList();
        }


        /// <summary>
        /// Get the points at the ends and in-between all cells.
        /// </summary>
        /// <param name="recursive">If true, separators will be retrieved from child cells as well.</param>
        /// <returns>A list of Vector3d points representing the boundaries between cells.</returns>
        public List<Vector3> GetCellSeparators(bool recursive = false)
        {
            var values = DomainsToSequence(recursive);
            var t = values.Select(v => v.MapFromDomain(curveDomain));
            var pts = t.Select(t0 => Curve.TransformAt(Curve.Domain.Min + t0 * Curve.Domain.Length).Origin).ToList();
            return pts;
        }

        /// <summary>
        /// Retrieve all grid segment cells recursively.
        /// For just top-level cells, get the Cells property.
        /// </summary>
        /// <returns>A list of all the bottom-level cells / child cells of this grid.</returns>
        public List<Grid1d> GetCells()
        {
            if (IsSingleCell)
            {
                return new List<Grid1d> { this };
            }
            List<Grid1d> resultCells = new List<Grid1d>();
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

        /// <summary>
        /// Retrieve geometric representation of a cell (currently just a line)
        /// </summary>
        /// <returns>A curve representing the extents of this grid / cell.</returns>
        public BoundedCurve GetCellGeometry()
        {
            if (Curve == null)
            {
                //I don't think this should ever happen
                return new Line(new Vector3(Domain.Min, 0, 0), new Vector3(Domain.Max, 0, 0));
            }

            //TODO: support subcurve output / distance-based rather than parameter-based sampling.

            var t1 = Domain.Min.MapFromDomain(curveDomain);
            var t2 = Domain.Max.MapFromDomain(curveDomain);

            var x1 = Curve.TransformAt(Curve.Domain.Min + curve.Domain.Length * t1);
            var x2 = Curve.TransformAt(Curve.Domain.Min + curve.Domain.Length * t2);

            return new Line(x1.Origin, x2.Origin);

        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Test if a given position lies nearly on the edge of a cell
        /// </summary>
        /// <param name="pos"></param>
        /// <returns></returns>
        private bool PositionIsAtCellEdge(double pos)
        {
            return Domain.IsCloseToBoundary(pos);
        }

        internal Grid1d SpawnSubGrid(Domain1d domain)
        {
            return new Grid1d(this, domain, curveDomain);
        }

        #endregion



    }

    #region Enums

    /// <summary>
    /// Methods for repeating a pattern of lengths or types
    /// </summary>
    public enum PatternMode
    {
        /// <summary>
        /// No Repeat. For a pattern [A, B, C], split A, B, C panels, and treat the remaining length according to FixedDivisionMode settings.
        /// </summary>
        None,
        /// <summary>
        /// For a pattern [A, B, C], split at A, B, C, A, B, C, A...
        /// </summary>
        Cycle,
        /// <summary>
        /// For a pattern [A, B, C], split at A, B, C, B, A, B, C, B, A
        /// </summary>
        Flip,
    }

    /// <summary>
    /// Describe how a target length should be treated
    /// </summary>
    public enum EvenDivisionMode
    {
        /// <summary>
        /// Closest match for a target length, can be greater or smaller in practice.
        /// </summary>
        Nearest,
        /// <summary>
        /// Round up the count — Only divide into segments shorter than the target length
        /// </summary>
        RoundUp,
        /// <summary>
        /// Round down the count — Only divide into segments longer than the target length
        /// </summary>
        RoundDown
    }

    /// <summary>
    /// Different ways to handle the "remainder" when dividing an arbitrary length by a fixed size
    /// </summary>
    public enum FixedDivisionMode
    {
        /// <summary>
        /// Take the remainder and split it across both ends of the grid
        /// </summary>
        RemainderAtBothEnds,
        /// <summary>
        /// Locate the remainder at the start of the grid
        /// </summary>
        RemainderAtStart,
        /// <summary>
        /// Locate the remainder at the end of the grid
        /// </summary>
        RemainderAtEnd,
        /// <summary>
        /// Locate the remainder at or near the middle of the grid.
        /// </summary>
        RemainderNearMiddle
    }

    #endregion

}
