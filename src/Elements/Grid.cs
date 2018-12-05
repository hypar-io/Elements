using Elements.Geometry;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Elements
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
        /// <param name="bottom">The bottom edge of the Grid.</param>
        /// <param name="top">The top edge of the Grid.</param>
        /// <param name="uDistance">The distance along the u parameter at which points will be created.</param>
        /// <param name="vDistance">The distance along the v parameter at which points will be created.</param>
        public Grid (Line bottom, Line top, double uDistance, double vDistance)
        {
            this._bottom = bottom;
            this._top = top;
            this._uDiv = CalculateEqualDivisions((int)Math.Ceiling(bottom.Length()/uDistance));
            this._vDiv = CalculateEqualDivisions((int)Math.Ceiling(top.Length()/vDistance));
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

        /// <summary>
        /// Construct a Grid.
        /// </summary>
        /// <param name="face">A face whose edges will be used to define the grid.</param>
        /// <param name="uDistance">The distance along the u parameter at which points will be created.</param>
        /// <param name="vDistance">The distance along the v parameter at which points will be created.</param>
        public Grid(Face face, double uDistance, double vDistance)
        {
            var f = face.Edges;
            this._bottom = f[0];
            this._top = f[2].Reversed();
            this._uDiv = CalculateEqualDivisions((int)Math.Ceiling(this._bottom.Length()/uDistance));
            this._vDiv = CalculateEqualDivisions((int)Math.Ceiling(f[1].Length()/vDistance));
            this._pts = CalculateGridPoints();
        }
    }
}