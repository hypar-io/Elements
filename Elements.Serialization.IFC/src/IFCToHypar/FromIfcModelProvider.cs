using Elements.Interfaces;
using Elements.Serialization.IFC.IFCToHypar.Converters;
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

        private readonly IIfcProductToElementConverter _fromIfcToElementsConverter;

        private List<IfcProduct> _ifcProducts;
        private List<IfcRelationship> _ifcRelationships;

        private readonly Dictionary<Element, IfcProduct> _elementToIfcProduct;
        private readonly List<string> _constructionErrors;

        public FromIfcModelProvider(string path, IList<string> idsToConvert = null, IIfcProductToElementConverter fromIfcConverter = null)
        {
            _constructionErrors = new List<string>();
            ExtractIfcProducts(path, idsToConvert);
            _elementToIfcProduct = new Dictionary<Element, IfcProduct>();

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
                    var element = _fromIfcToElementsConverter.ConvertToElement(product, _constructionErrors);
                    if (element != null)
                    {
                        elements.Add(element);
                        _elementToIfcProduct.Add(element, product);
                    }
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
        }

        private static IIfcProductToElementConverter GetStandardFromIfcConverter()
        {
            var converters = new List<IIfcProductToElementConverter>()
            {
                new IfcFloorToFloorConverter(),
                new IfcSpaceToSpaceConverter(),
                new IfcWallToWallConverter(),
                new IfcDoorToDoorConverter(),
                new IfcBeamToBeamConverter(),
                new IfcColumnToColumnConverter()
            };

            var standardConverter = new IfcBuildingElementToElementConverter();

            return new CompositeIfcToElementConverter(converters, standardConverter);
        }
    }
}
