using System;
using System.Collections.Generic;
using System.Text;

namespace Elements.CoreModels
{
    internal abstract class RepresentationProvider<E> where E : Element
    {
        public abstract List<RepresentationInstance> GetInstances(E element);
    }
}
