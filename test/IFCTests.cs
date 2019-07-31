using Elements.Geometry;
using System;
using System.IO;
using Xunit;
using Xunit.Abstractions;
using System.Collections.Generic;
using Elements.Geometry.Profiles;

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
        // [InlineData("example_1", "../../../models/IFC2X3/example_1.ifc")]
        // TODO: Reenable when IfcCompositeCurve is supported.
        // [InlineData("example_2", "../../../models/IFC2X3/example_2.ifc")]
        [InlineData("example_3", "../../../models/IFC2X3/example_3.ifc")]// new []{"0bKcgqsaHFN9FTVipKV_Ue","3Lkqsa9JzD0BBXIMnx2zgD"})]
        // [InlineData("wall_with_window_vectorworks", "../../../models/IFC2X3/wall_with_window_vectorworks.ifc")]
        public void IFC2X3(string name, string ifcPath, string[] idsToConvert = null)
        {
            this.Name = name;
            this.Model = Model.FromIFC(Path.Combine(Environment.CurrentDirectory, ifcPath), idsToConvert);
        }

        [Fact]
        public void Wall()
        {
            this.Name = "IfcWall";
            var line = new Line(Vector3.Origin, new Vector3(10,10,0));
            var line1 = new Line(new Vector3(10,10,0), new Vector3(10,15,0));
            var wallType = new WallType("test", 0.2);
            var wall = new StandardWall(line, wallType, 3);
            var wall1 = new StandardWall(line1, wallType, 2);
            this.Model.AddElement(wall);
            this.Model.AddElement(wall1);
        }

        [Fact]
        public void PlanWall()
        {
            this.Name = "IfcWallPlan";
            var planShape = Polygon.L(2,2,0.15);
            var wallType = new WallType("Thick Wall", BuiltInMaterials.Concrete);
            var wall1 = new Wall(planShape, wallType, 3.0);
            var wall2 = new Wall(planShape, wallType, 3.0, new Transform(0,0,3));
            this.Model.AddElement(wall1);
            this.Model.AddElement(wall2);
        }

        [Fact]
        public void Beams()
        {
            this.Name = "IfcBeams";
            var pts = Hypar(5.0, 5.0);
            var m1 = new Material("red", Colors.Red, 0f, 0f);
            var m2 = new Material("green", Colors.Green, 0f, 0f);

            var t1 = new StructuralFramingType("W16x31", WideFlangeProfileServer.Instance.GetProfileByName("W16x31"), m1);
            var t2 = new StructuralFramingType("W16x31", WideFlangeProfileServer.Instance.GetProfileByName("W16x31"), m2);
            for(var j=0; j<pts.Count; j++)
            {
                var colA = pts[j];
                List<Vector3> colB = null;
                if(j+1 < pts.Count)
                {
                    colB = pts[j+1];
                }

                for(var i=0; i<colA.Count; i++)
                {
                    var a = colA[i];
                    Vector3 b = null;
                    if(i+1 < colA.Count)
                    {
                        b = colA[i+1];
                        var line1 = new Line(a,b);
                        var beam1 = new Beam(line1, t1);
                        this.Model.AddElement(beam1);
                    }

                    if(colB != null)
                    {
                        var c = colB[i];
                        var line2 = new Line(a,c);
                        var beam2 = new Beam(line2, t2);
                        this.Model.AddElement(beam2);
                    }
                }
            } 
        }

        private List<List<Vector3>> Hypar(double a, double b)
        {
            var result = new List<List<Vector3>>();
            for(var x = -5; x<=5; x++)
            {
                var column = new List<Vector3>();
                for(var y=-5; y<=5; y++)
                {
                    var z = Math.Pow(y,2)/Math.Pow(b,2) - Math.Pow(x,2)/Math.Pow(a,2);
                    column.Add(new Vector3(x, y, z));
                }
                result.Add(column);
            }

            return result;
        }
    }
}