using Elements.Geometry;
using Elements.Tests;
using Elements;
using Xunit;

namespace Elements
{
    public class DoorTest : ModelTest
    {
        [Fact, Trait("Category", "Examples")]
        public void MakeDoorElement()
        {
            this.Name = nameof(MakeDoorElement);

            var line = new Line(new Vector3(0, 0, 0), new Vector3(10, 10, 0));
            var wall = new StandardWall(line, 0.1, 3.0);
            var door = new Door(wall.CenterLine, 0.5, 2.0, 2.0, 2 * 0.0254, DoorOpeningSide.LeftHand, DoorOpeningType.SingleSwing);
            wall.AddDoorOpening(door);

            Assert.Single(wall.Openings);

            this.Model.AddElement(wall);
            Model.AddElement(door);
        }
    }
}
