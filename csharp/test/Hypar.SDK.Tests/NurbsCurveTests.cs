using Xunit;
using Hypar.Geometry;
using System;
using System.Linq;

namespace Hypar.Tests
{
    public class NurbsCurveTests
    {
        [Fact]
        public void ValidData_Construct_Sucess()
        {
            var a = new Vector3(0,0,0);
            var b = new Vector3(1,1,0);
            var c = new Vector3(3,-2, 0);
            var d = new Vector3(5,9,0);
            var curve = new NurbsCurve(new[]{a,b,c,d}, 3);
            Assert.NotNull(curve);
            var tess = curve.Tessellate();
            Assert.True(tess.ToArray().Length > 3);
            Console.WriteLine(string.Join(",",tess));
        }
    }
}