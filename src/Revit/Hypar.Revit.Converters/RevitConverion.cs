
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
        public static Elements.Model RunConverters(Dictionary<BuiltInCategory, Element[]> elements, Document document)
        {
            var converters = GetAllConverters();
            var model = new Elements.Model();

            foreach (var converter in converters)
            {
                Type first = converter.GetInterfaces()[0].GenericTypeArguments[0];
                Type second = converter.GetInterfaces()[0].GenericTypeArguments[1];

                var naive = Activator.CreateInstance(converter);

                var naivemethod = naive.GetType().GetMethod("FromRevit");
                var cat = (BuiltInCategory)converter.GetProperty("Category").GetValue(naive);
                // var elem = elements[cat].First();
                foreach (var elem in elements[cat])
                {
                    var result = naivemethod.Invoke(naive, new object[] { Convert.ChangeType(elem, first), document });

                    model.AddElements(result as Elements.Element[]);
                }
            }

            return model;
        }
        private static Type[] GetAllConverters()
        {
            var converterInterface = typeof(IFromRevitConverter<,>);

            var converters = new List<Type>();

            foreach (var t in Assembly.GetExecutingAssembly().GetTypes())
            {
                if (t.IsAbstract) continue;
                if (t.IsInterface) continue;
                var inter = t.GetInterface("IFromRevitConverter`2");
                var allInter = t.GetInterfaces();
                if (inter != null)
                {
                    converters.Add(t);
                }
            }

            return converters.ToArray();
        }
    }
}