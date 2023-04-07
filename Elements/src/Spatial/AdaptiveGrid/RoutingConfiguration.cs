using System;
using System.Collections.Generic;
using System.Text;
using Elements.Geometry;

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
        /// <param name="mainLayer">OBSOLETE. Elevation at which route prefers to travel.</param>
        /// <param name="layerPenalty">OBSOLETE. Penalty if route travels through an elevation different from MainLayer.</param>
        /// <param name="supportedAngles">List of angles route can turn.</param>
        public RoutingConfiguration(double turnCost = 0,
                                    double mainLayer = 0,
                                    double layerPenalty = 1,
                                    List<double> supportedAngles = null)
        {
            TurnCost = turnCost;
            MainLayer = mainLayer;
            LayerPenalty = layerPenalty;
            SupportedAngles = supportedAngles;
            if (SupportedAngles != null && !SupportedAngles.Contains(0))
            {
                SupportedAngles.Add(0);
            }
        }

        /// <summary>
        /// Get a default initialized RoutingConfiguration.
        /// </summary>
        /// <returns></returns>
        public static RoutingConfiguration Default()
        {
            return new RoutingConfiguration(turnCost: 0);

        }

        /// <summary>
        /// Travel cost penalty if route changes it's direction.
        /// </summary>
        public readonly double TurnCost;

        /// <summary>
        /// Elevation at which route prefers to travel.
        /// </summary>
        [Obsolete("Use WeightModified instead")]
        public readonly double MainLayer;

        /// <summary>
        /// Travel cost penalty if route travels through an elevation different from MainLayer.
        /// </summary>
        [Obsolete("Use WeightModified instead")]
        public readonly double LayerPenalty;

        /// <summary>
        /// List of angles route can turn. Angles are between 0 and 90. 0 is auto-included.
        /// For turn angle bigger than 90 degrees - 180 degrees minus angle is checked.
        /// For example, 135 is the same as 45.
        /// </summary>
        public readonly List<double> SupportedAngles;
    }
}
