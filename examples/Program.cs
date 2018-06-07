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
            
            

            var massPerimeter = Profiles.Square(Vector3.Origin(), 22, 22);
            var mass = ElementsFactory.CreateMass()
                            .WithBottomProfile(massPerimeter)
                            .WithTopProfile(massPerimeter)
                            .WithTopAtElevation(20);

            var faces = mass.Faces();
            
            var grids = ElementsFactory.CreateGrids(4)
                            .WithinPerimeters(faces)
                            .WithUDivisions(5)
                            .WithVDivisions(new[]{0.0, 0.4, 0.55, 1.0});
            
            var panelProfile = Profiles.Square(Vector3.Origin(), 0.05, 1.0, 0.0, 0.0);
            
            // Create a function which generates a panel given a perimeter.
            var panelMaterial = new Material("panel", 0.0f, 1.0f, 1.0f, 0.75f, 1.0f, 1.0f);
            var createGlazedPanel = new Func<Polyline, IEnumerable<Element>>((Polyline p) => {
                var panel = Panel.WithinPerimeter(p).OfMaterial(panelMaterial);

                var mullions = Beam.AlongLines(p.Explode())
                                    .WithProfile(panelProfile)
                                    .WithUpAxis(p.Normal())
                                    .OfMaterial(BuiltIntMaterials.Steel);

                var results = new List<Element>();
                results.Add(panel);
                results.AddRange(mullions);
                return results;
            });

            var opaquePanelMaterial = new Material("opaquePanel", 0.0f, 0.0f, 0.0f, 1.0f, 0.1f, 0.1f);
            var createSolidPanel = new Func<Polyline, IEnumerable<Element>>((Polyline p) => {
                var panel = Panel.WithinPerimeter(p).OfMaterial(opaquePanelMaterial);
                return new[]{panel};
            });

            foreach(var g in grids)
            {
                var panelElements = g.InAllCellsAlongRow(0, createGlazedPanel);
                var panelElements2 = g.InAllCellsAlongRow(2, createGlazedPanel);
                var opaquePanels = g.InAllCellsAlongRow(1, createSolidPanel);

                model.AddElements(panelElements);
                model.AddElements(panelElements2);
                model.AddElements(opaquePanels);
            }

            model.AddElements(columns);
            model.AddElements(beams);
            model.AddElements(slabs);

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
