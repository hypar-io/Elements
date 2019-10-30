using System;
using System.Diagnostics;
using Elements.Geometry;
using Xunit;

namespace Elements.Tests
{
    public class PerformanceTests : ModelTest
    {
        [Fact(Skip="Performance")]
        public void GlTFWriteTest()
        {
            this.Name = "Performance_Edges";

            // Create 3000 masses
            var sw = new Stopwatch();
            sw.Start();
            
            var w = 1.0;
            var l = 1.0;
            var dim = 100;

            var profile = new Profile(Polygon.Rectangle(w,l));

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

            sw.Stop();
            Console.WriteLine($"{sw.Elapsed} for creating {dim*dim} masses.");
        }
    }
}