
using System;
using System.Collections.Generic;
using Elements;

namespace Elements.Components
{
    /// <summary>
    /// An instance of a component, containing multiple element instances.
    /// </summary>
    public class ComponentInstance : Element
    {
        public ComponentInstance(string name = null) : base(Guid.NewGuid(), name)
        {
            Instances = new List<Element>();
        }

        public List<Element> Instances { get; set; }
    }

}