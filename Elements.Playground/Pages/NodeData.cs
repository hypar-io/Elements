using System.Collections.Generic;

namespace Elements.Playground
{
    public class ParameterData
    {
        public string Type { get; set; }
        public string Name { get; set; }
        public string DefaultValue { get; set; }
    }

    public class NodeData
    {
        public string Id { get; set; }
        public string TypeName { get; set; }
        public string DisplayName { get; set; }
        public string Signature { get; set; }
        public Dictionary<string, ParameterData> ParameterData { get; set; }
        public bool IsConstructor { get; set; }
        public string MethodName { get; set; }
        public string ReturnType { get; set; }
        public bool IsStatic { get; set; }
    }
}