using Elements.Geometry;
using glTFLoader.Schema;
using IFC;
using System;
using System.Collections.Generic;
using System.Text;

namespace Elements.Serialization.IFC.IFCToHypar.RepresentationsExtraction
{
    /// <summary>Extracts and keeps the material information from IfcRepresentationItems.</summary>
    internal class MaterialExtractor
    {
        /// <summary>A mapping from the material name to the material itself.</summary>
        public Dictionary<string, Material> MaterialByName { get; private set; }
        /// <summary>A mapping from the material Guid to the material itself.</summary>
        public Dictionary<Guid, Material> MaterialByGuid { get; private set; }

        /// <summary>
        /// Create a MeterialExtractor that keeps information about materials of <paramref name="styledItems"/>.
        /// </summary>
        /// <param name="styledItems">A collection of IfcStyledItems that contain material information.</param>
        public MaterialExtractor(IEnumerable<IfcStyledItem> styledItems)
        {
            MaterialByName = new Dictionary<string, Material>();
            MaterialByGuid = new Dictionary<Guid, Material>();

            // Get a unique set of materials that are used for all 
            // styled items. While we're doing that we also build
            // a map of the items that are being styled and their materials.

            // TODO: Some of our test models use IfcMaterialList, which is 
            // deprecated in IFC4. In the AC-Smiley model, for example, the
            // windows use a representation with a material list that has glass
            // for the window and wood for the frame. I think styled items is 
            // the modern way to do this, so we shouldn't support material lists.
            foreach (var styledItem in styledItems)
            {
                var item = styledItem.Item; // The representation item that is styled.

                if (item == null)
                {
                    continue;
                }

                foreach (IfcStyleAssignmentSelect style in styledItem.Styles)
                {
                    if (style.Choice is IfcPresentationStyle) {
                        // See https://standards.buildingsmart.org/IFC/DEV/IFC4_2/FINAL/HTML/schema/ifcpresentationappearanceresource/lexical/ifcpresentationstyle.htm
                        continue;
                    }

                    if (!(style.Choice is IfcPresentationStyleAssignment styleAssign))
                    {
                        continue;
                    }

                    foreach (IfcPresentationStyleSelect presentationStyle in styleAssign.Styles)
                    {
                        if (!(presentationStyle.Choice is IfcSurfaceStyle surfaceStyle))
                        {
                            continue;
                        }

                        foreach (IfcSurfaceStyleElementSelect styleElement in surfaceStyle.Styles)
                        {
                            if (styleElement.Choice is IfcSurfaceStyleRendering rendering)
                            {
                                var transparency = (IfcRatioMeasure)rendering.Transparency;
                                var color = rendering.SurfaceColour.ToColor(1.0 - transparency);
                                var name = surfaceStyle.Name;

                                Material material;
                                if (!MaterialByName.ContainsKey(name))
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
                                    MaterialByName.Add(material.Name, material);
                                }
                                else
                                {
                                    material = MaterialByName[name];
                                }

                                MaterialByGuid.Add(item.Id, material);
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Extracts Material from <paramref name="repItem"/>.
        /// </summary>
        /// <param name="repItem">A representation item, from which the Material will be extracted.</param>
        public Material ExtractMaterial(IfcRepresentationItem repItem)
        {
            return MaterialByGuid.ContainsKey(repItem.Id) ? MaterialByGuid[repItem.Id] : null;
        }
    }
}
