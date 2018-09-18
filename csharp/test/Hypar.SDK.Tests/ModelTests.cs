using Newtonsoft.Json;
using System;
using System.IO;
using System.Linq;
using Xunit;
using Hypar.Elements;
using Hypar.Geometry;

namespace Hypar.Tests
{
    public class ModelTests
    {
        [Fact]
        public void Construct()
        {
            var model = new Model();
            Assert.NotNull(model);
        }

        [Fact]
        public void TestModel_SaveToGltf_Success()
        {
            var model = QuadPanelModel();
            model.SaveGltf("saveToGltf.gltf");
            Assert.True(File.Exists("saveToGltf.gltf"));
        }

        [Fact]
        public void TestModel_SaveToGlb_Success()
        {
            var model = QuadPanelModel();
            model.SaveGlb("saveToGlb.glb");
            Assert.True(File.Exists("saveToGlb.glb"));
        }

        [Fact]
        public void JsonSerialization()
        {
            var a = new Vector3(0,0,0);
            var b = new Vector3(1,0,0);
            var c = new Vector3(1,0,1);
            var d = new Vector3(0,0,1);
            var panel = new Panel(new []{a,b,c,d,}, BuiltInMaterials.Glass);
            panel.AddParameter("param1", new NumericParameter(42.0, NumericParameterType.Area));
            panel.AddParameter("param2", new NumericParameter(42.0, NumericParameterType.Force));

            var floor = new Floor(Profiles.Rectangular(), 5.0, 0.2, new []{Profiles.Rectangular(new Vector3(2,2), 1.0, 1.0)});
            var mass = new Mass(Profiles.Rectangular(), 5.0, 1.0);
            var line = new Line(Vector3.Origin, new Vector3(5,5,5));
            var beam = new Beam(line, new[]{Profiles.WideFlangeProfile()}, BuiltInMaterials.Steel);
            var column = new Column(new Vector3(5,5,5), 5.0, new[]{Profiles.WideFlangeProfile()}, BuiltInMaterials.Steel);
            var space = new Space(Profiles.Rectangular(), new []{Profiles.Rectangular(new Vector3(2,2), 1.0, 1.0)}, 5.0, 5.0);
            var model = new Model();
            model.AddElements(new Element[]{panel, floor, mass, beam, column, space});
            var json = model.ToJson();
            // Console.WriteLine(json);

            var newModel = Model.FromJson(json);
            var elements = newModel.Values;
            var newPanel = elements.OfType<Panel>().FirstOrDefault();
            var newFloor = elements.OfType<Floor>().FirstOrDefault();
            var newMass = elements.OfType<Mass>().FirstOrDefault();
            var newBeam = elements.OfType<Beam>().FirstOrDefault();
            var newSpace = elements.OfType<Space>().FirstOrDefault();

            Assert.Equal(panel.Perimeter.Count, newPanel.Perimeter.Count);
            Assert.Equal(panel.Id, newPanel.Id);
            Assert.Equal(floor.Id, newFloor.Id);
            Assert.Equal(floor.Material, newFloor.Material);
            Assert.Equal(floor.Perimeter.Vertices.Count, newFloor.Perimeter.Vertices.Count);
            Assert.Equal(floor.Thickness, newFloor.Thickness);
            Assert.Equal(floor.Elevation, newFloor.Elevation);
            Assert.Equal(mass.Perimeter.Vertices.Count, newMass.Perimeter.Vertices.Count);
            Assert.Equal(mass.Elevation, newMass.Elevation);
            Assert.Equal(mass.Height, newMass.Height);
            Assert.Equal(space.Elevation, newSpace.Elevation);
            Assert.Equal(space.Height, newSpace.Height);
            Assert.Equal(space.Perimeter, newSpace.Perimeter);
        }

        private Model QuadPanelModel()
        {
            var model = new Model();
            var a = new Vector3(0,0,0);
            var b = new Vector3(1,0,0);
            var c = new Vector3(1,0,1);
            var d = new Vector3(0,0,1);
            var panel = new Panel(new[]{a,b,c,d}, BuiltInMaterials.Glass);
            model.AddElement(panel);
            return model;
        }
    }
}
