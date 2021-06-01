using System;
using System.IO;
using Elements.Geometry;
using Xunit;

namespace Elements.DXF.Tests
{
    public class DxfTests
    {
        [Fact]
        public void CreateDxfFromFloor()
        {
            var profile = Polygon.Rectangle(5, 10);
            var floor = new Floor(profile, 2);
            var model = new Model();
            model.AddElement(floor);

            var renderer = new DXF.ModelToDxf();
            var stream = renderer.Render(model);
            stream.Position = 0;
            var filePath = "../../../results/floorDXF.dxf";
            using (var reader = new StreamReader(stream))
            {
                File.WriteAllText(filePath, reader.ReadToEnd());
            }
            Assert.True(File.Exists(filePath));
            Assert.True(File.ReadAllBytes(filePath).Length > 0);
        }
    }
}
