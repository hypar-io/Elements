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
            var perimeter = Profiles.Rectangular(new Vector3(), 20, 20);

            var p1 = Vector3.Origin();
            var p2 = Vector3.ByXYZ(20, 0, 0);
            var p3 = Vector3.ByXYZ(30, 50, 0);
            var p4 = Vector3.ByXYZ(0, 20, 0);
            var p5 = Vector3.ByXYZ(-20, 40,0);
            var p6 = Vector3.ByXYZ(-30, 10,0);
            var massPerimeter = new Polyline(new[]{p1, p2, p3, p4, p5, p6});
            var offsetPerimeter = massPerimeter.Offset(1.5);

            var massExternal = ElementsFactory.CreateMass()
                            .WithBottomProfile(massPerimeter)
                            .WithTopProfile(massPerimeter)
                            .WithTopAtElevation(20);
            
            var massInternal = ElementsFactory.CreateMass()
                            .WithBottomProfile(offsetPerimeter)
                            .WithTopProfile(offsetPerimeter)
                            .WithTopAtElevation(20);

            var columns = ElementsFactory.CreateBeams(6)
                                        .AlongLines(massInternal.VerticalEdges())
                                        .WithProfile(columnProfile)
                                        .OfMaterial(material);

            var beams = CreateBeams(girderProfile, material, offsetPerimeter);
            
            var slabs = Slab.WithinPerimeters(offsetPerimeter, offsetPerimeter)
                            .AtElevations(new[]{10.0, 20.0})
                            .WithThickness(0.3)
                            .OfMaterial(BuiltIntMaterials.Concrete);
            
            var faces = massExternal.Faces();
            
            var grids = ElementsFactory.CreateGrids(6)
                            .WithinPerimeters(faces)
                            .WithUDivisions(7)
                            .WithVDivisions(new[]{0.0, 0.4, 0.55, 1.0});
            
            var panelProfile = Profiles.Rectangular(Vector3.Origin(), 0.05, 1.0, 0.0, 0.0);
            
            // Create a function which generates a panel given a perimeter.
            var panelMaterial = new Material("panel", 0.4f, 1.0f, 1.0f, 0.75f, 1.0f, 1.0f);
            var createGlazedPanel = new Func<Polyline, IEnumerable<Element>>((Polyline p) => {
                var panel = Panel.WithinPerimeter(p).OfMaterial(panelMaterial);
                return new[]{panel};
            });

            var opaquePanelMaterial = new Material("opaquePanel", 0.0f, 0.0f, 0.0f, 1.0f, 0.1f, 0.1f);
            var createSolidPanel = new Func<Polyline, IEnumerable<Element>>((Polyline p) => {
                var panel = Panel.WithinPerimeter(p)
                            .OfMaterial(opaquePanelMaterial);
                
                return new[]{panel};
            });

            var createMullions = new Func<Polyline, IEnumerable<Element>>((Polyline p) => {
                var mullions = ElementsFactory.CreateBeams(4)
                                    .AlongLines(p.Segments())
                                    .WithProfile(panelProfile)
                                    .WithUpAxis(p.Normal())
                                    .OfMaterial(BuiltIntMaterials.Steel);
                var results = new List<Element>();
                results.AddRange(mullions);
                return results;
            });

            var selectLouverProfile = new Func<Beam, Polyline>((Beam b) => {
                var width = Math.Pow(b.CenterLine.Start.Z / 20.0, 3);
                var louverProfile = Profiles.Rectangular(Vector3.Origin(), width, 0.1, 0.0, -0.75);
                return louverProfile;
            });

            var createLouver = new Func<Line, Element>((Line l) => {
                return ElementsFactory.CreateBeam()
                                        .AlongLine(l)
                                        .WithProfile(selectLouverProfile)
                                        .OfMaterial(BuiltIntMaterials.Wood);
            });

            var createLouvers = new Func<Polyline, IEnumerable<Element>>((Polyline p) => {
                var g = ElementsFactory.CreateGrid()
                                        .WithinPerimeter(p)
                                        .WithUDivisions(10)
                                        .WithVDivisions(1);
                var louvers = g.AlongEachRowEdge(createLouver);
                var results = new List<Element>();
                results.AddRange(louvers);
                return results;
            });

            foreach(var g in grids)
            {
                var panelElements = g.InAllCellsAlongRow(0, createGlazedPanel);
                var panelElements2 = g.InAllCellsAlongRow(2, createGlazedPanel);
                var opaquePanels = g.InAllCellsAlongRow(1, createSolidPanel);
                var mullions = g.InAllCells(createMullions);
                var louvers = g.InAllCellsAlongRow(2, createLouvers);

                model.AddElements(panelElements);
                model.AddElements(panelElements2);
                model.AddElements(opaquePanels);
                model.AddElements(mullions);
                model.AddElements(louvers);
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

        public static IEnumerable<Beam> CreateBeams(Polyline profile, Material material, Polyline perimeter)
        {
            
            var lines = perimeter.Segments();

            var beams = new List<Beam>();
            foreach(var l in lines)
            {
                var a = l.Start;
                var b = l.End;
                var newLine = Line.FromStart(new Vector3(a.X, a.Y, 10)).ToEnd(new Vector3(b.X, b.Y, 10));
                var newLine2 = Line.FromStart(new Vector3(a.X, a.Y, 20)).ToEnd(new Vector3(b.X, b.Y, 20));
                
                var b1 = ElementsFactory.CreateBeam()
                                .AlongLine(newLine)
                                .WithProfile(profile)
                                .OfMaterial(material);
                var b2 = ElementsFactory.CreateBeam()
                                .AlongLine(newLine2)
                                .WithProfile(profile)
                                .OfMaterial(material);
                beams.Add(b1);
                beams.Add(b2);
            }

            return beams;
        }
    }
}
