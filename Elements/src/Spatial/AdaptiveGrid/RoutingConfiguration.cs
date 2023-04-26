using System;
using System.Collections.Generic;

namespace Elements.Spatial.AdaptiveGrid
{
    /// <summary>
    /// Object that holds common parameters that affect routing.
    /// </summary>
    public struct RoutingConfiguration
    {
        /// <summary>
        /// Construct new RoutingConfiguration structure.
        /// </summary>
        /// <param name="turnCost">Travel cost penalty if route changes it's direction.</param>
        /// <param name="supportedAngles">List of angles route can turn.</param>
        public RoutingConfiguration(double turnCost = 0,
                                    List<double> supportedAngles = null)
        {
            TurnCost = turnCost;
            SupportedAngles = supportedAngles;
            if (SupportedAngles != null && !SupportedAngles.Contains(0))
            {
                SupportedAngles.Add(0);
            }
        }

        /// <summary>
        /// Travel cost penalty if route changes it's direction.
        /// </summary>
        public readonly double TurnCost;

        /// <summary>
        /// List of angles route can turn. Angles are between 0 and 90. 0 is auto-included.
        /// For turn angle bigger than 90 degrees - 180 degrees minus angle is checked.
        /// For example, 135 is the same as 45.
        /// </summary>
        public readonly List<double> SupportedAngles;
    }
}
