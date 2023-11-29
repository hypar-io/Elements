using Elements.Geometry;
using Elements.Geometry.Solids;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Elements.Serialization.IFC.IFCToHypar.RepresentationsExtraction
{
    /// <summary>
    /// A piece of IfcProduct representation information.
    /// </summary>
    internal class RepresentationData
    {
        /// <summary>
        /// A transform of IfcRepresentationItem.
        /// </summary>
        public Transform Transform { get; set; }

        /// <summary>
        /// Representation instances.
        /// </summary>
        public List<RepresentationInstance> RepresentationInstances { get; private set; }
        /// <summary>
        /// An extrude that is used for the Element defining properties extraction.
        /// </summary>
        public Extrude Extrude { get; private set; }

        /// <summary>
        /// Construct a RepresentationData from a list of RepresentationInstances.
        /// </summary>
        /// <param name="representationInstances">The representation instances composing this RepresentationData.</param>
        public RepresentationData(List<RepresentationInstance> representationInstances)
        {
            RepresentationInstances = representationInstances;

            // Most Elements require parameters that can only be extracted
            // from an extrude representation. The first found extrude is
            // used for this purpose.
            Extrude = GetSolidOperations().OfType<Extrude>().FirstOrDefault();
        }

        /// <summary>
        /// Construct a RepresentationData from RepresentationInstances.
        /// </summary>
        /// <param name="representationInstances">The representation instances composing this RepresentationData.</param>
        public RepresentationData(params RepresentationInstance[] representationInstances) : this(new List<RepresentationInstance>(representationInstances)) 
        { 
        }

        /// <summary>
        /// Merge multiple representations into a single one.
        /// </summary>
        /// <param name="representations">Multiple representations that should be merged into a single one.</param>
        public RepresentationData(List<RepresentationData> representations)
        {
            // Combine solid operations of all representations.
            RepresentationInstances = representations.SelectMany(x => x.RepresentationInstances).ToList();

            // TODO: IfcProducts can include several extrudes. Now single extrude
            // is supported. Moreover, most of the representation parsers work
            // with the first found extrude. They require an extrude for the 
            // Element defining properties to be extracted.
            var extrudeReps = representations.Where(rep => rep.Extrude != null);
            if (extrudeReps.Any())
            {
                var extrudeRep = extrudeReps.First();
                Extrude = extrudeRep.Extrude;
            }
        }

        public IEnumerable<SolidOperation> GetSolidOperations()
        {
            var solids = RepresentationInstances
                .Select(ri => ri.Representation)
                .OfType<SolidRepresentation>()
                .SelectMany(sr => sr.SolidOperations);

            return solids;
        }
    }
}
