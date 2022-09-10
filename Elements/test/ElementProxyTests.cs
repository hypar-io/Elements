using Xunit;
using Elements.Geometry;

namespace Elements.Tests
{
    public class ElementProxyTests : ModelTest
    {
        [Fact]
        public void ProxiesAreReused()
        {
            // Creating a proxy of the same element twice via Element.Proxy should return you the exact same proxy, not a new proxy for each call.
            var mass = new Mass(new Profile(Polygon.Rectangle(1, 1)));
            var dependencyName = "Fake Dependency Name";
            var proxy1 = mass.Proxy(dependencyName);
            var proxy2 = mass.Proxy(dependencyName);
            Assert.Equal(proxy1.Id, proxy2.Id);
        }

        [Fact]
        public void ProxiesCanDeserialize()
        {
            var mass = new Mass(new Profile(Polygon.Rectangle(1, 1)));
            var dependencyName = "Fake Dependency Name";
            var proxy = mass.Proxy(dependencyName);
            var model = new Model();
            model.AddElement(proxy);
            var json = model.ToJson();
            var deserialized = Model.FromJson(json);
            var deserializedProxy = deserialized.GetElementOfType<ElementProxy<Mass>>(proxy.Id);
            Assert.NotNull(deserializedProxy);
        }

        [Fact]
        public void ProxyCacheCanBeCleared()
        {
            // Creating a proxy of the same element twice via Element.Proxy should return you the exact same proxy, not a new proxy for each call.
            var mass = new Mass(new Profile(Polygon.Rectangle(1, 1)));
            var dependencyName = "Fake Dependency Name";
            var proxy1 = mass.Proxy(dependencyName);
            var proxy2 = mass.Proxy(dependencyName);
            Assert.Equal(proxy1.Id, proxy2.Id);
            ElementProxy.ClearCache();
            var proxy3 = mass.Proxy(dependencyName);
            Assert.NotEqual(proxy1.Id, proxy3.Id);
        }
    }
}