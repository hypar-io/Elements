using Elements.Geometry;
using Elements;
using System;
using System.IO;
using System.Linq;
using Xunit;
using Xunit.Abstractions;

namespace Hypar.Tests
{
    public class IfcTests
    {
        private readonly ITestOutputHelper output;

        public IfcTests(ITestOutputHelper output)
        {
            this.output = output;
        }

        // [Fact]
        // public void SlabsFromIFC()
        // {
        //     var model = Model.FromIFC("/Users/ikeough/Documents/IFC-gen/lang/csharp/tests/models/AC-20-Smiley-West-10-Bldg.ifc");
        //     if(model != null)
        //     {
        //         model.SaveGlb("fromIfc.glb");
        //     }
        // }
    }
}