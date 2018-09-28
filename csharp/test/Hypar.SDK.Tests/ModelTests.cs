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

            var profile = new Profile(Polygon.Rectangle(), Polygon.Rectangle(new Vector3(2,2), 1.0, 1.0));
            var floor = new Floor(profile, 5.0, 0.2);
            var mass = new Mass(Polygon.Rectangle(), 5.0, 1.0);
            var line = new Line(Vector3.Origin, new Vector3(5,5,5));
            var beam = new Beam(line, new WideFlangeProfile(), BuiltInMaterials.Steel);
            var column = new Column(new Vector3(5,5,5), 5.0, new WideFlangeProfile(), BuiltInMaterials.Steel);
            var spaceProfile = new Profile(Polygon.Rectangle(), Polygon.Rectangle(new Vector3(2,2), 1.0, 1.0));
            var space = new Space(spaceProfile, 5.0, 5.0);
            var model = new Model();

            var wallLine = new Line(Vector3.Origin, new Vector3(10,0,0));
            var wallType = new WallType("test", 0.1, "A test wall type.");
            var wall = new Wall(wallLine, wallType, 4.0);

            model.AddElements(new Element[]{panel, floor, mass, beam, column, space, wall});
            var json = model.ToJson();
            Console.WriteLine(json);

            var newModel = Model.FromJson(json);
            var elements = newModel.Elements;
            var newPanel = elements.Values.OfType<Panel>().FirstOrDefault();
            var newFloor = elements.Values.OfType<Floor>().FirstOrDefault();
            var newMass = elements.Values.OfType<Mass>().FirstOrDefault();
            var newBeam = elements.Values.OfType<Beam>().FirstOrDefault();
            var newSpace = elements.Values.OfType<Space>().FirstOrDefault();
            var newWall = elements.Values.OfType<Wall>().FirstOrDefault();

            Assert.Equal(panel.Perimeter.Count, newPanel.Perimeter.Count);
            Assert.Equal(panel.Id, newPanel.Id);
            Assert.Equal(floor.Id, newFloor.Id);
            Assert.Equal(floor.Material, newFloor.Material);
            Assert.Equal(floor.Profile.Perimeter.Vertices.Count, newFloor.Profile.Perimeter.Vertices.Count);
            Assert.Equal(floor.Thickness, newFloor.Thickness);
            Assert.Equal(floor.Elevation, newFloor.Elevation);
            Assert.Equal(mass.Profile.Perimeter.Vertices.Count, newMass.Profile.Perimeter.Vertices.Count);
            Assert.Equal(mass.Elevation, newMass.Elevation);
            Assert.Equal(mass.Height, newMass.Height);
            Assert.Equal(space.Elevation, newSpace.Elevation);
            Assert.Equal(space.Height, newSpace.Height);
            Assert.Equal(space.Profile.Perimeter, newSpace.Profile.Perimeter);
            Assert.Equal(wall.Height, newWall.Height);
            Assert.Equal(wall.CenterLine, newWall.CenterLine);
            Assert.Equal(wall.ElementType, model.GetElementTypeById("test"));
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
