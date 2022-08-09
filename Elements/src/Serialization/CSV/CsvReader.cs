using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Reflection;

namespace Elements
{
    /// <summary>
    /// Reads and parses CSV file
    /// </summary>
    public class CsvReader
    {
        #region Constants

        /// <summary>
        /// The delimeter that is used inside CSV file
        /// </summary>
        protected const char delimiterChar = ',';

        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets amount of rows that should be skiped after first row with column names.
        /// This property should be used if column name takes more than one row.
        /// </summary>
        public int SkipRows { get; set; } = 0;

        #endregion

        #region Public logic

        /// <summary>
        /// Read data from the CSV file
        /// </summary>
        /// <typeparam name="T">The type of the elemens inside CSV file</typeparam>
        /// <param name="filePath">The file path</param>
        public virtual IList<T> Read<T>(string filePath) where T : class, new()
        {
            if (!File.Exists(filePath))
            {
                throw new ArgumentException($"File {filePath} doesn't exist");
            }
            using (var stream = File.OpenRead(filePath))
            {
                return Read<T>(stream).ToList();
            }
        }

        /// <summary>
        /// Read data from the CSV file
        /// </summary>
        /// <typeparam name="T">The type of the elemens inside the CSV file</typeparam>
        /// <param name="stream">The file stream</param>
        public virtual IList<T> Read<T>(Stream stream) where T : class, new()
        {
            var properties = CollectTypeInfo<T>();

            string[] columns;
            string[] rows;

            try
            {
                using (var sr = new StreamReader(stream))
                {
                    columns = sr.ReadLine().Split(delimiterChar);
                    rows = sr.ReadToEnd().Split(new string[] { Environment.NewLine }, StringSplitOptions.None);
                }
            }
            catch (Exception)
            {
                return null;
            }

            Dictionary<string, PropertyInfo> propertiesDescription = new Dictionary<string, PropertyInfo>();
            foreach (var property in properties)
            {
                string name = GetPropertyName(property);
                string columnName = columns.FirstOrDefault(column => column.Equals(name, StringComparison.InvariantCultureIgnoreCase));

                if (string.IsNullOrEmpty(columnName))
                    continue;

                if (!propertiesDescription.ContainsKey(columnName))
                    propertiesDescription.Add(columnName, property);
            }

            var data = new List<T>();

            for (int row = SkipRows; row < rows.Length; row++)
            {
                var line = rows[row];

                if (string.IsNullOrWhiteSpace(line))
                    continue;

                var parts = line.Split(delimiterChar);

                if (parts.Length == 1 && parts[0] != null && parts[0] == "EOF")
                    break;

                var instance = new T();
                for (int i = 0; i < parts.Length; i++)
                {
                    var value = parts[i];
                    var column = columns[i];

                    if (!propertiesDescription.ContainsKey(column))
                        continue;

                    var property = propertiesDescription[column];
                    value = SetValue(instance, value, property);
                }

                data.Add(instance);
            }

            return data;
        }

        /// <summary>
        /// Sets value to the instance property
        /// </summary>
        /// <typeparam name="T">The type of the instance</typeparam>
        /// <param name="instance">The instance of the type</param>
        /// <param name="value">The property value</param>
        /// <param name="property">The instance property</param>
        protected static string SetValue<T>(T instance, string value, PropertyInfo property) where T : class, new()
        {
            if (property.PropertyType == typeof(string) && !string.IsNullOrWhiteSpace(value))
            {
                if (value.IndexOf("\"") == 0)
                    value = value.Substring(1);

                if (value[value.Length - 1].ToString() == "\"")
                    value = value.Substring(0, value.Length - 1);
            }

            var converter = TypeDescriptor.GetConverter(property.PropertyType);
            object convertedvalue = GetDefaultValue(property.PropertyType);

            if (!string.IsNullOrEmpty(value))
            {
                convertedvalue = converter.ConvertFrom(value);
            }

            property.SetValue(instance, convertedvalue);
            return value;
        }

        #endregion

        #region Private logic

        private static object GetDefaultValue(Type type)
        {
            return type.IsValueType ? Activator.CreateInstance(type) : null;
        }

        /// <summary>
        /// Collects all public properties from the type
        /// </summary>
        /// <typeparam name="T">The type</typeparam>
        protected static IEnumerable<PropertyInfo> CollectTypeInfo<T>()
        {
            var type = typeof(T);
            var properties = type.GetProperties(BindingFlags.Public | BindingFlags.Instance
                | BindingFlags.GetProperty | BindingFlags.SetProperty);

            var query = properties.AsQueryable().Where(a => a.PropertyType.IsValueType || a.PropertyType.Name == "String");

            return from property in query
                   select property;
        }

        /// <summary>
        /// Gets the name of the property. It can name inside CsvPropertyAttribute or just property name
        /// </summary>
        /// <param name="property">The type property</param>
        protected static string GetPropertyName(PropertyInfo property)
        {
            var attribute = property.GetCustomAttribute<CsvPropertyAttribute>();

            if (attribute == null || string.IsNullOrEmpty(attribute.Name))
            {
                return property.Name;
            }
            else
            {
                return attribute.Name;
            }
        }

        #endregion
    }
}