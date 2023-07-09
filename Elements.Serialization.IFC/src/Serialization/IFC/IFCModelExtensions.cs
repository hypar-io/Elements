using Elements.Analysis;
using Elements.Geometry;
using IFC;
using STEP;
using System;
using System.Collections.Generic;
using System.Diagnostics;
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
        /// Construct a model from an IFC.
        /// </summary>
        /// <param name="path">The path to an IFC Express file.</param>
        /// <param name="constructionErrors">Error messages which ocurred during model construction.</param>
        public static Model FromIFC(string path, out List<string> constructionErrors)
        {
            List<STEPError> errors;
            var ifcModel = new Document(path, out errors);
            foreach (var error in errors)
            {
                Console.WriteLine("***IFC ERROR***" + error.Message);
            }

            var buildingElements = ifcModel.AllInstancesDerivedFromType<IfcElement>();

            var relVoids = ifcModel.AllInstancesOfType<IfcRelVoidsElement>();

            var model = new Model();

            constructionErrors = new List<string>();

            Dictionary<Guid, BuildingElement> elementDefinitions = new Dictionary<Guid, BuildingElement>();

            // var types = ifcModel.AllInstancesDerivedFromType<IfcBuildingElementType>();
            // var relTypes = ifcModel.AllInstancesOfType<IfcRelDefinesByType>();

            var materials = new Dictionary<string, Material>();
            var repMaterialMap = new Dictionary<Guid, Material>();

            // Get a unique set of materials that are used for all 
            // styled items. While we're doing that we also build
            // a map of the items that are being styled and their materials.

            // TODO: Some of our test models use IfcMaterialList, which is 
            // deprecated in IFC4. In the AC-Smiley model, for example, the
            // windows use a representation with a material list that has glass
            // for the window and wood for the frame. I think styled items is 
            // the modern way to do this, so we shouldn't support material lists.

            var styledItems = ifcModel.AllInstancesOfType<IfcStyledItem>();
            foreach (var styledItem in styledItems)
            {
                var item = styledItem.Item; // The representation item that is styled.
                if (item == null)
                {
                    continue;
                }
                foreach (IfcStyleAssignmentSelect style in styledItem.Styles)
                {
                    if (style.Choice is IfcPresentationStyleAssignment styleAssign)
                    {
                        foreach (IfcPresentationStyleSelect presentationStyle in styleAssign.Styles)
                        {
                            if (presentationStyle.Choice is IfcSurfaceStyle surfaceStyle)
                            {
                                foreach (IfcSurfaceStyleElementSelect styleElement in surfaceStyle.Styles)
                                {
                                    if (styleElement.Choice is IfcSurfaceStyleRendering rendering)
                                    {
                                        var transparency = (IfcRatioMeasure)rendering.Transparency;
                                        var color = rendering.SurfaceColour.ToColor(1.0 - transparency);
                                        var name = surfaceStyle.Name;
                                        Material material = null;
                                        if (!materials.ContainsKey(name))
                                        {
                                            material = new Material(color,
                                                                    0.4,
                                                                    0.4,
                                                                    false,
                                                                    null,
                                                                    false,
                                                                    true,
                                                                    null,
                                                                    true,
                                                                    null,
                                                                    0.0,
                                                                    false,
                                                                    surfaceStyle.Id,
                                                                    surfaceStyle.Name);
                                            materials.Add(material.Name, material);
                                        }
                                        else
                                        {
                                            material = materials[name];
                                        }
                                        repMaterialMap.Add(item.Id, material);
                                    }
                                }
                            }
                        }
                    }
                    else if (style.Choice is IfcPresentationStyle)
                    {
                        // See https://standards.buildingsmart.org/IFC/DEV/IFC4_2/FINAL/HTML/schema/ifcpresentationappearanceresource/lexical/ifcpresentationstyle.htm
                    }
                }
            }

            var sites = ifcModel.AllInstancesOfType<IfcSite>();
            foreach (var site in sites)
            {
                var transform = site.ObjectPlacement.ToTransform();
                var rep = site.GetRepresentationFromProduct(model,
                                                            constructionErrors,
                                                            repMaterialMap,
                                                            out Transform mapTransform,
                                                            out Guid mapId,
                                                            out Material materialHint);
                if (rep == null)
                {
                    continue;
                }
                var geom = new GeometricElement(transform,
                                                        materialHint ?? BuiltInMaterials.Default,
                                                        rep,
                                                        false,
                                                        IfcGuid.FromIfcGUID(site.GlobalId),
                                                        site.Name);
                model.AddElement(geom);
            }

            foreach (var buildingElement in buildingElements)
            {
                try
                {
                    var transform = new Transform();
                    transform.Concatenate(buildingElement.ObjectPlacement.ToTransform());

                    // Check if the building element is contained in a building storey
                    foreach (var cis in buildingElement.ContainedInStructure)
                    {
                        transform.Concatenate(cis.RelatingStructure.ObjectPlacement.ToTransform());
                    }

                    var rep = buildingElement.GetRepresentationFromProduct(model,
                                                                           constructionErrors,
                                                                           repMaterialMap,
                                                                           out Transform mapTransform,
                                                                           out Guid mapId,
                                                                           out Material materialHint);

                    if (rep == null)
                    {
                        constructionErrors.Add($"There was no representation for an element of type {buildingElement.GetType()}.");
                        continue;
                    }

                    if (mapTransform != null)
                    {
                        BuildingElement definition = null;
                        if (elementDefinitions.ContainsKey(mapId))
                        {
                            definition = elementDefinitions[mapId];
                        }
                        else
                        {
                            definition = new BuildingElement(transform,
                                                        materialHint ?? BuiltInMaterials.Default,
                                                        rep,
                                                        true,
                                                        IfcGuid.FromIfcGUID(buildingElement.GlobalId),
                                                        buildingElement.Name);
                            elementDefinitions.Add(mapId, definition);

                            definition.Representation.SkipCSGUnion = true;
                        }

                        // The cartesian transform needs to be applied 
                        // before the element transformation because it
                        // may contain scale and rotation.
                        var instanceTransform = new Transform(mapTransform);
                        instanceTransform.Concatenate(transform);
                        var instance = definition.CreateInstance(instanceTransform, "test");

                        model.AddElement(instance);
                    }
                    else
                    {
                        if (rep.SolidOperations.Count == 0)
                        {
                            constructionErrors.Add($"{buildingElement.GetType().Name} did not have any solid operations in its representation.");
                            continue;
                        }

                        // TODO: Handle IfcMappedItem
                        // - Idea: Make Representations an Element, so that they can be shared.
                        // - Idea: Make PropertySet an Element. PropertySets can store type properties.
                        var geom = new BuildingElement(transform,
                                                        materialHint ?? BuiltInMaterials.Default,
                                                        rep,
                                                        false,
                                                        IfcGuid.FromIfcGUID(buildingElement.GlobalId),
                                                        buildingElement.Name);

                        // geom.Representation.SkipCSGUnion = true;

                        var voids = relVoids.Where(v => v.RelatingBuildingElement == buildingElement).Select(v => v.RelatedOpeningElement).Cast<IfcOpeningElement>();
                        foreach (var v in voids)
                        {
                            var opening = v.ToOpening();
                            geom.Openings.Add(opening);
                        }
                        model.AddElement(geom);
                    }
                }
                catch (Exception ex)
                {
                    constructionErrors.Add(ex.Message);
                }
            }
            return model;
        }

        /// <summary>
        /// Write the model to IFC.
        /// </summary>
        /// <param name="model"></param>
        /// <param name="path">The path to the generated IFC STEP file.</param>
        /// <param name="updateElementsRepresentation">Indicates whether UpdateRepresentation should be called for all elements.</param>
        public static void ToIFC(this Model model, string path, bool updateElementsRepresentation = true)
        {
            var ifc = new Document("Elements", "Elements", Environment.UserName,
                                    null, null, null, "Elements", null, null,
                                    null, null, null, null, null, null
                                    );

            var proj = ifc.AllInstancesOfType<IfcProject>().FirstOrDefault();

            // Add a site
            var site = new IfcSite(IfcGuid.ToIfcGuid(Guid.NewGuid()),
                                   null,
                                   "Hypar Site",
                                   "The default site generated by Hypar",
                                   null,
                                   null,
                                   null,
                                   null,
                                   IfcElementCompositionEnum.ELEMENT,
                                   new IfcCompoundPlaneAngleMeasure(new List<int> { 0, 0, 0 }),
                                   new IfcCompoundPlaneAngleMeasure(new List<int> { 0, 0, 0 }),
                                   0,
                                   null,
                                   null);
            var projAggregate = new IfcRelAggregates(IfcGuid.ToIfcGuid(Guid.NewGuid()), proj, new List<IfcObjectDefinition> { site });

            // Add building and building storey
            var building = new IfcBuilding(IfcGuid.ToIfcGuid(Guid.NewGuid()),
                                           null,
                                           "Default Building",
                                           "The default building generated by Hypar.",
                                           null,
                                           null,
                                           null,
                                           null,
                                           IfcElementCompositionEnum.ELEMENT,
                                           0,
                                           0,
                                           null);
            var storey = new IfcBuildingStorey(IfcGuid.ToIfcGuid(Guid.NewGuid()),
                                               null,
                                               "Default Storey",
                                               "The default storey generated by Hypar",
                                               null,
                                               null,
                                               null,
                                               null,
                                               IfcElementCompositionEnum.ELEMENT,
                                               0);
            var aggregate = new IfcRelAggregates(IfcGuid.ToIfcGuid(Guid.NewGuid()), building, new List<IfcObjectDefinition> { storey });

            // Aggregate the building into the site
            var siteAggregate = new IfcRelAggregates(IfcGuid.ToIfcGuid(Guid.NewGuid()), site, new List<IfcObjectDefinition> { building });

            ifc.AddEntity(site);
            ifc.AddEntity(projAggregate);
            ifc.AddEntity(building);
            ifc.AddEntity(storey);
            ifc.AddEntity(aggregate);
            ifc.AddEntity(siteAggregate);

            var products = new List<IfcProduct>();
            var context = ifc.AllInstancesOfType<IfcGeometricRepresentationContext>().FirstOrDefault();

            // IfcRelAssociatesMaterial
            // IfcMaterialDefinitionRepresentation
            // https://forums.buildingsmart.org/t/where-and-how-will-my-colors-be-saved-in-ifc/1806/12
            var styleAssignments = new Dictionary<Guid, List<IfcStyleAssignmentSelect>>();

            var white = Colors.White.ToIfcColourRgb();
            ifc.AddEntity(white);

            // TODO: Fix color support in all applications.
            // https://forums.buildingsmart.org/t/why-is-it-so-difficult-to-get-colors-to-show-up/2312/12
            foreach (var m in model.AllElementsOfType<Material>())
            {
                var material = new IfcMaterial(m.Name, null, "Hypar");
                ifc.AddEntity(material);

                var color = m.Color.ToIfcColourRgb();
                ifc.AddEntity(color);

                var transparency = new IfcNormalisedRatioMeasure(1.0 - m.Color.Alpha);

                var shading = new IfcSurfaceStyleShading(color, transparency);
                ifc.AddEntity(shading);

                var styles = new List<IfcSurfaceStyleElementSelect>{
                    new IfcSurfaceStyleElementSelect(shading),
                };
                var surfaceStyle = new IfcSurfaceStyle(material.Name, IfcSurfaceSide.BOTH, styles);
                ifc.AddEntity(surfaceStyle);

                var styleAssign = new IfcStyleAssignmentSelect(surfaceStyle);
                var assignments = new List<IfcStyleAssignmentSelect>() { styleAssign };
                styleAssignments.Add(m.Id, assignments);
            }


            foreach (var e in model.Elements.Values.Where(e =>
            {
                var t = e.GetType();
                return ((e is GeometricElement &&
                        !((GeometricElement)e).IsElementDefinition) || e is ElementInstance) &&
                        t != typeof(ModelCurve) &&
                        t != typeof(ModelPoints) &&
                        t != typeof(AnalysisMesh);
            }))
            {
                try
                {
                    products.AddRange(e.ToIfcProducts(context, ifc, styleAssignments, updateElementsRepresentation));
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"There was an error writing an element of type {e.GetType()} to IFC: " + ex.Message);
                    Debug.WriteLine(ex.StackTrace);
                    continue;
                }
            }

            var spatialRel = new IfcRelContainedInSpatialStructure(IfcGuid.ToIfcGuid(Guid.NewGuid()), products, storey);
            ifc.AddEntity(spatialRel);

            if (File.Exists(path))
            {
                File.Delete(path);
            }
            File.WriteAllText(path, ifc.ToSTEP());
        }
    }
}