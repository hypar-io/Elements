
using System;
using System.Collections.Generic;
using Elements.Geometry;
using Elements.Geometry.Solids;

namespace Elements.Fittings
{
    static class FittingRepresentationStorage
    {
        private static readonly Dictionary<string, List<RepresentationInstance>> _fittings = new Dictionary<string, List<RepresentationInstance>>();
        public static Dictionary<string, List<RepresentationInstance>> Fittings => _fittings;

        public static void SetFittingRepresentation(Fitting fitting, Func<IList<SolidOperation>> makeSolids, Boolean unioned = true, Boolean updateTransform = true)
        {
            var hash = fitting.GetRepresentationHash();
            if (!_fittings.ContainsKey(hash))
            {
                var solids = makeSolids();
                var representationInstances = new List<RepresentationInstance>();
                if (unioned)
                {
                    representationInstances = new List<RepresentationInstance> { new RepresentationInstance(new SolidRepresentation(solids), fitting.Material) };
                }
                else
                {
                    foreach (var solid in solids)
                    {
                        representationInstances.Add(new RepresentationInstance(new SolidRepresentation(solid), fitting.Material));
                    }
                }
                _fittings.Add(hash, representationInstances);
            }
            fitting.RepresentationInstances = _fittings[hash];

            if (updateTransform)
            {
                fitting.Transform = fitting.GetRotatedTransform().Concatenated(new Transform(fitting.Transform.Origin));
            }
        }
    }
}