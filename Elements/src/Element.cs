#pragma warning disable CS1591


using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Newtonsoft.Json;

namespace Elements
{
    /// <summary>
    /// An object which is identified 
    /// with a unique identifier and a name.
    /// </summary>
    public abstract partial class Element
    {
        // This partial definition exists only to mark the class as abstract.

        internal virtual int SortPriority => 0;

        /// <summary>
        /// The default implementation of GatherSubElements does a recursive 
        /// search for sub elements to be stored in the model. Override this
        /// method to provide a more precise search.
        /// </summary>
        /// <param name="elements">The dictionary to contain all sub elements</param>
        internal virtual void GatherSubElements(Dictionary<Guid, Element> elements)
        {
            // Look at all public properties of the element.
            // For all properties which inherit from element, add those
            // to the elements dictionary first. This will ensure that
            // those elements will be read out and be available before
            // an attempt is made to deserialize the element itself.
            RecursiveGatherSubElements(this, elements);
        }

        private static HashSet<Type> SkipTypes = new HashSet<Type>
        {
            typeof(string),
            typeof(int),
            typeof(uint),
            typeof(double),
            typeof(decimal),
            typeof(short),
            typeof(ushort),
            typeof(long),
            typeof(ulong)
        };


        private void RecursiveGatherSubElements(object obj, Dictionary<Guid, Element> elements)
        {
            if (obj == null)
            {
                return;
            }

            var t = obj.GetType();

            // Ignore value types and strings
            // as they won't have properties that
            // could be elements.
            if (!t.IsClass || SkipTypes.Contains(t))
            {
                return;
            }

            var e = obj as Element;

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
                        RecursiveGatherSubElements(item, elements);
                    }
                    continue;
                }

                var dict = pValue as IDictionary;
                if (dict != null)
                {
                    foreach (var value in dict.Values)
                    {
                        RecursiveGatherSubElements(value, elements);
                    }
                    continue;
                }

                RecursiveGatherSubElements(pValue, elements);
            }

            if (e != null)
            {
                if (!elements.ContainsKey(e.Id))
                {
                    elements.Add(e.Id, e);
                }
            }
        }
    }
}