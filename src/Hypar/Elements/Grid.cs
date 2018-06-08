using Hypar.Geometry;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Hypar.Elements
{
    public class Grid
    {
        private double[] _uDiv;
        private double[] _vDiv;
        private Polyline _perimeter;

        public int Columns
        {
            get{return _uDiv.Length - 1;}
        }

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

        public IEnumerable<Polyline> CellsInColumn(int n)
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
                yield return new Polyline(new[]{a,b,c,d});
            }
        }

        public IEnumerable<Polyline> CellsInRow(int n)
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
                yield return new Polyline(new[]{a, b, c, d});
            }
        }

        private Vector3[][] CalculateGridPoints()
        {
            var pts = new Vector3[this._uDiv.Length][];
            var lines = this._perimeter.Segments();
            var edge1 = lines.ElementAt(0).Reversed();
            var edge2 = lines.ElementAt(2);

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

        public IEnumerable<Polyline> Cells()
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
                    yield return new Polyline(new[]{a,b,c,d});
                }
            }
        }

        public Grid(Polyline perimeter, int uDivisions = 1, int vDivisions = 1)
        {
            this._perimeter = perimeter;
            this._uDiv = CalculateEqualDivisions(uDivisions);
            this._vDiv = CalculateEqualDivisions(vDivisions);
        }

        /// <summary>
        /// Set the perimeter of the grid.
        /// </summary>
        /// <param name="perimeter"></param>
        /// <returns></returns>
        public Grid WithinPerimeter(Polyline perimeter)
        {
            this._perimeter = perimeter;
            return this;
        }

        /// <summary>
        /// Set the number of divisions of the grid in the u direction.
        /// </summary>
        /// <param name="u"></param>
        /// <returns></returns>
        public Grid WithUDivisions(int u)
        {
            this._uDiv = CalculateEqualDivisions(u);
            return this;
        }

        public Grid WithUDivisions(double[] uDivisions)
        {
            this._uDiv = uDivisions;
            return this;
        }

        /// <summary>
        /// Set the number of divisions of the grid in the v direction.
        /// </summary>
        /// <param name="v"></param>
        /// <returns></returns>
        public Grid WithVDivisions(int v)
        {
            this._vDiv = CalculateEqualDivisions(v);
            return this;
        }

        public Grid WithVDivisions(double[] vDivisions)
        {
            this._vDiv = vDivisions;
            return this;
        }

        /// <summary>
        /// Execute a creation function for each cell in the grid.
        /// </summary>
        /// <param name="creator"></param>
        /// <returns></returns>
        public IEnumerable<Element> InAllCells(Func<Polyline, IEnumerable<Element>> creator)
        {
            var result = new List<Element>();
            foreach(var c in this.Cells())
            {
                result.AddRange(creator(c));
            }
            return result;
        }

        public IEnumerable<Element> InAllCellsAlongColumn(int n, Func<Polyline, IEnumerable<Element>> creator)
        {
            var result = new List<Element>();
            foreach(var c in this.CellsInColumn(n))
            {
                result.AddRange(creator(c));
            }
            return result;
        }

        public IEnumerable<Element> InAllCellsAlongRow(int n, Func<Polyline, IEnumerable<Element>> creator)
        {
            var result = new List<Element>();
            foreach(var c in this.CellsInRow(n))
            {
                result.AddRange(creator(c));
            }
            return result;
        }

        public IEnumerable<Element> AlongEachRowEdge(Func<Line, Element> creator)
        {
            var result = new List<Element>();
            foreach(var c in this.Cells())
            {
                result.Add(creator(c.Segment(0)));
            }
            return result;
        }

        public IEnumerable<Element> AlongEachColumnEdge(Func<Line, Element> creator)
        {
            var result = new List<Element>();
            foreach(var c in this.Cells())
            {
                result.Add(creator(c.Segment(1)));
            }
            return result;
        }
    }

    public static class GridExtensions
    {
        public static IEnumerable<Grid> WithinPerimeters(this IEnumerable<Grid> grids, IEnumerable<Polyline> perimeters)
        {
            var gridArr = grids.ToArray();
            var perimetersArr = perimeters.ToArray();

            for(var i=0; i<grids.Count(); i++)
            {
                gridArr[i].WithinPerimeter(perimetersArr[i]);
            }
            return grids;
        }

        public static IEnumerable<Grid> WithUDivisions(this IEnumerable<Grid> grids, int u)
        {
            foreach(var g in grids)
            {
                g.WithUDivisions(u);
            }
            return grids;
        }

        public static IEnumerable<Grid> WithUDivisions(this IEnumerable<Grid> grids, double[] u)
        {
            foreach(var g in grids)
            {
                g.WithUDivisions(u);
            }
            return grids;
        }

        public static IEnumerable<Grid> WithVDivisions(this IEnumerable<Grid> grids, int v)
        {
            foreach(var g in grids)
            {
                g.WithVDivisions(v);
            }
            return grids;
        }

        public static IEnumerable<Grid> WithVDivisions(this IEnumerable<Grid> grids, double[] v)
        {
            foreach(var g in grids)
            {
                g.WithVDivisions(v);
            }
            return grids;
        }
    }
}