using System;
using GeoAPI.Geometries;

namespace AECSpaces
{ 
    /// <summary>
    /// Represents a position in space with x,y, and z double values and provides utilities for repositioning the point through deltas or rotation.
    /// </summary>
    public class AECPoint
    {
        private double x;
        private double y;
        private double z;
        private readonly string id;

        public AECPoint(double X = 0, double Y = 0, double Z = 0)
        {
            x = X;
            y = Y;
            z = Z;
            id = Guid.NewGuid().ToString();
        }//constructor

        /// <summary>
        /// Returns the unique id.
        /// </summary>
        public string ID
        {
            get { return id; }
        }//ID

        /// <summary>
        /// Sets or returns the X value.
        /// </summary>
        public double X
        {
            get { return x; }
            set { x = value; }
        }//X

        /// <summary>
        /// Sets or returns the Y value.
        /// </summary>
        public double Y
        {
            get { return y; }
            set { y = value; }
        }//Y

        /// <summary>
        /// Sets or returns the Z value.
        /// </summary>
        /// 
        public double Z
        {
            get { return z; }
            set { z = value; }
        }//Z

        /// <summary>
        /// Returns Coords of the x, y, and z values.
        /// </summary>
        /// 
        public AECCoords XYZ => new AECCoords(x, y, z);

        /// <summary>
        /// Returns a coordinate x, and y values.
        /// </summary>
        public Coordinate CoordinateXY => new Coordinate(x, y);

        /// <summary>
        /// Returns a coordinate x, y, and z values.
        /// </summary>
        public Coordinate CoordinateXYZ => new Coordinate(x, y, z);

        /// <summary>
        /// Compares the coordinates of this point to the delivered point to determine whether they are colocated.
        /// </summary>
        public bool IsColocated(AECPoint point)
        {
            if (X == point.x && Y == point.y && Z == point.Z) return true;
            return false;
        }//IsColocated

        /// <summary>
        /// Changes the coordinates by the delivered deltas.
        /// </summary>
        public void MoveBy(double xDelta = 0, double yDelta = 0, double zDelta = 0)
        {
            X += xDelta;
            Y += yDelta;
            Z += zDelta;
        }//MoveBy

        /// <summary>
        /// Rotates the point horizontally by the specified angle around the delivered point. If no point is supplied, the point is rotated around the origin.
        /// </summary>
        public void Rotate(double angle = Math.PI, AECPoint point = null, bool radians = false)
        {
            if (point == null) point = new AECPoint(0, 0, 0);
            Rotate(angle, point.X, point.Y, radians);
        }//Rotate

        /// <summary>
        /// Rotates the point horizontally by the specified angle around the delivered pivot coordinates.
        /// </summary>
        public void Rotate(double angle = 0, double x = 0, double y = 0, bool radians = false)
        {
            if (!radians) { angle *= (Math.PI / 180); }
            double newX = ((Math.Cos(angle) * (X - x)) - (Math.Sin(angle) * (Y - y))) + x;
            double newY = ((Math.Sin(angle) * (X - x)) + (Math.Cos(angle) * (Y - y))) + y;
            X = Math.Round(newX, digits: 12);
            Y = Math.Round(newY, digits: 12);
        }//Rotate
    }//AECPoint
}//AECSpaces
