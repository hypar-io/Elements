using Hypar.Geometry;
using Hypar.Elements;
using System;
using System.IO;
using System.Linq;
using Xunit;

namespace Hypar.Tests
{
    public class GridTests
    {
        [Fact]
        public void Example()
        {
            var a = new Vector3();
            var b = new Vector3(10,0,0);
            var c = new Vector3(0,0,10);
            var d = new Vector3(10,0,10);
            var grid = new Grid(new Line(a,b), new Line(c,d), 5, 5);

            var model = new Model();
            foreach(var cell in grid.Cells())
            {
                var panel = new Panel(cell.Shrink(0.2));
                model.AddElement(panel);
            }
            model.SaveGlb("grid.glb");
        }

        [Fact]
        public void ValidValues_Construct_Success()
        {
            var bottom = new Line(new Vector3(0,0,0), new Vector3(20,0,0));
            var top = new Line(new Vector3(20,10,30), new Vector3(0,0,30));
            var grid = new Grid(bottom, top, 5, 5);
                                        
            var profile = new WideFlangeProfile("test", 0.5, 0.5, 0.1, 0.1, VerticalAlignment.Center);
            Assert.Equal(25, grid.Cells().Length);
        }
    }
}