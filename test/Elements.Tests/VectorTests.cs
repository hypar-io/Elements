using Elements.Geometry;
using System;
using Xunit;

namespace Elements.Tests
{
    public class VectorTests
    {
        [Fact]
        public void Vector3_TwoVectors_Equal()
        {
            var v1 = new Vector3(5,5,5);
            var v2 = new Vector3(5,5,5);
            Assert.True(v1.Equals(v2));

            var v3 = new Vector3();
            Assert.True(!v1.Equals(v3));
        }

        [Fact]
        public void Vector3_AnglesBetween_Success()
        {
            var a = Vector3.XAxis;
            var b = Vector3.YAxis;
            Assert.Equal(Math.PI/2 * 180 / Math.PI, a.AngleTo(b), 5);

            var c = new Vector3(1,1,0);
            Assert.Equal(Math.PI/4 * 180 /Math.PI, a.AngleTo(c), 5);

            Assert.Equal(0.0, a.AngleTo(a), 5);
        }

        [Fact]
        public void Vector3_Parallel_AngleBetween_Success()
        {
            var a = Vector3.XAxis;
            var b = Vector3.XAxis;
            Assert.True(a.IsParallelTo(b));

            var c = a.Negate();
            Assert.True(a.IsParallelTo(c));
        }

        [Fact]
        public void Project()
        {
            var p = new Plane(new Vector3(0,0,5), Vector3.ZAxis);
            var v = new Vector3(5,5,0);
            var v1 = v.Project(p);
            Assert.Equal(v.X, v1.X);
            Assert.Equal(v.Y, v1.Y);
            Assert.Equal(5.0, v1.Z);
        }

        [Fact]
        public void DistanceToPlane()
        {
            var v = new Vector3(0.5,0.5,1.0);
            var p = new Plane(Vector3.Origin, Vector3.ZAxis);
            Assert.Equal(1.0, v.DistanceTo(p));

            v = new Vector3(0.5,0.5,-1.0);
            Assert.Equal(-1.0, v.DistanceTo(p));

            p = new Plane(Vector3.Origin, Vector3.YAxis);
            v = new Vector3(0.5, 1.0, 0.5);
            Assert.Equal(1.0, v.DistanceTo(p));

            v = Vector3.Origin;
            Assert.Equal(0.0, v.DistanceTo(p));
        }

        [Fact]
        public void AreCoplanar()
        {   
            var a = Vector3.Origin;
            var b = new Vector3(1,0);
            var c = new Vector3(1,1,1);

            // Any three points are coplanar.
            Assert.True(new[]{a,b,c}.AreCoplanar());

            var d = new Vector3(5,5);
            Assert.False(new[]{a,b,c,d}.AreCoplanar());

            var e = new Vector3(1,0,0);
            var f = new Vector3(1,1,0);
            var g = new Vector3(1,1,1);
            var h = new Vector3(1,0,1);
            Assert.True(new[]{e,f,g,h}.AreCoplanar());
        }

        [Fact]
        public void CCW()
        {
            var a = new Vector3();
            var b = new Vector3(5,0);
            var c = new Vector3(5,5);
            var d = new Vector3(10,0);
            Assert.True(Vector3.CCW(a,b,c) > 0);
            Assert.True(Vector3.CCW(c,b,a) < 0);
            Assert.True(Vector3.CCW(a,b,d) == 0);
        }
    }
}