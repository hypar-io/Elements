using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Autodesk.Revit.DB;


namespace Hypar.Revit.Converters
{
    public interface IRevitConverter<TRevit, THypar>
    {
        BuiltInCategory Category { get; }
        THypar[] FromRevit(TRevit revitElement, Autodesk.Revit.DB.Document doc);
    }

    public static class ConversionRunner
    {
        // TODO this dictionary is to find an appropriate converter given the category of an element.
        // Next step will be to have a dictionary to lookup converters based on the hypar elements needed.
        private static Dictionary<BuiltInCategory, List<object>> _converters = null;

        public static Dictionary<BuiltInCategory, List<object>> Converters
        {
            get
            {
                if (_converters == null)
                {
                    _converters = GetAllConverters();
                }
                return _converters;
            }
        }

        /// <summary>
        /// Run all converters available on the dictionary of incoming elements.
        /// </summary>
        /// <param name="elements">Dictionary of elements grouped by their BuiltInCategory.</param>
        /// <param name="document">The Revit document where elements originated.</param>
        /// <param name="conversionExceptions">An outgoing list of exceptions that occurred during converting elements.</param>
        public static Elements.Model RunConverters(Dictionary<BuiltInCategory, Element[]> elements, Document document, out List<Exception> conversionExceptions)
        {
            var model = new Elements.Model();
            conversionExceptions = new List<Exception>();

            // TODO use delegates to improve speed https://stackoverflow.com/questions/10313979/methodinfo-invoke-performance-issue 
            // blog ref https://blogs.msmvps.com/jonskeet/2008/08/09/making-reflection-fly-and-exploring-delegates/

            foreach (var converterList in Converters.Values)
            {
                var converter = converterList[0];
                Type revitType = converter.GetType().GetInterfaces()[0].GenericTypeArguments[0];

                var fromRevitMethod = converter.GetType().GetMethod("FromRevit");
                var cat = (BuiltInCategory)converter.GetType().GetProperty("Category").GetValue(converter);
                foreach (var elem in elements[cat])
                {
                    try
                    {
                        var result = fromRevitMethod.Invoke(converter, new object[] { Convert.ChangeType(elem, revitType), document });
                        model.AddElements(result as Elements.Element[]);
                    }
                    catch (Exception e)
                    {
                        conversionExceptions.Add(e);
                    }
                }
            }

            return model;
        }

        private static Dictionary<BuiltInCategory, List<object>> GetAllConverters()
        {
            var converterInterface = typeof(IRevitConverter<,>);

            var converters = new Dictionary<BuiltInCategory, List<object>>();
            foreach (var converterType in Assembly.GetExecutingAssembly().GetTypes())
            {
                if (converterType.IsAbstract || converterType.IsInterface) continue;
                var inter = converterType.GetInterface("IRevitConverter`2");
                if (inter != null)
                {
                    var instanceOfConverter = Activator.CreateInstance(converterType);
                    var cat = (BuiltInCategory)converterType.GetProperty("Category").GetValue(instanceOfConverter);
                    if (!converters.ContainsKey(cat))
                    {
                        converters[cat] = new List<object>();
                    }
                    converters[cat].Add((object)instanceOfConverter);
                }
            }

            return converters;
        }
    }
}