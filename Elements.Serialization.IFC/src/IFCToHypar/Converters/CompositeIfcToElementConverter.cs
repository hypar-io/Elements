using Elements.Serialization.IFC.IFCToHypar.RepresentationsExtraction;
using IFC;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Elements.Serialization.IFC.IFCToHypar.Converters
{
    internal class CompositeIfcToElementConverter : IIfcProductToElementConverter
    {
        private readonly List<IIfcProductToElementConverter> _converters;
        private readonly IIfcProductToElementConverter _defaultConverter;

        public CompositeIfcToElementConverter(List<IIfcProductToElementConverter> converters, IIfcProductToElementConverter defaultConverter)
        {
            _converters = converters;
            _defaultConverter = defaultConverter;
        }

        public Element ConvertToElement(IfcProduct ifcProduct, RepresentationData representationData, List<string> constructionErrors)
        {
            Element result;
            
            foreach (var converter in _converters)
            {
                if (!converter.Matches(ifcProduct))
                {
                    continue;
                }

                result = converter.ConvertToElement(ifcProduct, representationData, constructionErrors);

                if (result != null)
                {
                    return result;
                }
            }

            return _defaultConverter.ConvertToElement(ifcProduct, representationData, constructionErrors);
        }

        public bool Matches(IfcProduct ifcProduct)
        {
            return _converters.Any(converter => converter.Matches(ifcProduct)) || _defaultConverter.Matches(ifcProduct);
        }
    }
}
