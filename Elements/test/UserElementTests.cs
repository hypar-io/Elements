using Elements.Geometry;
using Elements.Geometry.Solids;
using Elements.Serialization.JSON;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;
using Xunit;

namespace Elements.Tests
{
    public class TestUserElement : GeometricElement
    {
        public Line CenterLine { get; set; }

        // Used to test serialization of top level elements.
        [JsonConverter(typeof(ElementConverter<Profile>))]
        public Profile Profile { get; set; }

        // Used to test serialization of lists of sub elements.
        [JsonConverter(typeof(ElementConverter<List<Mass>>))]
        public List<Mass> SubElements { get; set; }

        // Used to test dictionaries of sub elements.
        [JsonConverter(typeof(ElementConverter<Dictionary<string, Element>>))]
        public Dictionary<string, Element> DictionaryElements { get; set; }

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
            this.SubElements = new List<Mass>();
            this.DictionaryElements = new Dictionary<string, Element>();
        }

        public override void UpdateRepresentations()
        {
            this.Representation.SolidOperations.Clear();

            var t = this.CenterLine.TransformAt(0);
            var x = new Line(t.Origin, t.Origin + t.XAxis * this.CenterLine.Length());
            var y = new Line(t.Origin, t.Origin + t.YAxis * this.CenterLine.Length());

            var profileInsideUpdate = new Profile(Polygon.Rectangle(1, 1), new List<Polygon>(), Guid.NewGuid(), "");

            this.Representation.SolidOperations.Add(new Sweep(this.Profile, this.CenterLine, 0.0, 0.0, 0.0, false));
            this.Representation.SolidOperations.Add(new Sweep(this.Profile, x, 0.0, 0.0, 0.0, false));
            this.Representation.SolidOperations.Add(new Sweep(this.Profile, y, 0.0, 0.0, 0.0, false));
            this.Representation.SolidOperations.Add(new Extrude(profileInsideUpdate, 8, Vector3.ZAxis, false));
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

            var p = new Profile(Polygon.Rectangle(2, 2));
            var m1 = new Mass(p, 1, BuiltInMaterials.Wood);
            ue.SubElements.Add(m1);
            ue.DictionaryElements.Add("foo", m1);

            this.Model.AddElement(ue);

            var json = this.Model.ToJson();
            var newModel = Model.FromJson(json);

            var newUe = newModel.AllElementsOfType<TestUserElement>().First();

            // Plus one because of the profile that will be added from
            // UpdateRepresentation() call during serialization.
            Assert.Equal(this.Model.Elements.Count + 1, newModel.Elements.Count);

            Assert.Equal(ue.Representation.SolidOperations.Count, newUe.Representation.SolidOperations.Count);
            Assert.Equal(ue.Id, newUe.Id);
            Assert.Equal(ue.Transform, newUe.Transform);
            Assert.Equal(ue.SubElements[0].Id, newUe.SubElements[0].Id);
            Assert.Equal(ue.DictionaryElements["foo"].Id, newUe.DictionaryElements["foo"].Id);

            // Three profiles.
            // 1. The user element
            // 2. The one for the sub-element masses.
            // 3. The one created during UpdateRepresentation
            // 4. The one created when the model is deserialized
            //    and update representation is called while adding elements.
            // TODO: This is not good. This creates a new profile in the model
            // during every subsequent deserialization. As a general rule,
            // update representations should not be used to create new elements.
            Assert.Equal(4, newModel.AllElementsOfType<Profile>().Count());
        }
    }
}