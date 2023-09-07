using System;
using System.Collections.Generic;
using System.Reflection;

namespace Elements.Utilities
{
    /// <summary>
    /// Helpfull reflection methods
    /// </summary>
    internal static class ReflectionUtils
    {
        /// <summary>
        /// Try to fin property by path inside an object (e.g. RepresentationInstances[0].Material)
        /// </summary>
        /// <param name="element">The object that potentially has the property by the path</param>
        /// <param name="path">The property path</param>
        /// <param name="objectInstanceWithProperty">The object instance with the property.
        ///  (e.g. Materil instance if the path is RepresentationInstances[0].Material)</param>
        /// <param name="propertyInfo">The property info</param>
        /// <returns></returns>
        public static bool TryFindPropertyByPath(object element, string path, out object objectInstanceWithProperty, out PropertyInfo propertyInfo)
        {
            if (element == null)
            {
                throw new ArgumentNullException("value");
            }
            if (path == null)
            {
                throw new ArgumentNullException("path");
            }

            Type currentType = element.GetType();
            propertyInfo = null;
            objectInstanceWithProperty = element;
            object propertyValue = element;

            foreach (string propertyName in path.Split('.'))
            {
                if (currentType != null)
                {
                    objectInstanceWithProperty = propertyValue;
                    propertyInfo = null;
                    int brackStart = propertyName.IndexOf("[");
                    int brackEnd = propertyName.IndexOf("]");

                    propertyInfo = currentType.GetProperty(brackStart > 0 ? propertyName.Substring(0, brackStart) : propertyName);
                    propertyValue = propertyInfo.GetValue(objectInstanceWithProperty, null);

                    if (propertyInfo == null)
                    {
                        return false;
                    }

                    if (brackStart > 0 && propertyValue != null)
                    {
                        string index = propertyName.Substring(brackStart + 1, brackEnd - brackStart - 1);
                        foreach (Type iType in propertyValue.GetType().GetInterfaces())
                        {
                            if (iType.IsGenericType && iType.GetGenericTypeDefinition() == typeof(IDictionary<,>))
                            {
                                propertyValue = typeof(ReflectionUtils).GetMethod("GetDictionaryElement")
                                                     .MakeGenericMethod(iType.GetGenericArguments())
                                                     .Invoke(null, new object[] { propertyValue, index });
                                break;
                            }
                            if (iType.IsGenericType && iType.GetGenericTypeDefinition() == typeof(IList<>))
                            {
                                propertyValue = typeof(ReflectionUtils).GetMethod("GetListElement")
                                                     .MakeGenericMethod(iType.GetGenericArguments())
                                                     .Invoke(null, new object[] { propertyValue, index });
                                break;
                            }
                        }
                    }

                    currentType = propertyValue?.GetType();
                }
                else
                {
                    return false;
                }
            }
            return true;
        }

        public static TValue GetDictionaryElement<TKey, TValue>(IDictionary<TKey, TValue> dictionary, object index)
        {
            TKey key = (TKey)Convert.ChangeType(index, typeof(TKey), null);
            return dictionary[key];
        }

        public static T GetListElement<T>(IList<T> list, object index)
        {
            return list[Convert.ToInt32(index)];
        }
    }
}