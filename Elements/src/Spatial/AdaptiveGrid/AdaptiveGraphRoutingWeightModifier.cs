using Elements.Geometry;
using System;
using System.Collections.Generic;
using System.Text;

namespace Elements.Spatial.AdaptiveGrid
{
    public partial class AdaptiveGraphRouting
    {
        private Dictionary<string, WeightModifier> _weightModifiers =
            new Dictionary<string, WeightModifier>();

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
        /// Create WeightModifier that sets factor to all edges lying on given plane.
        /// </summary>
        /// <param name="name">Name of new WeightModifier.</param>
        /// <param name="plane">Plane to check if edge lays on.</param>
        /// <param name="factor">Factor of new WeightModifier.</param>
        /// <returns>Created WeightModifier.</returns>
        public WeightModifier AddPlaneModifier(string name, Plane plane, double factor)
        {
            var modifier = new WeightModifier(
                name,
                new Func<Vertex, Vertex, bool>((a, b) =>
                {
                    return plane.SignedDistanceTo(a.Point) > _grid.Tolerance ||
                           plane.SignedDistanceTo(b.Point) > _grid.Tolerance;
                }),
                factor);
            AddWeightModifier(modifier);
            return modifier;
        }

        private double ModifierFactor(Vertex a, Vertex b)
        {
            double modifierFactor = -1;
            foreach (var modifier in _weightModifiers)
            {
                if (modifier.Value.Condition(a, b))
                {
                    if (modifierFactor == -1)
                    {
                        modifierFactor = modifier.Value.Factor;
                    }
                    else
                    {
                        modifierFactor = Math.Min(modifierFactor, modifier.Value.Factor);
                    }
                }
            }

            return modifierFactor != -1 ? modifierFactor : 1;
        }
    }
}
