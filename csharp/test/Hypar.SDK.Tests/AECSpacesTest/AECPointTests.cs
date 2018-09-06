using AECSpaces;
using Xunit;

namespace AECSpacesTest
{
    public class PointTests
    {
        [Fact]
        public void AreColocated()
        {
            AECPoint point0 = new AECPoint(1, 0, 0);
            AECPoint point1 = new AECPoint(1, 0, 0);
            AECPoint point2 = new AECPoint(1, 1, 0);
            Assert.True(point0.IsColocated(point1));
            Assert.False(point0.IsColocated(point2));
        }

        [Fact]
        public void MoveBy()
        {
            AECPoint point = new AECPoint(1, 0, 0);
            point.MoveBy(20, 20, 20);
            Assert.Equal(21, point.X, 0);
            Assert.Equal(20, point.Y, 0);
            Assert.Equal(20, point.Z, 0);
        }

        [Fact]
        public void Rotate()
        {
            AECPoint point = new AECPoint(3, 0, 0);
            point.Rotate(90, 0, 0);
            Assert.Equal(0, point.X, 0);
            Assert.Equal(3, point.Y, 0);
        }
    }
}
