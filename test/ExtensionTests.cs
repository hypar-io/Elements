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
            var newModel = Model.FromJson(json);
            Assert.Equal(1, newModel.Extensions.Count());
            Assert.Equal(1, newModel.Elements.Count);
        }
    }
}