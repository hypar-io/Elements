#pragma warning disable CS1591

using System.Collections.Generic;

namespace Elements.Interfaces
{
    public interface IProperty
    {
        /// <summary>
        /// The description of the property.
        /// </summary>
        string Description{get;}
    }

    public interface IPropertySingleValue<TValue> : IProperty
    {
        TValue Value {get;}
        UnitType UnitType{get;}
    }

    public interface IPropertySet
    {
        Dictionary<string, IProperty> Properties{get;}
    }
}