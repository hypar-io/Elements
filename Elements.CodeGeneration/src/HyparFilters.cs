
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
    /// <summary>
    /// This class contains text filtering methods.  They are used during rendering in liquid templates.
    /// We shouldn't actually need to implement these ourselves. See the TODO in the source code for more information.
    /// </summary>
    public static class HyparFilters
    {
        /// <summary>
        /// Return the string in lower camel case.
        /// </summary>
        /// <param name="context">The DotLiquid.Context this filter is running in.</param>
        /// <param name="input">The string to be formatted.</param>
        /// <param name="firstCharacterMustBeAlpha">Should the @ character be prepended to the string.</param>
        public static string Lowercamelcase(Context context, string input, bool firstCharacterMustBeAlpha = true)
        {
            return ConversionUtilities.ConvertToLowerCamelCase(input, firstCharacterMustBeAlpha);
        }

        /// <summary>
        /// Return the string turned into a save C# identifier lowercased.
        /// </summary>
        /// <param name="context">The DotLiquid.Context this filter is running in.</param>
        /// <param name="input">The string to be formatted.</param>
        /// <param name="firstCharacterMustBeAlpha">Should the @ character be prepended to the string.</param>
        public static string Safeidentifierlower(Context context, string input, bool firstCharacterMustBeAlpha = true)
        {
            return input.ToSafeIdentifier(true);
        }

        /// <summary>
        /// Return the string turned into a save C# identifier uppercased.
        /// </summary>
        /// <param name="context">The DotLiquid.Context this filter is running in.</param>
        /// <param name="input">The string to be formatted.</param>
        /// <param name="firstCharacterMustBeAlpha">Should the @ character be prepended to the string.</param>
        public static string Safeidentifierupper(Context context, string input, bool firstCharacterMustBeAlpha = true)
        {
            return input.ToSafeIdentifier();
        }

        /// <summary>
        /// Return a string that is literalized with double double quotes.
        /// </summary>
        /// <param name="context">The DotLiquid.Context this filter is running in.</param>
        /// <param name="input">The string to be formatted.</param>
        /// <param name="firstCharacterMustBeAlpha">Should the @ character be prepended to the string.</param>
        public static string Literalquotes(Context context, string input, bool firstCharacterMustBeAlpha = true)
        {
            return input.LiteralQuotes();
        }

        /// <summary>
        /// Convert the string into a tabbed text block with line breaks.
        /// </summary>
        /// <param name="input">The string to be formatted.</param>
        /// <param name="tabCount">The number of tabs that should be used.</param>
        public static string Csharpdocs(string input, int tabCount)
        {
            return ConversionUtilities.ConvertCSharpDocs(input, tabCount);
        }

        /// <summary>
        /// Return an empty list of the input object's type.
        /// </summary>
        /// <param name="context">The DotLiquid.Context this filter is running in.</param>
        /// <param name="input">The string to be formatted.</param>
        public static IEnumerable<object> Empty(Context context, object input)
        {
            return Enumerable.Empty<object>();
        }

        /// <summary>
        /// Add the desired number of tabs to the input string.
        /// </summary>
        /// <param name="context">The DotLiquid.Context this filter is running in.</param>
        /// <param name="input">The string to be formatted.</param>
        /// <param name="tabCount">The number of tabs that should be used.</param>
        public static string Tab(Context context, string input, int tabCount)
        {
            return ConversionUtilities.Tab(input, tabCount);
        }

        /// <summary>
        /// Specify a default value for typical arguments for Element types.
        /// </summary>
        /// <param name="context">The DotLiquid.Context this filter is running in.</param>
        /// <param name="input">The string to be formatted.</param>
        /// <param name="baseClass">The base class of this type.</param>
        public static string Defaultforelementargument(Context context, string input, string baseClass = null)
        {
            var defaults = new Dictionary<string, string>
            {
                {"id", "default"},
                {"name", "null"},
                {"isElementDefinition", "false"},
                {"representation", "null"},
                {"material", "null"},
                {"transform", "null"},
            };
            if ((baseClass == "Element" || baseClass == "GeometricElement") && defaults.ContainsKey(input))
            {
                return $"{input} = {defaults[input]}";
            }
            return input;
        }
    }
}