using Elements.Geometry;
using IFC;
using System;
using System.Collections.Generic;
using System.Text;

namespace Elements.Serialization.IFC.IFCToHypar.Converters
{
    internal class IfcBuildingElementToElementConverter : IIfcProductToElementConverter
    {
        private readonly Dictionary<Guid, GeometricElement> _elementDefinitions;
        private readonly Dictionary<Guid, Material> _repMaterialMap;

        public IfcBuildingElementToElementConverter()
        {
            _elementDefinitions = new Dictionary<Guid, GeometricElement>();
            _repMaterialMap = new Dictionary<Guid, Material>();
        }

        public Element ConvertToElement(IfcProduct ifcProduct, List<string> constructionErrors)
        {
            if (!(ifcProduct is IfcBuildingElement buildingElement))
            {
                return null;
            }

            var transform = new Transform();
            transform.Concatenate(buildingElement.ObjectPlacement.ToTransform());

            // Check if the building element is contained in a building storey
            foreach (var cis in buildingElement.ContainedInStructure)
            {
                transform.Concatenate(cis.RelatingStructure.ObjectPlacement.ToTransform());
            }

            var rep = buildingElement.GetRepresentationFromProduct(constructionErrors,
                                                                   _repMaterialMap,
                                                                   out Transform mapTransform,
                                                                   out Guid mapId,
                                                                   out Material materialHint);

            if (rep == null)
            {
                constructionErrors.Add($"#{buildingElement.StepId}: There was no representation for an element of type {buildingElement.GetType()}.");
                return null;
            }

            if (mapTransform != null)
            {
                GeometricElement definition;
                if (_elementDefinitions.ContainsKey(mapId))
                {
                    definition = _elementDefinitions[mapId];
                }
                else
                {
                    definition = new GeometricElement(transform,
                                                materialHint ?? BuiltInMaterials.Default,
                                                rep,
                                                true,
                                                IfcGuid.FromIfcGUID(buildingElement.GlobalId),
                                                buildingElement.Name);
                    _elementDefinitions.Add(mapId, definition);

                    //definition.SkipCSGUnion = true;
                }

                // The cartesian transform needs to be applied 
                // before the element transformation because it
                // may contain scale and rotation.
                var instanceTransform = new Transform(mapTransform);
                instanceTransform.Concatenate(transform);
                var instance = definition.CreateInstance(instanceTransform, "test");
                return instance;
            }
            else
            {
                if (rep.SolidOperations.Count == 0)
                {
                    constructionErrors.Add($"#{buildingElement.StepId}: {buildingElement.GetType().Name} did not have any solid operations in its representation.");
                    return null;
                }

                // TODO: Handle IfcMappedItem
                // - Idea: Make Representations an Element, so that they can be shared.
                // - Idea: Make PropertySet an Element. PropertySets can store type properties.
                var geom = new GeometricElement(transform,
                                                materialHint ?? BuiltInMaterials.Default,
                                                rep,
                                                false,
                                                IfcGuid.FromIfcGUID(buildingElement.GlobalId),
                                                buildingElement.Name);

                // geom.Representation.SkipCSGUnion = true;
                return geom;
            }
        }

        public bool Matches(IfcProduct ifcProduct)
        {
            return ifcProduct is IfcBuildingElement;
        }
    }
}
