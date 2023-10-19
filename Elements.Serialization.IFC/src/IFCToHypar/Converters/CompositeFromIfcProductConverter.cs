using Elements.Serialization.IFC.IFCToHypar.RepresentationsExtraction;
using IFC;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Elements.Serialization.IFC.IFCToHypar.Converters
{
    internal class CompositeFromIfcProductConverter : IFromIfcProductConverter
    {
        private readonly List<IFromIfcProductConverter> _converters;
        private readonly IFromIfcProductConverter _defaultConverter;

        public CompositeFromIfcProductConverter(List<IFromIfcProductConverter> converters, IFromIfcProductConverter defaultConverter)
        {
            _converters = converters;
            _defaultConverter = defaultConverter;
        }

        public GeometricElement ConvertToElement(IfcProduct ifcProduct, RepresentationData representationData, List<string> constructionErrors)
        {
            GeometricElement result;
            
            foreach (var converter in _converters)
            {
                if (!converter.CanConvert(ifcProduct))
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

        public bool CanConvert(IfcProduct ifcProduct)
        {
            return _converters.Any(converter => converter.CanConvert(ifcProduct)) || _defaultConverter.CanConvert(ifcProduct);
        }
    }
}
