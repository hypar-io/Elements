using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using Elements.Geometry;
using Xunit;

namespace Elements.Serialization.DXF.Tests
{
    public class DxfTests
    {
        [Fact]
        public void CreateDxf()
        {
            var profile = Polygon.Rectangle(5, 10);
            var floor = new Floor(profile, 2);
            var model = new Model();
            model.AddElement(floor);
            var symbol1 = new Symbol(new GeometryReference(null, new List<object>() { Polygon.Rectangle(1, 3) }), SymbolCameraPosition.Top);
            var contentElement1 = new ContentElement(null, new BBox3((0, 0), (1, 1)), 1, Vector3.XAxis, new List<Symbol> { symbol1 }, isElementDefinition: true);
            var symbol2 = new Symbol(new GeometryReference("https://hypar-content-catalogs.s3.us-west-2.amazonaws.com/test-2d-content/geo_ex.json", null), SymbolCameraPosition.Top);
            var contentElement2 = new ContentElement(null, new BBox3((0, 0), (1, 1)), 1, Vector3.XAxis, new List<Symbol> { symbol2 }, isElementDefinition: true);

            for (int i = 0; i < 10; i++)
            {
                var xform = new Transform();
                xform.Rotate(Vector3.ZAxis, (i / 10.0) * 45.0);
                xform.Move((i * 2, 0, 0));
                model.AddElement(contentElement1.CreateInstance(new Transform(xform), null));
                xform.Move((0, 3, 0));
                model.AddElement(contentElement2.CreateInstance(xform, null));
            }
            var mappingConfig = new MappingConfiguration();
            mappingConfig.Layers.AddRange(new[] {new MappingConfiguration.Layer
            {
                LayerName = "A-FLOOR",
                LayerColor = Colors.Red,
                Lineweight = 50,
                Types = new List<string> { "Elements.Floor" }
            },
            new MappingConfiguration.Layer
            {
                LayerName = "I-FURN",
                LayerColor = Colors.Orange,
                Types = new List<string> { "Elements.ContentElement" }
            }});

            var renderer = new DXF.ModelToDxf();
            renderer.SetMappingConfiguration(mappingConfig);
            var stream = renderer.Render(model);
            stream.Position = 0;
            var filePath = "../../../results/TestOutput.dxf";
            Directory.CreateDirectory(Path.GetDirectoryName(filePath));
            using (var reader = new StreamReader(stream))
            {
                File.WriteAllText(filePath, reader.ReadToEnd());
            }
            Assert.True(File.Exists(filePath));
            Assert.True(File.ReadAllBytes(filePath).Length > 0);
        }

        [Fact]
        public void DxfFromModel()
        {
            var jsonPath = "../../../TestModels/TestModel3.json";
            var json = File.ReadAllText(jsonPath);
            var model = Model.FromJson(json);
            var renderer = new DXF.ModelToDxf();

            var configJson = File.ReadAllText("../../../TestModels/cad-standard.json");
            var config = JsonSerializer.Deserialize<MappingConfiguration>(configJson);
            renderer.SetMappingConfiguration(config);
            var stream = renderer.Render(model);
            stream.Position = 0;
            var filePath = "../../../results/FromJson.dxf";
            Directory.CreateDirectory(Path.GetDirectoryName(filePath));
            using (var reader = new StreamReader(stream))
            {
                File.WriteAllText(filePath, reader.ReadToEnd());
            }
        }
    }
}
