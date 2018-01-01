using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace PrimeTesting.password
{
    [TestClass]
    public class GuessPassword
    {
        public static readonly string GeneSet = " abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ!.";
        public static readonly string Target = "Hello World!";

        private readonly Random _random = new Random();
        private readonly Stopwatch _stopwatch = new Stopwatch();

        public string GenerateParent(int length)
        {
            var genes = string.Empty;
            while (genes.Length < length)
            {
                var sampleSize = Math.Min(length - genes.Length, GeneSet.Length);
                genes += RandomSample(GeneSet, sampleSize);
            }

            return genes;
        }

        public static int Fitness(string expected, string guess)
        {
            var lenght = Math.Min(expected.Length, guess.Length);
            var sum = 0;
            for (var i = 0; i < lenght; i++)
            {
                if (expected[i] == guess[i])
                    sum++;
            }

            return sum;
        }

        public string Mutate(string parent)
        {
            var index = _random.Next(parent.Length);
            var childGenes = parent.ToCharArray();
            var newGene = RandomSample(GeneSet, 1)[0];
            var alternate = RandomSample(GeneSet, 1)[0];
            childGenes[index] = newGene == childGenes[index] ? alternate : newGene;
            return new string(childGenes);
        }

        public void Display(string guess, int count)
        {
            var fitness = Fitness(Target, guess);
            Console.WriteLine("{0}\t{1}\t{2} ms\t{3}", guess, fitness, _stopwatch.ElapsedMilliseconds, count);
        }

        [TestMethod]
        public void Test1()
        {
            _stopwatch.Reset();
            _stopwatch.Start();
            var bestParent = GenerateParent(Target.Length);
            var bestFitness = Fitness(Target, bestParent);
            var count = 0;
            var total = 0L;
            Display(bestParent, count);

            while (true)
            {
                var child = Mutate(bestParent);
                var childFitness = Fitness(Target, child);
                count++;
                if (bestFitness >= childFitness)
                    continue;
                Display(child, count);
                total += count;
                count = 0;
                if (childFitness >= bestParent.Length)
                    break;
                bestFitness = childFitness;
                bestParent = child;
            }

            _stopwatch.Stop();
            Console.WriteLine("Total {0} tries / {1} ms", total, _stopwatch.ElapsedMilliseconds);
        }

        public string RandomSample(string input, int k)
        {
            var array = input.ToCharArray().OrderBy(x => _random.Next()).Take(k).ToArray();
            return new string(array);
        }

        [TestMethod]
        public void RandomSampleTest()
        {
            for (var i = 0; i < 10; i++)
            {
                var x = RandomSample(GeneSet, Target.Length);
                Console.WriteLine(x);
            }
        }

        [TestMethod]
        public void GenerateParentTest()
        {
            var parent = GenerateParent(Target.Length);
            Console.WriteLine(parent);
        }

        [TestMethod]
        public void FitnessTest()
        {
            Assert.AreEqual(0, Fitness("abc", "def"));
            Assert.AreEqual(1, Fitness("abb", "acc"));
        }
    }
};