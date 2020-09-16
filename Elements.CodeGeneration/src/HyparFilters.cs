
using System.Collections.Generic;
using System.Linq;
using DotLiquid;
using Elements.Generate.StringUtils;
using NJsonSchema;

namespace Elements.Generate
{
    // TODO Delete this HyparFilters class when this issue gets resolved. https://github.com/RicoSuter/NJsonSchema/issues/1199
    // This HyparFilters class contains filters that are copied directly from the NJsonSchema repo
    // because the filters are not public but we need to register them globally for async code gen.
    // Copied from https://github.com/RicoSuter/NJsonSchema/blob/687efeabdc30ddacd235e85213f3594458ed48b4/src/NJsonSchema.CodeGeneration/DefaultTemplateFactory.cs#L183
    public static class HyparFilters
    {
        public static string Lowercamelcase(Context context, string input, bool firstCharacterMustBeAlpha = true)
        {
            return ConversionUtilities.ConvertToLowerCamelCase(input, firstCharacterMustBeAlpha);
        }

        public static string Safeidentifierlower(Context context, string input, bool firstCharacterMustBeAlpha = true)
        {
            return input.ToSafeIdentifier(true);
        }

        public static string Safeidentifierupper(Context context, string input, bool firstCharacterMustBeAlpha = true)
        {
            return input.ToSafeIdentifier();
        }

        public static string Csharpdocs(string input, int tabCount)
        {
            return ConversionUtilities.ConvertCSharpDocs(input, tabCount);
        }

        public static IEnumerable<object> Empty(Context context, object input)
        {
            return Enumerable.Empty<object>();
        }

        public static string Tab(Context context, string input, int tabCount)
        {
            return ConversionUtilities.Tab(input, tabCount);
        }
    }
}