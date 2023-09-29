using Elements.Geometry;
using Elements.Serialization.IFC.IFCToHypar.RepresentationsExtraction;
using IFC;
using System;
using System.Collections.Generic;
using System.Text;

namespace Elements.Serialization.IFC.IFCToHypar.Converters
{
    internal class FromIfcElementConverter : IFromIfcProductConverter
    {
        private readonly Dictionary<Guid, GeometricElement> _elementDefinitions;

        public FromIfcElementConverter()
        {
            _elementDefinitions = new Dictionary<Guid, GeometricElement>();
        }

        public Element ConvertToElement(IfcProduct ifcProduct, RepresentationData repData, List<string> constructionErrors)
        {
            if (!(ifcProduct is IfcElement ifcElement))
            {
                return null;
            }

            if (repData == null)
            {
                constructionErrors.Add($"#{ifcElement.StepId}: There was no representation for an element of type {ifcElement.GetType()}.");
                return null;
            }

            var mappingInfo = repData.MappingInfo;

            if (mappingInfo == null)
            {
                if (repData.SolidOperations.Count == 0)
                {
                    constructionErrors.Add($"#{ifcElement.StepId}: {ifcElement.GetType().Name} did not have any solid operations in its representation.");
                    return null;
                }

                // TODO: Handle IfcMappedItem
                // - Idea: Make Representations an Element, so that they can be shared.
                // - Idea: Make PropertySet an Element. PropertySets can store type properties.
                var geom = new GeometricElement(repData.Transform,
                                                repData.Material ?? BuiltInMaterials.Default,
                                                new Representation(repData.SolidOperations),
                                                false,
                                                IfcGuid.FromIfcGUID(ifcElement.GlobalId),
                                                ifcElement.Name);

                // geom.Representation.SkipCSGUnion = true;
                return geom;
            }

            GeometricElement definition;
            if (_elementDefinitions.ContainsKey(mappingInfo.MappingId))
            {
                definition = _elementDefinitions[mappingInfo.MappingId];
            }
            else
            {
                definition = new GeometricElement(repData.Transform,
                                            repData.Material ?? BuiltInMaterials.Default,
                                            new Representation(repData.SolidOperations),
                                            true,
                                            IfcGuid.FromIfcGUID(ifcElement.GlobalId),
                                            ifcElement.Name);
                _elementDefinitions.Add(mappingInfo.MappingId, definition);

                //definition.SkipCSGUnion = true;
            }

            // The cartesian transform needs to be applied 
            // before the element transformation because it
            // may contain scale and rotation.
            var instanceTransform = new Transform(mappingInfo.MappingTransform);
            instanceTransform.Concatenate(repData.Transform);
            var instance = definition.CreateInstance(instanceTransform, ifcProduct.Name ?? "");
            return instance;
        }

        public bool Matches(IfcProduct ifcProduct)
        {
            return ifcProduct is IfcElement;
        }
    }
}
