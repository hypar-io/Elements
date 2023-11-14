using System;
using System.Collections.Generic;
using System.Text;

namespace Elements.Representations.DoorRepresentations
{
    internal abstract class DoorRepresentationFactory
    {
        /// <summary>
        /// Create curve 2D representation of a <paramref name="door"/>.
        /// </summary>
        /// <param name="door">Parameters of <paramref name="door"/> 
        /// will be used for the representation creation.</param>
        /// <returns>A curve 2D representation, created from properties of <paramref name="door"/>.</returns>
        public abstract RepresentationInstance CreateDoorCurveRepresentation(Door door);

        /// <summary>
        /// Create solid representation of a <paramref name="door"/>.
        /// </summary>
        /// <param name="door">Parameters of <paramref name="door"/> 
        /// will be used for the representation creation.</param>
        /// <returns>A solid representation, created from properties of <paramref name="door"/>.</returns>
        public abstract RepresentationInstance CreateDoorSolidRepresentation(Door door);

        /// <summary>
        /// Create solid representation of a <paramref name="door"/> frame.
        /// </summary>
        /// <param name="door">Parameters of <paramref name="door"/> 
        /// will be used for the representation creation.</param>
        /// <returns>A solid frame representation, created from properties of <paramref name="door"/>.</returns>
        public abstract RepresentationInstance CreateDoorFrameRepresentation(Door door);

        public List<RepresentationInstance> CreateAllRepresentationInstances(Door door)
        {
            return new List<RepresentationInstance>()
            {
                CreateDoorCurveRepresentation(door),
                CreateDoorSolidRepresentation(door),
                CreateDoorFrameRepresentation(door)
            };
        }
    }
}
