using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Diagnostics;

namespace PrimeTesting.password
{
    [TestClass]
    public class GuessPassword
    {
        public const string GeneSet = " abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ!.,";
        public const string target = "Hello World!";

        public static int Fitness(string genes, string target)
        {
            var lenght = Math.Min(genes.Length, target.Length);
            var sum = 0;
            for (var i = 0; i < lenght; i++)
            {
                if (genes[i] == target[i])
                    sum++;
            }

            return sum;
        }

        public static int Fitness(string guess)
        {
            return Fitness(target, guess);
        }

        public void Display(string geneSet, string guess, Stopwatch stopwatch)
        {
            var fitness = Fitness(guess);
            Console.WriteLine("{0}\t{1}\t{2} ms", guess, fitness, stopwatch.ElapsedMilliseconds);
        }

        [TestMethod]
        public void Test1()
        {
            var genetic = new Genetic();
            var password = genetic.BestFitness(Fitness, target.Length, target.Length, GeneSet, Display);
            Assert.AreEqual(target, password);
        }

        [TestMethod]
        public void RandomSampleTest()
        {
            var genetic = new Genetic();
            for (var i = 0; i < 10; i++)
            {
                var x = genetic.RandomSample(GeneSet, 10);
                Console.WriteLine(x);
            }
        }

        [TestMethod]
        public void GenerateParentTest()
        {
            var genetic = new Genetic();
            var parent = genetic.GenerateParent(GeneSet, 10);
            Console.WriteLine(parent);
        }
    }
}