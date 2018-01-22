using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Diagnostics;

namespace GeneticAlgorithms.Password
{
    [TestClass]
    public class PasswordGeneratorTests
    {
        public const string GeneSet = " abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ!.,";

        public static int Fitness(string guess, string target)
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

        public static void Display(Chromosome candidate, Stopwatch stopwatch)
        {
            Console.WriteLine("{0}\t{1}\t{2} ms", candidate.Genes, candidate.Fitness, stopwatch.ElapsedMilliseconds);
        }

        private static Stopwatch GuessPassword(string target)
        {
            var watch = Stopwatch.StartNew();
            int FitnessFun(string guess) => Fitness(target, guess);
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

        [TestMethod]
        public void RandomPasswordTest()
        {
            const int length = 150;
            var target = Genetic.RandomSample(GeneSet, length);
            Assert.AreEqual(length, target.Length);
            GuessPassword(target);
        }

        [TestMethod]
        public void RandomSampleTest()
        {
            for (var i = 0; i < 10; i++)
            {
                var x = Genetic.RandomSample(GeneSet, 10);
                Console.WriteLine(x);
            }
        }

        [TestMethod]
        public void GenerateParentTest()
        {
            const string target = "Hello World!";
            int Fitness(string guess) => PasswordGeneratorTests.Fitness(target, guess);
            var parent = Genetic.GenerateParent(GeneSet, 10, Fitness);
            Console.WriteLine("{0} : {1}", parent.Fitness, parent.Genes);
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