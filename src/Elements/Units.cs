using System;

namespace Elements
{
    /// <summary>
    /// Unit conversions and utilities.
    /// </summary>
    public static class Units
    {
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
    }
}