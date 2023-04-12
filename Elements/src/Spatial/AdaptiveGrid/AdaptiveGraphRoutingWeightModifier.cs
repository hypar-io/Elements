using Elements.Geometry;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Elements.Spatial.AdaptiveGrid
{
    public partial class AdaptiveGraphRouting
    {
        private Dictionary<string, WeightModifier> _weightModifiers =
            new Dictionary<string, WeightModifier>();

        private Dictionary<string, Func<double, double, double>> _groupFactorAggregators =
            new Dictionary<string, Func<double, double, double>>();

        /// <summary>
        /// Get WeightModifier with given name.
        /// </summary>
        /// <param name="name">Name of WeightModifier.</param>
        /// <returns>WeightModifier object.</returns>
        public WeightModifier GetWeightModifier(string name)
        {
            if (_weightModifiers.TryGetValue(name, out var modifier))
            {
                return modifier;
            }
            return null;
        }

        /// <summary>
        /// Add WeightModifier to the dictionary of modifiers.
        /// </summary>
        /// <param name="modifier">WeightModifier to add.</param>
        public void AddWeightModifier(WeightModifier modifier)
        {
            _weightModifiers[modifier.Name] = modifier;
        }

        /// <summary>
        /// Remove WeightModifier from the dictionary of modifiers.
        /// </summary>
        /// <param name="modifier">WeightModifier to remove.</param>
        /// <returns>False if WeightModifier is not present in the dictionary of modifiers.</returns>
        public bool RemoveWeightModifier(WeightModifier modifier)
        {
            return _weightModifiers.Remove(modifier.Name);
        }

        /// <summary>
        /// Remove all WeightModifier from the dictionary of modifiers.
        /// </summary>
        public void ClearWeightModifiers()
        {
            _weightModifiers.Clear();
        }

        /// <summary>
        /// Set factor aggregator function for weight modifiers group.
        /// <see cref="AggregateFactorMin"/>
        /// <see cref="AggregateFactorMax"/>
        /// <see cref="AggregateFactorMultiply"/>
        /// </summary>
        /// <param name="groupName">Group name.</param>
        /// <param name="groupFactorAggregator">Factor aggregator function.</param>
        public void SetWeightModifiersGroupFactorAggregator(string groupName, Func<double, double, double> groupFactorAggregator)
        {
            _groupFactorAggregators[groupName] = groupFactorAggregator;
        }

        /// <summary>
        /// Get list of WeightModifier with specified group name.
        /// </summary>
        /// <param name="groupName">Group name. Must be not null.</param>
        /// <returns>List of WeightModifier with the specified group name.</returns>
        /// <exception cref="ArgumentNullException">Throws if <paramref name="groupName"/> is null.</exception>
        public List<WeightModifier> GetGroupWeightModifiers(string groupName)
        {
            if (groupName == null)
            {
                throw new ArgumentNullException(nameof(groupName));
            }
            return _weightModifiers.Values.Where(m => groupName.Equals(m.Group)).ToList();
        }

        /// <summary>
        /// Create WeightModifier that sets the factor on all edges lying on a given plane.
        /// </summary>
        /// <param name="name">Name of new WeightModifier.</param>
        /// <param name="plane">Plane to check if edge lays on.</param>
        /// <param name="factor">Factor of new WeightModifier.</param>
        /// <param name="group">Group name of new WeightModifier.</param>
        /// <returns>Created WeightModifier.</returns>
        public WeightModifier AddPlanarWeightModifier(string name, Plane plane, double factor, string group = null)
        {
            var modifier = new WeightModifier(
                name,
                new Func<Vertex, Vertex, bool>((a, b) =>
                {
                    return Math.Abs(plane.SignedDistanceTo(a.Point)) < _grid.Tolerance &&
                           Math.Abs(plane.SignedDistanceTo(b.Point)) < _grid.Tolerance;
                }),
                factor,
                group);
            AddWeightModifier(modifier);
            return modifier;
        }

        /// <summary>
        /// Create WeightModifier that sets the factor on all edges
        /// parallel(both points must be withing influence radius of the polyline) or intersecting with given polyline.
        /// </summary>
        /// <param name="name">Name of new WeightModifier.</param>
        /// <param name="polyline">Polyline to check if edge is affected or intersected.</param>
        /// <param name="factor">Factor of new WeightModifier.</param>
        /// <param name="influenceDistance">Influence radius of polyline.</param>
        /// <param name="is2D">Whether edge and polyline comparison should be considered in 2d or 3d.</param>
        /// <param name="group">Group name of new WeightModifier.</param>
        /// <returns>Created WeightModifier.</returns>
        public WeightModifier AddPolylineWeightModifier(string name, Polyline polyline, double factor, double influenceDistance, bool is2D, string group = null)
        {
            var modifier = new WeightModifier(
                name,
                new Func<Vertex, Vertex, bool>((a, b) =>
                {
                    var line = new Line(a.Point, b.Point);
                    if (is2D)
                    {
                        try
                        {
                            polyline = new Polyline(polyline.Vertices.Select(v => new Vector3(v.X, v.Y)).ToList());
                            line = new Line(new Vector3(a.Point.X, a.Point.Y), new Vector3(b.Point.X, b.Point.Y));
                        }
                        catch
                        {
                            return false;
                        }
                    }
                    var hintLine = new RoutingHintLine(polyline, factor, influenceDistance, true, is2D);
                    var isAffectedParallely = hintLine.Affects(a.Point, b.Point);
                    var isIntersected = polyline.Intersects(line, out _, includeEnds: true);
                    return isAffectedParallely || isIntersected;
                }),
                factor,
                group);
            AddWeightModifier(modifier);
            return modifier;
        }

        /// <summary>
        /// Check if edge passes any modifier check and returns the aggregated value among them.
        /// If an edge meets the condition of several WeightModifier objects
        /// they will be grouped by group name and factor aggregator function will be applied to WeightModifiers factors. <see cref="SetWeightModifiersGroupFactorAggregator"/>.
        /// By default - the lowest factor of group is chosen.
        /// Finally, factors of all groups will be multiplied.
        /// Returns 1 if no modifiers applied.
        /// </summary>
        /// <param name="a">Start Vertex</param>
        /// <param name="b">End Vertex</param>
        private double ModifierFactor(Vertex a, Vertex b)
        {
            var modifiersGroups = _weightModifiers.Values.GroupBy(m => m.Group);
            var modifierFactors = new List<double>();
            foreach (var modifiersGroup in modifiersGroups)
            {
                var appliedModifiers = modifiersGroup.Where(modifier => modifier.Condition(a, b));
                if (!appliedModifiers.Any())
                {
                    modifierFactors.Add(1);
                    continue;
                }

                if (!_groupFactorAggregators.TryGetValue(modifiersGroup.Key, out var aggregateFactorFunc))
                {
                    aggregateFactorFunc = AggregateFactorMin;
                }
                var modifierFactor = appliedModifiers.Select(m => m.Factor).Aggregate(aggregateFactorFunc);
                modifierFactors.Add(modifierFactor);
            }

            return modifierFactors.Any() ? modifierFactors.Aggregate(AggregateFactorMultiply) : 1;
        }

        /// <summary>
        /// Return minimum of factors.
        /// </summary>
        /// <param name="a">First factor.</param>
        /// <param name="b">Second factor.</param>
        /// <returns>Minimum of factors.</returns>
        public static double AggregateFactorMin(double a, double b)
        {
            return Math.Min(a, b);
        }

        /// <summary>
        /// Return maximum of factors.
        /// </summary>
        /// <param name="a">First factor.</param>
        /// <param name="b">Second factor.</param>
        /// <returns>Maximum of factors.</returns>
        public static double AggregateFactorMax(double a, double b)
        {
            return Math.Max(a, b);
        }

        /// <summary>
        /// Multiply factors.
        /// </summary>
        /// <param name="a">First factor.</param>
        /// <param name="b">Second factor.</param>
        /// <returns>Result of multiplication of factors.</returns>
        public static double AggregateFactorMultiply(double a, double b)
        {
            return a * b;
        }
    }
}
