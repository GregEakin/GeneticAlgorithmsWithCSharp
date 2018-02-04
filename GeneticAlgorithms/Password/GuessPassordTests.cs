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

        private static int GetFitness(string guess, string target)
        {
            var lenght = Math.Min(guess.Length, target.Length);
            var sum = 0;
            for (var i = 0; i < lenght; i++)
            {
                if (guess[i] == target[i])
                    sum++;
            }

            return sum;
        }

        private static void Display(Chromosome candidate, Stopwatch stopwatch)
        {
            Console.WriteLine("{0}\t{1}\t{2} ms", candidate.Genes, candidate.Fitness, stopwatch.ElapsedMilliseconds);
        }

        private static Stopwatch GuessPassword(string target)
        {
            var watch = Stopwatch.StartNew();
            int FitnessFun(string guess) => GetFitness(target, guess);
            void DisplayFun(Chromosome candidate) => Display(candidate, watch);

            var answer = Genetic.BestFitness(FitnessFun, target.Length, target.Length, GeneSet, DisplayFun);
            Assert.AreEqual(target.Length, answer.Fitness);
            Assert.AreEqual(target, answer.Genes);

            return watch;
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

        private readonly Random _random = new Random();

        [TestMethod]
        public void RandomPasswordTest()
        {
            const int length = 150;
            var target = string.Join("",
                Enumerable.Range(0, length).Select(x => GeneSet[_random.Next(GeneSet.Length)]));
            Assert.AreEqual(length, target.Length);
            GuessPassword(target);
        }

        //[TestMethod]
        public void Benchmark()
        {
            const int count = 100;
            const int length = 150;

            var timings = new long[count];
            for (var i = 0; i < count; i++)
            {
                var target = Genetic.RandomSample(GeneSet, length);
                var watch = GuessPassword(target);
                timings[i] = watch.ElapsedMilliseconds;
            }

            var mean = 0.0;
            var sd = 0.0;
            Console.WriteLine("Mean {0}, SD = {1}", mean, sd);
        }
    }
}