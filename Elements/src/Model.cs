#pragma warning disable CS1591

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Collections;
using Newtonsoft.Json;
using Elements.Serialization.JSON;
using Elements.Geometry;
using Elements.Validators;

namespace Elements
{
    /// <summary>
    /// A container for elements.
    /// </summary>
    public partial class Model
    {
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
            var validator = Validator.Instance.GetFirstValidatorForType<Model>();
            if (validator != null)
            {
                validator.PreConstruct(new object[] { @transform, @elements });
            }

            this.Transform = @transform;
            this.Elements = @elements;
        }

        /// <summary>
        /// Add an element to the model.
        /// This operation recursively searches the element's properties
        /// for element sub-properties and adds those elements to the elements
        /// dictionary before adding the element itself.
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

                    // Whenever a property is changed on an element
                    // we do an additional add here to ensure that 
                    // any properties of element types were subsequently 
                    // added to the model.
                    element.PropertyChanged += (target, args) =>
                    {
                        var geom = element as GeometricElement;
                        if (geom != null)
                        {
                            geom.UpdateRepresentations();
                            AddElement(geom);
                        }
                    };
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
        /// Get all entities of the specified Type.
        /// </summary>
        /// <typeparam name="T">The Type of element to return.</typeparam>
        /// <returns>A collection of elements of the specified type.</returns>
        public IEnumerable<T> AllElementsOfType<T>()
        {
            return this.Elements.Values.OfType<T>();
        }

        /// <summary>
        /// Serialize the model to JSON.
        /// </summary>
        public string ToJson(bool indent = false)
        {
            return Newtonsoft.Json.JsonConvert.SerializeObject(this, indent ? Formatting.Indented : Formatting.None);
        }

        /// <summary>
        /// Deserialize a model from JSON.
        /// </summary>
        /// <param name="json">The JSON representing the model.</param>
        /// <param name="errors">A collection of deserialization errors.</param>
        public static Model FromJson(string json, List<string> errors = null)
        {
            // When user elements have been loaded into the app domain, they haven't always been
            // loaded into the InheritanceConverter's Cache.  This does have some overhead,
            // but is useful here, at the Model level, to ensure user types are available.
            JsonInheritanceConverter.RefreshUserElementTypeCache();
            errors = errors ?? new List<string>();
            var model = Newtonsoft.Json.JsonConvert.DeserializeObject<Model>(json, new JsonSerializerSettings()
            {
                Error = (sender, args) =>
                {
                    errors.Add(args.ErrorContext.Error.Message);
                    args.ErrorContext.Handled = true;
                }
            });
            JsonInheritanceConverter.Elements.Clear();
            return model;
        }

        private List<Element> RecursiveGatherSubElements(object obj)
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

            t.GetCustomAttribute(typeof(JsonIgnoreAttribute));
            var props = t.GetProperties(BindingFlags.Public | BindingFlags.Instance)
                         .Where(p => p.GetCustomAttribute(typeof(JsonIgnoreAttribute)) == null);
            foreach (var p in props)
            {
                var pValue = p.GetValue(obj, null);
                if (pValue == null)
                {
                    continue;
                }

                var elems = pValue as IList;
                if (elems != null)
                {
                    foreach (var item in elems)
                    {
                        elements.AddRange(RecursiveGatherSubElements(item));
                    }
                    continue;
                }

                // Get the properties dictionaries.
                var dict = pValue as IDictionary;
                if (dict != null)
                {
                    foreach (var value in dict.Values)
                    {
                        elements.AddRange(RecursiveGatherSubElements(value));
                    }
                    continue;
                }

                elements.AddRange(RecursiveGatherSubElements(pValue));
            }

            if (e != null)
            {
                elements.Add(e);
            }

            return elements;
        }
    }
}