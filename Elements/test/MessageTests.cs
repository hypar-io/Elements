using Elements;
using Elements.Geometry;
using Elements.Annotations;
using Xunit;

namespace Elements.Tests
{
    public class MessageTests
    {
        [Fact]
        public void MessageFromPointHasCorrectTransform()
        {
            var point = new Vector3(2.1, 4.2, 2);
            var message = Message.FromPoint("Test", point);
            Assert.Equal(point, message.Transform.Origin);
        }
    }
}
