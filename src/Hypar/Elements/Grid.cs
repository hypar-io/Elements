using Hypar.Geometry;
using System.Collections.Generic;

namespace Hypar.Elements
{
    public class Grid
    {
        private List<List<Cell>> m_cells = new List<List<Cell>>();

        public IEnumerable<IEnumerable<Cell>> Cells
        {
            get{return m_cells;}
        }

        public Grid(Line edge1, Line edge2, int uDiv, int vDiv)
        {
            var pts = new List<List<Vector3>>();

            var uStep = 1.0/(double)uDiv;
            var vStep = 1.0/(double)vDiv;

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
            
            for(var i=0; i<pts.Count-1; i++)
            {
                var cells = new List<Cell>();
                var rowA = pts[i];
                var rowB = pts[i+1];
                for(var j=0; j<rowA.Count-1; j++)
                {
                    var a = rowA[j];
                    var b = rowA[j+1];
                    var c = rowB[j+1];
                    var d = rowB[j];
                    cells.Add(new Cell(a,b,c,d));
                }
                this.m_cells.Add(cells);
            }

        }
    }

    public class Cell
    {
        public Polygon3 Perimeter{get;}

        public Cell (Vector3 a, Vector3 b, Vector3 c, Vector3 d)
        {
            this.Perimeter = new Polygon3(new []{a,b,c,d});
        }
    }
}