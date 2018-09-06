using System.Collections.Generic;
using AECSpaces;
using Xunit;

namespace AECSpacesTest
{
    public class VectorTests
    {
        [Fact]
        public void Add()
        {
            AECVector thisVector = new AECVector(4, 8, 10);
            AECVector thatVector = new AECVector(9, 2, 7);
            thisVector.Add(thatVector);

            Assert.Equal(13, thisVector.X, 0);
            Assert.Equal(10, thisVector.Y, 0);
            Assert.Equal(17, thisVector.Z, 0);
        }

        [Fact]
        public void AddVector()
        {
            AECVector thisVector = new AECVector(4, 8, 10);
            AECVector thatVector = new AECVector(9, 2, 7);
            AECVector addVector = thisVector.AddVector(thatVector);

            Assert.Equal(13, addVector.X, 0);
            Assert.Equal(10, addVector.Y, 0);
            Assert.Equal(17, addVector.Z, 0);
        }

        [Fact]
        public void CrossProduct()
        {
            AECVector thisVector = new AECVector(2, 3, 4);
            AECVector thatVector = new AECVector(5, 6, 7);
            AECVector xVector = thisVector.CrossProduct(thatVector);

            Assert.Equal(-3, xVector.X, 0);
            Assert.Equal(6, xVector.Y, 0);
            Assert.Equal(-3, xVector.Z, 0);
        }

        [Fact]
        public void DotProduct()
        {
            AECVector thisVector = new AECVector(4, 8, 10);
            AECVector thatVector = new AECVector(9, 2, 7);
            double dotProduct = thisVector.DotProduct(thatVector);

            Assert.Equal(122, dotProduct, 0);
        }

        [Fact]
        public void Length()
        {
            AECVector vector = new AECVector(1, 1, 1);
  
            Assert.Equal(1.7320508, vector.Length, 5);
        } 

        [Fact]
        public void RaiseTo()
        {
            AECVector vector = new AECVector(5, 5, 5);
            vector.RaiseTo(2);

            Assert.Equal(25, vector.X, 0);
            Assert.Equal(25, vector.Y, 0);
            Assert.Equal(25, vector.Z, 0);
        }

        [Fact]
        public void Subtract()
        {
            AECVector thisVector = new AECVector(4, 8, 10);
            AECVector thatVector = new AECVector(9, 2, 7);
            thisVector.Subtract(thatVector);

            Assert.Equal(-5, thisVector.X, 0);
            Assert.Equal(6, thisVector.Y, 0);
            Assert.Equal(3, thisVector.Z, 0);
        }

        [Fact]
        public void SubtractVector()
        {
            AECVector thisVector = new AECVector(4, 8, 10);
            AECVector thatVector = new AECVector(9, 2, 7);
            AECVector subVector = thisVector.SubtractVector(thatVector);

            Assert.Equal(-5, subVector.X, 0);
            Assert.Equal(6, subVector.Y, 0);
            Assert.Equal(3, subVector.Z, 0);
        }

        [Fact]
        public void Unit()
        {
            AECVector vector = new AECVector(5, 5, 5);
            AECVector unit = vector.Unit;

            Assert.Equal(0.5773502691896257, unit.X, 0);
            Assert.Equal(0.5773502691896257, unit.Y, 0);
            Assert.Equal(0.5773502691896257, unit.Z, 0);
        } 

    }
}
