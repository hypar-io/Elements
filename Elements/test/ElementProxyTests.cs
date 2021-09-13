using Elements.Geometry;
using System;
using Xunit;

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
    }
}