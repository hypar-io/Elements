using System;
using System.Collections.Generic;
using System.Linq;

[assembly: System.Runtime.CompilerServices.InternalsVisibleTo("Elements.Tests")]
namespace Elements.Fittings
{
    /// <summary>
    /// Library of equivalent lengths for retrieval during pressure calculations.
    /// </summary>
    public static class EquivalentLength
    {
        /// <summary>
        /// Get the equivalent length of a fitting for use in the Hazen-Williams equation.
        /// Lengths are taken from this Schedule 40 steel table: https://up.codes/s/equivalent-pipe-lengths-of-valves-and-fittings
        /// </summary>
        /// <param name="fitting"></param>
        /// <param name="CFactor">The C Factor which </param>
        /// <returns></returns>
        public static double OfFitting(Fitting fitting, double CFactor)
        {
            var multiplier = GetMultiplierForCFactor(CFactor);
            switch (fitting)
            {
                case Wye y:
                    return multiplier * FetchSizeOrLargerFromList(y.Trunk.Diameter, WyeEquivalentLengths);
                case Elbow e:
                    return multiplier * FetchSizeOrLargerFromList(e.End.Diameter, Elbow90EquivalentLengths);
                default:
                    throw new ArgumentException("No equivalent length for this fitting type.");
            }
        }
        private static SortedList<double, double> _multipliers = new SortedList<double, double>
            {
                { 100, 0.713 },
                { 120, 1},
                { 130, 1.16 },
                { 140, 1.33 },
                { 150, 1.51 },
            };

        internal static double GetMultiplierForCFactor(double cFactor)
        {
            if (cFactor < 100 || cFactor > 150)
            {
                throw new ArgumentException("The C Factor must be between 100 and 150.");
            }

            var upper = _multipliers.First(k => k.Key >= cFactor);
            var lower = _multipliers.Last(k => k.Key < cFactor);
            var interpolated = (cFactor - lower.Key) / (upper.Key - lower.Key) * (upper.Value - lower.Value) + lower.Value;
            return interpolated;
        }

        private static double FetchSizeOrLargerFromList(double size, SortedList<double, double> lookup)
        {
            return lookup.First(x => x.Key >= size).Value;
        }

        private static SortedList<double, double> WyeEquivalentLengths = new SortedList<double, double>
        {
            {.015, 0.9},
            {.020, 1.2},
            {.025, 1.5},
            {.032, 1.8},
            {.040, 2.4},
            {.050, 3.0},
            {.065, 3.7},
            {.080, 4.6},
            {.090, 5.2},
            {.100, 6.1},
            {.125, 7.6},
            {.150, 9.1},
            {.200, 10.7},
            {.250, 15.2},
            {.300, 18.3},
        };

        private static SortedList<double, double> Elbow90EquivalentLengths = new SortedList<double, double>
        {
            {.015, 0.3},
            {.020, 0.6},
            {.025, 0.6},
            {.032, 0.9},
            {.040, 1.2},
            {.050, 1.5},
            {.065, 1.8},
            {.080, 2.1},
            {.090, 2.4},
            {.100, 3.0},
            {.125, 3.7},
            {.150, 4.3},
            {.200, 5.5},
            {.250, 6.7},
            {.300, 8.2},
        };
    }
}