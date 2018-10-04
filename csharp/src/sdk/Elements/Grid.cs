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

        private Vector3[][] _pts;

        /// <summary>
        /// The number of columns in the grid.
        /// </summary>
        /// <returns></returns>
        public int ColumnCount
        {
            get{return _uDiv.Length - 1;}
        }

        /// <summary>
        /// The number of rows in the grid.
        /// </summary>
        /// <returns></returns>
        public int RowCount
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
        /// <exception cref="System.ArgumentOutOfRangeException">Thrown when the specified column index is greater than the number of available columns.</exception>
        public Vector3[][] CellsInColumn(int n)
        {
            if(n < 0 || n >= this._uDiv.Length - 1)
            {
                throw new ArgumentOutOfRangeException("The column index must be greater than or equal to zero and less than the number of columns.");
            }

            var c1 = this._pts[n];
            var c2 = this._pts[n+1];

            var results = new Vector3[c1.Length][];

            for(var i=0; i<c1.Length-1; i++)
            {
                var a = c1[i];
                var b = c1[i+1];
                var c = c2[i+1];
                var d = c2[i];
                results[i] = new[]{a,b,c,d};
            }

            return results;
        }

        /// <summary>
        /// Get all cells in a row.
        /// </summary>
        /// <param name="n"></param>
        /// <exception cref="System.ArgumentException">Thrown when the row index is greater than number of rows.</exception>
        public Vector3[][] CellsInRow(int n)
        {
            if(n < 0 || n >= this._vDiv.Length - 1)
            {
                throw new ArgumentOutOfRangeException("The column index must be greater than or equal to zero and less than the number of columns.");
            }

            var results = new Vector3[this._pts.Length][];

            for(var i=0; i<this._pts.Length-1; i++)
            {
                var a = this._pts[i][n];
                var b = this._pts[i][n+1];
                var c = this._pts[i+1][n+1];
                var d = this._pts[i+1][n];
                results[i] = new[]{a, b, c, d};
            }

            return results;
        }

        /// <summary>
        /// Get all cells.
        /// </summary>
        /// <returns></returns>
        public Vector3[,][] Cells()
        {
            var results  = new Vector3[this._pts.GetLength(0)-1,this._pts[0].Length-1][];

            for(var i=0; i<this._pts.GetLength(0)-1; i++)
            {   
                var rowA = this._pts[i];
                var rowB = this._pts[i+1];

                for(var j=0; j<this._pts[i].Length-1; j++)
                {
                    var a = rowA[j];
                    var b = rowA[j+1];
                    var c = rowB[j+1];
                    var d = rowB[j];
                    results[i,j] = new[]{a,b,c,d};
                }
            }

            return results;
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
            this._pts = CalculateGridPoints();
        }

        /// <summary>
        /// Construct a grid.
        /// </summary>
        /// <param name="face">A face whose edges will be used to define the grid.</param>
        /// <param name="uDivisions">The number of grid divisions in the u direction.</param>
        /// <param name="vDivisions">The number of grid divisions in the v direction.</param>
        public Grid(Face face, int uDivisions = 1, int vDivisions = 1)
        {
            var f = face.Edges;
            this._bottom = f[0];
            this._top = f[2].Reversed();
            this._uDiv = CalculateEqualDivisions(uDivisions);
            this._vDiv = CalculateEqualDivisions(vDivisions);
            this._pts = CalculateGridPoints();
        }
    }
}