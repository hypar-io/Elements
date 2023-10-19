using Elements.Serialization.IFC.IFCToHypar.RepresentationsExtraction;
using IFC;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Elements.Serialization.IFC.IFCToHypar.Converters
{
    /// <summary>Uses a list of IFromIfcProductConverter to convert an IfcProduct to a GeometricElement.</summary>
    internal class CompositeFromIfcProductConverter : IFromIfcProductConverter
    {
        private readonly List<IFromIfcProductConverter> _converters;
        private readonly IFromIfcProductConverter _defaultConverter;

        /// <summary>
        /// Create a CompositeFromIfcProductConverter that uses <paramref name="converters"/> and <paramref name="defaultConverter"/>
        /// to convert an IfcProduct to a GeometricElement.
        /// </summary>
        /// <param name="converters">A list, where CompositeFromIfcProductConverter looks for a converter that can convert an IfcProduct
        /// to a GeometricElement.</param>
        /// <param name="defaultConverter">A fallback converter, which will be used if none of <paramref name="converters"/> can convert
        /// an IfcProduct to a GeometricElement.</param>
        public CompositeFromIfcProductConverter(List<IFromIfcProductConverter> converters, IFromIfcProductConverter defaultConverter)
        {
            _converters = converters;
            _defaultConverter = defaultConverter;
        }

        /// <summary>
        /// Looks for a converter that can convert <paramref name="ifcProduct"/> to a GeometricElement within _converters.
        /// If none of _converters can do the conversion, _defaultConverter is used instead.
        /// Returns null if the conversion was unsuccessful.
        /// </summary>
        /// <param name="ifcProduct">IfcProduct to convert to a GeometricElement.</param>
        /// <param name="representationData">Parsed Representation of <paramref name="ifcProduct"/>.</param>
        /// <param name="constructionErrors">The list of construction errors that appeared during conversion.</param>
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

        /// <summary>
        /// Returns true, if any of _converters or _defaultConverter can convert <paramref name="ifcProduct"/> to a GeometricElement.
        /// </summary>
        /// <param name="ifcProduct">IfcProduct that will be checked if it can be converted with this converter.</param>
        public bool CanConvert(IfcProduct ifcProduct)
        {
            return _converters.Any(converter => converter.CanConvert(ifcProduct)) || _defaultConverter.CanConvert(ifcProduct);
        }
    }
}
