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
    /// <summary>Provides a Model, converted from IFC.</summary>
    internal class FromIfcModelProvider
    {
        /// <summary>The Model, converted from IFC.</summary>
        public Model Model { get; private set; }

        private readonly IFromIfcProductConverter _fromIfcToElementsConverter;
        private readonly IfcRepresentationDataExtractor _representationDataExtractor;

        private readonly List<IfcProduct> _ifcProducts;
        private readonly List<IfcRelationship> _ifcRelationships;

        private readonly Dictionary<Element, IfcProduct> _elementToIfcProduct;
        private readonly List<string> _constructionErrors;

        private readonly MaterialExtractor _materialExtractor;

        /// <summary>
        /// Create FromIfcModelProvider that provides a Model, build from data, extracted from IFC file.
        /// </summary>
        /// <param name="path">A path to IFC file.</param>
        /// <param name="idsToConvert">Only IfcProducts with these ids will be converted.</param>
        /// <param name="fromIfcConverter">An object that converts IfcProducts to GeometricElements. 
        /// If null, a fallback default converter will be created.</param>
        /// <param name="representationExtractor">An object that extracts RepresentationData from IfcRepresentations
        /// of IfcProduct. If null, a fallback default extractor will be created.</param>
        public FromIfcModelProvider(string path,
                                    IList<string> idsToConvert = null,
                                    IFromIfcProductConverter fromIfcConverter = null,
                                    IfcRepresentationDataExtractor representationExtractor = null)
        {
            _constructionErrors = new List<string>();

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

            _elementToIfcProduct = new Dictionary<Element, IfcProduct>();

            _representationDataExtractor = representationExtractor ?? GetDefaultRepresentationDataExtractor(_materialExtractor);
            _fromIfcToElementsConverter = fromIfcConverter ?? GetDefaultFromIfcConverter();

            var elements = GetElementsFromIfcProducts();
            HandleRelationships(elements);

            Model = new Model();
            Model.AddElements(elements);
        }

        /// <summary>
        /// Returns the list of construction errors that appeared during the conversion.
        /// </summary>
        public List<string> GetConstructionErrors()
        {
            return _constructionErrors;
        }

        /// <summary>
        /// Converts the extracted IfcProducts into Elements.
        /// </summary>
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
                }
            }

            return elements;
        }

        /// <summary>
        /// Converts <paramref name="product"/> into Element.
        /// </summary>
        /// <param name="product">IfcProduct that will be converted into an Element.</param>
        private Element ConvertIfcProductToElement(IfcProduct product)
        {
            // Extract RepresentationData from IfcRepresentations of IfcProduct.
            var repData = _representationDataExtractor.ExtractRepresentationData(product);

            if (repData == null)
            {
                return null;
            }

            // If the product doesn't have an IfcMappedItem representation, it will be converted to
            // a GeometricElement.
            var element = _fromIfcToElementsConverter.ConvertToElement(product, repData, _constructionErrors);
            return element;
        }

        /// <summary>
        /// Apply the extracted relationships to the converted Elements.
        /// </summary>
        private void HandleRelationships(List<Element> elements)
        {
            var elementsWithOpenings = elements.Where(element => element is IHasOpenings).ToList();
            var ifcOpenings = _ifcRelationships.OfType<IfcRelVoidsElement>().ToList();

            // Convert IfcOpeningElements into Openings and attach them to the corresponding
            // converted Elements.
            foreach (var elementWithOpenings in elementsWithOpenings)
            {
                var ifcElement = _elementToIfcProduct[elementWithOpenings];
                var openings = ifcOpenings.Where(v => v.RelatingBuildingElement == ifcElement)
                    .Select(v => v.RelatedOpeningElement).Cast<IfcOpeningElement>()
                    .SelectMany(io => io.ToOpenings());

                var openingsOwner = (IHasOpenings) elementWithOpenings;
                openingsOwner.Openings.AddRange(openings);
            }
        }

        /// <summary>
        /// Create the default IFromIfcProductConverter. It will be used, if 
        /// IFromIfcProductConverter is not specified in the constructor.
        /// </summary>
        private static IFromIfcProductConverter GetDefaultFromIfcConverter()
        {
            var converters = new List<IFromIfcProductConverter>()
            {
                new FromIfcFloorConverter(),
                new FromIfcSpaceConverter(),
                new FromIfcWallConverter(),
                new FromIfcDoorConverter(),
                new FromIfcBeamConverter(),
                new FromIfcColumnConverter()
            };

            var defaultConverter = new FromIfcElementConverter();

            return new CompositeFromIfcProductConverter(converters, defaultConverter);
        }

        /// <summary>
        /// Create the default IfcRepresentationDataExtractor. It will be used, if 
        /// IfcRepresentationDataExtractor is not specified in the constructor.
        /// </summary>
        private static IfcRepresentationDataExtractor GetDefaultRepresentationDataExtractor(MaterialExtractor materialExtractor)
        {
            IfcRepresentationDataExtractor extractor = new IfcRepresentationDataExtractor();

            extractor.AddRepresentationParser(new IfcFacetedBrepParser(materialExtractor));
            extractor.AddRepresentationParser(new IfcExtrudedAreaSolidParser(materialExtractor));
            extractor.AddRepresentationParser(new IfcMappedItemParser(extractor));
            extractor.AddRepresentationParser(new IfcBooleanClippingResultParser(extractor));

            return extractor;
        }
    }
}
