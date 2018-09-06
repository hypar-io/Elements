using System;

namespace AECSpaces
{
    /// <summary>
    /// Represents a vector and provides some basic vector math operations.
    /// </summary>
    ///
    public class AECVector
    {
        private double x;
        private double y;
        private double z;

        public AECVector(double X = 0, double Y = 0, double Z = 0)
        {
            this.X = X;
            this.Y = Y;
            this.Z = Z;
        }//constructor

        public AECVector(AECPoint thisPoint, AECPoint thatPoint)
        {
            FromPoints(thisPoint, thatPoint);
        }//constructor

        public double X { get => x; set => x = value; }
        public double Y { get => y; set => y = value; }
        public double Z { get => z; set => z = value; }

        /// <summary>
        /// Returns the length of the vector.
        /// </summary>
        ///
        public double Length
        {
            get
            {
                return Math.Sqrt(Math.Pow(X, 2) + Math.Pow(Y, 2) + Math.Pow(Z, 2));
            }//get
        }//method

        /// <summary>
        /// Returns a new unit vector derived from this vector.
        /// </summary>
        ///
        public AECVector Unit
        {
            get
            {
                return new AECVector(X / Length, Y / Length, Z / Length);
            }//get
        }//method

        /// <summary>
        /// Adds the delivered vector to this vector.
        /// </summary>
        ///
        public void Add(AECVector that)
        {
            X += that.X;
            Y += that.Y;
            Z += that.Z;
        }//method

        /// <summary>
        /// Returns a new vector which is the result of the delivered vector added to this vector.
        /// </summary>
        ///
        public AECVector AddVector(AECVector that)
        {
            AECVector addVector = new AECVector
            {
                X = X + that.X,
                Y = Y + that.Y,
                Z = Z + that.Z
            };
            return addVector;
        }//method

        /// <summary>
        /// Returns the cross product of this vector and the delivered vector.
        /// </summary>
        ///
        public AECVector CrossProduct(AECVector that)
        {
            AECVector xVector = new AECVector
            {
                X = (this.Y * that.Z) - (this.Z * that.Y),
                Y = (this.Z * that.X) - (this.X * that.Z),
                Z = (this.X * that.Y) - (this.Y * that.X)
            };
            return xVector;
        }//method

        /// <summary>
        /// Divides this vector by a scalar.
        /// </summary>
        ///
        public void Divide(double scalar)
        {
            X /= scalar;
            Y /= scalar;
            Z /= scalar;
        }//method

        /// <summary>
        /// Returns the dot product of this vector and the delivered vector.
        /// </summary>
        ///
        public double DotProduct(AECVector that)
        {
            return (this.X * that.X) + (this.Y * that.Y) + (this.Z * that.Z);
        }//method

        /// <summary>
        /// Configures this vector as calculated from two points.
        /// </summary>
        ///
        public void FromPoints(AECPoint thisPoint, AECPoint thatPoint)
        {
            X = thisPoint.X - thatPoint.X;
            Y = thisPoint.Y - thatPoint.Y;
            Z = thisPoint.Z - thatPoint.Z;
        }//method

        /// <summary>
        /// Multiplies this vector by a scalar.
        /// </summary>
        ///
        public void Multiply(double scalar)
        {
            X *= scalar;
            Y *= scalar;
            Z *= scalar;
        }//method

        /// <summary>
        /// Raises this vector to a power.
        /// </summary>
        ///
        public void RaiseTo(double power)
        {
            X = Math.Pow(X, power);
            Y = Math.Pow(Y, power);
            Z = Math.Pow(Z, power);
        }//method

        /// <summary>
        /// Subtracts the delivered vector from this vector.
        /// </summary>
        ///
        public void Subtract(AECVector that)
        {
            X -= that.X;
            Y -= that.Y;
            Z -= that.Z;
        }//method

        /// <summary>
        /// Returns a new vector which is the result of the delivered vector subtracted from this vector.
        /// </summary>
        ///
        public AECVector SubtractVector(AECVector that)
        {
            AECVector subVector = new AECVector
            {
                X = X - that.X,
                Y = Y - that.Y,
                Z = Z - that.Z
            };
            return subVector;
        }//method
    }//class
}//namespace
