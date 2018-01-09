using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace PrimeTesting.linearEquation
{
    [TestClass]
    public class FractionTests
    {
        [TestMethod]
        public void AddTest1()
        {
            var a = new Fraction(3, 6);
            var b = new Fraction(5, 8);
            var sum = a + b;
            Assert.AreEqual(new Fraction(9, 8), sum);
        }

        [TestMethod]
        public void AddTest2()
        {
            var a = 3;
            var b = new Fraction(5, 7);
            var sum = a + b;
            Assert.AreEqual(new Fraction(26, 7), sum);
        }

        [TestMethod]
        public void AddTest3()
        {
            var a = new Fraction(3, 5);
            var b = 2;
            var sum = a + b;
            Assert.AreEqual(new Fraction(13, 5), sum);
        }

        [TestMethod]
        public void SubtractTest1()
        {
            var a = new Fraction(3, 5);
            var b = new Fraction(1, 3);
            var difference = a - b;
            Assert.AreEqual(new Fraction(4, 15), difference);
        }

        [TestMethod]
        public void SubtractTest2()
        {
            var a = 2;
            var b = new Fraction(3, 5);
            var difference = a - b;
            Assert.AreEqual(new Fraction(7, 5), difference);
        }

        [TestMethod]
        public void SubtractTest3()
        {
            var a = new Fraction(3, 5);
            var b = 2;
            var difference = a - b;
            Assert.AreEqual(new Fraction(-7, 5), difference);
        }

        [TestMethod]
        public void MultiplyTest1()
        {
            var a = new Fraction(3, 2);
            var b = new Fraction(2, 7);
            var product = a * b;
            Assert.AreEqual(new Fraction(6, 14), product);
        }

        [TestMethod]
        public void MultiplyTest2()
        {
            var a = 3;
            var b = new Fraction(2, 7);
            var product = a * b;
            Assert.AreEqual(new Fraction(6, 7), product);
        }

        [TestMethod]
        public void MultiplyTest3()
        {
            var a = new Fraction(2, 7);
            var b = 3;
            var product = a * b;
            Assert.AreEqual(new Fraction(6, 7), product);
        }

        [TestMethod]
        public void DivideTest1()
        {
            var a = new Fraction(4, 5);
            var b = new Fraction(2, 7);
            var product = a / b;
            Assert.AreEqual(new Fraction(14, 5), product);
        }

        [TestMethod]
        public void DivideTest2()
        {
            var a = 5;
            var b = new Fraction(2, 7);
            var product = a / b;
            Assert.AreEqual(new Fraction(35, 2), product);
        }

        [TestMethod]
        public void DivideTest3()
        {
            var a = new Fraction(2, 7);
            var b = 3;
            var product = a / b;
            Assert.AreEqual(new Fraction(2, 21), product);
        }

        [TestMethod]
        public void NegateTest()
        {
            var a = new Fraction(2, 7);
            var negate = -a;
            Assert.AreEqual(new Fraction(-2, 7), negate);
        }
    }
}