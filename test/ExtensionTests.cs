using System;
using System.Linq;
using Xunit;

namespace Elements.Tests
{
    class TestElement : Element
    {
    }

    public class ExtensionsTests
    {
        [Fact]
        public void ExtensionTests()
        {
            var model = new Model();
            model.AddElement(new TestElement());
            var json = model.ToJson();
            Console.WriteLine(json);
            var newModel = Model.FromJson(json);
            Assert.Equal(newModel.Extensions.Count(), 1);
            Assert.Equal(newModel.Elements.Count, 1);
        }
    }
}