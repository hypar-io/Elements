using System;
using System.Diagnostics;
using Elements.Geometry;
using Elements.Serialization.glTF;
using Xunit;
using Xunit.Abstractions;

namespace Elements.Tests
{
    public class PerformanceTests : ModelTest
    {
        private ITestOutputHelper _helper;
        public PerformanceTests(ITestOutputHelper helper)
        {
            this._helper = helper;
        }
        
        [Fact]
        public void GlTFWriteTest()
        {
            this.Name = "Performance_Edges";

            // Create 3000 masses
            var sw = new Stopwatch();
            sw.Start();
            
            var w = 1.0;
            var l = 1.0;
            var dim = 60;

            var profile = new Profile(Polygon.Rectangle(w,l, new Vector3(w, l)));

            // Create 3600 masses.
            for(var i=0; i<dim;i++)
            {
                for(var j=0; j<dim; j++)
                {
                    // Console.WriteLine($"Creating mass {i},{j}");
                    var mass = new Mass(profile, 1, BuiltInMaterials.Mass, new Transform(new Vector3(w*i, l*j)));
                    this.Model.AddElement(mass);
                }
            }

            this.Model.ToGlTF("Performance_Elements.gltf", false);

            sw.Stop();
            _helper.WriteLine($"{sw.Elapsed} for creating {dim*dim} masses.");

        }
    }
}