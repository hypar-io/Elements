using System.Collections.Generic;

namespace Elements
{
    public abstract class BuilderBase
    {
        protected List<string> _errors = new List<string>();

        public BuilderBase(List<string> errors)
        {
            _errors = errors;
        }
    }
}