using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace GeneticAlgorithms.RegularExpressions
{
    [TestClass]
    public class FitnessTests
    {
        [TestMethod]
        public void CompareToEqual()
        {
            Fitness.UseRegexLength = false;

            var fit1 = new Fitness(1, 2, 3, 4);
            var fit2 = new Fitness(1, 2, 3, 0);
            Assert.IsTrue(fit1.CompareTo(fit2) == 0);
        }


        [TestMethod]
        public void CompareToLessThan()
        {
            Fitness.UseRegexLength = true;

            var fit1 = new Fitness(20, 2, 3, 4);
            var fit2 = new Fitness(1, 2, 3, 0);
            Assert.IsTrue(fit1.CompareTo(fit2) > 0);
        }

        [TestMethod]
        public void CompareToGreaterThan()
        {
            Fitness.UseRegexLength = true;

            var fit1 = new Fitness(1, 2, 3, 5);
            var fit2 = new Fitness(20, 2, 3, 0);
            Assert.IsTrue(fit1.CompareTo(fit2) < 0);
        }

        [TestMethod]
        public void CompareToEqualLength()
        {
            Fitness.UseRegexLength = true;

            var fit1 = new Fitness(1, 2, 3, 4);
            var fit2 = new Fitness(1, 2, 3, 4);
            Assert.IsTrue(fit1.CompareTo(fit2) == 0);
        }

        [TestMethod]
        public void CompareToLessThanLength()
        {
            Fitness.UseRegexLength = true;

            var fit1 = new Fitness(1, 2, 3, 4);
            var fit2 = new Fitness(1, 2, 3, 5);
            Assert.IsTrue(fit1.CompareTo(fit2) > 0);
        }

        [TestMethod]
        public void CompareToGreaterThanLength()
        {
            Fitness.UseRegexLength = true;

            var fit1 = new Fitness(1, 2, 3, 5);
            var fit2 = new Fitness(1, 2, 3, 4);
            Assert.IsTrue(fit1.CompareTo(fit2) < 0);
        }
    }
}