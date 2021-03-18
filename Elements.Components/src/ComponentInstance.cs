
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
        /// <summary>
        /// Construct a new ComponentInstance (You probably should use ComponentDefinition.Instantiate() to 
        /// create Component Instances, rather than making them manually)
        /// /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public ComponentInstance(string name = null) : base(Guid.NewGuid(), name)
        {
            Instances = new List<Element>();
        }

        /// <summary>
        /// The internal elements contained in this ComponentInstance
        /// </summary>

        public List<Element> Instances { get; set; }
    }

}