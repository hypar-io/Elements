using System;
using System.Collections.Generic;
using System.IO;

namespace Elements.Fittings
{
    public class FittingsSizeManager
    {
        private static string _elbowFittingSettingsPath;
        private static Dictionary<double, double> _elbowDiameterToSideLengthLookup = null;
        internal static Dictionary<double, double> ElbowDiameterToSideLengthLookup
        {
            get
            {
                if (!string.IsNullOrWhiteSpace(_elbowFittingSettingsPath))
                {
                    _elbowDiameterToSideLengthLookup ??= FittingPropertiesFromTxt(_elbowFittingSettingsPath, ParseOneDoubleProperty);
                }
                return _elbowDiameterToSideLengthLookup;
            }
        }

        private static string _crossFittingSettingsPath;
        private static Dictionary<double, double> _crossDiameterToFullLengthLookup = null;
        internal static Dictionary<double, double> CrossDiameterToFullLengthLookup
        {
            get
            {
                if (!string.IsNullOrWhiteSpace(_crossFittingSettingsPath))
                {
                    _crossDiameterToFullLengthLookup ??= FittingPropertiesFromTxt(_crossFittingSettingsPath, ParseOneDoubleProperty);
                }
                return _crossDiameterToFullLengthLookup;
            }
        }

        private static string _teeFittingSettingsPath;
        private static Dictionary<double, (double longSidelength, double shortSideLength)> _teeDiameterToLengthLookup = null;
        internal static Dictionary<double, (double longSidelength, double shortSideLength)> TeeDiameterToLengthLookup
        {
            get
            {
                if (!string.IsNullOrWhiteSpace(_teeFittingSettingsPath))
                {
                    _teeDiameterToLengthLookup ??= FittingPropertiesFromTxt(_teeFittingSettingsPath, ParseTwoDoubleProperties);
                }
                return _teeDiameterToLengthLookup;
            }
        }

        private static string _reducerFittingSettingsPath;
        private static Dictionary<double, double> _reducerDiameterToLengthLookup = null;
        internal static Dictionary<double, double> ReducerDiameterToLengthLookup
        {
            get
            {
                if (!string.IsNullOrWhiteSpace(_reducerFittingSettingsPath))
                {
                    _reducerDiameterToLengthLookup ??= FittingPropertiesFromTxt(_reducerFittingSettingsPath, ParseOneDoubleProperty);
                }
                return _reducerDiameterToLengthLookup;
            }
        }

        public static void SetElbowFittingSettingsPath(string path)
        {
            _elbowFittingSettingsPath = path;
            _elbowDiameterToSideLengthLookup = null;
        }

        public static void SetCrossFittingSettingsPath(string path)
        {
            _crossFittingSettingsPath = path;
            _crossDiameterToFullLengthLookup = null;
        }

        public static void SetTeeFittingSettingsPath(string path)
        {
            _teeFittingSettingsPath = path;
            _teeDiameterToLengthLookup = null;
        }

        public static void SetReducerFittingSettingsPath(string path)
        {
            _reducerFittingSettingsPath = path;
            _reducerDiameterToLengthLookup = null;
        }

        private static double ParseOneDoubleProperty(string[] fields)
        {
            return double.Parse(fields[1]);
        }

        private static (double longSidelength, double shortSideLength) ParseTwoDoubleProperties(string[] fields)
        {
            return (double.Parse(fields[1]), double.Parse(fields[2]));
        }

        private static Dictionary<double, T> FittingPropertiesFromTxt<T>(string filepath, Func<string[], T> parsingFunc)
        {
            var properties = new Dictionary<double, T>();
            var wholePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, filepath);
            if (!File.Exists(wholePath))
            {
                return properties;
            }

            var lines = File.ReadAllLines(wholePath);
            foreach (var line in lines)
            {
                var fields = line.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                try
                {
                    properties.Add(double.Parse(fields[0]), parsingFunc(fields));
                }
                catch { }
            }
            return properties;
        }
    }
}