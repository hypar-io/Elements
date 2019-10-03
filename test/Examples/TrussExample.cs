using Elements.ElementTypes;
using Elements.Geometry;
using Elements.Geometry.Profiles;
using Xunit;

namespace Elements.Tests.Examples
{
    public class TrussExample : ModelTest
    {
        [Fact]
        public void Truss()
        {
            this.Name = "Elements_Truss";

            // <example>
            // Create a framing type.
            var profile = WideFlangeProfileServer.Instance.GetProfileByName("W33x387");
            var framingType = new StructuralFramingType("W33x387", profile, BuiltInMaterials.Steel);

            // Create a truss.
            var truss = new Truss(new Vector3(0, 0, 0), new Vector3(0,30,10), 3.0, 10, framingType, framingType, framingType, BuiltInMaterials.Steel, 0.1, 0.1); 
            // </example>

            this.Model.AddElement(truss);
            
            Assert.Equal(this.Model.Elements.Count, truss.Elements.Count + 1);
        }
    }
}