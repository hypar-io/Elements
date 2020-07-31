using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Autodesk.Revit.DB;


namespace Hypar.Revit.Converters
{
    public interface IRevitConverter<R, H>
    {
        BuiltInCategory Category { get; }
        H[] FromRevit(R revitElement, Autodesk.Revit.DB.Document doc);
    }

    // TODO this dictionary is to find an appropriate converter given the category of an element
    // Next step will be to have a dicionary to lookup converters basd on the hypar elements needed
    public static class ConversionRunner
    {
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
        public static Elements.Model RunConverters(Dictionary<BuiltInCategory, Element[]> elements, Document document, out List<Exception> conversionExceptions)
        {
            var model = new Elements.Model();
            conversionExceptions = new List<Exception>();

            // TODO use delegates to improve speed https://stackoverflow.com/questions/10313979/methodinfo-invoke-performance-issue 
            // blog ref https://blogs.msmvps.com/jonskeet/2008/08/09/making-reflection-fly-and-exploring-delegates/

            foreach (var converterKVP in Converters)
            {
                var converter = converterKVP.Value[0];
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
                if (converterType.IsAbstract) continue;
                if (converterType.IsInterface) continue;
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