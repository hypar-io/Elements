using Xunit;
using Elements;
using Elements.Geometry;
using Elements.Geometry.Profiles;
using System;
using System.Linq;

namespace Elements.Tests
{
    public class TrussTests : ModelTest
    {
        [Fact]
        public void Truss()
        {
            this.Name = "Truss";
            var model = new Model();
            var profile = WideFlangeProfileServer.Instance.GetProfileByName("W33x387");
            var truss = new Truss(new Vector3(0, 0, 0), new Vector3(0,30,0), 3.0, 10, profile, profile, profile, BuiltInMaterials.Steel, 0.1, 0.1); 
            this.Model.AddElement(truss);
        }

        [Fact]
        public void Serialize()
        {
            var model = new Model();
            var profile = WideFlangeProfileServer.Instance.GetProfileByName("W33x387");
            var truss = new Truss(new Vector3(0, 0, 0), new Vector3(0,10,0), 1.0, 10, profile, profile, profile, BuiltInMaterials.Steel, 0.1, 0.1);
            model.AddElement(truss);
            var json = model.ToJson();
            var newModel = Model.FromJson(json);
            var newTruss = newModel.ElementsOfType<Truss>().FirstOrDefault();
            Assert.Equal(truss.Divisions, newTruss.Divisions);
            Assert.Equal(truss.Depth, newTruss.Depth);
            Assert.Equal(truss.Start, newTruss.Start);
            Assert.Equal(truss.End, newTruss.End);
        }
    }
}