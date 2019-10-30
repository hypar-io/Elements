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
        /// Each property of the element which has a type of Element
        /// will be added to the entities collection before the element itself.
        /// This enables serializers to reference Elements by id.
        /// Properties which are IList of Element will have each of their items
        /// added to the entities collection as well.  
        /// </summary>
        /// <param name="element">The element to add to the model.</param>
        /// <exception>Thrown when an element 
        /// with the same Id already exists in the model.</exception>
        public void AddElement(Element element)
        {
            if (element == null || 
                this.Elements.ContainsKey(element.Id))
            {
                return;
            }

            RecursiveExpandElementData(element);
            this.Elements.Add(element.Id, element);
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
            return Newtonsoft.Json.JsonConvert.SerializeObject(this);
        }

        /// <summary>
        /// Deserialize a model from JSON.
        /// </summary>
        /// <param name="data">The JSON representing the model.</param>
        /// <param name="errors">A collection of deserialization errors.</param>
        public static Model FromJson(string data, List<string> errors = null)
        {
            errors = errors ?? new List<string>();
            var model = Newtonsoft.Json.JsonConvert.DeserializeObject<Model>(data, new JsonSerializerSettings(){
                Error = (sender, args)=>{
                    errors.Add(args.ErrorContext.Error.Message);
                    args.ErrorContext.Handled = true;
                }
            });
            JsonInheritanceConverter.Identifiables.Clear();
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

        private void RecursiveExpandElementData(object element)
        {
            var props = element.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance);
            foreach(var p in props)
            {
                var pValue = p.GetValue(element);
                if(pValue == null)
                {
                    continue;
                }
                
                if (typeof(Element).IsAssignableFrom(p.PropertyType))
                {
                    var ident =(Element)pValue;
                    if(this.Elements.ContainsKey(ident.Id))
                    {
                        this.Elements[ident.Id] = ident;
                    }
                    else
                    {
                        this.Elements.Add(ident.Id, ident);
                    }
                }
                else if(p.PropertyType.IsGenericType && 
                        p.PropertyType.GetGenericTypeDefinition() == typeof(List<>))
                {
                    var listType = p.PropertyType.GetGenericArguments()[0];
                    if(typeof(Element).IsAssignableFrom(listType))
                    {
                        var list = (IList)pValue;
                        foreach(Element e in list)
                        {
                            AddElement(e);
                        }
                    }
                }
            }
        }
    }
}