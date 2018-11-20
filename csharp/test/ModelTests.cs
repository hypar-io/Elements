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
        public void TestModel_SaveToBase64_Success()
        {
            var model = QuadPanelModel();
            var base64 = model.ToBase64String();
            var bytes = Convert.FromBase64String(base64);
            File.WriteAllBytes("saveFromBase64String.glb", bytes);
        }

        [Fact]
        public void JsonSerialization()
        {
            var a = new Vector3(0,0,0);
            var b = new Vector3(1,0,0);
            var c = new Vector3(1,0,1);
            var d = new Vector3(0,0,1);
            var panel = new Panel(new []{a,b,c,d,}, BuiltInMaterials.Glass);
            panel.AddParameter("param1", new Parameter(42.0, ParameterType.Area));
            panel.AddParameter("param2", new Parameter(42.0, ParameterType.Force));

            var profile = new Profile(Polygon.Rectangle(), Polygon.Rectangle(new Vector3(2,2), 1.0, 1.0));
            var floorType = new FloorType("test", 0.2);
            var floor = new Floor(profile, floorType, 5.0);
            var mass = new Mass(Polygon.Rectangle(), 5.0, 1.0);
            var line = new Line(Vector3.Origin, new Vector3(5,5,5));
            var beam = new Beam(line, new WideFlangeProfile("test_beam"), BuiltInMaterials.Steel);
            var column = new Column(new Vector3(5,5,5), 5.0, new WideFlangeProfile("test_column"), BuiltInMaterials.Steel);
            var spaceProfile = new Profile(Polygon.Rectangle(), Polygon.Rectangle(new Vector3(2,2), 1.0, 1.0));
            var space = new Space(spaceProfile, 5.0, 5.0);
            var model = new Model();

            var wallLine = new Line(Vector3.Origin, new Vector3(10,0,0));
            var wallType = new WallType("test", 0.1, "A test wall type.");
            var wall = new Wall(wallLine, wallType, 4.0);

            model.AddElements(new Element[]{panel, floor, mass, beam, column, space, wall});
            var json = model.ToJson();
            
            var json2 = JsonConvert.SerializeObject(model, Formatting.Indented);
            Console.WriteLine($"Json serialization comparison (char length): serializer={json.Length}, unmodified={json2.Length}, {((double)json.Length/(double)json2.Length)*100.0}%");

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
            Assert.Equal(floor.ElementType.Thickness, newFloor.ElementType.Thickness);
            Assert.Equal(floor.Elevation, newFloor.Elevation);
            Assert.Equal(mass.Profile.Perimeter.Vertices.Count, newMass.Profile.Perimeter.Vertices.Count);
            Assert.Equal(mass.Elevation, newMass.Elevation);
            Assert.Equal(mass.Height, newMass.Height);
            Assert.Equal(space.Elevation, newSpace.Elevation);
            Assert.Equal(space.Height, newSpace.Height);
            Assert.Equal(space.Profile.Perimeter, newSpace.Profile.Perimeter);
            Assert.Equal(wall.Height, newWall.Height);
            Assert.Equal(wall.CenterLine, newWall.CenterLine);
            Assert.Equal(wall.ElementType, model.ElementTypes[newWall.ElementType.Id]);
        }

        // [Fact]
        // public void DeserializeFoo()
        // {
        //     var file = File.ReadAllText("/Users/ikeough/Downloads/ec600c0b-65e0-43ea-a5ac-6491c951ebe6_elements.json");
        //     var model = Model.FromJson(file);
        // }

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
