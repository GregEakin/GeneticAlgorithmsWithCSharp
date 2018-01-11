using System;
using System.Diagnostics;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace GeneticAlgorithms.sortedNumbers
{
    [TestClass]
    public class SortedNumbersTests
    {
        public static int Fitness(int[] genes, int size)
        {
            var count = genes.Count(n => n == 1);
            return count;
        }

        public static void Display(Chromosome<int, int> candidate, Stopwatch watch, int size)
        {
            Console.WriteLine("{0} ... {1}\t{2:3.2f}\t{3}",
                string.Join(", ", candidate.Genes.Take(5)),
                string.Join(", ", candidate.Genes.Skip(size - 5)),
                candidate, watch.ElapsedMilliseconds);
        }

        [TestMethod]
        public void Test1()
        {
            const int length = 100;

            var genetic = new Genetic<int, int>();
            var geneSet = new[] {0, 1};
            var watch = new Stopwatch();
            void DisplayFun(Chromosome<int, int> candidate) => Display(candidate, watch, length);

            var optimalFitness = length;
            watch.Start();
            var best = genetic.BestFitness(Fitness, length, optimalFitness, geneSet, DisplayFun);
            watch.Stop();
            Assert.IsTrue(best.Fitness >= length);
        }
    }
}