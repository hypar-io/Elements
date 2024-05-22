using System.Collections.Generic;

namespace Elements.Serialization.glTF
{
    internal class NodeExtension
    {
        public NodeExtension(string name, Dictionary<string, object> attributes)
        {
            Name = name;
            Attributes = attributes;
        }

        public NodeExtension(string name)
        {
            Name = name;
        }

        public NodeExtension(string name, string attributeName, object attributeValue)
        {
            Name = name;
            Attributes.Add(attributeName, attributeValue);
        }

        public string Name { get; set; }

        public Dictionary<string, object> Attributes { get; } = new Dictionary<string, object>();
    }
}