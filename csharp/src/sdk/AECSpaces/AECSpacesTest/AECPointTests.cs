using Microsoft.VisualStudio.TestTools.UnitTesting;
using AECSpaces;

namespace AECSpacesTest
{
    [TestClass]
    public class PointTests
    {
        [TestMethod]
        public void AreColocated()
        {
            AECPoint point0 = new AECPoint(1, 0, 0);
            AECPoint point1 = new AECPoint(1, 0, 0);
            AECPoint point2 = new AECPoint(1, 1, 0);
            Assert.IsTrue(point0.IsColocated(point1));
            Assert.IsFalse(point0.IsColocated(point2));
        }//method

        [TestMethod]
        public void MoveBy()
        {
            AECPoint point = new AECPoint(1, 0, 0);
            point.MoveBy(20, 20, 20);
            Assert.AreEqual(21, point.X, 0);
            Assert.AreEqual(20, point.Y, 0);
            Assert.AreEqual(20, point.Z, 0);
        }//method

        [TestMethod]
        public void Rotate()
        {
            AECPoint point = new AECPoint(3, 0, 0);
            point.Rotate(90, 0, 0);
            Assert.AreEqual(0, point.X, 0);
            Assert.AreEqual(3, point.Y, 0);
        }//method
    }//class
}//method
