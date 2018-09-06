using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using AECSpaces;

namespace AECSpacesTest
{
    [TestClass]
    public class VectorTests
    {
        [TestMethod]
        public void Add()
        {
            AECVector thisVector = new AECVector(4, 8, 10);
            AECVector thatVector = new AECVector(9, 2, 7);
            thisVector.Add(thatVector);

            Assert.AreEqual(13, thisVector.X, 0);
            Assert.AreEqual(10, thisVector.Y, 0);
            Assert.AreEqual(17, thisVector.Z, 0);
        }//method

        [TestMethod]
        public void AddVector()
        {
            AECVector thisVector = new AECVector(4, 8, 10);
            AECVector thatVector = new AECVector(9, 2, 7);
            AECVector addVector = thisVector.AddVector(thatVector);

            Assert.AreEqual(13, addVector.X, 0);
            Assert.AreEqual(10, addVector.Y, 0);
            Assert.AreEqual(17, addVector.Z, 0);
        }//method

        [TestMethod]
        public void CrossProduct()
        {
            AECVector thisVector = new AECVector(2, 3, 4);
            AECVector thatVector = new AECVector(5, 6, 7);
            AECVector xVector = thisVector.CrossProduct(thatVector);

            Assert.AreEqual(-3, xVector.X, 0);
            Assert.AreEqual(6, xVector.Y, 0);
            Assert.AreEqual(-3, xVector.Z, 0);
        }//method

        [TestMethod]
        public void DotProduct()
        {
            AECVector thisVector = new AECVector(4, 8, 10);
            AECVector thatVector = new AECVector(9, 2, 7);
            double dotProduct = thisVector.DotProduct(thatVector);

            Assert.AreEqual(122, dotProduct, 0);
        }//method

        [TestMethod]
        public void Length()
        {
            AECVector vector = new AECVector(1, 1, 1);
  
            Assert.AreEqual(1.7320508, vector.Length, 0.00000001);
        }//method 

        [TestMethod]
        public void RaiseTo()
        {
            AECVector vector = new AECVector(5, 5, 5);
            vector.RaiseTo(2);

            Assert.AreEqual(25, vector.X, 0);
            Assert.AreEqual(25, vector.Y, 0);
            Assert.AreEqual(25, vector.Z, 0);
        }//method

        [TestMethod]
        public void Subtract()
        {
            AECVector thisVector = new AECVector(4, 8, 10);
            AECVector thatVector = new AECVector(9, 2, 7);
            thisVector.Subtract(thatVector);

            Assert.AreEqual(-5, thisVector.X, 0);
            Assert.AreEqual(6, thisVector.Y, 0);
            Assert.AreEqual(3, thisVector.Z, 0);
        }//method

        [TestMethod]
        public void SubtractVector()
        {
            AECVector thisVector = new AECVector(4, 8, 10);
            AECVector thatVector = new AECVector(9, 2, 7);
            AECVector subVector = thisVector.SubtractVector(thatVector);

            Assert.AreEqual(-5, subVector.X, 0);
            Assert.AreEqual(6, subVector.Y, 0);
            Assert.AreEqual(3, subVector.Z, 0);
        }//method

        [TestMethod]
        public void Unit()
        {
            AECVector vector = new AECVector(5, 5, 5);
            AECVector unit = vector.Unit;

            Assert.AreEqual(0.5773502691896257, unit.X, 0);
            Assert.AreEqual(0.5773502691896257, unit.Y, 0);
            Assert.AreEqual(0.5773502691896257, unit.Z, 0);
        }//method 

    }//class
}//namespace
