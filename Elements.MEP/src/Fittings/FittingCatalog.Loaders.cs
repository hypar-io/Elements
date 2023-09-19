using glTFLoader.Schema;
using Microsoft.CodeAnalysis.CSharp;
using System;
using System.Collections.Generic;
using System.Data.SqlTypes;
using System.IO;
using System.Linq;

namespace Elements.Fittings
{
    public partial class FittingCatalog
    {
        /// <summary>
        /// Load fitting parts from csv.
        /// </summary>
        /// <typeparam name="T">Type of fitting part.</typeparam>
        /// <param name="filePath">Filepath.</param>
        /// <param name="parsingFunc">Function that parses csv file row with column header to index map into part type.
        /// A function that converts csv cells of row and a column header to index map to an instance of fitting part type.</param>
        /// <returns>Loaded fitting parts.</returns>
        public static List<T> LoadFittingPartsFromCsv<T>(string filePath, Func<string[], Dictionary<string, int>, T> parsingFunc)
        {
            var parts = new List<T>();
            if (!File.Exists(filePath))
            {
                var wholePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, filePath);
                if (!File.Exists(wholePath))
                {
                    return parts;
                }
            }

            var lines = File.ReadAllLines(filePath);
            char cellDelimeter = ',';
            var headerlineIndex = 0;
            var headerToIndexMap = lines[headerlineIndex]
                .Split(cellDelimeter)
                .Select((x, i) => new { Header = x.Trim(), Index = i })
                .Where(pair => !string.IsNullOrWhiteSpace(pair.Header))
                .ToDictionary(x => x.Header, x => x.Index);
            foreach (var line in lines.Skip(headerlineIndex + 1))
            {
                var tokens = line.Split(cellDelimeter);
                var part = parsingFunc(tokens, headerToIndexMap);
                if (part != null)
                {
                    parts.Add(part);
                }
            }

            return parts;
        }

        /// <summary>
        /// Gets cell value from csv.
        /// </summary>
        /// <param name="header">Column header.</param>
        /// <param name="headerToIndexMap">Column header to column index map.</param>
        /// <param name="cells">Cells of row.</param>
        /// <returns>String value of cell.</returns>
        public static string GetCellValueCsv(string header, Dictionary<string, int> headerToIndexMap, string[] cells)
        {
            if (headerToIndexMap.ContainsKey(header) && headerToIndexMap[header] < cells.Length)
            {
                var s = cells[headerToIndexMap[header]];
                return s;
            }
            return string.Empty;
        }

        /// <summary>
        /// Get double value from  from csv. 
        /// </summary>
        /// <param name="name">Column name.</param>
        /// <param name="headerToIndexMap">Column header to column index map.</param>
        /// <param name="cells">Cells of row.</param>
        /// <param name="result">Value that had been read.</param>
        /// <param name="defaultValue">Optional default value, is set to "result" if named column is missing.</param>
        /// <returns>True if data is successfully retrieved.</returns>
        public static bool TryReadDouble(string name, 
                                         Dictionary<string, int> headerToIndexMap, 
                                         string[] cells,
                                         out double result,
                                         double? defaultValue = null)
        {
            var value = GetCellValueCsv(name, headerToIndexMap, cells);
            if (defaultValue.HasValue && string.IsNullOrEmpty(value))
            {
                result = defaultValue.Value;
                return true;
            }
            else
            {
                var parsed = double.TryParse(value, out result);
                return parsed;
            }
        }
    }
}