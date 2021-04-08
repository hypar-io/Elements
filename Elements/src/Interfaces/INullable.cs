#pragma warning disable CS1591

using System.Collections.Generic;

namespace Elements.Interfaces
{
    public interface INullable<T>
    {
        /// <summary>
        /// Return an instance of the type that replaces any null values.
        /// </summary>
        T CoerceNull();
    }
}