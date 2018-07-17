using Hypar.Elements;
using Hypar.Geometry;
using System.IO;
using System.Linq;
using Xunit;

namespace Hypar.Tests
{
    public class PanelTests
    {
        [Fact]
        public void Single_WithinPerimeter_Success()
        {
            var p = Profiles.Rectangular();
            var panel = Panel.WithinPerimeter(p);
            Assert.Equal(BuiltInMaterials.Default, panel.Material);
            // Assert.Equal(Vector3.ZAxis(), panel.Normal);
            // Assert.Equal(p, panel.Perimeter);
        }

        [Fact]
        public void Collection_WithinPerimeter_Success()
        {
            var p1 = Profiles.Rectangular();
            var p2 = Profiles.Rectangular(width:10, height:5);
            var panels = new[]{p1,p2}.WithinEachCreate<Panel>(p => {
                return Panel.WithinPerimeter(p);
            });
            Assert.Equal(2, panels.Count());
        }

        [Fact]
        public void Params_WithinPerimeter_Success()
        {
            var p1 = Profiles.Rectangular();
            var p2 = Profiles.Rectangular(width:10, height:5);
            var panels = new[]{p1,p2}.WithinEachCreate<Panel>(p => {
                return Panel.WithinPerimeter(p);
            });
            Assert.Equal(2, panels.Count());
        }
    }
}