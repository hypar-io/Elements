using NJsonSchema;
using NJsonSchema.CodeGeneration;
using NJsonSchema.CodeGeneration.CSharp;
using System.Linq;

namespace Elements.Generate
{
    public class TypeResolver : CSharpTypeResolver
    {

        private readonly string[] _typesToMakeNullable = new[] { "Vector3", "Color" };
        public TypeResolver(CSharpGeneratorSettings settings) : base(settings)
        {
        }

        public override string Resolve(JsonSchema schema, bool isNullable, string typeNameHint)
        {
            // NJsonSchema will treat a an object schema with empty `properties`
            // as just an `object`, and then fail to codegen it. We need to
            // force it to recognize even an empty input schema as valid, so it
            // generates the Inputs class in all cases.
            if (schema.Type == JsonObjectType.Object && schema.Properties.Count() == 0)
            {
                // `GetOrGenerateTypeName` has a side effect of storing the type
                // in the Types dictionary, which is what governs which types
                // get generated.
                GetOrGenerateTypeName(schema, typeNameHint);
                return typeNameHint;
            }
            var baseResult = base.Resolve(schema, isNullable, typeNameHint);
            // if we encounter a built-in value type, like Vector3, which is supposed to be
            // nullable, we should make it a nullable type.
            // We could make this more future-proof by determining at runtime whether a type should
            // get this treatment, instead of relying on a hard-coded list.
            if (_typesToMakeNullable.Contains(baseResult) && isNullable)
            {
                return $"{baseResult}?";
            }
            return baseResult;
        }
    }
}