using Elements.Geometry;
using Elements.Interfaces;
using Elements.Serialization.IFC.IFCToHypar.Converters;
using Elements.Serialization.IFC.IFCToHypar.RepresentationsExtraction;
using Elements.Serialization.IFC.IFCToHypar.RepresentationsExtraction.Parsers;
using IFC;
using STEP;
using System;
using System.Collections.Generic;
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
        private readonly List<string> _constructionErrors;

        private MaterialExtractor _materialExtractor;

        public FromIfcModelProvider(string path, IList<string> idsToConvert = null, IFromIfcProductConverter fromIfcConverter = null, IfcRepresentationDataExtractor representationExtractor = null)
        {
            _constructionErrors = new List<string>();
            ExtractIfcProducts(path, idsToConvert);
            _elementToIfcProduct = new Dictionary<Element, IfcProduct>();

            _representationDataExtractor = representationExtractor ?? GetStandardRepresentationDataExtractor(_materialExtractor);
            _fromIfcToElementsConverter = fromIfcConverter ?? GetStandardFromIfcConverter(_materialExtractor);

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
                    var repData = _representationDataExtractor.ExtractRepresentationData(product);

                    if (repData == null)
                    {
                        continue;
                    }

                    var element = _fromIfcToElementsConverter.ConvertToElement(product, repData, _constructionErrors);

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

        private void HandleRelationships(List<Element> elements)
        {
            var elementsWithOpenings = elements.Where(element => element is IHasOpenings);
            var ifcOpenings = _ifcRelationships.OfType<IfcRelVoidsElement>();

            foreach (var elementWithOpenings in elementsWithOpenings)
            {
                var ifcElement = _elementToIfcProduct[elementWithOpenings];
                var openings = ifcOpenings.Where(v => v.RelatingBuildingElement == ifcElement).Select(v => v.RelatedOpeningElement).Cast<IfcOpeningElement>();
                ((IHasOpenings)elementWithOpenings).Openings.AddRange(openings.Select(io => io.ToOpening()));
            }
        }

        private void ExtractIfcProducts(string path, IList<string> idsToConvert = null)
        {
            var ifcModel = new Document(path, out List<STEPError> errors);

            if (idsToConvert != null && idsToConvert.Count > 0)
            {
                _ifcProducts = ifcModel.AllInstancesDerivedFromType<IfcProduct>().Where(i => idsToConvert.Contains(i.GlobalId)).ToList();
                _ifcRelationships = ifcModel.AllInstancesDerivedFromType<IfcRelationship>().Where(i => idsToConvert.Contains(i.GlobalId)).ToList();
            }
            else
            {
                _ifcProducts = ifcModel.AllInstancesDerivedFromType<IfcProduct>().ToList();
                _ifcRelationships = ifcModel.AllInstancesDerivedFromType<IfcRelationship>().ToList();
            }

            var styledItems = ifcModel.AllInstancesOfType<IfcStyledItem>().ToList();
            _materialExtractor = new MaterialExtractor(styledItems);
        }

        private static IFromIfcProductConverter GetStandardFromIfcConverter(MaterialExtractor materialExtractor)
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

            var standardConverter = new FromIfcElementConverter();

            return new CompositeFromIfcProductConverter(converters, standardConverter);
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
