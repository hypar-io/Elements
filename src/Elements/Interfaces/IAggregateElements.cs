#pragma warning disable CS1591

using System.Collections.Generic;

namespace Elements.Interfaces
{
    public interface IAggregateElements
    {
        List<Element> Elements{get;}
    }
}