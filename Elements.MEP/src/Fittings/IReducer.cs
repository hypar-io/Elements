using System.IO.Compression;
using System;
using System.Collections.Generic;
using System.Linq;
using Elements.Geometry;
using Elements.Geometry.Solids;

namespace Elements.Fittings
{
    // TODO IReducer allows for multi-reducer pipe assemblies.  We should see if this can be removed
    // in favor of a base class so we can remove the IComponent interface.
    public interface IReducer : IComponent
    {
        Transform BranchSideTransform { get; }

        Port End { get; set; }

        Port Start { get; set; }

        double Length();

        void Move(Vector3 translation);
    }
}