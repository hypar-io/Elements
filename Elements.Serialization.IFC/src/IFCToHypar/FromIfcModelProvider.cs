using Elements.Geometry;
using Elements.Interfaces;
using Elements.Serialization.IFC.IFCToHypar.Converters;
using Elements.Serialization.IFC.IFCToHypar.RepresentationsExtraction;
using Elements.Serialization.IFC.IFCToHypar.RepresentationsExtraction.Parsers;
using IFC;
using STEP;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace Elements.Serialization.IFC.IFCToHypar
{
    internal class FromIfcModelProvider
    {
        public Model Model { get; private set; }

        private readonly IFromIfcProductConverter _fromIfcToElementsConverter;
        private readonly IfcRepresentationDataExtractor _representationDataExtractor;

        private List<IfcProduct> _ifcProducts;
        private List<IfcRelationship> _ifcRelationships;

        private readonly Dictionary<Element, IfcProduct> _elementToIfcProduct;
        private readonly Dictionary<Guid, GeometricElement> _elementDefinitions;
        private readonly List<string> _constructionErrors;

        private MaterialExtractor _materialExtractor;

        public FromIfcModelProvider(string path,
                                    IList<string> idsToConvert = null,
                                    IFromIfcProductConverter fromIfcConverter = null,
                                    IfcRepresentationDataExtractor representationExtractor = null)
        {
            _constructionErrors = new List<string>();
            ExtractIfcProducts(path, idsToConvert);
            _elementToIfcProduct = new Dictionary<Element, IfcProduct>();
            _elementDefinitions = new Dictionary<Guid, GeometricElement>();

            _representationDataExtractor = representationExtractor ?? GetStandardRepresentationDataExtractor(_materialExtractor);
            _fromIfcToElementsConverter = fromIfcConverter ?? GetStandardFromIfcConverter();

            var elements = GetElementsFromIfcProducts();
            HandleRelationships(elements);

            Model = new Model();
            Model.AddElements(elements);
        }

        public List<string> GetConstructionErrors()
        {
            return _constructionErrors;
        }

        private List<Element> GetElementsFromIfcProducts()
        {
            var elements = new List<Element>();

            foreach (var product in _ifcProducts)
            {
                // TODO: Parameter List<string> constructionErrors and exceptions handling are both used to catch construction errors.
                // Refactor the code so the only one of these approaches is used.
                try
                {
                    var element = ConvertIfcProductToElement(product);

                    if (element == null)
                    {
                        continue;
                    }

                    elements.Add(element);
                    _elementToIfcProduct.Add(element, product);
                }
                catch (Exception ex)
                {
                    _constructionErrors.Add(ex.Message);
                    continue;
                }
            }

            return elements;
        }

        private Element ConvertIfcProductToElement(IfcProduct product)
        {
            var repData = _representationDataExtractor.ExtractRepresentationData(product);

            if (repData == null)
            {
                return null;
            }

            if (repData.MappingInfo != null)
            {
                // TODO: Handle IfcMappedItem
                // - Idea: Make Representations an Element, so that they can be shared.
                // - Idea: Make PropertySet an Element. PropertySets can store type properties.
                GeometricElement definition;
                if (_elementDefinitions.ContainsKey(repData.MappingInfo.MappingId))
                {
                    definition = _elementDefinitions[repData.MappingInfo.MappingId];
                }
                else
                {
                    definition = _fromIfcToElementsConverter.ConvertToElement(product, repData, _constructionErrors);

                    if (definition == null)
                    {
                        //Debug.Assert(false, "Cannot convert definition to GeometricElement.");
                        return null;
                    }

                    definition.IsElementDefinition = true;
                    _elementDefinitions.Add(repData.MappingInfo.MappingId, definition);
                    //definition.SkipCSGUnion = true;
                }

                // The cartesian transform needs to be applied 
                // before the element transformation because it
                // may contain scale and rotation.
                var instanceTransform = new Transform(repData.MappingInfo.MappingTransform);
                instanceTransform.Concatenate(repData.Transform);
                var instance = definition.CreateInstance(instanceTransform, product.Name ?? "");
                return instance;
            }

            var element = _fromIfcToElementsConverter.ConvertToElement(product, repData, _constructionErrors);
            return element;
        }

        private void HandleRelationships(List<Element> elements)
        {
            var elementsWithOpenings = elements.Where(element => element is IHasOpenings).ToList();
            var ifcOpenings = _ifcRelationships.OfType<IfcRelVoidsElement>().ToList();

            foreach (var elementWithOpenings in elementsWithOpenings)
            {
                var ifcElement = _elementToIfcProduct[elementWithOpenings];
                var ifcOpeningElements = ifcOpenings.Where(v => v.RelatingBuildingElement == ifcElement)
                    .Select(v => v.RelatedOpeningElement).Cast<IfcOpeningElement>().ToList();
                var openings = ifcOpeningElements.SelectMany(io => io.ToOpening()).ToList();

                var openingsOwner = (IHasOpenings) elementWithOpenings;
                openingsOwner.Openings.AddRange(openings);
            }
        }

        private void ExtractIfcProducts(string path, IList<string> idsToConvert = null)
        {
            var ifcModel = new Document(path, out List<STEPError> errors);
            // TODO: IfcOpeningElement is prohibited because openings are handled during
            // relationships processing.
            // It is possible that there are more IfcElement types that should be excluded.
            var prohibitedElements = ifcModel.AllInstancesOfType<IfcOpeningElement>();

            if (idsToConvert != null && idsToConvert.Count > 0)
            {
                _ifcProducts = ifcModel.AllInstancesDerivedFromType<IfcProduct>()
                    .Where(i => idsToConvert.Contains(i.GlobalId)).Except(prohibitedElements).ToList();
                _ifcRelationships = ifcModel.AllInstancesDerivedFromType<IfcRelationship>()
                    .Where(i => idsToConvert.Contains(i.GlobalId)).ToList();
            }
            else
            {
                _ifcProducts = ifcModel.AllInstancesDerivedFromType<IfcProduct>().Except(prohibitedElements).ToList();
                _ifcRelationships = ifcModel.AllInstancesDerivedFromType<IfcRelationship>().ToList();
            }

            var styledItems = ifcModel.AllInstancesOfType<IfcStyledItem>().ToList();
            _materialExtractor = new MaterialExtractor(styledItems);
        }

        private static IFromIfcProductConverter GetStandardFromIfcConverter()
        {
            var converters = new List<IFromIfcProductConverter>()
            {
                new FromIfcFloorConverter(),
                new FromIfcSpaceConverter(),
                new FromIfcWallConverter(),
                new FromIfcDoorConverter(),
                new FromIfcBeamConverter(),
                new FromIfcColumnConverter(),
                new FromIfcSiteConverter()
            };

            var defaultConverter = new FromIfcElementConverter();

            return new CompositeFromIfcProductConverter(converters, defaultConverter);
        }

        private static IfcRepresentationDataExtractor GetStandardRepresentationDataExtractor(MaterialExtractor materialExtractor)
        {
            IfcRepresentationDataExtractor extractor = new IfcRepresentationDataExtractor(materialExtractor);

            extractor.AddRepresentationParser(new IfcFacetedBrepParser());
            extractor.AddRepresentationParser(new IfcExtrudedAreaSolidParser());
            extractor.AddRepresentationParser(new IfcMappedItemParser(extractor));
            extractor.AddRepresentationParser(new IfcBooleanClippingResultParser());

            return extractor;
        }
    }
}
