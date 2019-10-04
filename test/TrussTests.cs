using Xunit;
using Elements.Geometry;
using Elements.Geometry.Profiles;
using System.Linq;
using Elements.ElementTypes;

namespace Elements.Tests
{
    public class TrussTests : ModelTest
    {
        [Fact]
        public void Serialize()
        {
            this.Name = "Truss";

            var profile = WideFlangeProfileServer.Instance.GetProfileByName("W8x13");
            var chordType = new StructuralFramingType("W33x387", profile, new Material("Chord", Colors.Aqua));
            var webType = new StructuralFramingType("W33x387", profile, new Material("Web", Colors.Orange));
            var truss = new Truss(new Vector3(0, 0, 0), new Vector3(0,10,0), 1.0, 10, chordType, chordType, webType, BuiltInMaterials.Steel, 0.1, 0.1);
            this.Model.AddElement(truss);
            
            var json = this.Model.ToJson();
            var newModel = Model.FromJson(json);
            var newTruss = newModel.ElementsOfType<Truss>().FirstOrDefault();
            Assert.Equal(truss.Divisions, newTruss.Divisions);
            Assert.Equal(truss.Depth, newTruss.Depth);
            Assert.Equal(truss.Start, newTruss.Start);
            Assert.Equal(truss.End, newTruss.End);
        }
    }
}