#pragma warning disable CS1591
#pragma warning disable CS1570

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Collections;
using System.Text.Json.Serialization;
using Elements.Serialization.JSON;
using Elements.Geometry;
using Elements.Geometry.Solids;
using Elements.GeoJSON;
using System.IO;
using System.Text.Json;
using System.Diagnostics;

namespace Elements
{
    /// <summary>
    /// A container for elements.
    /// </summary>
    public class Model
    {
        /// <summary>The origin of the model.</summary>
        [JsonPropertyName("Origin")]
        [Obsolete("Use Transform instead.")]
        public Position Origin { get; set; }

        /// <summary>The transform of the model.</summary>
        [JsonPropertyName("Transform")]
        public Transform Transform { get; set; }

        /// <summary>A collection of Elements keyed by their identifiers.</summary>
        [JsonPropertyName("Elements")]
        [System.ComponentModel.DataAnnotations.Required]
        public System.Collections.Generic.IDictionary<Guid, Element> Elements { get; set; } = new System.Collections.Generic.Dictionary<Guid, Element>();

        /// <summary>
        /// Construct a model.
        /// </summary>
        /// <param name="origin">The origin of the model.</param>
        /// <param name="transform">The transform of the model.</param>
        /// <param name="elements">A collection of elements.</param>
        [Newtonsoft.Json.JsonConstructor]
        [System.Text.Json.Serialization.JsonConstructor]
        public Model(Position @origin, Transform @transform, System.Collections.Generic.IDictionary<Guid, Element> @elements)
        {
            this.Origin = @origin;
            this.Transform = @transform;
            this.Elements = @elements;
        }

        /// <summary>
        /// Construct an empty model.
        /// </summary>
        public Model()
        {
            this.Transform = new Transform();
        }

        /// <summary>
        /// Construct a model.
        /// </summary>
        /// <param name="transform">The model's transform.</param>
        /// <param name="elements">The model's elements.</param>
        public Model(Transform @transform, System.Collections.Generic.IDictionary<Guid, Element> @elements)
        {
            this.Transform = @transform;
            this.Elements = @elements;
        }

        /// <summary>
        /// Add an element to the model.
        /// This operation recursively searches the element's properties
        /// for element sub-properties and adds those elements to the elements
        /// dictionary before adding the element itself.
        /// Properties of the following types are be supported for introspection:
        /// Element, IList<Element>, IDictionary<Element>, Representation, IList<SolidOperation>
        /// </summary>
        /// <param name="element">The element to add to the model.</param>
        /// <param name="gatherSubElements">Should sub-elements in properties be
        /// added to the model's elements collection?</param>
        public void AddElement(Element element, bool gatherSubElements = true)
        {
            if (element == null || this.Elements.ContainsKey(element.Id))
            {
                return;
            }

            // Some elements compute profiles and transforms
            // during UpdateRepresentation. Call UpdateRepresentation
            // here to ensure these values are correct in the JSON.

            // TODO: This is really expensive. This should be removed
            // when all internal types have been updated to not create elements
            // during UpdateRepresentation. This is now possible because
            // geometry operations are reactive to changes in their properties.
            if (element is GeometricElement element1)
            {
                element1.UpdateRepresentations();
            }

            if (gatherSubElements)
            {
                // Look at all public properties of the element.
                // For all properties which inherit from element, add those
                // to the elements dictionary first. This will ensure that
                // those elements will be read out and be available before
                // an attempt is made to deserialize the element itself.
                var subElements = RecursiveGatherSubElements(element);
                foreach (var e in subElements)
                {
                    if (!this.Elements.ContainsKey(e.Id))
                    {
                        this.Elements.Add(e.Id, e);
                    }
                }
            }
            else
            {
                if (!this.Elements.ContainsKey(element.Id))
                {
                    this.Elements.Add(element.Id, element);
                }
            }
        }

        /// <summary>
        /// Add a collection of elements to the model.
        /// </summary>
        /// <param name="elements">The elements to add to the model.</param>
        /// <param name="gatherSubElements">Should sub-elements in properties be
        /// added to the model's elements collection?</param>
        public void AddElements(IEnumerable<Element> elements, bool gatherSubElements = true)
        {
            foreach (var e in elements)
            {
                AddElement(e, gatherSubElements);
            }
        }

        /// <summary>
        /// Add elements to the model.
        /// </summary>
        /// <param name="elements">The elements to add to the model.</param>
        public void AddElements(params Element[] elements)
        {
            foreach (var e in elements)
            {
                AddElement(e);
            }
        }

        /// <summary>
        /// Get an entity by id from the Model.
        /// </summary>
        /// <param name="id">The identifier of the element.</param>
        /// <returns>An entity or null if no entity can be found
        /// with the provided id.</returns>
        public T GetElementOfType<T>(Guid id) where T : Element
        {
            if (this.Elements.ContainsKey(id))
            {
                return (T)this.Elements[id];
            }
            return null;
        }

        /// <summary>
        /// Get the first entity with the specified name.
        /// </summary>
        /// <param name="name"></param>
        /// <returns>An entity or null if no entity can be found
        /// with the provided name.</returns>
        public T GetElementByName<T>(string name) where T : Element
        {
            var found = this.Elements.FirstOrDefault(e => e.Value.Name == name);
            if (found.Equals(new KeyValuePair<long, Element>()))
            {
                return null;
            }
            return (T)found.Value;
        }

        /// <summary>
        /// Get all elements of the type T.
        /// </summary>
        /// <typeparam name="T">The type of element to return.</typeparam>
        /// <returns>A collection of elements of the specified type.</returns>
        public IEnumerable<T> AllElementsOfType<T>() where T : Element
        {
            return Elements.Values.OfType<T>();
        }

        /// <summary>
        /// Get all elements assignable from type T. This will include
        /// types which derive from T and types which implement T if T 
        /// is an interface.
        /// </summary>
        /// <typeparam name="T">The type of the element from which returned elements derive.</typeparam>
        /// <returns>A collection of elements derived from the specified type.</returns>
        public IEnumerable<T> AllElementsAssignableFromType<T>() where T : Element
        {
            return Elements.Values.Where(e => typeof(T).IsAssignableFrom(e.GetType())).Cast<T>();
        }

        /// <summary>
        /// Serialize the model to JSON.
        /// </summary>
        public string ToJson(bool indent = false, bool gatherSubElements = true)
        {
            // var exportModel = CreateExportModel(gatherSubElements);

            var serializerOptions = new JsonSerializerOptions
            {
                WriteIndented = indent
            };
            serializerOptions.Converters.Add(new ElementConverterFactory());
            return JsonSerializer.Serialize(this, serializerOptions);
        }

        /// <summary>
        /// Serialize the model to a JSON file.
        /// </summary>
        /// <param name="path">The path of the file on disk.</param>
        /// <param name="gatherSubElements"></param>
        public void ToJson(string path, bool gatherSubElements = true)
        {
            var exportModel = CreateExportModel(gatherSubElements);

            // Json.net recommends writing to a stream for anything over 85k to avoid a string on the large object heap.
            // https://www.newtonsoft.com/json/help/html/Performance.htm
            using (FileStream s = File.Create(path))
            {
                JsonSerializer.Serialize(s, exportModel);
            }
        }

        internal Model CreateExportModel(bool gatherSubElements)
        {
            // Recursively add elements and sub elements in the correct
            // order for serialization. We do this here because element properties
            // may have been null when originally added, and we need to ensure
            // that they have a value if they've been set since.
            var exportModel = new Model();
            foreach (var kvp in this.Elements)
            {
                exportModel.AddElement(kvp.Value, gatherSubElements);
            }
            exportModel.Transform = this.Transform;
            return exportModel;
        }

        /// <summary>
        /// Deserialize a model from JSON.
        /// </summary>
        /// <param name="json">The JSON representing the model.</param>
        public static Model FromJson(string json)
        {
            var sw = new Stopwatch();
            sw.Start();
            var typeCache = AppDomainTypeCache.BuildAppDomainTypeCache(out _);
            Console.WriteLine($"{sw.ElapsedMilliseconds}ms for creating type cache.");

            Model model = null;
            using (var document = JsonDocument.Parse(json))
            {
                JsonElement root = document.RootElement;
                JsonElement elementsElement = root.GetProperty("Elements");

                var options = new JsonSerializerOptions()
                {
                    PropertyNameCaseInsensitive = true,
                };

                // Our custom reference handler will cache elements by id as
                // they are deserialized, supporting reading elements by id
                // from JSON.
                var refHandler = new ElementReferenceHandler(typeCache, elementsElement);
                options.ReferenceHandler = refHandler;

                // We use the model converter here so that we have a chance to 
                // intercept the creation of elements when things go wrong.
                options.Converters.Add(new ModelConverter());

                model = JsonSerializer.Deserialize<Model>(json, options);

                // Resetting the reference handler, empties the internal
                // elements cache.
                refHandler.Reset(typeCache, elementsElement);
            }

            return model;
        }

        private List<Element> RecursiveGatherSubElements(object obj)
        {
            // A dictionary created for the purpose of caching properties
            // that we need to recurse, for types that we've seen before.
            var props = new Dictionary<Type, List<PropertyInfo>>();

            return RecursiveGatherSubElementsInternal(obj, props);
        }

        private List<Element> RecursiveGatherSubElementsInternal(object obj, Dictionary<Type, List<PropertyInfo>> properties)
        {
            var elements = new List<Element>();

            if (obj == null)
            {
                return elements;
            }

            var e = obj as Element;
            if (e != null && Elements.ContainsKey(e.Id))
            {
                // Do nothing. The Element has already
                // been added. This assumes that that the sub-elements
                // have been added as well and we don't need to continue.
                return elements;
            }

            var t = obj.GetType();

            // Ignore value types and strings
            // as they won't have properties that
            // could be elements.
            if (!t.IsClass || t == typeof(string))
            {
                return elements;
            }

            List<PropertyInfo> constrainedProps;
            if (properties.ContainsKey(t))
            {
                constrainedProps = properties[t];
            }
            else
            {
                // This query had a nice little speed boost when we filtered for
                // valid types first then filtered for custom attributes.
                constrainedProps = t.GetProperties(BindingFlags.Public | BindingFlags.Instance).Where(p => IsValidForRecursiveAddition(p.PropertyType) && p.GetCustomAttribute<System.Text.Json.Serialization.JsonIgnoreAttribute>() == null).ToList();
                properties.Add(t, constrainedProps);
            }

            foreach (var p in constrainedProps)
            {
                try
                {
                    var pValue = p.GetValue(obj, null);
                    if (pValue == null)
                    {
                        continue;
                    }

                    if (pValue is IList elems)
                    {
                        foreach (var item in elems)
                        {
                            elements.AddRange(RecursiveGatherSubElementsInternal(item, properties));
                        }
                        continue;
                    }

                    // Get the properties dictionaries.
                    if (pValue is IDictionary dict)
                    {
                        foreach (var value in dict.Values)
                        {
                            elements.AddRange(RecursiveGatherSubElementsInternal(value, properties));
                        }
                        continue;
                    }

                    elements.AddRange(RecursiveGatherSubElementsInternal(pValue, properties));
                }
                catch (Exception ex)
                {
                    throw new Exception($"The {p.Name} property or one of its children was not valid for introspection. Check the inner exception for details.", ex);
                }
            }

            if (e != null)
            {
                elements.Add(e);
            }

            return elements;
        }

        /// <summary>
        /// Check whether a type is valid for introspection.
        /// TODO: When representations become elements, we should
        /// remove the inclusion for Representation, but keep that
        /// for SolidOperation.
        /// </summary>
        /// <param name="t">The type to check.</param>
        /// <returns>Return true if a type is valid for introspection, otherwise false.</returns>
        internal static bool IsValidForRecursiveAddition(Type t)
        {
            if (t.IsGenericType)
            {
                var genT = t.GetGenericArguments();
                if (genT.Length == 1)
                {
                    if (typeof(IList<>).MakeGenericType(genT[0]).IsAssignableFrom(t))
                    {
                        if (!IsValidListType(genT[0]))
                        {
                            return false;
                        }

                        return true;
                    }
                }
                else if (genT.Length == 2)
                {
                    if (typeof(IDictionary<,>).MakeGenericType(genT).IsAssignableFrom(t))
                    {
                        if (typeof(Element).IsAssignableFrom(genT[1]))
                        {
                            return true;
                        }
                        return false;
                    }
                }
            }

            if (t.IsArray)
            {
                return typeof(Element).IsAssignableFrom(t.GetElementType());
            }

            return typeof(Element).IsAssignableFrom(t)
                   || typeof(Representation).IsAssignableFrom(t)
                   || typeof(SolidOperation).IsAssignableFrom(t);
        }

        private static bool IsValidListType(Type t)
        {
            return typeof(Element).IsAssignableFrom(t)
                   || typeof(SolidOperation).IsAssignableFrom(t);
        }
    }

    public static class ModelExtensions
    {
        /// <summary>
        /// Get all elements of a certain type from a specific model name in a dictionary of models.
        /// </summary>
        /// <param name="models">Dictionary of models keyed by string.</param>
        /// <param name="modelName">The name of the model.</param>
        /// <typeparam name="T">The type of element we want to retrieve.</typeparam>
        /// <returns></returns>
        public static List<T> AllElementsOfType<T>(this Dictionary<string, Model> models, string modelName) where T : Element
        {
            var elements = new List<T>();
            models.TryGetValue(modelName, out var model);
            if (model != null)
            {
                elements.AddRange(model.AllElementsOfType<T>());
            }
            return elements;
        }

        /// <summary>
        /// Get all proxies of a certain type from a specific model name in a dictionary of models.
        /// </summary>
        /// <param name="models">Dictionary of models keyed by string</param>
        /// <param name="modelName">The name of the model</param>
        /// <typeparam name="T">The type of element we want to retrieve</typeparam>
        /// <returns></returns>
        public static List<ElementProxy<T>> AllProxiesOfType<T>(this Dictionary<string, Model> models, string modelName) where T : Element
        {
            return models.AllElementsOfType<T>(modelName).Proxies(modelName).ToList();
        }
    }
}