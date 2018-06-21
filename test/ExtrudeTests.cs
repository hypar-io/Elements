using Hypar.Elements;
using Hypar.Geometry;
using Xunit;

namespace Hypar.Tests
{
    public class TestProxy : Proxy
    {
        public TestProxy(Material material) : base(material){}

        public override Mesh Tessellate()
        {
            var a = new Vector3(0,0,0);
            var b = new Vector3(1,1,0);
            var c = new Vector3(3,-2, 0);
            var d = new Vector3(5,2,0);
            var curve = new NurbsCurve(new[]{a,b,c,d}, 3);
            var extrude = new Extrude(curve, new Vector3(0,0,1), 5);
            return extrude.Tessellate();
        }
    }

    public class ExtrudeTests
    {
        [Fact]
        public void ValidInputs_Construct_Success()
        {
            var material = new Material("blue", 0.0f, 0.0f, 1.0f, 1.0f, 0.5f, 0.5f);
            var proxy = new TestProxy(material);
            var model = new Model();
            model.AddElement(proxy);
            model.SaveGlb("ExtrudeProxy.glb");
        }
    }
}