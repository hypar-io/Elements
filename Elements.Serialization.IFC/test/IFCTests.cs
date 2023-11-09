using Elements.Geometry;
using Elements.Serialization.IFC;
using Elements.Serialization.glTF;
using System;
using System.IO;
using Xunit;
using Xunit.Abstractions;
using System.Collections.Generic;
using Elements.Geometry.Profiles;
using System.Linq;

namespace Elements.IFC.Tests
{
    public class IfcTests
    {
        private const string basePath = "models";

        private readonly ITestOutputHelper output;

        private readonly WideFlangeProfileFactory _profileFactory = new WideFlangeProfileFactory();

        public IfcTests(ITestOutputHelper output)
        {
            this.output = output;
        }

        [Theory]
        // [InlineData("rac_sample", "../../../models/IFC4/rac_advanced_sample_project.ifc")]
        // [InlineData("rme_sample", "../../../models/IFC4/rme_advanced_sample_project.ifc")]
        // [InlineData("rst_sample", "../../../models/IFC4/rst_advanced_sample_project.ifc")]
        [InlineData("AC-20-Smiley-West-10-Bldg", "../../../models/IFC4/AC-20-Smiley-West-10-Bldg.ifc", 1972, 120, 539, 270, 9, 140, 10, 2)]
        // TODO: Some walls are extracted incorrectly and intersecting the roof. It happens because
        // IfcBooleanClippingResultParser doesn't handle the boolean clipping operation.
        // In order to fix it surface support is required.
        // The Plane case isn't implemented because some critical information about IfcPlane is
        // missing during it's extraction.
        // TODO: German names are converted incorrectly.
        // TODO: The entrance door has an incorrect representation. It happens because during
        // the UpdateRepresentation the default representation of a door is created instead of
        // the extracted one.
        [InlineData("AC20-Institute-Var-2", "../../../models/IFC4/AC20-Institute-Var-2.ifc", 1513, 5, 577, 121, 7, 82, 0, 21)]
        // [InlineData("20160125WestRiverSide Hospital - IFC4-Autodesk_Hospital_Sprinkle", "../../../models/IFC4/20160125WestRiverSide Hospital - IFC4-Autodesk_Hospital_Sprinkle.ifc")]
        public void FromIFC4(string name,
                         string ifcPath,
                         int expectedElementsCount,
                         int expectedCountOfFloors,
                         int expectedCountOfOpenings,
                         int expectedCountOfWalls,
                         int expectedCountOfDoors,
                         int expectedCountOfSpaces,
                         int expectedCountOfBeams,
                         int expectedCountOfErrors
            )
        {
            var model = IFCModelExtensions.FromIFC(Path.Combine(Environment.CurrentDirectory, ifcPath), out var ctorErrors);

            int countOfFloors = model.AllElementsOfType<Floor>().Count();
            int countOfOpenings = model.AllElementsOfType<Opening>().Count();
            int countOfWalls = model.AllElementsOfType<Wall>().Count();
            int countOfDoors = model.AllElementsOfType<Door>().Count();
            int countOfSpaces = model.AllElementsOfType<Space>().Count();
            int countOfBeams = model.AllElementsOfType<Beam>().Count();

            Assert.Equal(expectedElementsCount, model.Elements.Count);

            Assert.Equal(expectedCountOfFloors, countOfFloors);
            Assert.Equal(expectedCountOfOpenings, countOfOpenings);
            Assert.Equal(expectedCountOfWalls, countOfWalls);
            Assert.Equal(expectedCountOfDoors, countOfDoors);
            Assert.Equal(expectedCountOfSpaces, countOfSpaces);
            Assert.Equal(expectedCountOfBeams, countOfBeams);

            foreach (var e in ctorErrors)
            {
                this.output.WriteLine(e);
            }

            Assert.Equal(expectedCountOfErrors, ctorErrors.Count);

            model.ToJson(ConstructJsonPath(name));
            model.ToGlTF(ConstructGlbPath(name));
        }

        [Theory(Skip = "IFC2X3")]
        [InlineData("example_1", "../../../models/IFC2X3/example_1.ifc")]
        // TODO: Reenable when IfcCompositeCurve is supported.
        // [InlineData("example_2", "../../../models/IFC2X3/example_2.ifc")]
        [InlineData("example_3", "../../../models/IFC2X3/example_3.ifc")]// new []{"0bKcgqsaHFN9FTVipKV_Ue","3Lkqsa9JzD0BBXIMnx2zgD"})]
        [InlineData("wall_with_window_vectorworks", "../../../models/IFC2X3/wall_with_window_vectorworks.ifc")]
        public void IFC2X3(string name, string ifcPath, string[] idsToConvert = null)
        {
            var model = IFCModelExtensions.FromIFC(Path.Combine(Environment.CurrentDirectory, ifcPath), out var ctorErrors, idsToConvert);
            foreach (var e in ctorErrors)
            {
                this.output.WriteLine(e);
            }
            model.ToGlTF(ConstructGlbPath(name));
        }

        [Fact]
        public void InstanceOpenings()
        {
            var model = System.IO.File.ReadAllText("../../../models/Hypar/instance-openings-test-model.json");
            var hyparModel = Model.FromJson(model);
            var walls = hyparModel.AllElementsOfType<StandardWall>();
            var path = ConstructIfcPath("instance-openings-test");
            hyparModel.ToIFC(path);

            var file = System.IO.File.ReadAllLines(path);

            var wallCount = file.Count(x => x.Contains("IFCWALLSTANDARDCASE"));
            var openingCount = file.Count(x => x.Contains("IFCRELVOIDSELEMENT"));
            var floorCount = file.Count(x => x.Contains("IFCSLAB"));

            Assert.Equal(wallCount, 4);
            Assert.Equal(openingCount, 5);
            Assert.Equal(floorCount, 1);
        }

        [Fact]
        public void SpaceTemplate()
        {
            var model = System.IO.File.ReadAllText("../../../models/Hypar/space-planning.json");
            var hyparModel = Model.FromJson(model);
            var path = ConstructIfcPath("space-planning-test");
            hyparModel.ToIFC(path);
        }

        [Fact]
        public void Doors()
        {
            var model = new Model();

            // Add 2 walls.
            var wallLine1 = new Line(Vector3.Origin, new Vector3(10, 10, 0));
            var wallLine2 = new Line(new Vector3(10, 10, 0), new Vector3(10, 15, 0));
            var wall1 = new StandardWall(wallLine1, 0.2, 3, name: "Wall1");
            var wall2 = new StandardWall(wallLine2, 0.2, 2, name: "Wall2");

            model.AddElement(wall1);
            model.AddElement(wall2);

            var door1 = new Door(wallLine1, 0.5, 1.5, 2.0, DoorOpeningSide.LeftHand, DoorOpeningType.DoubleSwing);
            var door2 = new Door(wallLine2, 0.5, 1.5, 1.8, DoorOpeningSide.LeftHand, DoorOpeningType.DoubleSwing);

            model.AddElement(door1);
            model.AddElement(door2);

            model.ToIFC(ConstructIfcPath("IfcDoor"));
        }

        [Fact]
        public void Wall()
        {
            var line = new Line(Vector3.Origin, new Vector3(10, 10, 0));
            var line1 = new Line(new Vector3(10, 10, 0), new Vector3(10, 15, 0));
            var wall = new StandardWall(line, 0.2, 3);
            var wall1 = new StandardWall(line1, 0.2, 2);
            var model = new Model();
            model.AddElement(wall);
            model.AddElement(wall1);
            model.ToIFC(ConstructIfcPath("IfcWall"));
        }

        [Fact]
        public void PlanWall()
        {
            var planShape = Polygon.L(2, 2, 0.15);
            var wall1 = new Wall(planShape, 3.0);
            var wall2 = new Wall(planShape, 3.0, BuiltInMaterials.Concrete, new Transform(0, 0, 3));
            var model = new Model();
            model.AddElement(wall1);
            model.AddElement(wall2);
            model.ToIFC(ConstructIfcPath("IfcWallPlan"));
        }

        [Fact]
        public void Floor()
        {
            var planShape = Polygon.L(2, 4, 1.5);
            var floor = new Floor(planShape, 0.1);
            var floor1 = new Floor(planShape, 0.1, new Transform(0, 0, 2));
            var o = new Opening(Polygon.Rectangle(0.5, 0.5), Vector3.ZAxis, transform: new Transform(0.5, 0.5, 0));
            floor.Openings.Add(o);

            var model = new Model();
            model.AddElement(floor);
            model.AddElement(floor1);

            var ifcPath = ConstructIfcPath("IfcFloor");
            model.ToIFC(ifcPath);
            model.ToGlTF(ConstructGlbPath("IfcFloor"));

            var newModel = IFCModelExtensions.FromIFC(ifcPath, out var ctorErrors);
            foreach (var e in ctorErrors)
            {
                this.output.WriteLine(e);
            }

            Assert.Equal(model.Elements.Values.Count, newModel.Elements.Values.Count);
            newModel.ToGlTF(ConstructGlbPath("IfcFloor2"));
        }

        [Fact]
        public void Beams()
        {
            var model = new Model();

            var pts = Hypar(5.0, 5.0);
            var m1 = new Material("red", Colors.Red, 0f, 0f);
            var m2 = new Material("green", Colors.Green, 0f, 0f);

            var prof = _profileFactory.GetProfileByType(WideFlangeProfileType.W10x100);
            for (var j = 0; j < pts.Count; j++)
            {
                var colA = pts[j];
                List<Vector3> colB = null;
                if (j + 1 < pts.Count)
                {
                    colB = pts[j + 1];
                }

                for (var i = 0; i < colA.Count; i++)
                {
                    var a = colA[i];
                    if (i + 1 < colA.Count)
                    {
                        Vector3 b = colA[i + 1];
                        var line1 = new Line(a, b);
                        var beam1 = new Beam(line1, prof, material: m1, name: $"Hypar's beam {j}_{i}");
                        model.AddElement(beam1);
                    }

                    if (colB != null)
                    {
                        var c = colB[i];
                        var line2 = new Line(a, c);
                        var beam2 = new Beam(line2, prof, material: m2);
                        model.AddElement(beam2);
                    }
                }
            }
            model.ToIFC(ConstructIfcPath("IfcBeams"));
        }

        private List<List<Vector3>> Hypar(double a, double b)
        {
            var result = new List<List<Vector3>>();
            for (var x = -5; x <= 5; x++)
            {
                var column = new List<Vector3>();
                for (var y = -5; y <= 5; y++)
                {
                    var z = Math.Pow(y, 2) / Math.Pow(b, 2) - Math.Pow(x, 2) / Math.Pow(a, 2);
                    column.Add(new Vector3(x, y, z));
                }
                result.Add(column);
            }

            return result;
        }

        private string ConstructIfcPath(string modelName)
        {
            var modelsDirectory = Path.Combine(Environment.CurrentDirectory, basePath);
            if (!Directory.Exists(modelsDirectory))
            {
                Directory.CreateDirectory(modelsDirectory);
            }
            return Path.GetFullPath(Path.Combine(Environment.CurrentDirectory, basePath, $"{modelName}.ifc"));
        }

        private string ConstructGlbPath(string modelName)
        {
            var modelsDirectory = Path.Combine(Environment.CurrentDirectory, basePath);
            if (!Directory.Exists(modelsDirectory))
            {
                Directory.CreateDirectory(modelsDirectory);
            }
            return Path.GetFullPath(Path.Combine(modelsDirectory, $"{modelName}.glb"));
        }

        private string ConstructJsonPath(string modelName)
        {
            var modelsDirectory = Path.Combine(Environment.CurrentDirectory, basePath);
            if (!Directory.Exists(modelsDirectory))
            {
                Directory.CreateDirectory(modelsDirectory);
            }
            return Path.GetFullPath(Path.Combine(modelsDirectory, $"{modelName}.json"));
        }
    }
}