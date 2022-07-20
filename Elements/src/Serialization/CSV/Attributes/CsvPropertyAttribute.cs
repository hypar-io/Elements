using System;

namespace Elements
{
    /// <summary>
    /// The CSV property attribute
    /// </summary>
    public class CsvPropertyAttribute : Attribute
    {
        /// <summary>
        /// Gets or sets name of the column inside csv file
        /// </summary>
        public string Name { get; set; }
    }
}