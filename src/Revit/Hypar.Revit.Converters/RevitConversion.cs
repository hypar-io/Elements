using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Autodesk.Revit.DB;


namespace Hypar.Revit.Converters
{
    public interface IFromRevitConverter<R, H>
    {
        BuiltInCategory Category { get; }
        H[] FromRevit(R revitElement, Autodesk.Revit.DB.Document doc);
    }

    public static class ConversionRunner
    {
        static Type[] _converters = null;
        static Type[] Converters
        {
            get
            {
                if (_converters == null)
                {
                    GetAllConverters();
                }
                return _converters;
            }
        }
        public static BuiltInCategory[] AllCategories
        {
            get
            {
                return Converters.Select(converter =>
                {
                    var naive = Activator.CreateInstance(converter);
                    var cat = (BuiltInCategory)converter.GetProperty("Category").GetValue(naive);
                    return cat;
                }).ToArray();
            }
        }
        public static Elements.Model RunConverters(Dictionary<BuiltInCategory, Element[]> elements, Document document, out List<Exception> conversionExceptions)
        {
            var model = new Elements.Model();
            conversionExceptions = new List<Exception>();

            foreach (var converter in Converters)
            {
                Type revitType = converter.GetInterfaces()[0].GenericTypeArguments[0];
                Type hyparType = converter.GetInterfaces()[0].GenericTypeArguments[1];

                var naive = Activator.CreateInstance(converter);

                var naivemethod = naive.GetType().GetMethod("FromRevit");
                var cat = (BuiltInCategory)converter.GetProperty("Category").GetValue(naive);
                foreach (var elem in elements[cat])
                {
                    try
                    {
                        var result = naivemethod.Invoke(naive, new object[] { Convert.ChangeType(elem, revitType), document });
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
        private static void GetAllConverters()
        {
            var converterInterface = typeof(IFromRevitConverter<,>);
            var converters = new List<Type>();

            foreach (var t in Assembly.GetExecutingAssembly().GetTypes())
            {
                if (t.IsAbstract) continue;
                if (t.IsInterface) continue;
                var inter = t.GetInterface("IFromRevitConverter`2");
                if (inter != null)
                {
                    converters.Add(t);
                }
            }

            _converters = converters.ToArray();
        }
    }
}