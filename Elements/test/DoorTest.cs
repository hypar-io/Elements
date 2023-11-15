﻿using Elements.Geometry;
using Elements.Tests;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace Elements
{
    public class DoorTest : ModelTest
    {
        [Fact, Trait("Category", "Examples")]
        public void Example()
        {
            this.Name = "Elements_Door";

            var line = new Line(new Vector3(0, 0, 0), new Vector3(10, 10, 0));
            var wall = new StandardWall(line, 0.1, 3.0);
            var door = new Door(wall.CenterLine, 0.5, 2.0, 2.0, Door.DOOR_THICKNESS, DoorOpeningSide.LeftHand, DoorOpeningType.SingleSwing);
            wall.AddDoorOpening(door);

            this.Model.AddElement(wall);
            Model.AddElement(door);
        }
    }
}