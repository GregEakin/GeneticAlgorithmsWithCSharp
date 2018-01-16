using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace GeneticAlgorithms.Cards
{
    [TestClass]
    public class CardTests
    {
        private static readonly Random Random = new Random();

        public static Fitness Fitness(int[] genes, int size)
        {
            var sum = genes.Take(5).Sum();
            var product = genes.Skip(5).Aggregate(1, (acc, val) => acc * val);
            var duplicate = genes.Length - new HashSet<int>(genes).Count;
            return new Fitness(sum, product, duplicate);
        }

        public static void Display(Chromosome<int, Fitness> candidate, Stopwatch watch)
        {
            Console.WriteLine("{0} - {1}\t{2},\t{3} ms",
                string.Join(", ", candidate.Genes.Take(5)),
                string.Join(", ", candidate.Genes.Skip(5)),
                candidate.Fitness, watch.ElapsedMilliseconds);
        }

        public static int[] Mutate(int[] input, int[] geneSet, Genetic<int, Fitness> genetic)
        {
            var genes = new int[input.Length];
            Array.Copy(input, genes, input.Length);
            var duplicates = genes.Length - new HashSet<int>(genes).Count;
            if (duplicates == 0)
            {
                var count = Random.Next(1, 4);
                while (count-- > 0)
                {
                    var randomSample = genetic.RandomSample(geneSet, 2);
                    var indexA = randomSample[0] - 1;
                    var indexB = randomSample[1] - 1;
                    var temp = genes[indexA];
                    genes[indexA] = genes[indexB];
                    genes[indexB] = temp;
                }
            }
            else
            {
                var indexA = Random.Next(genes.Length);
                var indexB = Random.Next(geneSet.Length);
                genes[indexA] = geneSet[indexB];
            }

            return genes;
        }

        [TestMethod]
        public void FitnessTest()
        {
            var genes = new[] {1, 1, 1, 1, 1, 2, 2, 2, 2, 2};
            var fitness = Fitness(genes, genes.Length);
            Assert.AreEqual(5, fitness.Sum);
            Assert.AreEqual(32, fitness.Product);
            Assert.AreEqual(8, fitness.Duplicate);
        }

        [TestMethod]
        public void DispalyTest()
        {
            var genes = new[] {1, 1, 1, 1, 1, 2, 2, 2, 2, 2};
            var fitness = Fitness(genes, genes.Length);
            var chromosome = new Chromosome<int, Fitness>(genes, fitness);
            var watch = Stopwatch.StartNew();

            Display(chromosome, watch);
        }

        [TestMethod]
        public void CardTest()
        {
            var genetic = new Genetic<int, Fitness>();
            var geneSet = Enumerable.Range(1, 10).ToArray();
            var watch = Stopwatch.StartNew();

            void DisplayFun(Chromosome<int, Fitness> candidate) => Display(candidate, watch);
            int[] MutateFun(int[] genes) => Mutate(genes, geneSet, genetic);

            var optimalFitness = new Fitness(36, 360, 0);
            var best = genetic.BestFitness(Fitness, 10, optimalFitness, geneSet, DisplayFun, MutateFun);
            Assert.IsTrue(optimalFitness.CompareTo(best.Fitness) <= 0);
        }
    }
}