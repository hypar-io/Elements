using System.Text.Json.Serialization;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Elements
{
    /// <summary>
    /// Wrapper/utilities for ElementProxy generics.
    /// </summary>
    public class ElementProxy
    {
        private static Dictionary<string, Dictionary<Guid, object>> _cache = new Dictionary<string, Dictionary<Guid, object>>();

        /// <summary>
        /// Clears the current proxy cache. Use this at the beginning of functions so that the previous cache is not polluting our current run.
        /// </summary>
        public static void ClearCache()
        {
            Console.WriteLine("Clearing proxy cache.");
            _cache.Clear();
        }

        /// <summary>
        /// Get an element proxy. Will reuse one if it has already been created via GetProxy, or will create a new one if it didn't exist.
        /// </summary>
        /// <param name="element"></param>
        /// <param name="dependencyName"></param>
        /// <returns></returns>
        internal static ElementProxy<T> GetProxy<T>(T element, string dependencyName) where T : Element
        {
            if (!_cache.TryGetValue(dependencyName, out var dictById))
            {
                dictById = new Dictionary<Guid, object>();
                _cache.Add(dependencyName, dictById);
            }
            if (dictById.TryGetValue(element.Id, out var foundProxy))
            {
                return (ElementProxy<T>)foundProxy;
            }
            var proxy = new ElementProxy<T>(element, dependencyName);
            dictById.Add(element.Id, proxy);
            return proxy;
        }
    }

    /// <summary>
    /// Proxy for an element from another function.
    /// This is used to attach additional information to upstream elements.
    /// Proxies created via Element.Proxy() are intended to be reused, so we are not creating multiple proxies for each element.
    /// Proxies deserialized from other functions are not added to the current proxy cache, so that each function will create its own, new proxies for each element.
    /// </summary>
    public class ElementProxy<T> : Element where T : Element
    {
        private T _element = null;

        /// <summary>
        /// ID of element that this is a proxy for.
        /// </summary>
        [JsonPropertyName("elementId")]
        public Guid ElementId { get; set; }

        /// <summary>
        /// Dependency string for the dependency that this element came from.
        /// </summary>
        [JsonPropertyName("dependency")]
        public string Dependency { get; set; }

        /// <summary>
        /// Original element that the proxy refers to. If null, the element needs to be hydrated.
        /// </summary>
        [JsonIgnore]
        public T Element
        {
            get
            {
                return this._element;
            }
        }

        /// <summary>
        /// JSON constructor only for deserializing other models.
        /// </summary>
        [JsonConstructor]
        public ElementProxy(Guid elementId, string dependencyName, Guid id = default(Guid), string name = null) : base(id, name)
        {
            this.ElementId = elementId;
            this.Dependency = dependencyName;
        }

        /// <summary>
        /// Create a new proxy within this function. Not intended to be used anywhere outside of ElementProxy.GetProxy().
        /// </summary>
        internal ElementProxy(T element, string dependencyName, Guid id = default(Guid), string name = null) : base(id, name)
        {
            this.ElementId = element.Id;
            this.Dependency = dependencyName;
            this._element = element;
        }

        /// <summary>
        /// Re-populate the element reference in a proxy if it is missing. Does nothing if it is already populated.
        /// </summary>
        /// <param name="models">Keyed dictionary of available models to search for the element reference in.</param>
        public T Hydrate(Dictionary<string, Model> models)
        {
            if (this.Element != null)
            {
                // we're already hydrated
                return this.Element;
            }
            var model = models[this.Dependency];
            if (model == null)
            {
                throw new Exception($"Could not find model for dependency {this.Dependency}");
            }
            model.Elements.TryGetValue(this.ElementId, out var element);
            this._element = element as T;
            return this.Element;
        }
    }

    /// <summary>
    /// Extension methods for element proxies.
    /// </summary>
    public static class ElementProxyExtensions
    {
        /// <summary>
        /// Create a proxy for this element, or get the existing proxy already created for this element if it exists.
        /// </summary>
        /// <param name="element"></param>
        /// <param name="dependencyName">The name of the dependency this element came from. The assumption is that we only need proxies for elements from dependencies.</param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static ElementProxy<T> Proxy<T>(this T element, string dependencyName) where T : Element
        {
            return ElementProxy.GetProxy<T>(element, dependencyName);
        }

        /// <summary>
        /// Create or get proxies for a list of elements.
        /// </summary>
        /// <param name="elements"></param>
        /// <param name="dependencyName">The name of the dependency these elements came from. The assumption is that we only need proxies for elements from dependencies.</param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static IEnumerable<ElementProxy<T>> Proxies<T>(this IEnumerable<T> elements, string dependencyName) where T : Element
        {
            return elements.Select(e => e.Proxy(dependencyName));
        }

        /// <summary>
        /// Grab the proxy for an element from a list of proxies.
        /// </summary>
        /// <param name="proxies">Proxies to search for teh element in.</param>
        /// <param name="element">The element to find a proxy for.</param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static ElementProxy<T> Proxy<T>(this IEnumerable<ElementProxy<T>> proxies, T element) where T : Element
        {
            return proxies.FirstOrDefault(e => e.ElementId == element.Id);
        }

        /// <summary>
        /// Re-populate a list of element proxies' element references.
        /// </summary>
        /// <param name="proxies"></param>
        /// <param name="models">Keyed dictionary of available models to search for the element reference in.</param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static IEnumerable<T> Hydrate<T>(this IEnumerable<ElementProxy<T>> proxies, Dictionary<string, Model> models) where T : Element
        {
            return proxies.Select(p => p.Hydrate(models)).ToList();
        }

        /// <summary>
        /// Returns a list of element proxies' element references. Assumes they have already been hydrated.
        /// </summary>
        /// <param name="proxies"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static IEnumerable<T> Elements<T>(this IEnumerable<ElementProxy<T>> proxies) where T : Element
        {
            return proxies.Select(p => p.Element).ToList();
        }
    }
}