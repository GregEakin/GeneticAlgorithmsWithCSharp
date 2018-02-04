using System;
using System.Diagnostics;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace GeneticAlgorithms.Password
{
    [TestClass]
    public class GuessPassordTests
    {
        private const string GeneSet = " abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ!.,";

        private readonly Random _random = new Random();

        private static int GetFitness(string guess, string target)
        {
            var sum = target.Zip(guess, (expected, actual) => expected == actual).Count(c => c);
            return sum;
        }

        private static void Display(Chromosome candidate, Stopwatch stopwatch)
        {
            Console.WriteLine("{0}\t{1}\t{2} ms", candidate.Genes, candidate.Fitness, stopwatch.ElapsedMilliseconds);
        }

        private static void GuessPassword(string target)
        {
            var watch = Stopwatch.StartNew();
            int FnGetFitness(string guess) => GetFitness(target, guess);
            void FnDisplay(Chromosome candidate) => Display(candidate, watch);

            var optimalFitness = target.Length;
            var best = Genetic.GetBest(FnGetFitness, target.Length, optimalFitness, GeneSet, FnDisplay);
            Assert.AreEqual(target.Length, best.Fitness);
            Assert.AreEqual(target, best.Genes);
        }

        [TestMethod]
        public void HelloWorldTest()
        {
            const string target = "Hello World!";
            GuessPassword(target);
        }

        [TestMethod]
        public void LongPwTest()
        {
            const string target = "For I am fearfully and wonderfully made.";
            GuessPassword(target);
        }

        [TestMethod]
        public void RandomPasswordTest()
        {
            const int length = 150;
            var target = string.Join("",
                Enumerable.Range(0, length).Select(x => GeneSet[_random.Next(GeneSet.Length)]));
            Assert.AreEqual(length, target.Length);
            GuessPassword(target);
        }

        [TestMethod]
        public void BenchmarkTest()
        {
            Benchmark.Run(RandomPasswordTest);
        }
    }
}