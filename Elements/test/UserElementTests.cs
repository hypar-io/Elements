using Elements.Geometry;
using Elements.Geometry.Solids;
using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace Elements.Tests
{
    public class TestUserElement : GeometricElement
    {
        public Line CenterLine { get; set; }

        public Profile Profile { get; set; }

        public List<Element> SubElements { get; set; }

        internal TestUserElement() : base(null,
                                            BuiltInMaterials.Default,
                                            new Representation(new List<SolidOperation>()),
                                            false,
                                            Guid.NewGuid(),
                                            null)
        { }

        public TestUserElement(Line centerLine,
                               Profile profile,
                               Material material = null,
                               bool isElementDefinition = false,
                               Guid id = default(Guid),
                               string name = null) : base(new Transform(),
                                                          material = material != null ? material : BuiltInMaterials.Default,
                                                          new Representation(new List<SolidOperation>()),
                                                          isElementDefinition,
                                                          id = id != default(Guid) ? id : Guid.NewGuid(),
                                                          name)
        {
            this.CenterLine = centerLine;
            this.Profile = profile;
            this.SubElements = new List<Element>();
        }

        public override void UpdateRepresentations()
        {
            this.Representation.SolidOperations.Clear();

            var t = this.CenterLine.TransformAt(0);
            var x = new Line(t.Origin, t.Origin + t.XAxis * this.CenterLine.Length());
            var y = new Line(t.Origin, t.Origin + t.YAxis * this.CenterLine.Length());

            this.Representation.SolidOperations.Add(new Sweep(this.Profile, this.CenterLine, 0.0, 0.0, 0.0, false));
            this.Representation.SolidOperations.Add(new Sweep(this.Profile, x, 0.0, 0.0, 0.0, false));
            this.Representation.SolidOperations.Add(new Sweep(this.Profile, y, 0.0, 0.0, 0.0, false));
        }
    }

    public class UserElementTests : ModelTest
    {
        [Fact]
        public void CreateCustomElement()
        {
            this.Name = "UserElement";

            var line = new Line(Vector3.Origin, new Vector3(5, 5, 5));
            var m = new Material("UserElementGreen", Colors.Green);
            var ue = new TestUserElement(line, new Profile(Polygon.L(1, 2, 0.5)), m);

            var p = new Profile(Polygon.Rectangle(1, 1));
            var m1 = new Mass(p, 1, BuiltInMaterials.Wood);
            ue.SubElements.Add(m1);

            this.Model.AddElement(ue);

            var json = this.Model.ToJson();
            var newModel = Model.FromJson(json);
            var newUe = newModel.AllElementsOfType<TestUserElement>().First();

            Assert.Equal(6, newModel.Elements.Count);
            Assert.Equal(ue.Representation.SolidOperations.Count, newUe.Representation.SolidOperations.Count);
            Assert.Equal(ue.Id, newUe.Id);
            Assert.Equal(ue.Transform, newUe.Transform);

            // Two profiles. The one for the user element
            // and the one for the sub-element masses.
            Assert.Equal(2, newModel.AllElementsOfType<Profile>().Count());
        }
    }
}