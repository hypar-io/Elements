using Elements.Geometry;
using Elements.Geometry.Solids;
using Elements.Properties;
using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace Elements.Tests
{
    [UserElement]
    public class TestUserElement : GeometricElement
    {
        public Line CenterLine { get; set; }
        public Profile Profile { get; set; }

        /// <summary>
        /// Something you want to measure.
        /// </summary>
        public NumericProperty Length => new NumericProperty(this.CenterLine.Length(), NumericPropertyUnitType.Length);

        public TestUserElement(Line centerLine,
                               Profile profile,
                               Material material = null,
                               Guid id = default(Guid),
                               string name = null) : base(new Transform(),
                                                          material = material != null ? material : BuiltInMaterials.Default,
                                                          new Representation(new List<SolidOperation>()),
                                                          id = id != null ? id : Guid.NewGuid(),
                                                          name)
        {
            this.CenterLine = centerLine;
            this.Profile = profile;

            var t = this.CenterLine.TransformAt(0);
            var x = new Line(t.Origin, t.XAxis * this.CenterLine.Length());
            var y = new Line(t.Origin, t.YAxis * this.CenterLine.Length());
            this.Representation.SolidOperations.Add(new Sweep(this.Profile, this.CenterLine, 0.0, 0.0, 0.0, false));
            this.Representation.SolidOperations.Add(new Sweep(this.Profile, x,  0.0, 0.0, 0.0, false));
            this.Representation.SolidOperations.Add(new Sweep(this.Profile, y,  0.0, 0.0, 0.0, false));
        }

        /// <summary>
        /// Update the representations.
        /// </summary>
        public override void UpdateRepresentations()
        {
            return;
        }
    }

    public class UserElementTests : ModelTest
    {
        [Fact]
        public void CreateCustomElement()
        {
            this.Name = "UserElement";

            var line = new Line(Vector3.Origin, new Vector3(5, 5, 5));
            var ue = new TestUserElement(line, new Profile(Polygon.L(1, 2, 0.5)));
            this.Model.AddElement(ue);

            // var beam = new Beam(line, new Profile(Polygon.Rectangle(0.25, 0.25)), BuiltInMaterials.Steel);
            // this.Model.AddElement(beam);

            var json = this.Model.ToJson();
            var newModel = Model.FromJson(json);
            Assert.Equal(3, newModel.Elements.Count);
            Assert.Equal(1, newModel.AllElementsOfType<TestUserElement>().ToArray().Count());
        }
    }
}