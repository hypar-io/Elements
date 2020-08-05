using System;
using System.Collections.Generic;
using System.IO;
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
        const string CONVERTER_FOLDER_NAME = "Converters";
        private static List<object> _converters = null;

        public static List<object> Converters
        {
            get
            {
                if (_converters == null)
                {
                    _converters = LoadAllConverters();
                }
                return _converters;
            }
        }
        private static BuiltInCategory[] _allCategories = null;
        public static BuiltInCategory[] AllCategories
        {
            get
            {
                if (_allCategories == null)
                {
                    _allCategories = Converters.Select(converter => (BuiltInCategory)converter.GetType().GetProperty("Category").GetValue(converter)).ToArray();
                }
                return _allCategories;
            }

        }

        /// <summary>
        /// Run all converters available on the dictionary of incoming elements.
        /// </summary>
        /// <param name="elements">Dictionary of elements grouped by their BuiltInCategory.</param>
        /// <param name="document">The Revit document where elements originated.</param>
        /// <param name="conversionExceptions">An outgoing list of exceptions that occurred during converting elements.</param>
        public static Elements.Model RunConverters(Element[] elements, Document document, out List<Exception> conversionExceptions)
        {
            var model = new Elements.Model();
            conversionExceptions = new List<Exception>();

            // TODO use delegates to improve speed https://stackoverflow.com/questions/10313979/methodinfo-invoke-performance-issue 
            // blog ref https://blogs.msmvps.com/jonskeet/2008/08/09/making-reflection-fly-and-exploring-delegates/

            var elementGroups = elements.GroupBy(e => (BuiltInCategory)e.Category.Id.IntegerValue).ToDictionary(g => g.Key);

            foreach (var converter in Converters)
            {
                var category = (BuiltInCategory)converter.GetType().GetProperty("Category").GetValue(converter);
                if (!elementGroups.ContainsKey(category))
                {
                    continue;
                }
                Type revitType = converter.GetType().GetInterfaces()[0].GenericTypeArguments[0];

                var fromRevitMethod = converter.GetType().GetMethod("FromRevit");
                foreach (var elem in elementGroups[category])
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

        private static List<object> LoadAllConverters()
        {
            var converterInterface = typeof(IRevitConverter<,>);

            var converters = new List<object>();
            foreach (var converterType in GetAllConverterTypes())
            {
                var instanceOfConverter = Activator.CreateInstance(converterType);
                var cat = (BuiltInCategory)converterType.GetProperty("Category").GetValue(instanceOfConverter);
                converters.Add((object)instanceOfConverter);
            }

            return converters;
        }

        private static bool IsTypeAConverter(Type t)
        {
            if (t.IsAbstract || t.IsInterface)
            {
                return false;
            }
            return t.GetInterface("IRevitConverter`2") != null;
        }

        private static Type[] GetAllConverterTypes()
        {
            var defaultConverterTypes = Assembly.GetExecutingAssembly().GetTypes().Where(t => IsTypeAConverter(t));

            var converterFolderPath = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), CONVERTER_FOLDER_NAME);
            var customConverterTypes = LoadCustomConverterTypes(converterFolderPath);

            return defaultConverterTypes.Concat(customConverterTypes).ToArray();
        }

        private static Type[] LoadCustomConverterTypes(string converterFolderPath)
        {
            var converterAssemblies = new List<Assembly>();

            if (Directory.Exists(converterFolderPath))
            {
                var dllPaths = Directory.EnumerateFiles(converterFolderPath, "*.dll");
                foreach (string dllPath in dllPaths)
                {
                    try
                    {
                        var loaded = AppDomain.CurrentDomain.Load(dllPath);
                        converterAssemblies.Add(loaded);
                    }
                    catch
                    {
                        // TODO log to the local log file that an assembly could not be loaded
                        continue;
                    }
                }
            }
            // TODO in an `else` log to the local log file that a folder was expected but did not exist
            return converterAssemblies.SelectMany(a => a.GetTypes().Where(t => IsTypeAConverter(t))).ToArray();
        }
    }
}