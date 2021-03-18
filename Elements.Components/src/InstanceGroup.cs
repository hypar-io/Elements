using System;
using System.Collections.Generic;
using Elements;
using Elements.Geometry;

namespace Elements.Components
{
    // TODO: Consolidate with `ComponentInstance`, I think this is doing the same thing pretty much. 
    /// <summary>
    /// An element representing a group of element instances.
    /// </summary>
    public class InstanceGroup : GeometricElement
    {
        /// <summary>
        /// The element instances contained within this group.
        /// </summary>
        public List<ElementInstance> Instances { get; set; }

        /// <summary>
        /// Construct a new instance group
        /// </summary>
        public InstanceGroup() : base(new Transform(), null, null, true, Guid.NewGuid(), null)
        {
            this.Instances = new List<ElementInstance>();
        }

    }
}