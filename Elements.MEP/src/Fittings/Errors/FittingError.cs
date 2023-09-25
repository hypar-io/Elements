using Elements.Annotations;
using Elements.Geometry;
using System;
using System.Collections.Generic;
using System.Text;

namespace Elements.Fittings
{
    public class FittingError
    {
        public Vector3? Location { get; private set; }
        public string Text { get; private set; }

        public FittingError(string text, Vector3? location = null)
        {
            Text = text;
            Location = location;
        }

        public virtual Message GetMessage()
        {
            return Message.FromPoint(Text, Location);
        }
    }
}
