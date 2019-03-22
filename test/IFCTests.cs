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

        [Theory(Skip="IFC4")]
        // [InlineData("rac_sample", "../../../models/IFC4/rac_advanced_sample_project.ifc")]
        // [InlineData("rme_sample", "../../../models/IFC4/rme_advanced_sample_project.ifc")]
        // [InlineData("rst_sample", "../../../models/IFC4/rst_advanced_sample_project.ifc")]
        [InlineData("AC-20-Smiley-West-10-Bldg", "../../../models/IFC4/AC-20-Smiley-West-10-Bldg.ifc")]
        [InlineData("AC20-Institute-Var-2", "../../../models/IFC4/AC20-Institute-Var-2.ifc")]
        // [InlineData("20160125WestRiverSide Hospital - IFC4-Autodesk_Hospital_Sprinkle", "../../../models/20160125WestRiverSide Hospital - IFC4-Autodesk_Hospital_Sprinkle.ifc")]
        public void IFC4(string name, string ifcPath)
        {
            this.Name = name;
            this.Model = Model.FromIFC(Path.Combine(Environment.CurrentDirectory, ifcPath));
        }

        [Theory]
        [InlineData("example_1", "../../../models/IFC2X3/example_1.ifc")]
        [InlineData("example_2", "../../../models/IFC2X3/example_2.ifc")]
        [InlineData("example_3", "../../../models/IFC2X3/example_3.ifc")]
        [InlineData("wall_with_window_vectorworks", "../../../models/IFC2X3/wall_with_window_vectorworks.ifc")]
        public void IFC2X3(string name, string ifcPath)
        {
            this.Name = name;
            this.Model = Model.FromIFC(Path.Combine(Environment.CurrentDirectory, ifcPath));
        }
    }
}