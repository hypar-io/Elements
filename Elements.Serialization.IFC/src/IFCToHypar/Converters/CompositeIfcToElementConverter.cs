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

        public Element ConvertToElement(IfcProduct ifcProduct, List<string> constructionErrors)
        {
            var matchingConverters = _converters.Where(converter => converter.Matches(ifcProduct)).ToList();
            var theConverter = matchingConverters.Any() ? matchingConverters.First() : _defaultConverter;
            return theConverter.ConvertToElement(ifcProduct, constructionErrors);
        }

        public bool Matches(IfcProduct ifcProduct)
        {
            return _converters.Any(converter => converter.Matches(ifcProduct)) || _defaultConverter.Matches(ifcProduct);
        }
    }
}
