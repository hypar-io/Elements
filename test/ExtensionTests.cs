using System;
using System.Linq;
using Xunit;
using Elements.Serialization.JSON;

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
            Assert.Single(newModel.Extensions);
            Assert.Single(newModel.Elements);
        }
    }
}