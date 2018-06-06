using Hypar.Geometry;
using System.Collections.Generic;
using System.Linq;

namespace Hypar.Elements
{
    public class Grid
    {
        private int _uDiv;
        private int _vDiv;
        private Polyline _perimeter;

        public Grid(Polyline perimeter)
        {
            this._perimeter = perimeter;
        }

        public IEnumerable<Polyline> Cells()
        {
            var pts = new List<List<Vector3>>();

            var uStep = 1.0/(double)this._uDiv;
            var vStep = 1.0/(double)this._vDiv;
            var lines = this._perimeter.Explode();
            var edge1 = lines.ElementAt(0).Reversed();
            var edge2 = lines.ElementAt(2);

            for(var u=0.0; u<=1.0; u += uStep)
            {
                var start = edge1.PointAt(u);
                var end = edge2.PointAt(u);
                var l = new Line(start, end);
                
                var col = new List<Vector3>();
                for(var v=0.0; v<=1.0; v+= vStep )
                {
                    col.Add(l.PointAt(v));
                }
                pts.Add(col);
            }
            
            var cells = new List<Polyline>();
            for(var i=0; i<pts.Count-1; i++)
            {
                // var cells = new List<Cell>();
                var rowA = pts[i];
                var rowB = pts[i+1];
                for(var j=0; j<rowA.Count-1; j++)
                {
                    var a = rowA[j];
                    var b = rowA[j+1];
                    var c = rowB[j+1];
                    var d = rowB[j];
                    cells.Add(new Polyline(new[]{a,b,c,d}));
                }
                // this.m_cells.Add(cells);
            }
            return cells;
        }

        public static Grid WithinPerimeter(Polyline perimeter)
        {
            var g = new Grid(perimeter);
            return g;
        }

        public static IEnumerable<Grid> WithinPerimeters(IEnumerable<Polyline> perimeters)
        {
            var grids = new List<Grid>();
            foreach(var p in perimeters)
            {
                grids.Add(Grid.WithinPerimeter(p));
            }
            return grids;
        }

        public Grid WithUDivisions(int u)
        {
            this._uDiv = u;
            return this;
        }

        public Grid WithVDivisions(int v)
        {
            this._vDiv = v;
            return this;
        }
    }

    public static class GridExtensions
    {
        public static IEnumerable<Grid> WithUDivisions(this IEnumerable<Grid> grids, int u)
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
    }
}