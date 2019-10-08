using Elements.Geometry;
using Elements.Properties;
using System.Linq;
using Xunit;

namespace Elements.Tests
{
    [UserElement]
    public class FooBar : Element
    {
        public Line Foo {get;set;}
        public Profile Bar {get;set;}

        public NumericProperty Baz {get;set;}

        public FooBar(Line line, Profile profile, NumericProperty baz)
        {
            this.Foo = line;
            this.Bar = profile;
            this.Baz = baz;
        }
    }

    public class UserElementTests : ModelTest
    {
        [Fact]
        public void CreateCustomElement()
        {
            var line = new Line(Vector3.Origin, new Vector3(5,5,5));
            var measure = new NumericProperty(42.0, NumericPropertyUnitType.Force);

            var ue = new FooBar(line, new Profile(Polygon.L(1,2,0.5)), measure);
            var model = new Model();
            model.AddElement(ue);

            var beam = new Beam(line, new Profile(Polygon.Rectangle(0.25,0.25)), BuiltInMaterials.Steel);
            model.AddElement(beam);

            var json = model.ToJson();
            var newModel = Model.FromJson(json);
            Assert.Equal(5, model.Count);
            Assert.Equal(1, model.AllEntitiesOfType<FooBar>().ToArray().Count());
        }
    }
}