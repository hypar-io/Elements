using Xunit;
using Hypar.Elements;
using Hypar.Geometry;
using System;
using System.Linq;

namespace Hypar.Tests
{
    public class TrussTests
    {
        [Fact]
        public void Example()
        {
            var model = new Model();
            var profile = WideFlangeProfileServer.Instance.GetProfileByName("W33x387");
            var truss = new Truss(new Vector3(0, 0, 0), new Vector3(0,10,0), 1.0, 10, profile, profile, profile, BuiltInMaterials.Steel, 0.1, 0.1); 
            model.AddElement(truss);
            model.SaveGlb("truss.glb");
        }

        [Fact]
        public void Serialize()
        {
            var model = new Model();
            var profile = WideFlangeProfileServer.Instance.GetProfileByName("W33x387");
            var truss = new Truss(new Vector3(0, 0, 0), new Vector3(0,10,0), 1.0, 10, profile, profile, profile, BuiltInMaterials.Steel, 0.1, 0.1);
            model.AddElement(truss);
            var json = model.ToJson();
            Console.WriteLine(json);
            var newModel = Model.FromJson(json);
            var newTruss = newModel.ElementsOfType<Truss>().FirstOrDefault();
            Assert.Equal(truss.Divisions, newTruss.Divisions);
            Assert.Equal(truss.Depth, newTruss.Depth);
            Assert.Equal(truss.Start, newTruss.Start);
            Assert.Equal(truss.End, newTruss.End);
        }
    }
}