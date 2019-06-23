using Elements.Geometry.Interfaces;
using Hypar.Elements.Interfaces;
using IFC;
using STEP;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Elements.Serialization.IFC
{
    /// <summary>
    /// Extension methods for converting IFC types to Element types.
    /// </summary>
    public static partial class IFCExtensions
    {
        /// <summary>
        /// Load a model from IFC.
        /// </summary>
        /// <param name="path">The path to an IFC STEP file.</param>
        /// <param name="idsToConvert">An array of element ids to convert.</param>
        /// <returns>A model.</returns>
        internal static Model FromIFC(string path, IList<string> idsToConvert = null)
        {
            List<STEPError> errors;
            var ifcModel = new Document(path, out errors);
            foreach(var error in errors)
            {
                Console.WriteLine("***IFC ERROR***" + error.Message);
            }

            IEnumerable<IfcSlab> ifcSlabs = null;
            IEnumerable<IfcSpace> ifcSpaces = null;
            IEnumerable<IfcWallStandardCase> ifcWalls = null;
            IEnumerable<IfcBeam> ifcBeams = null;
            IEnumerable<IfcColumn> ifcColumns = null;
            IEnumerable<IfcRelVoidsElement> ifcVoids = null;
            IEnumerable<IfcRelAssociatesMaterial> ifcMaterials = null;

            if(idsToConvert != null && idsToConvert.Count > 0)
            {
                ifcSlabs = ifcModel.AllInstancesOfType<IfcSlab>().Where(i=>idsToConvert.Contains(i.GlobalId));
                ifcSpaces = ifcModel.AllInstancesOfType<IfcSpace>().Where(i=>idsToConvert.Contains(i.GlobalId));
                ifcWalls = ifcModel.AllInstancesOfType<IfcWallStandardCase>().Where(i=>idsToConvert.Contains(i.GlobalId));
                ifcBeams = ifcModel.AllInstancesOfType<IfcBeam>().Where(i=>idsToConvert.Contains(i.GlobalId));
                ifcColumns = ifcModel.AllInstancesOfType<IfcColumn>().Where(i=>idsToConvert.Contains(i.GlobalId));
                ifcVoids = ifcModel.AllInstancesOfType<IfcRelVoidsElement>().Where(i=>idsToConvert.Contains(i.GlobalId));
                ifcMaterials = ifcModel.AllInstancesOfType<IfcRelAssociatesMaterial>().Where(i=>idsToConvert.Contains(i.GlobalId));
            }
            else
            {
                ifcSlabs = ifcModel.AllInstancesOfType<IfcSlab>();
                ifcSpaces = ifcModel.AllInstancesOfType<IfcSpace>();
                ifcWalls = ifcModel.AllInstancesOfType<IfcWallStandardCase>();
                ifcBeams = ifcModel.AllInstancesOfType<IfcBeam>();
                ifcColumns = ifcModel.AllInstancesOfType<IfcColumn>();
                ifcVoids = ifcModel.AllInstancesOfType<IfcRelVoidsElement>();
                ifcMaterials = ifcModel.AllInstancesOfType<IfcRelAssociatesMaterial>();
            }

            var openings = new List<Opening>();
            foreach (var v in ifcVoids)
            {
                var element = v.RelatingBuildingElement;
                // var elementTransform = element.ObjectPlacement.ToTransform();
                var o = ((IfcOpeningElement)v.RelatedOpeningElement).ToOpening();
                openings.Add(o);
            }

            var wallType = new WallType("Default Wall", 1.0);

            var slabs = ifcSlabs.Select(s => s.ToFloor(ifcVoids.Where(v=>v.RelatingBuildingElement == s).Select(v=>v.RelatedOpeningElement).Cast<IfcOpeningElement>()));
            var spaces = ifcSpaces.Select(sp => sp.ToSpace());
            var walls = ifcWalls.Select(w => w.ToWall(
                ifcVoids.Where(v=>v.RelatingBuildingElement == w).Select(v=>v.RelatedOpeningElement).Cast<IfcOpeningElement>(),
                wallType));
            var beams = ifcBeams.Select(b => b.ToBeam());
            var columns = ifcColumns.Select(c => c.ToColumn());

            var model = new Model();
            model.AddElements(slabs);
            model.AddElements(spaces);
            model.AddElements(walls);
            model.AddElements(beams);
            model.AddElements(columns);
            // if (openings.Any())
            // {
            //     model.AddElements(openings);
            // }

            return model;
        }
        
        /// <summary>
        /// Write the model to IFC.
        /// </summary>
        /// <param name="model"></param>
        /// <param name="path">The path to the generated IFC STEP file.</param>
        public static void ToIFC(this Model model, string path)
        {
            var ifc = new Document("Elements", "Elements", Environment.UserName, 
                                    null, null, null, "Elements", null, null,
                                    null, null, null, null, null, null
                                    );

            var proj = ifc.AllInstancesOfType<IfcProject>().FirstOrDefault();

            // Add a site
            var site = new IfcSite(IfcGuid.ToIfcGuid(Guid.NewGuid()), null, IfcElementCompositionEnum.ELEMENT);
            var projAggregate = new IfcRelAggregates(IfcGuid.ToIfcGuid(Guid.NewGuid()), null, proj, new List<IfcObjectDefinition>{site});

            // Add building and building storey
            var building = new IfcBuilding(IfcGuid.ToIfcGuid(Guid.NewGuid()), null, IfcElementCompositionEnum.ELEMENT);
            var storey = new IfcBuildingStorey(IfcGuid.ToIfcGuid(Guid.NewGuid()), null, IfcElementCompositionEnum.ELEMENT);
            var aggregate = new IfcRelAggregates(IfcGuid.ToIfcGuid(Guid.NewGuid()), null, building, new List<IfcObjectDefinition>{storey});
            
            // Aggregate the building into the site
            var siteAggregate = new IfcRelAggregates(IfcGuid.ToIfcGuid(Guid.NewGuid()), null, site, new List<IfcObjectDefinition>{building});

            ifc.AddEntity(site);
            ifc.AddEntity(projAggregate);
            ifc.AddEntity(building);
            ifc.AddEntity(storey);
            ifc.AddEntity(aggregate);
            ifc.AddEntity(siteAggregate);

            var products = new List<IfcProduct>();
            var context = ifc.AllInstancesOfType<IfcGeometricRepresentationContext>().FirstOrDefault();
            
            foreach(var e in model.Elements.Values)
            {
                try
                {
                    IfcProductDefinitionShape shape = null;
                    var localPlacement = e.Transform.ToIfcLocalPlacement(ifc);
                    IfcGeometricRepresentationItem geom = null;

                    if(e is ISweepAlongCurve)
                    {
                        var sweep = (ISweepAlongCurve)e;
                        geom = sweep.ToIfcSurfaceCurveSweptAreaSolid(e.Transform, ifc);
                    }
                    else if(e is IExtrude)
                    {
                        var extrude = (IExtrude)e;
                        geom = extrude.ToIfcExtrudedAreaSolid(e.Transform, ifc);
                    }
                    else if(e is ILamina)
                    {
                        var lamina = (ILamina)e;
                        geom = lamina.ToIfcShellBasedSurfaceModel(e.Transform, ifc);
                    }
                    else
                    {
                        throw new Exception("Only IExtrude and ISweepAlongCurve representations are currently supported.");
                    }
                    
                    shape = ToIfcProductDefinitionShape(geom, context, ifc);

                    ifc.AddEntity(shape);
                    ifc.AddEntity(localPlacement);
                    ifc.AddEntity(geom);

                    var product = ConvertElementToIfcProduct(e, localPlacement, shape);
                    products.Add(product);
                    ifc.AddEntity(product);

                    // If the element has openings,
                    // Make opening relationships in
                    // the IfcElement.
                    if(e is IHasOpenings)
                    {
                        var openings = (IHasOpenings)e;

                        foreach(var o in openings.Openings)
                        {
                            var element = (IfcElement)products.Last();
                            var opening = o.ToIfcOpeningElement(context, ifc, localPlacement);
                            var voidRel = new IfcRelVoidsElement(IfcGuid.ToIfcGuid(Guid.NewGuid()), null, element, opening);
                            element.HasOpenings.Add(voidRel);
                            ifc.AddEntity(opening);
                            ifc.AddEntity(voidRel);
                        }
                    }
                }
                catch(Exception ex)
                {
                    Console.WriteLine($"There was an error writing an element of type {e.GetType()} to IFC: " + ex.Message);
                    Console.WriteLine(ex.StackTrace);
                    continue;
                }
            }

            var spatialRel = new IfcRelContainedInSpatialStructure(IfcGuid.ToIfcGuid(Guid.NewGuid()), null, products, storey);
            ifc.AddEntity(spatialRel);

            if(File.Exists(path))
            {
                File.Delete(path);
            }
            File.WriteAllText(path, ifc.ToSTEP(path));
        }
    }
}