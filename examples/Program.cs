using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Hypar.Elements;
using Hypar.Geometry;

namespace examples
{
    class Program
    {
        static void Main(string[] args)
        {
            var sw = new Stopwatch();
            sw.Start();

            var model = new Model();

            var d = 2.0;
            var columnProfile = Profiles.WideFlangeProfile(1.0, d, 0.1, 0.1, Profiles.VerticalAlignment.Center);
            var girderProfile = Profiles.WideFlangeProfile(1.0, d, 0.1, 0.1, Profiles.VerticalAlignment.Top);
            var material = new Material("orange", 1.0f, 1.0f, 0.0f, 1.0f, 0.1f, 0.0f);
            var perimeter = Profiles.Square(new Vector3(), 20, 20);

            var columns = CreateColums(columnProfile, material);
            var beams = CreateBeams(girderProfile, material, perimeter);
            var slabs = CreateSlabs(perimeter);

            var panelMaterial = new Material("panel", 0.0f, 1.0f, 1.0f, 0.75f, 1.0f, 1.0f);

            var massPerimeter = Profiles.Square(Vector3.Origin(), 22, 22);
            var mass = Mass.WithBottomProfile(massPerimeter)
                            .WithTopProfile(massPerimeter)
                            .WithTopAtElevation(20);

            var grids = Grid.WithinPerimeters(mass.Faces())
                            .WithUDivisions(5)
                            .WithVDivisions(5);
            
            var panelProfile = Profiles.Square(Vector3.Origin(), 0.05, 1.0, 0.0, 0.0);
            foreach(var g in grids)
            {
                var cells = g.Cells();

                var panels = Panel.WithinPerimeters(cells)
                                    .OfMaterial(panelMaterial);
                model.AddElements(panels);

                foreach(var c in cells)
                {
                    var mullions = Beam.AlongLines(c.Explode())
                                        .WithProfile(panelProfile)
                                        .WithUpAxis(c.Normal())
                                        .OfMaterial(BuiltIntMaterials.Steel);

                    model.AddElements(mullions);
                    
                }
                
            }
            model.AddElements(columns);
            model.AddElements(beams);
            model.AddElements(slabs);
            // model.AddElement(mass);

            // var wf = Profiles.WideFlangeProfile(0.5, 1.0, 0.1, 0.1);
            // var sys1 = new BeamSystem(slabs.ElementAt(0), 5, wf, material);
            // foreach(var b in sys1.Beams)
            // {
            //     model.AddElement(b);
            // }

            // var sys2 = new BeamSystem(slabs.ElementAt(1), 5, wf, material);
            // foreach(var b in sys2.Beams)
            // {
            //     model.AddElement(b);
            // }

            Console.WriteLine($"{sw.Elapsed} for creating building elements.");
            sw.Reset();

            sw.Start();
            model.SaveGlb("building.glb");
            Console.WriteLine($"{sw.Elapsed} for creating glb.");
        }

        public static IEnumerable<Beam> CreateColums(Polyline profile, Material material)
        {
            var colA = Line.FromStart(new Vector3(-10, -10)).ToEnd(new Vector3(-10, -10, 20));
            var colB = Line.FromStart(new Vector3(10,-10)).ToEnd(new Vector3(10, -10, 20));
            var colC = Line.FromStart(new Vector3(10, 10)).ToEnd(new Vector3(10, 10, 20));
            var colD = Line.FromStart(new Vector3(-10,10)).ToEnd(new Vector3(-10, 10, 20));
            
            var col1 = Beam.AlongLine(colA)
                            .WithProfile(profile)
                            .OfMaterial(material);
            var col2 = Beam.AlongLine(colB)
                            .WithProfile(profile)
                            .OfMaterial(material);
            var col3 = Beam.AlongLine(colC)
                            .WithProfile(profile)
                            .OfMaterial(material);
            var col4 = Beam.AlongLine(colD)
                            .WithProfile(profile)
                            .OfMaterial(material);
            
            return new[]{col1, col2, col3, col4};
        }

        public static IEnumerable<Beam> CreateBeams(Polyline profile, Material material, Polyline perimeter)
        {
            
            var lines = perimeter.Explode();

            var beams = new List<Beam>();
            foreach(var l in lines)
            {
                var a = l.Start;
                var b = l.End;
                var newLine = Line.FromStart(new Vector3(a.X, a.Y, 10)).ToEnd(new Vector3(b.X, b.Y, 10));
                var newLine2 = Line.FromStart(new Vector3(a.X, a.Y, 20)).ToEnd(new Vector3(b.X, b.Y, 20));
                var b1 = Beam.AlongLine(newLine)
                                .WithProfile(profile)
                                .OfMaterial(material);
                var b2 = Beam.AlongLine(newLine2)
                                .WithProfile(profile)
                                .OfMaterial(material);
                beams.Add(b1);
                beams.Add(b2);
            }

            return beams;
        }

        public static IEnumerable<Slab> CreateSlabs(Polyline perimeter)
        {
            var d = perimeter.BoundingBox.Max.Y - perimeter.BoundingBox.Min.Y;

            var slabs = Slab.WithinPerimeters(perimeter, perimeter)
                            .AtElevations(new[]{10.0, 20.0})
                            .WithThickness(0.3)
                            .OfMaterial(BuiltIntMaterials.Concrete);

            return slabs;
        }
    }
}
