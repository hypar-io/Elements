using Hypar.Elements;
using Hypar.Geometry;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Amazon.Lambda.Core;

// Assembly attribute to enable the Lambda function's JSON input to be converted into a .NET class.
[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.Json.JsonSerializer))]

namespace Hypar.Example
{
    public class Function
    {
        /// <summary>
        /// A simple function that takes a string and does a ToUpper
        /// </summary>
        /// <param name="input"></param>
        /// <param name="context"></param>
        /// <returns></returns>
        public Dictionary<string,object> FunctionHandler(Dictionary<string, object> args, ILambdaContext context)
        {
            var verticalDivisions = (Int64)args["verticalDivisions"];
            var louverCount = (Int64)args["louverCount"];
            var louverWidth = (double)args["louverWidth"];

            var model = CreateBuilding((int)verticalDivisions, (int)louverCount, (int)louverWidth);
            return model.ToHypar();
        }

        private Model CreateBuilding(int verticalDivisions, int louverCount, double louverWidth)
        {
            var model = new Model();

            var box = new Box();
            model.AddElement(box);
            return model;
            
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

            var massExternal = Mass.WithBottomProfile(massPerimeter)
                                    .WithTopProfile(massPerimeter)
                                    .WithTopAtElevation(20);
            
            var massInternal = Mass.WithBottomProfile(offsetPerimeter)
                                    .WithTopProfile(offsetPerimeter)
                                    .WithTopAtElevation(20);

            var generateColumn = new Func<Line, Beam>((Line l) => {
                return Beam.AlongLine(l)
                            .WithProfile(columnProfile)
                            .OfMaterial(material);
            });

            var columns = massInternal.VerticalEdges()
                                        .AlongEachCreate<Beam>(generateColumn);
            model.AddElements(columns);

            var beams = CreateBeams(model, girderProfile, material, offsetPerimeter);
            model.AddElements(beams);
            
            /*
            var perimeters = new[]{offsetPerimeter,offsetPerimeter};
            var generateSlab = new Func<Polyline, Slab>((Polyline p) => {
                return Slab.WithinPerimeter(p);
            });
            
            var slabs = perimeters.WithinEachCreate<Slab>(generateSlab)
                                    .AtElevation(new[]{10.0, 20.0})
                                    .WithThickness(0.3)
                                    .OfMaterial(BuiltInMaterials.Concrete);
            model.AddElements(slabs);

            var createFaceGrid = new Func<Polyline, Grid>((Polyline p) => 
            {
                return ElementsFactory.CreateGrid()
                                        .WithinPerimeter(p)
                                        .WithUDivisions(verticalDivisions)
                                        .WithVDivisions(new[]{0.0, 0.4, 0.55, 1.0});
            });

            var panelProfile = Profiles.Rectangular(Vector3.Origin(), 0.05, 1.0, 0.0, 0.0);
            var panelMaterial = new Material("panel", 0.4f, 1.0f, 1.0f, 0.75f, 1.0f, 1.0f);
            var createGlazedPanel = new Func<Polyline, Panel>((Polyline p) => {
                return Panel.WithinPerimeter(p).OfMaterial(panelMaterial);
            });

            var opaquePanelMaterial = new Material("opaquePanel", 0.0f, 0.0f, 0.0f, 1.0f, 0.1f, 0.1f);
            var generateOpaquePanel = new Func<Polyline, Panel>((Polyline p) => {
                return Panel.WithinPerimeter(p).OfMaterial(opaquePanelMaterial);
            });

            var generateMullions = new Func<Polyline, IEnumerable<Beam>>((Polyline p) => {
                return p.Segments()
                        .AlongEachCreate<Beam>(l => {
                            return Beam.AlongLine(l); 
                        })
                        .WithProfile(panelProfile)
                        .WithUpAxis(p.Normal())
                        .OfMaterial(BuiltInMaterials.Steel);
            });

            var selectLouverProfile = new Func<Beam, Polyline>((Beam b) => {
                var louverProfile = Profiles.Rectangular(Vector3.Origin(), louverWidth, 0.1, 0.0, -0.75);
                return louverProfile;
            });

            var generateLouvers = new Func<Polyline, IEnumerable<Beam>>((Polyline p) => {
                var g = ElementsFactory.CreateGrid()
                            .WithinPerimeter(p)
                            .WithUDivisions(louverCount)
                            .WithVDivisions(1);

                var louvers = g.RowEdges().AlongEachCreate<Beam>(l => {
                    return Beam.AlongLine(l)
                            .WithProfile(selectLouverProfile)
                            .OfMaterial(BuiltInMaterials.Wood);
                });
                return louvers;
            });

            var grids = massExternal.Faces().WithinEachCreate<Grid>(createFaceGrid);

            foreach(var g in grids)
            {
                var panelElements = g.CellsInRow(0).WithinEachCreate<Panel>(createGlazedPanel);
                var panelElements2 = g.CellsInRow(2).WithinEachCreate<Panel>(createGlazedPanel);
                var opaquePanels = g.CellsInRow(1).WithinEachCreate<Panel>(generateOpaquePanel);
                var mullions = g.AllCells().WithinEachCreate<IEnumerable<Beam>>(generateMullions);
                var louvers = g.CellsInRow(2).WithinEachCreate<IEnumerable<Beam>>(generateLouvers);

                model.AddElements(panelElements);
                model.AddElements(panelElements2);
                model.AddElements(opaquePanels);
                foreach(var m in mullions)
                {
                    model.AddElements(m);
                }
                foreach(var l in louvers)
                {
                    model.AddElements(l);
                }
            }*/

            return model;
        }

        private IEnumerable<Beam> CreateBeams(Model model, Polyline profile, Material material, Polyline perimeter)
        {
            var results = new List<Beam>();
            foreach(var l in perimeter.Segments())
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
                results.Add(b1);
                results.Add(b2);
            }
            return results;
        }
    }
}
