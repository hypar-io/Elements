using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Elements
{
    /// <summary>
    /// Proxy for an element from another function.
    /// This is used to attach additional information to upstream elements.
    /// </summary>
    public class ElementProxy<T> : Element where T : Element
    {
        [JsonIgnore]
        private T _element = null;

        /// <summary>
        /// ID of element that this is a proxy for.
        /// </summary>
        [JsonProperty("elementId")]
        public Guid ElementId { get; set; }

        /// <summary>
        /// Dependency string for the dependency that this element came from.
        /// </summary>
        [JsonProperty("dependency")]
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
        /// JSON constructor only.
        /// </summary>
        /// <returns></returns>
        [JsonConstructor]
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
        /// <typeparam name="T"></typeparam>
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
        /// Create a proxy for this element.
        /// </summary>
        /// <param name="element"></param>
        /// <param name="dependencyName">The name of the dependency this element came from. The assumption is that we only need proxies for elements from dependencies.</param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static ElementProxy<T> Proxy<T>(this T element, string dependencyName) where T : Element
        {
            var proxy = new ElementProxy<T>(element, dependencyName);
            return proxy;
        }

        /// <summary>
        /// Create proxies for a list of elements.
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
    }
}