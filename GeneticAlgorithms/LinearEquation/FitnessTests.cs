using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace GeneticAlgorithms.LinearEquation
{
    [TestClass]
    public class FitnessTests
    {
        [TestMethod]
        public void ReferenceEqualsTest()
        {
            var fitness = new Fitness(new Fraction(3, 2));
            Assert.IsTrue(fitness.CompareTo(fitness) == 0);
        }

        [TestMethod]
        public void EqualToTest()
        {
            var fitness1 = new Fitness(new Fraction(1, 2));
            var fitness2 = new Fitness(new Fraction(2, 4));
            Assert.IsTrue(fitness1.CompareTo(fitness2) == 0);
        }

        [TestMethod]
        public void GreaterThanTest()
        {
            var fitness1 = new Fitness(new Fraction(1,3));
            var fitness2 = new Fitness(new Fraction(1,2));
            Assert.IsTrue(fitness1.CompareTo(fitness2) < 0);
        }

        [TestMethod]
        public void ToStringTest()
        {
            var fitness = new Fitness(new Fraction(2,3));
            Assert.AreEqual("diff: 0.67", fitness.ToString());
        }
    }
}