using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Elements.Fittings
{
    /// <summary>
    /// Fitting catalog, that contains lists of each of the part types.
    /// </summary>
    public partial class FittingCatalog
    {
        public List<PipePart> Pipes { get; set; }
        public List<ElbowPart> Elbows { get; set; }
        public List<TeePart> Tees { get; set; }
        public List<CrossPart> Crosses { get; set; }
        public List<ReducerPart> Reducers { get; set; }
        public List<CouplerPart> Couplers { get; set; }

        /// <summary>
        /// Find best reducer part from catalog.
        /// </summary>
        /// <param name="largeDiameter">The larger diameter of reducer.</param>
        /// <param name="smallDiameter">The smaller diameter of reducer.</param>
        /// <returns>The most suitable reducer part.</returns>
        public ReducerPart GetBestReducerPart(double largeDiameter, double smallDiameter)
        {
            return Reducers?.FirstOrDefault(r => r.DiameterLarge.ApproximatelyEquals(largeDiameter)
                                                 && r.DiameterSmall.ApproximatelyEquals(smallDiameter));
        }

        /// <summary>
        /// Find best elbow part from catalog.
        /// </summary>
        /// <param name="diameter">Diameter.</param>
        /// <param name="angle">Angle.</param>
        /// <returns>The most suitable elbow part.</returns>
        public ElbowPart GetBestElbowPart(double diameter, double angle)
        {
            if (angle.ApproximatelyEquals(135, 1))
            {
                angle = 45;
            }
            var possibleElbows = Elbows?.Where(e => e.Angle.ApproximatelyEquals(angle, 1));
            if (possibleElbows == null || !possibleElbows.Any())
            {
                return null;
            }

            var closestDiameter = GetClosestDiameter(diameter, possibleElbows.Select(t => (double?)t.Diameter));
            var elbowPart = possibleElbows.FirstOrDefault(t => t.Diameter.ApproximatelyEquals(closestDiameter.Value));
            return elbowPart;
        }

        /// <summary>
        /// Find best tee part from catalog.
        /// </summary>
        /// <param name="trunkDiameter">Trunk diameter.</param>
        /// <param name="branchDiameter">Branch diameter.</param>
        /// <param name="angle">Angle.</param>
        /// <returns>The most suitable tee part.</returns>
        public TeePart GetBestTeePart(double trunkDiameter, double branchDiameter, double angle)
        {
            var teesWithRightAngle = Tees?.Where(t => t.Angle.ApproximatelyEquals(angle, 1));
            if (teesWithRightAngle == null || !teesWithRightAngle.Any())
            {
                return null;
            }

            var closestTrunkDiameter = GetClosestDiameter(trunkDiameter, teesWithRightAngle.Select(t => (double?)t.Diameter));
            var possibleTees = teesWithRightAngle.Where(t => t.Diameter.ApproximatelyEquals(closestTrunkDiameter.Value));
            var closestBranchDiameter = GetClosestDiameter(branchDiameter, possibleTees.Select(t => (double?)t.BranchDiameter));
            var teePart = possibleTees.FirstOrDefault(t => t.BranchDiameter.ApproximatelyEquals(closestBranchDiameter.Value));
            return teePart;
        }

        /// <summary>
        /// Find best cross part from catalog.
        /// </summary>
        /// <param name="trunkDiameter">Trunk diameter.</param>
        /// <param name="branch1Diameter">First branch diameter.</param>
        /// <param name="branch2Diameter">Second branch diameter.</param>
        /// <param name="angle1">First branch angle.</param>
        /// <param name="angle2">Second branch angle.</param>
        /// <returns>The most suitable cross part.</returns>
        public CrossPart GetBestCrossPart(double trunkDiameter, double branch1Diameter, double branch2Diameter, double angle1, double angle2)
        {
            var crossesWithRightAngle = Crosses?.Where(e => e.Angle1.ApproximatelyEquals(angle1, 1)
                                         && e.Angle2.ApproximatelyEquals(angle2, 1));
            if (crossesWithRightAngle != null && crossesWithRightAngle.Any())
            {
                CrossPart crossPart = GetBestCrossPart(trunkDiameter, branch1Diameter, branch2Diameter, crossesWithRightAngle);
                return crossPart;
            }
            else
            {
                crossesWithRightAngle = Crosses?.Where(e => e.Angle1.ApproximatelyEquals(angle2, 1)
                                         && e.Angle2.ApproximatelyEquals(angle1, 1));
                if (crossesWithRightAngle != null && crossesWithRightAngle.Any())
                {
                    return null;
                }
                CrossPart crossPart = GetBestCrossPart(trunkDiameter, branch2Diameter, branch1Diameter, crossesWithRightAngle);
                return crossPart;
            }
        }

        private double? GetClosestDiameter(double diameter, IEnumerable<double?> possibleDiameters)
        {
            possibleDiameters = possibleDiameters.OrderBy(d => d);
            return possibleDiameters?.FirstOrDefault(d => d.Value.ApproximatelyEquals(diameter) || d > diameter)
                   ?? possibleDiameters?.LastOrDefault(d => d < diameter);
        }

        private CrossPart GetBestCrossPart(double trunkDiameter, double branch1Diameter, double branch2Diameter, IEnumerable<CrossPart> crossesWithRightAngles)
        {
            var closestTrunkDiameter = GetClosestDiameter(trunkDiameter, crossesWithRightAngles.Select(t => (double?)t.PipeDiameter));
            var possibleCrosses = crossesWithRightAngles.Where(t => t.PipeDiameter.ApproximatelyEquals(closestTrunkDiameter.Value));

            var closestBranchDiameter1 = GetClosestDiameter(branch1Diameter, possibleCrosses.Select(t => (double?)t.BranchDiameter1));
            possibleCrosses = possibleCrosses.Where(t => t.BranchDiameter1.ApproximatelyEquals(closestBranchDiameter1.Value));

            var closestBranchDiameter2 = GetClosestDiameter(branch2Diameter, possibleCrosses.Select(t => (double?)t.BranchDiameter2));
            var crossPart = possibleCrosses.FirstOrDefault(t => t.BranchDiameter1.ApproximatelyEquals(closestBranchDiameter1.Value));
            return crossPart;
        }
    }
}