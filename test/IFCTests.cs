using Elements.Geometry;
using Elements;
using System;
using System.IO;
using System.Linq;
using Xunit;
using Xunit.Abstractions;

namespace Elements.Tests
{
    public class IfcTests : ModelTest
    {
        private readonly ITestOutputHelper output;

        public IfcTests(ITestOutputHelper output)
        {
            this.output = output;
        }

        [Fact]
        public void IFC()
        {
            this.Name = "IFC";
            this.Model = Model.FromIFC("/Users/ikeough/Documents/IFC-gen/lang/csharp/tests/models/AC-20-Smiley-West-10-Bldg.ifc");
            // this.Model = Model.FromIFC("/Users/ikeough/Documents/IFC-gen/lang/csharp/tests/models/20160125WestRiverSide Hospital - IFC4-Autodesk_Hospital_Sprinkle.ifc");
            // this.Model = Model.FromIFC("/Users/ikeough/Documents/IFC-gen/lang/csharp/tests/models/AC20-Institute-Var-2.ifc");
        }
    }
}