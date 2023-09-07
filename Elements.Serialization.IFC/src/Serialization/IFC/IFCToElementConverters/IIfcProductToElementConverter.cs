using IFC;
using System;
using System.Collections.Generic;
using System.Text;

namespace Elements.Serialization.IFC.Serialization.IFC.IFCToElementConverters
{
    internal interface IIfcProductToElementConverter
    {
        Element ConvertToElement(IfcProduct product, List<string> constructionErrors);
        bool Matches(IfcProduct ifcProduct);
    }
}
