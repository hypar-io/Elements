using System;
using System.Collections.Generic;
using System.Text;

namespace Elements.Geometry.Solids
{
    /// <summary>
    /// A solid operation defined by an existing solid.
    /// </summary>
    public class ImportSolid : SolidOperation
    {
        /// <summary>
        /// Create an import solid.
        /// </summary>
        /// <param name="solid">The solid which was imported.</param>
        /// <param name="isVoid">Is the operation a void?</param>
        public ImportSolid(Solid solid, bool isVoid = false) : base(isVoid)
        {
            _solid = solid;
        }
    }
}
