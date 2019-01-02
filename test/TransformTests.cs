using Elements;
using Elements.Geometry;
using System;
using Xunit;

namespace Elements.Tests
{
    public class TransformTests
    {   
        [Fact]
        public void Example()
        {
            var model = new Model();
            var m1 = new Mass(Polygon.Rectangle(),0.0, 1.0, new Material("yellow", Colors.Yellow));
            model.AddElement(m1);

            var j = 1.0;
            var count = 10;
            for(var i=0.0; i<360.0; i+= 360.0/(double)count)
            {
                var m2 = new Mass(Polygon.Rectangle(), 0.0, 1.0, new Material($"color_{j}", new Color((float)j - 1.0f, 0.0f, 0.0f, 1.0f)));
                m2.Transform.Scale(new Vector3(j,j,j));
                m2.Transform.Move(new Vector3(3, 0, 0));
                m2.Transform.Rotate(Vector3.ZAxis, i);
                model.AddElement(m2);
                j += 1.0/(double)count;
            }

            model.SaveGlb("transform.glb");
        }

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