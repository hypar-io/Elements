using System.Collections.Generic;
using Elements.Geometry;

namespace Elements.Flow
{
    public class ConnectionComparer : IComparer<Connection>
    {
        public ConnectionComparer(Connection outgoing, bool straightFirst = false)
        {
            Outgoing = outgoing;
            StraightFirst = straightFirst;
        }

        public Connection Outgoing { get; }
        public bool StraightFirst { get; }

        public int Compare(Connection x, Connection y)
        {
            var trunkDirection = Outgoing?.Direction() ?? Vector3.XAxis;

            //Two incoming connections can't occupy the same direction but they can be at 180 degrees.
            //In this case use trunk direction. 3 connections can always define a plane.
            var normal = x.Direction().Cross(y.Direction());
            if (normal.IsZero())
            {
                normal = trunkDirection.Cross(x.Direction());
            }
            if (normal.IsZero())
            {
                throw new Fittings.CannotMakeConnectionException("Two connections can't occupy the same direction.", Outgoing.Start.Position);
            }

            //Direction is relative to view point. Here we assume that positive Z direction is preferred.
            //If Z coordinate is 0, assume the same for Y or X.
            if ((!normal.Z.ApproximatelyEquals(0) && normal.Z < 0) ||
               (!normal.Y.ApproximatelyEquals(0) && normal.Y < 0) ||
               (!normal.Z.ApproximatelyEquals(0) && normal.X < 0))
            {
                normal = normal.Negate();
            }

            if (StraightFirst)
            {
                if (x.Direction().PlaneAngleTo(trunkDirection, normal).ApproximatelyEquals(0, 1))
                {
                    return -1;
                }
                if (y.Direction().PlaneAngleTo(trunkDirection, normal).ApproximatelyEquals(0, 1))
                {
                    return 1;
                }
            }

            if (x.Direction().Negate().PlaneAngleTo(trunkDirection) < y.Direction().Negate().PlaneAngleTo(trunkDirection))
            {
                return -1;
            }
            else
            {
                return 1;
            }
        }
    }
}