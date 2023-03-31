using System;
using System.Globalization;
using System.Runtime.Serialization;

namespace Elements
{
    /// <summary>
    /// Unit conversions and utilities.
    /// </summary>
    public static class Units
    {
        /// <summary>
        /// Pi/2
        /// </summary>
        public const double PI_2 = Math.PI / 2;

        /// <summary>
        /// Convert from meters to feet.
        /// </summary>
        /// <param name="meters"></param>
        /// <returns>The provided value converted to feet.</returns>
        public static double MetersToFeet(double meters)
        {
            return meters * 3.28084;
        }

        /// <summary>
        /// Convert from feet to meters.
        /// </summary>
        /// <param name="feet"></param>
        /// <returns>The provided value converted to meters.</returns>
        public static double FeetToMeters(double feet)
        {
            return feet * 0.3048;
        }

        /// <summary>
        /// Convert from meters to inches.
        /// </summary>
        /// <param name="meters"></param>
        /// <returns>The provided value converted to inches.</returns>
        public static double MetersToInches(double meters)
        {
            return meters * 39.3701;
        }

        /// <summary>
        /// Convert from inches to meters.
        /// </summary>
        /// <param name="inches">A value of inches.</param>
        /// <returns>The provided value converted to meters.</returns>
        public static double InchesToMeters(double inches)
        {
            return inches * 0.0254;
        }

        /// <summary>
        /// Convert from degrees to radians
        /// </summary>
        /// <param name="degrees"></param>
        /// <returns>The provided value converted to radians</returns>
        public static double DegreesToRadians(double degrees)
        {
            return degrees * Math.PI / 180.0;
        }

        /// <summary>
        /// Convert from radians to degrees.
        /// </summary>
        /// <param name="radians"></param>
        /// <returns>The provided value converted to radians.</returns>
        public static double RadiansToDegrees(double radians)
        {
            return radians * 180.0 / Math.PI;
        }

        /// <summary>
        /// Get the conversion factor from the provided length unit to meters.
        /// </summary>
        /// <param name="from">The length unit.</param>
        public static double GetConversionToMeters(LengthUnit from)
        {
            var conversion = 1.0;
            switch (from)
            {
                case LengthUnit.Kilometer:
                    conversion = 1000.0;
                    break;
                case LengthUnit.Meter:
                    conversion = 1.0;
                    break;
                case LengthUnit.Centimeter:
                    conversion = 0.01;
                    break;
                case LengthUnit.Millimeter:
                    conversion = 0.001;
                    break;
                case LengthUnit.Foot:
                    conversion = Units.FeetToMeters(1.0);
                    break;
                case LengthUnit.Inch:
                    conversion = Units.InchesToMeters(1.0);
                    break;

            }
            return conversion;
        }

        /// <summary>
        /// Convert from decimal feet to feet and fractional inches.
        /// </summary>
        /// <param name="decimalFeet">The value to convert to a fractional inches representation.</param>
        /// <param name="roundDigits">The number of fractional digits in the return value.</param>
        /// <param name="precision">Fractional precision described as a double. i.e. 1/64th -> 0.015625</param>
        /// <returns></returns>
        public static string FeetToFeetAndFractionalInches(double decimalFeet, int roundDigits = 5, double precision = 1 / 64.0)
        {
            double wholeFeet = 0.0;
            double partialFeet = 0.0;

            if (decimalFeet < 0)
            {
                wholeFeet = Math.Ceiling(decimalFeet);
                if (wholeFeet == 0)
                    partialFeet = decimalFeet;
                else
                    partialFeet = wholeFeet - decimalFeet;
            }
            else
            {
                wholeFeet = Math.Floor(decimalFeet);
                partialFeet = decimalFeet - wholeFeet;
            }

            string fractionalInches = InchesToFractionalInches(Math.Round(partialFeet * 12.0, roundDigits), precision: precision);

            if (fractionalInches == "11 1\"" || fractionalInches == "12\"")
            {
                //add a foot to the whole feet
                wholeFeet += 1.0;
                fractionalInches = "0\"";
            }
            else if (fractionalInches == "-11 1\"" || fractionalInches == "-12\"")
            {
                wholeFeet -= 1.0;
                fractionalInches = "0\"";
            }

            string feet = string.Empty;
            if (wholeFeet != 0.0)
                feet = string.Format("{0}'", wholeFeet);

            if (wholeFeet.ApproximatelyEquals(0.0) && (partialFeet * 12.0).ApproximatelyEquals(0.0))
                feet = "0'";

            return string.Format("{0} {1}", feet, fractionalInches).Trim();
        }

        /// <summary>
        /// Convert from decimal inches to fractional inches
        /// </summary>
        /// <param name="decimalInches">The value to convert to a fractional inches representation.</param>
        /// <param name="roundDigits">The number of fractional digits in the return value.</param>
        /// <param name="precision">Fractional precision described as a double. i.e. 1/64th -> 0.015625</param>
        /// <returns></returns>
        public static string InchesToFractionalInches(double decimalInches, int roundDigits = 5, double precision = 1 / 64.0)
        {
            decimalInches = RoundToSignificantDigits(decimalInches, roundDigits);
            string inches = ParseWholeInchesToString(decimalInches);
            string fraction = ParsePartialInchesToString(decimalInches, precision);
            string sign = decimalInches < 0 ? "-" : string.Empty;

            if (string.IsNullOrEmpty(inches) && string.IsNullOrEmpty(fraction))
            {
                return "0\"";
            }

            if (string.IsNullOrEmpty(fraction))
            {
                return string.Format("{0}{1}\"", sign, inches).Trim();
            }

            if (string.IsNullOrEmpty(inches))
            {
                return string.Format("{0}{1}\"", sign, fraction).Trim();
            }

            if (fraction == "1")
            {
                fraction = string.Empty;
                inches = (double.Parse(inches) + 1).ToString(CultureInfo.InvariantCulture);
                return string.Format("{0}{1}\"", sign, inches).Trim();
            }

            return string.Format("{0}{1} {2}\"", sign, inches, fraction).Trim();
        }

        private static double RoundToSignificantDigits(double value, int digits)
        {
            if (value.ApproximatelyEquals(0))
            {
                return 0;
            }

            double scale = Math.Pow(10, Math.Floor(Math.Log10(Math.Abs(value))) + 1);
            return scale * Math.Round(value / scale, digits);
        }

        private static string ParseWholeInchesToString(double value)
        {
            double result = value < 0 ?
                Math.Abs(System.Math.Ceiling(value)) :
                Math.Abs(System.Math.Floor(value));

            if (result.ApproximatelyEquals(0.0))
            {
                return string.Empty;
            }

            return result.ToString();
        }

        private static string ParsePartialInchesToString(double value, double precision = 1 / 64.0)
        {
            string result = value < 0 ?
                CreateFraction(Math.Abs(value - Math.Ceiling(value)), precision) :
                CreateFraction(Math.Abs(value - Math.Floor(value)), precision);

            return result;
        }

        private static string CreateFraction(double value, double precision)
        {
            double numerator = Math.Round(value / precision);
            double denominator = 1 / precision;

            if (numerator.ApproximatelyEquals(denominator))
                return "1";

            if (numerator != 0.0)
            {
                while (numerator % 2 == 0.0)
                {
                    numerator = numerator / 2;
                    denominator = denominator / 2;
                }

                return string.Format("{0}/{1}", numerator, denominator);
            }

            return string.Empty;
        }

        /// <summary>
        /// Units of length.
        /// </summary>
        public enum LengthUnit
        {
            /// <summary>
            /// Kilometer
            /// </summary>
            Kilometer,
            /// <summary>
            /// Meter
            /// </summary>
            Meter,
            /// <summary>
            /// Centimeter
            /// </summary>
            Centimeter,
            /// <summary>
            /// Millimeter
            /// </summary>
            Millimeter,
            /// <summary>
            /// Foot
            /// </summary>
            Foot,
            /// <summary>
            /// Inch
            /// </summary>
            Inch
        }

        /// <summary>
        /// Cardinal directions.
        /// </summary>
        public enum CardinalDirection
        {
            /// <summary>
            /// North
            /// </summary>
            North,
            /// <summary>
            /// South
            /// </summary>
            South,
            /// <summary>
            /// East
            /// </summary>
            East,
            /// <summary>
            /// West
            /// </summary>
            West
        }

        /// <summary>
        /// Unit types.
        /// </summary>
        public enum UnitType
        {
            /// <summary>
            /// None
            /// </summary>
            [EnumMember(Value = "none")]
            None,
            /// <summary>
            /// Area
            /// </summary>
            [EnumMember(Value = "area")]
            Area,
            /// <summary>
            /// Force
            /// </summary>
            [EnumMember(Value = "force")]
            Force,
            /// <summary>
            /// Length
            /// </summary>
            [EnumMember(Value = "length")]
            Length,
            /// <summary>
            /// Mass
            /// </summary>
            [EnumMember(Value = "mass")]
            Mass,
            /// <summary>
            /// Plane Angle
            /// </summary>
            [EnumMember(Value = "plane_angle")]
            PlaneAngle,
            /// <summary>
            /// Pressure
            /// </summary>
            [EnumMember(Value = "pressure")]
            Pressure,
            /// <summary>
            /// Time
            /// </summary>
            [EnumMember(Value = "time")]
            Time,
            /// <summary>
            /// Volume
            /// </summary>
            [EnumMember(Value = "volume")]
            Volume,
        }
    
        internal static bool IsParameterBetween(double a, double min, double max)
        {
            return min < max ? a >= min && a <= max:
                               a <= min && a >= max;
        }
    }
}