using IFC;
using STEP;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Elements.Serialization.IFC
{
    /// <summary>
    /// Extension methods for writing elements to and from IFC.
    /// </summary>
    public static class IFCModelExtensions
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

            var slabs = ifcSlabs.Select(s => s.ToFloor(ifcVoids.Where(v=>v.RelatingBuildingElement == s).Select(v=>v.RelatedOpeningElement).Cast<IfcOpeningElement>()));
            var spaces = ifcSpaces.Select(sp => sp.ToSpace());
            var walls = ifcWalls.Select(w => w.ToWall(
                ifcVoids.Where(v=>v.RelatingBuildingElement == w).Select(v=>v.RelatedOpeningElement).Cast<IfcOpeningElement>()));
            var beams = ifcBeams.Select(b => b.ToBeam());
            var columns = ifcColumns.Select(c => c.ToColumn());

            var model = new Model();
            model.AddElements(slabs);
            model.AddElements(spaces);
            model.AddElements(walls);
            model.AddElements(beams);
            model.AddElements(columns);

            return model;
        }
        
        /// <summary>
        /// Write the model to IFC.
        /// </summary>
        /// <param name="model"></param>
        /// <param name="path">The path to the generated IFC STEP file.</param>
        internal static void ToIFC(Model model, string path)
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
            
            var styles = new Dictionary<string, IfcSurfaceStyle>();

            foreach(var e in model.AllEntitiesOfType<Element>())
            {
                try
                {
                    products.AddRange(e.ToIfcProducts(context, ifc, styles));
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