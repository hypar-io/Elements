using System.Collections.Generic;

namespace Elements.Geometry.Interfaces
{
    /// <summary>
    /// A boundary representation containing a collection of
    /// bounded faces.
    /// </summary>
    public interface IBRep
    {
        /// <summary>
        /// A type descriptor for use in deserialization.
        /// </summary>
        string Type { get; }
        
        /// <summary>
        /// The Faces of the BRep.
        /// </summary>
        IFace[] Faces{get;}

        /// <summary>
        /// The BRep's Material.
        /// </summary>
        Material Material{get;}
    }
}