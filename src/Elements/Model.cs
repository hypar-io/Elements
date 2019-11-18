#pragma warning disable CS1591

using Elements.GeoJSON;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Collections;
using Newtonsoft.Json;
using Elements.Serialization.JSON;
using Elements.Serialization.IFC;

namespace Elements
{
    /// <summary>
    /// A container for elements, element types, materials, and profiles.
    /// </summary>
    public partial class Model
    {
        /// <summary>
        /// Construct an empty model.
        /// </summary>
        public Model()
        {
            this.Origin = new Position(0, 0);
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
            if(element == null || this.Elements.ContainsKey(element.Id))
            {
                return;
            }

            if(gatherSubElements)
            {
                // Look at all public properties of the element. 
                // For all properties which inherit from element, add those
                // to the elements dictionary first. This will ensure that 
                // those elements will be read out and be available before 
                // an attempt is made to deserialize the element itself.
                var subElements = RecursiveGatherSubElements(element);
                foreach(var e in subElements)
                {
                    if(!this.Elements.ContainsKey(e.Id))
                    {
                        this.Elements.Add(e.Id, e);
                    }
                }
            }
            else
            {
                if(!this.Elements.ContainsKey(element.Id))
                {
                    this.Elements.Add(element.Id, element);
                }
            }
        }

        /// <summary>
        /// Add a collection of elements to the model.
        /// </summary>
        /// <param name="elements">The elements to add to the model.</param>
        public void AddElements(IEnumerable<Element> elements)
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
        public T GetElementOfType<T>(Guid id) where T: Element
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
        public T GetElementByName<T>(string name) where T: Element
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
        public string ToJson() 
        {
            // Recursively add elements and sub elements in the correct
            // order for serialization. We do this here because element properties
            // may have been null when originally added, and we need to ensure
            // that they have a value if they've been set since.
            var exportModel = new Model();
            foreach(var kvp in this.Elements)
            {
                exportModel.AddElement(kvp.Value);
            }
            exportModel.Origin = this.Origin;
            return Newtonsoft.Json.JsonConvert.SerializeObject(exportModel);
        }

        /// <summary>
        /// Deserialize a model from JSON.
        /// </summary>
        /// <param name="json">The JSON representing the model.</param>
        /// <param name="errors">A collection of deserialization errors.</param>
        public static Model FromJson(string json, List<string> errors = null)
        {
            errors = errors ?? new List<string>();
            var model = Newtonsoft.Json.JsonConvert.DeserializeObject<Model>(json, new JsonSerializerSettings(){
                Error = (sender, args)=>{
                    errors.Add(args.ErrorContext.Error.Message);
                    args.ErrorContext.Handled = true;
                }
            });
            JsonInheritanceConverter.Elements.Clear();
            return model;
        }

        /// <summary>
        /// Serialize the model to IFC.
        /// </summary>
        /// <param name="path">The output path for the IFC file.</param>
        public void ToIFC(string path)
        {
            IFCModelExtensions.ToIFC(this, path);
        }

        /// <summary>
        /// Deserialize a model from IFC.
        /// </summary>
        /// <param name="path">The path to the IFC file.</param>
        /// <param name="idsToConvert">An optional collection of IFC identifiers to convert.</param>
        public static Model FromIFC(string path, string[] idsToConvert = null)
        {
            return IFCModelExtensions.FromIFC(path, idsToConvert);
        }

        private static List<Element> RecursiveGatherSubElements(object obj)
        {
            var elements = new List<Element>();

            if(obj == null)
            {
                return elements;
            }

            var t = obj.GetType();

            // Ignore value types and strings
            // as they won't have properties that
            // could be elements.
            if(!t.IsClass || t == typeof(string))
            {
                return elements;
            }
            t.GetCustomAttribute(typeof(JsonIgnoreAttribute));
            var props = t.GetProperties(BindingFlags.Public | BindingFlags.Instance)
                         .Where(p=>p.GetCustomAttribute(typeof(JsonIgnoreAttribute)) != null);
            foreach(var p in props)
            {
                var pValue = p.GetValue(obj,null);
                if(pValue == null)
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
                }
                else {
                    elements.AddRange(RecursiveGatherSubElements(pValue));
                }
            }

            var e = obj as Element;
            if(e != null)
            {
                elements.Add(e);
            }

            return elements;
        }
    }
}