using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Elements.Serialization.JSON
{
    // This is a hack of the reference resolver.
    // We don't actually use reference resolution (ex: $ref, $id) in our element converter,
    // but we use a reference resolver because it can be passed down through
    // serialization and made available to both properties with converter
    // attributes, and objects serialized/deserialized using converters
    // from the serializer's options.
    // https://docs.microsoft.com/en-us/dotnet/standard/serialization/system-text-json-preserve-references?pivots=dotnet-6-0#persist-reference-metadata-across-multiple-serialization-and-deserialization-calls
    internal class ElementReferenceResolver : ReferenceResolver
    {
        public Dictionary<string, Type> TypeCache { get; }
        public JsonElement DocumentElements { get; }

        private readonly Dictionary<string, object> _referenceIdToObjectMap = new Dictionary<string, object>();
        private readonly Dictionary<object, string> _objectToReferenceIdMap = new Dictionary<object, string>();

        public ElementReferenceResolver(Dictionary<string, Type> typeCache, JsonElement documentElements)
        {
            TypeCache = typeCache;
            DocumentElements = documentElements;
        }

        public override void AddReference(string referenceId, object value)
        {
            // TODO: Enable this when we move to a .net version that supports TryAdd
            // if (!_referenceIdToObjectMap.TryAdd(referenceId, value))
            // {
            //     throw new JsonException();
            // }

            if (_referenceIdToObjectMap == null)
            {
                throw new JsonException(nameof(_referenceIdToObjectMap));
            }

            if (!_referenceIdToObjectMap.ContainsKey(referenceId))
            {
                _referenceIdToObjectMap.Add(referenceId, value);
            }
        }

        public override string GetReference(object value, out bool alreadyExists)
        {
            if (_objectToReferenceIdMap.TryGetValue(value, out string referenceId))
            {
                alreadyExists = true;
            }
            else
            {
                // _referenceCount++;
                // referenceId = _referenceCount.ToString();
                _objectToReferenceIdMap.Add(value, referenceId);
                alreadyExists = false;
            }

            return referenceId;
        }

        public override object ResolveReference(string referenceId)
        {
            if (!_referenceIdToObjectMap.TryGetValue(referenceId, out object value))
            {
                // In the default version of this reference resolver
                // they throw an exception when a reference cannot be found.
                // But we want to be more permissive and just return null.
                return null;
            }

            return value;
        }
    }

    internal class ElementReferenceHandler : ReferenceHandler
    {
        public ElementReferenceHandler(Dictionary<string, Type> typeCache, JsonElement documentElements) => Reset(typeCache, documentElements);
        private ReferenceResolver _rootedResolver;
        public override ReferenceResolver CreateResolver() => _rootedResolver;
        public void Reset(Dictionary<string, Type> typeCache, JsonElement documentElements) => _rootedResolver = new ElementReferenceResolver(typeCache, documentElements);
    }
}