
using System;
using System.Linq;
using System.Text.RegularExpressions;

namespace Elements.Generate.StringUtils
{
    /// <summary>
    /// Utility methods for working with strings.
    /// </summary>
    public static class StringUtilities
    {
        // TODO: This is a direct copy of the method in Hypar.Model.
        // We should move type generation out of elements to a place where it can refer to Hypar.Model.
        /// <summary>
        /// Return the string turned into a string safe to use as a C# identifier.
        /// </summary>
        /// <param name="name"></param>
        /// <param name="firstCharLowercase"></param>
        public static string ToSafeIdentifier(this string name, bool firstCharLowercase = false)
        {
            if (String.IsNullOrWhiteSpace(name) || String.IsNullOrEmpty(name))
            {
                throw new ArgumentException("Invalid name.");
            }
            //remove special characters except for space, dash, period, and underscore
            name = Regex.Replace(name, @"[^A-Za-z0-9 _\.-]", "");
            //remove any numbers or whitespace/separator characters from the front
            while (Char.IsDigit(name.First()) || name.First() == ' ' || name.First() == '_' || name.First() == '-' || name.First() == '.')
            {
                if (name.Length < 2)
                {
                    throw new ArgumentException("Name is invalid: nothing is left after removing numeric characters. Names must contain at least one letter.");
                }
                name = name.Substring(1);
            }
            //split on whitespace or - or _
            var splits = name.Split(new[] { ' ', '-', '_', '.' });
            var cleanName = "";
            var dontCapitalize = firstCharLowercase;
            foreach (var split in splits)
            {
                if (split.Length == 0)
                {
                    continue;
                }
                if (dontCapitalize)
                {
                    cleanName += split.First().ToString().ToLower();
                    dontCapitalize = false;
                }
                else
                {
                    cleanName += split.First().ToString().ToUpper();
                }
                if (split.Length > 1)
                {
                    cleanName += split.Substring(1);
                }
            }
            if (string.IsNullOrEmpty(cleanName))
            {
                throw new ArgumentException("Names must have at least one letter character.");
            }
            return cleanName;
        }


        /// <summary>
        /// returns a string with double quotes doubled, suitable for string literals.
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static string LiteralQuotes(this string str)
        {
            return str.Replace("\"", "\"\"");
        }
    }
}