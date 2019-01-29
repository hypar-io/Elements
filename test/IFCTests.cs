using Elements.Geometry;
using Elements;
using System;
using System.IO;
using System.Linq;
using Xunit;
using Xunit.Abstractions;
using System.Reflection;

namespace Elements.Tests
{
    public class IfcTests : ModelTest
    {
        private readonly ITestOutputHelper output;

        public IfcTests(ITestOutputHelper output)
        {
            this.output = output;
        }

        [Theory]
        // [InlineData("rac_sample", "../../../models/rac_advanced_sample_project.ifc")]
        // [InlineData("rme_sample", "../../../models/rme_advanced_sample_project.ifc")]
        // [InlineData("rst_sample", "../../../models/rst_advanced_sample_project.ifc")]
        [InlineData("AC-20-Smiley-West-10-Bldg", "../../../models/AC-20-Smiley-West-10-Bldg.ifc")]
        [InlineData("AC20-Institute-Var-2", "../../../models/AC20-Institute-Var-2.ifc")]
        [InlineData("20160125WestRiverSide Hospital - IFC4-Autodesk_Hospital_Sprinkle", "../../../models/20160125WestRiverSide Hospital - IFC4-Autodesk_Hospital_Sprinkle.ifc")]
        public void IFC(string name, string ifcPath)
        {
            this.Name = name;
            this.Model = Model.FromIFC(Path.Combine(Environment.CurrentDirectory, ifcPath));
        }
    }
}