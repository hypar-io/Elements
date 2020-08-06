
using Elements.Geometry;
using Xunit;

namespace Elements.Tests
{
    public class MatrixTests {
        [Fact]
        public void DeterminantTest()
        {
            var matrix = new Matrix(new Vector3(6, 1, 1),
                                    new Vector3(4, -2, 5),
                                    new Vector3(2, 8, 7),
                                    new Vector3(0, 0, 0));

            var det = matrix.Determinant();
            Assert.Equal(det, -306.0);
        }
    }
}