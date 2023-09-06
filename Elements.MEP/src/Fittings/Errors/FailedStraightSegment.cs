using Elements.Annotations;
using Elements.Fittings;
using Elements.Geometry;
using System;
using System.Collections.Generic;
using System.Text;

namespace Elements.Fittings
{
    public class FailedStraightSegment : FittingError
    {
        private const string _message = "Some connections could not be piped";

        public Fitting Start { get; private set; } = null;
        public Port StartPort { get; private set; } = null;
        public Fitting End { get; private set; } = null;
        public Port EndPort { get; private set; } = null;

        public FailedStraightSegment(Fitting first, Port firstPort, Fitting second, Port secondPort)
            : base(_message,
                   firstPort.Position + (secondPort.Position - firstPort.Position) / 2)
        {
            int compare = first.ComponentLocator.CompareTo(second.ComponentLocator);
            (Start, End) = compare > 0 ? (first, second) : (second, first);
            (StartPort, EndPort) = compare > 0 ? (firstPort, secondPort) : (secondPort, firstPort);
        }

        public FailedStraightSegment(Fitting fitting, Port port)
            : base(_message, port.Position)
        {
            Start = fitting;
            StartPort = port;
        }
    }
}
