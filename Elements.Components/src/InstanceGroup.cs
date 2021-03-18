using System;
using System.Collections.Generic;
using Elements;
using Elements.Geometry;

namespace Elements.Components
{
    // TODO: Consolidate with `ComponentInstance`, I think this is doing the same thing pretty much. 
    public class InstanceGroup : GeometricElement
    {
        public List<ElementInstance> Instances { get; set; }
        public InstanceGroup(IEnumerable<ElementInstance> instances, Transform transform, Material material, Representation representation, bool isElementDefinition, Guid id, string name) : base(transform, material, representation, isElementDefinition, id, name)
        {
            this.Instances = new List<ElementInstance>(instances);
        }

        public InstanceGroup() : base(new Transform(), null, null, true, Guid.NewGuid(), null)
        {
            this.Instances = new List<ElementInstance>();
        }

    }
}