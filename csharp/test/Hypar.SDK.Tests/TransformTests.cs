using Hypar.Geometry;
using Xunit;

namespace Hypar.Tests
{
    public class TransformTests
    {   
        [Fact]
        public void Transform()
        {
            var t = new Transform(Vector3.Origin, Vector3.XAxis, Vector3.YAxis.Negated());
            var v = new Vector3(0.5,0.5,0.0);
            var vt = t.OfPoint(v);
            Assert.Equal(0.5, vt.X);
            Assert.Equal(0.0, vt.Y);
            Assert.Equal(0.5, vt.Z);
        }

        [Fact]
        public void Transform_Translate()
        {
            var t = new Transform(new Vector3(5,0,0), Vector3.XAxis, Vector3.YAxis.Negated());
            var v = new Vector3(0.5,0.5,0.0);
            var vt = t.OfPoint(v);
            Assert.Equal(5.5, vt.X);
            Assert.Equal(0.0, vt.Y);
            Assert.Equal(0.5, vt.Z);
        }
    }
    
}