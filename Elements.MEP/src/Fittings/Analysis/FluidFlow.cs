using System;

namespace Elements.Fittings
{
    /// <summary>
    /// Basic fluid flow calculator for fluids.
    /// </summary>
    public static class FluidFlow
    {
        const double UNIT_WEIGHT_WATER = 9810; // in N/m^3 at 4 deg C

        /// <summary>
        /// Compute the pressure drop of a single meter of pipe with a given flow rate and diameter.
        /// Taken from : https://en.wikipedia.org/wiki/Hazen%E2%80%93Williams_equation#SI_units
        /// </summary>
        /// <param name="C">The hazen williams roughness coefficient, normally between 90-140.  130 for copper, 150 for PVC.</param>
        /// <param name="flowRate">Flow rate in m3/s.</param>
        /// <param name="diameter">Inner diameter of the pipe in m.</param>
        /// <returns>The pressure drop in pascals</returns>
        public static double HazenWilliamsPD(double C, double flowRate, double diameter)
        {
            var headLoss = (10.67 * Math.Pow(flowRate, 1.852)) / (Math.Pow(C, 1.852) * Math.Pow(diameter, 4.87));
            return UNIT_WEIGHT_WATER * headLoss;
        }

        /// <summary>
        /// Given a height in meters get the static pressure gain.
        /// </summary>
        /// <param name="heightDelta">Height in meters</param>
        /// <returns>Static gain in pascals</returns>
        public static double StaticGainForHeightDelta(double heightDelta)
        {
            var pressureGainKg_per_m2 = FluidFlowGlobals.Rho * FluidFlowGlobals.Gravity * heightDelta;

            return pressureGainKg_per_m2;
        }

        /// <summary>
        /// Compute fluid velocity in m/s.
        /// </summary>
        /// <param name="flowRate">In m^3/s</param>
        /// <param name="diameter">In m</param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        internal static double FluidVelocity(double flowRate, double diameter)
        {
            return flowRate / (Math.PI * Math.Pow(diameter, 2) / 4);
        }
    }
}