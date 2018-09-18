using Hypar.Geometry;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Hypar.Elements
{
    /// <summary>
    /// A grid comprised of rows and columns with each cell represented by a polyline.
    /// </summary>
    public class Grid
    {
        private double[] _uDiv;
        private double[] _vDiv;
        private Line _bottom;
        private Line _top;

        /// <summary>
        /// The number of columns in the grid.
        /// </summary>
        /// <returns></returns>
        public int Columns
        {
            get{return _uDiv.Length - 1;}
        }

        /// <summary>
        /// The number of rows in the grid.
        /// </summary>
        /// <returns></returns>
        public int Rows
        {
            get{return _vDiv.Length - 1;}
        }
        
        private double[] CalculateEqualDivisions(int n)
        {
            var uStep = 1.0/(double)n;
            var result = new double[n+1];
            for(var i=0; i<=n; i++)
            {
                result[i] = uStep*i;
            }
            return result;
        }

        private Vector3[][] CalculateGridPoints()
        {
            var pts = new Vector3[this._uDiv.Length][];
            var edge1 = this._bottom;
            var edge2 = this._top;

            for(var i=0; i<_uDiv.Length; i++)
            {
                var u = _uDiv[i];

                var start = edge1.PointAt(u);
                var end = edge2.PointAt(u);
                var l = new Line(start, end);
                
                var col = new Vector3[_vDiv.Length];
                for(var j=0; j<_vDiv.Length; j++ )
                {
                    var v = _vDiv[j];
                    col[j] = l.PointAt(v);
                }
                pts[i]=col;
            }
            return pts;
        }

        /// <summary>
        /// Get all cells in a column.
        /// </summary>
        /// <param name="n"></param>
        /// <returns></returns>
        public IEnumerable<IEnumerable<Vector3>> CellsInColumn(int n)
        {
            if(n < 0 || n >= this._uDiv.Length - 1)
            {
                throw new ArgumentOutOfRangeException("The column index must be greater than or equal to zero and less than the number of columns.");
            }

            var pts = CalculateGridPoints();

            var c1 = pts[n];
            var c2 = pts[n+1];
            for(var i=0; i<c1.Length-1; i++)
            {
                var a = c1[i];
                var b = c1[i+1];
                var c = c2[i+1];
                var d = c2[i];
                yield return new List<Vector3>(new[]{a,b,c,d});
            }
        }

        /// <summary>
        /// Get all cells in a row.
        /// </summary>
        /// <param name="n"></param>
        /// <returns></returns>
        public IEnumerable<IEnumerable<Vector3>> CellsInRow(int n)
        {
            if(n < 0 || n >= this._vDiv.Length - 1)
            {
                throw new ArgumentOutOfRangeException("The column index must be greater than or equal to zero and less than the number of columns.");
            }

            var pts = CalculateGridPoints();
            for(var i=0; i<pts.Length-1; i++)
            {
                var a = pts[i][n];
                var b = pts[i][n+1];
                var c = pts[i+1][n+1];
                var d = pts[i+1][n];
                yield return new List<Vector3>(new[]{a, b, c, d});
            }
        }

        /// <summary>
        /// Get all cells.
        /// </summary>
        /// <returns></returns>
        public IEnumerable<Vector3[]> Cells()
        {
            var pts = CalculateGridPoints();

            for(var i=0; i<pts.GetLength(0)-1; i++)
            {   
                var rowA = pts[i];
                var rowB = pts[i+1];

                for(var j=0; j<pts[i].Length-1; j++)
                {
                    var a = rowA[j];
                    var b = rowA[j+1];
                    var c = rowB[j+1];
                    var d = rowB[j];
                    yield return new []{a,b,c,d};
                }
            }
        }

        /// <summary>
        /// Get the edges of all rows.
        /// </summary>
        /// <returns></returns>
        public IEnumerable<Line> RowEdges()
        {
            foreach(var c in this.Cells())
            {
                yield return new Line(c.ElementAt(0), c.ElementAt(1));
            }
        }

        /// <summary>
        /// Get the edges of all columns.
        /// </summary>
        /// <returns></returns>
        public IEnumerable<Line> ColumnEdges()
        {
            foreach(var c in this.Cells())
            {
                yield return new Line(c.ElementAt(1), c.ElementAt(2));
            }
        }

        /// <summary>
        /// Construct a grid.
        /// </summary>
        /// <param name="bottom"></param>
        /// <param name="top"></param>
        /// <param name="uDivisions">The number of grid divisions in the u direction.</param>
        /// <param name="vDivisions">The number of grid divisions in the v direction.</param>
        public Grid(Line bottom, Line top, int uDivisions = 1, int vDivisions = 1)
        {
            this._bottom = bottom;
            this._top = top;
            this._uDiv = CalculateEqualDivisions(uDivisions);
            this._vDiv = CalculateEqualDivisions(vDivisions);
        }

        /// <summary>
        /// Construct a grid.
        /// </summary>
        /// <param name="face">A face whose edges will be used to define the grid.</param>
        /// <param name="uDivisions">The number of grid divisions in the u direction.</param>
        /// <param name="vDivisions">The number of grid divisions in the v direction.</param>
        public Grid(Face face, int uDivisions = 1, int vDivisions = 1)
        {
            var f = face.ToArray();
            this._bottom = f[0];
            this._top = f[2].Reversed();
            this._uDiv = CalculateEqualDivisions(uDivisions);
            this._vDiv = CalculateEqualDivisions(vDivisions);
        }
    }
}