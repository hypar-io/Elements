using Elements;
using Elements.Geometry;
using Elements.Geometry.Interfaces;
using Elements.Geometry.Profiles;
using System;
using System.Linq;
using Xunit;

namespace Elements.Tests
{
    public class ColumnTest : ModelTest
    {
        [Fact]
        public void Column()
        {
            this.Name = "Elements_Column";

            var profile = WideFlangeProfileServer.Instance.GetProfileByName("W44x335");
            var framingType = new StructuralFramingType(profile.Name, profile, BuiltInMaterials.Steel);
            var column = new Column(Vector3.Origin, 3.0, framingType);
            
            this.Model.AddElement(column);
        }
    }
}