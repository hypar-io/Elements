using Elements.ElementTypes;
using Elements.Geometry;
using System.Linq;
using Xunit;

namespace Elements.Tests
{
    [UserElement]
    public class FooBar : Element
    {
        public Line Foo {get;set;}
        public Profile Bar {get;set;}

        public FooBar(Line line, Profile profile)
        {
            this.Foo = line;
            this.Bar = profile;
        }
    }

    public class UserElementTests : ModelTest
    {
        [Fact]
        public void CreateCustomElement()
        {
            var line = new Line(Vector3.Origin, new Vector3(5,5,5));
            var ue = new FooBar(line, new Profile(Polygon.L(1,2,0.5)));
            var model = new Model();
            model.AddElement(ue);

            var st = new StructuralFramingType("test", new Profile(Polygon.Rectangle(0.25,0.25), null), BuiltInMaterials.Steel);
            var beam = new Beam(line, st);
            model.AddElement(beam);

            var json = model.ToJson();
            var newModel = Model.FromJson(json);
            Assert.Equal(5, model.Count);
            Assert.Equal(1, model.AllEntitiesOfType<FooBar>().ToArray().Count());
        }
    }
}