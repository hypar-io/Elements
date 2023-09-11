using System;
using System.Collections.Generic;
using System.Text;

namespace Elements
{
    /// <summary>
    /// Collection of utility methods and properties for conversions and calculations.
    /// </summary>
    public static class MathUtils
    {
        /// <summary>
        /// Flow rate unit conversion rate from meters cubed per second to liters per minute.
        /// </summary>
        public const double Meters3PerSecondToLitersPerMinute = 60_000;

        /// <summary>
        /// Pressure unit conversion rate from bars to pascals.
        /// </summary>
        public const double BarToPascals = 100_000;

        /// <summary>
        /// Convert k factor from (liters/min)/(bar^0.5) to (m3/s)/(pascal^0.5).
        /// </summary>
        /// <param name="kFactor"></param>
        /// <returns>K factor in (m3/s)/(pascal^0.5) units.</returns>
        public static double ConvertKFactorFromLitersMinutesBars(double kFactor)
        {
            return kFactor / Meters3PerSecondToLitersPerMinute / Math.Sqrt(BarToPascals);
        }

        /// <summary>
        /// Calculate flow rate from pressure and k factor.
        /// </summary>
        /// <param name="pressure">Pressure in pascals.</param>
        /// <param name="kFactor">K factor in (m3/s)/(pascal^0.5).</param>
        /// <returns>Flow rate in m3/s.</returns>
        public static double CalculateKFactorFlowRate(double pressure, double kFactor)
        {
            return Math.Sqrt(pressure) * kFactor;
        }
    }
}
