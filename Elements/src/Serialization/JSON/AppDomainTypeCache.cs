#pragma warning disable CS1591

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Elements.Geometry;
using Elements.Geometry.Solids;

namespace Elements.Serialization.JSON
{
    public static class AppDomainTypeCache
    {
        /// <summary>
        /// The type cache needs to contains all types that will have a discriminator.
        /// This includes base types, like elements, and all derived types like Wall.
        /// We use reflection to find all public types available in the app domain
        /// that have a JsonConverterAttribute whose converter type is the
        /// Elements.Serialization.JSON.JsonInheritanceConverter.
        /// </summary>
        /// <returns>A dictionary containing all found types keyed by their full name.</returns>
        internal static Dictionary<string, Type> BuildAppDomainTypeCache(out List<string> failedAssemblyErrors)
        {
            var typeCache = new Dictionary<string, Type>();

            failedAssemblyErrors = new List<string>();

            var skipAssembliesPrefices = new[] { "System", "SixLabors" };
            foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies().Where(a =>
            {
                var name = a.GetName().Name;
                return !skipAssembliesPrefices.Any(p => name.StartsWith(p));
            }))
            {
                var types = Array.Empty<Type>();
                try
                {
                    types = assembly.GetTypes();
                }
                catch (ReflectionTypeLoadException)
                {
                    failedAssemblyErrors.Add($"Failed to load assembly: {assembly.FullName}");
                    continue;
                }
                foreach (var t in types)
                {
                    try
                    {
                        if (IsValidTypeForElements(t) && !typeCache.ContainsKey(t.FullName))
                        {
                            // At this point we don't know the generic type arguments,
                            // so we key the type in the cache as Elements.ProxyElement<>
                            // Later, when we set the discriminator, we include the
                            // type argument like Elements.ProxyElement<Elements.Mass>, and
                            // we do some string deconstruction to match the two and to
                            // extract the type arguments.
                            typeCache.Add(t.IsGenericType ? $"{t.FullName.Split('`').First()}<>" : t.FullName, t.IsGenericType ? t.GetGenericTypeDefinition() : t);
                        }
                    }
                    catch (TypeLoadException)
                    {
                        failedAssemblyErrors.Add($"Failed to load type: {t.FullName}");
                        continue;
                    }
                }
            }

            return typeCache;
        }

        private static bool IsValidTypeForElements(Type t)
        {
            if (t.IsPublic && t.IsClass)
            {
                var attrib = t.GetCustomAttribute<System.Text.Json.Serialization.JsonConverterAttribute>();
                if (attrib != null && attrib.ConverterType.GenericTypeArguments.Length > 0)
                {
                    var valid = attrib.ConverterType.GenericTypeArguments[0] == t
                        || attrib.ConverterType == typeof(ElementConverter<Element>)
                        || attrib.ConverterType == typeof(ElementConverter<SolidOperation>)
                        || attrib.ConverterType == typeof(ElementConverter<Curve>);
                    return valid;
                }
            }

            return false;
        }
    }
}