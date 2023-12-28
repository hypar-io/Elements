using System.Collections.Generic;
using Elements.Geometry;
using Elements.Search;
using Xunit;
using Xunit.Abstractions;

namespace Elements.Tests
{
    public class SpaceGraphTests : ModelTest
    {
        private readonly ITestOutputHelper _output;

        public SpaceGraphTests(ITestOutputHelper output)
        {
            _output = output;
        }

        [Fact]
        public void SpaceGraphFromRoom()
        {
            this.Name = nameof(SpaceGraphFromRoom);

            var roomPoly = Polygon.Rectangle(5, 5);
            var walls = new List<StandardWall>();
            var wallThickness = Units.InchesToMeters(5);
            foreach (var (from, to) in roomPoly.Edges())
            {
                walls.Add(new StandardWall(new Line(from, to), wallThickness, 3.0, BuiltInMaterials.Concrete));
            }

            var sofa = new Mass(Polygon.Rectangle(Units.FeetToMeters(6), Units.FeetToMeters(2)), Units.FeetToMeters(2))
            {
                Name = "Sofa",
                Material = BuiltInMaterials.Wood
            };
            var desk = new Mass(Polygon.Rectangle(Units.FeetToMeters(4), Units.FeetToMeters(2)), Units.FeetToMeters(3))
            {
                Name = "Desk",
                Material = BuiltInMaterials.Wood
            };
            var lamp = new Mass(Polygon.Rectangle(Units.FeetToMeters(0.5), Units.FeetToMeters(0.5)), Units.FeetToMeters(1.5))
            {
                Name = "Lamp",
                Material = BuiltInMaterials.Steel
            };
            var table = new Mass(new Circle(Vector3.Origin, 1).ToPolygon(), Units.FeetToMeters(2.5))
            {
                Name = "Table",
                Material = BuiltInMaterials.Mass
            };

            var wall1d = walls[0].CenterLine.Direction();
            var wall1n = wall1d.Cross(Vector3.ZAxis);
            var sofaT = new Transform(walls[0].CenterLine.PointAtNormalized(0.3) + wall1n * -(Units.FeetToMeters(1) + wallThickness / 2), wall1d, Vector3.ZAxis);
            sofa.Transform = sofaT;

            var wall3d = walls[2].CenterLine.Direction();
            var wall3n = wall3d.Cross(Vector3.ZAxis);
            var deskT = new Transform(walls[2].CenterLine.PointAtNormalized(0.3) + wall3n * -(Units.FeetToMeters(1) + wallThickness / 2), wall3d, Vector3.ZAxis);
            desk.Transform = deskT;

            var lampT = deskT.Concatenated(new Transform(new Vector3(Units.FeetToMeters(1.5), 0, Units.FeetToMeters(3))));
            lamp.Transform = lampT;

            var model = new Model();
            model.AddElements(walls);
            model.AddElement(sofa);
            model.AddElement(desk);
            model.AddElement(lamp);
            model.AddElement(table);
            model.UpdateRepresentations();
            model.UpdateBoundsAndComputedSolids();

            var graph = SpaceGraph.FromModel(model);
            _output.WriteLine(graph.ToString());

            model.AddElements(sofaT.ToModelCurves());
            model.AddElements(deskT.ToModelCurves());
            model.AddElements(lampT.ToModelCurves());

            this.Model = model;
            graph.ToDot("spacegraph.dot");
        }
    }
}