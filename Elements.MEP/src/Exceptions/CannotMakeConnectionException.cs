using System;
using Elements.Geometry;

namespace Elements.Fittings
{
    public class CannotMakeConnectionException : Exception
    {
        public Vector3 Location { get; set; }
        public CannotMakeConnectionException(string message, Vector3 location) : base(message)
        {
            Location = location;
        }
    }
}