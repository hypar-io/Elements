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
            var line = new Line((2.1, 4.2, 2), (3.3, 5, 3));
            var message = Message.FromPoint("Test", line.Start);
            Assert.Equal(line.Start, message.Transform.Origin);
        }
    }
}
