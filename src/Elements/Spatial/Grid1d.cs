using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using Elements.Geometry;

namespace Elements.Spatial
{
    /// <summary>
    /// Represents a "1-dimensional grid", akin to a number line that can be subdivided.
    /// </summary>
    /// <example>
    /// [!code-csharp[Main](../../test/Elements.Tests/Examples/Grid1dExample.cs?name=example)]
    /// </example>
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
        public List<Grid1d> Cells { get; private set; }

        /// <summary>
        /// Numerical domain of this Grid
        /// </summary>
        public Domain1d Domain { get; }


        /// <summary>
        /// Returns true if this 1D Grid has no subdivisions / sub-grids. 
        /// </summary>
        public bool IsSingleCell => Cells == null || Cells.Count == 0;

        #endregion

        #region Private fields

        // The curve this was generated from, often a line.
        // subdivided cells maintain the complete original curve,
        // rather than a subcurve. 
        private readonly Curve curve;

        // we have to maintain an internal curve domain because subsequent subdivisions of a grid
        // based on a curve retain the entire curve; this domain allows us to map from the subdivided
        // domain back to the original curve.
        private Domain1d curveDomain;

        #endregion

        #region Constructors

        /// <summary>
        /// Default constructor with optional length parameter
        /// </summary>
        /// <param name="length">Length of the grid domain</param>
        public Grid1d(double length = 1.0) : this(new Domain1d(0, length))
        {

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
        public Grid1d(Curve curve)
        {
            this.curve = curve;
            Domain = new Domain1d(0, curve.Length());
            curveDomain = Domain;
        }

        /// <summary>
        /// This constructor is only for internal use by subdivision / split methods. 
        /// </summary>
        /// <param name="curve">The entire curve of the parent grid</param>
        /// <param name="domain">The domain of the new subdivided segment</param>
        /// <param name="curveDomain">The entire domain of the parent grid's curve</param>
        private Grid1d(Curve curve, Domain1d domain, Domain1d curveDomain)
        {
            this.curve = curve;
            this.curveDomain = curveDomain;
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
                    new Grid1d(curve, newDomains[0], curveDomain),
                    new Grid1d(curve, newDomains[1], curveDomain)
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
                        new Grid1d(curve, newDomains[0], curveDomain),
                        new Grid1d(curve, newDomains[1], curveDomain)
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
            }
            OnTopLevelGridChange();

        }

        /// <summary>
        /// Split a cell at a relative position measured from its domain start or end. 
        /// </summary>
        /// <param name="position">The relative position at which to split.</param>
        /// <param name="fromEnd">If true, measure the position from the end rather than the start</param>
        public void SplitAtOffset(double position, bool fromEnd = false)
        {
            position = fromEnd ? Domain.Max - position : Domain.Min + position;
            if (PositionIsAtCellEdge(position))
            {
                return; // this should be swallowed silently rather than left for Domain.Includes to handle.  
            }
            if (!Domain.Includes(position))
            {
                throw new Exception("Offset position was beyond the grid's domain.");
            }
            SplitAtPosition(position);
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
            Cells = new List<Grid1d>(newDomains.Select(d => new Grid1d(curve, d, curveDomain)));
            OnTopLevelGridChange();
        }

        /// <summary>
        /// Divide a grid by an approximate length. The length will be adjusted to generate whole-number
        /// subdivisions, governed by an optional DivisionMode.
        /// </summary>
        /// <param name="targetLength">The approximate length by which to divide the grid.</param>
        /// <param name="divisionMode">Whether to permit any size cell, or only larger or smaller cells by rounding up or down.</param>
        public void DivideByApproximateLength(double targetLength, EvenDivisionMode divisionMode = EvenDivisionMode.Nearest)
        {
            if(targetLength <= Vector3.EPSILON)
            {
                throw new ArgumentException($"Unable to divide. Target Length {targetLength} is too small.");
            }
            var numDivisions = Domain.Length / targetLength;
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
        /// Divide a grid by constant length subdivisions, starting from a position. 
        /// </summary>
        /// <param name="length">The length of subdivisions</param>
        /// <param name="position">The position along the domain at which to begin subdividing.</param>
        public void DivideByFixedLengthFromPosition(double length, double position)
        {
            if (!Domain.Includes(position))
            {
                throw new ArgumentException($"Position {position} is outside of the grid extents: {Domain}");
            }
            if (length < Vector3.EPSILON)
            {
                throw new ArgumentException($"Length {length} is smaller than tolerance.");
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
            if(lengthPattern.Any(p => p.length <= Vector3.EPSILON))
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
            if (totalPatternLength > Domain.Length)
            {
                throw new ArgumentException("The grid could not be constructed. Pattern length exceeds grid length.");
            }

            var remainderSize = Domain.Length - totalPatternLength;

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
            for (int i = 0; i < patternSegments.Count; i++)
            {
                var cellOffset = offset > 0 ? 1 : 0;
                Cells[i + cellOffset].Type = patternSegments[i].typeName;
            }
            // This is necessary because otherwise name changes don't propogate back to a parent 2d grid.
            // TODO: find a better system than this to manage 1d/2d synchronization — this one involves
            // a lot of unnecessary regeneration. 
            OnTopLevelGridChange();

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
                if (runningLength < Domain.Length)
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

        #endregion

        #region Cell Retrieval

        /// <summary>
        /// Retrieve a cell by index
        /// </summary>
        /// <param name="i">The index</param>
        /// <returns>A Grid1d representing the selected cell/segment.</returns>
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


        private List<double> DomainsToSequence()
        {
            //assumes that cells are always properly ordered...
            return Cells.Select(c => c.Domain.Min).Union(new[] { Cells.Last().Domain.Max }).ToList();
        }

        /// <summary>
        /// Retrieve all grid segment cells recursively.
        /// For just top-level cells, get the Cells property.
        /// </summary>
        /// <returns>A list of all the bottom-level cells / child cells of this grid.</returns>
        public List<Grid1d> GetCells()
        {
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
        public Curve GetCellGeometry()
        {
            if (curve == null)
            {
                //I don't think this should ever happen
                return new Line(new Vector3(Domain.Min, 0, 0), new Vector3(Domain.Max, 0, 0));
            }

            //TODO: support subcurve output / distance-based rather than parameter-based sampling.

            var t1 = Domain.Min.MapFromDomain(curveDomain);
            var t2 = Domain.Max.MapFromDomain(curveDomain);

            var x1 = curve.TransformAt(t1);
            var x2 = curve.TransformAt(t2);

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

        #endregion

        #region Events
        /// <summary>
        /// Handler for a grid event
        /// </summary>
        /// <param name="sender">The Grid1d that spawned this event</param>
        /// <param name="e">Event args</param>
        public delegate void Grid1dEventHandler(Grid1d sender, EventArgs e);


        /// <summary>
        /// Fired when the cells of this grid change
        /// </summary>
        public event Grid1dEventHandler TopLevelGridChange;

        /// <summary>
        /// Fired when the cells of this grid change
        /// </summary>
        protected virtual void OnTopLevelGridChange()
        {
            Grid1dEventHandler handler = TopLevelGridChange;
            handler?.Invoke(this, new EventArgs());
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
