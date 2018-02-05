using GeneticAlgorithms.MagicSquare;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace GeneticAlgorithms.LinearEquation
{
    [TestClass]
    public class LinearEquationTests
    {
        public delegate Fraction Equation(IReadOnlyList<Fraction> genes);

        private static readonly Random Random = new Random();

        public static Fitness Fitness(List<Fraction> genes, Equation[] equations)
        {
            try
            {
                var fitness = equations.Aggregate(new Fraction(0),
                    (current, equation) => current + Fraction.Abs(equation(genes)));
                return new Fitness(fitness);
            }
            catch (ArgumentException)
            {
                // divide by zero?
                return new Fitness(new Fraction(int.MaxValue));
            }
        }

        [TestMethod]
        public void FitnessTest()
        {
            var child = new List<Fraction> {new Fraction(2), new Fraction(-1)};
            Fraction E1(IReadOnlyList<Fraction> genes) => genes[0] + 2 * genes[1] - 4;
            Fraction E2(IReadOnlyList<Fraction> genes) => 4 * genes[0] + 4 * genes[1] - 12;
            var equations = new Equation[] {E1, E2};
            var fitness = Fitness(child, equations);
            Assert.AreEqual(new Fraction(12), fitness.TotalDifference);
        }

        public static void Display(Chromosome<Fraction, Fitness> candidate, Stopwatch watch)
        {
            var symbols = "xyza".ToCharArray();
            var values = Enumerable.Range(0, candidate.Genes.Count)
                .Select(i => $"{symbols[i]} = {candidate.Genes[i]}");
            var result = string.Join(", ", values);
            Console.WriteLine("{0}\t{1}\t{2} ms", result, candidate.Fitness, watch.ElapsedMilliseconds);
        }

        [TestMethod]
        public void DisplayTest()
        {
            var range = Enumerable.Range(-5, 11).ToArray();
            var geneSet =
                (from n in range from d in range.Where(i => i != 0) select new Fraction(n, d))
                .Distinct().OrderBy(g => g);

            var genes = geneSet.OrderBy(g => Random.Next()).Take(4).ToList();
            var fitness = new Fitness(new Fraction(42));
            var chromosome = new Chromosome<Fraction, Fitness>(genes, fitness);
            var watch = Stopwatch.StartNew();
            Display(chromosome, watch);
        }

        [TestMethod]
        public void GeneSetTest()
        {
            var range = Enumerable.Range(-5, 11).ToArray();
            var geneSet =
                (from n in range from d in range.Where(i => i != 0) select new Fraction(n, d))
                .Distinct().OrderBy(g => g).ToArray();
            foreach (var gene in geneSet)
                Console.WriteLine("{0}, {1}", gene, (double) gene);
        }

        public static void Mutate(List<Fraction> input, Fraction[] sortedGeneSet, Window window, int[] geneIndexes)
        {
            var genes = input.ToArray();
            window.Slide();
            var indexes = Random.Next(10) == 0
                ? geneIndexes.OrderBy(g => Random.Next()).Take(1 + Random.Next(geneIndexes.Length - 1)).ToArray()
                : new[] {geneIndexes[Random.Next(geneIndexes.Length)]};
            foreach (var index in indexes)
            {
                var geneSetIndex = Array.IndexOf(sortedGeneSet, genes[index]);
                var start = Math.Max(0, geneSetIndex - window.Size);
                var stop = Math.Min(sortedGeneSet.Length - 1, geneSetIndex + window.Size);
                var genesetIndex2 = Random.Next(start, stop);
                genes[index] = sortedGeneSet[genesetIndex2];
            }
        }

        [TestMethod]
        public void MutateTest()
        {
            var watch = Stopwatch.StartNew();
            var fitness = new Fitness(new Fraction(0));

            var range = Enumerable.Range(-5, 11).ToArray();
            var geneSet =
                (from n in range from d in range.Where(i => i != 0) select new Fraction(n, d))
                .Distinct().OrderBy(g => g);

            var genes = geneSet.OrderBy(g => Random.Next()).Take(4).ToList();
            Display(new Chromosome<Fraction, Fitness>(genes, fitness), watch);

            var sortedGeneSet = genes.OrderBy(g => g).ToArray();
            // Display(new Chromosome<Fraction, Fitness>(sortedGeneSet, fitness), watch);

            var geneIndexes = Enumerable.Range(0, genes.Count).ToArray();
            var window = new Window(1, genes.Count, genes.Count);
            Mutate(genes, sortedGeneSet, window, geneIndexes);
            Display(new Chromosome<Fraction, Fitness>(genes, fitness), watch);
        }

        [TestMethod]
        public void TwoUnknonwsTest()
        {
            var range = Enumerable.Range(-5, 11).ToArray();
            var geneSet =
                (from n in range from d in range.Where(i => i != 0) select new Fraction(n, d))
                .Distinct().OrderBy(g => g).ToArray();
            Fraction E1(IReadOnlyList<Fraction> genes) => genes[0] + 2 * genes[1] - 4;
            Fraction E2(IReadOnlyList<Fraction> genes) => 4 * genes[0] + 4 * genes[1] - 12;
            var equations = new Equation[] {E1, E2};
            var best = SolveUnknown(2, geneSet, equations);
            Assert.AreEqual(new Fraction(0), best.Fitness.TotalDifference);
            CollectionAssert.AreEqual(new[] {new Fraction(2), new Fraction(1)}, best.Genes);
        }

        [TestMethod]
        public void ThreeUnknonwsTest()
        {
            var range = Enumerable.Range(-5, 11).ToArray();
            var geneSet =
                (from n in range from d in range.Where(i => i != 0) select new Fraction(n, d))
                .Distinct().OrderBy(g => g).ToArray();
            Fraction E1(IReadOnlyList<Fraction> genes) => 6 * genes[0] - 2 * genes[1] + 8 * genes[2] - 20;
            Fraction E2(IReadOnlyList<Fraction> genes) => genes[1] + 8 * genes[0] * genes[2] + 1;
            Fraction E3(IReadOnlyList<Fraction> genes) => 2 * genes[2] * 6 / genes[0] + 3 * genes[1] / 2 - 6;

            var equations = new Equation[] {E1, E2, E3};
            var best = SolveUnknown(3, geneSet, equations);
            Assert.AreEqual(new Fraction(0), best.Fitness.TotalDifference);
            CollectionAssert.AreEqual(new[] {new Fraction(2, 3), new Fraction(-5), new Fraction(3, 4)}, best.Genes);
        }

        [TestMethod]
        public void FourUnknonwsTest()
        {
            var range = Enumerable.Range(-13, 27).ToArray();
            var geneSet =
                (from n in range from d in range.Where(i => i != 0) select new Fraction(n, d))
                .Distinct().OrderBy(g => g).ToArray();

            Fraction E1(IReadOnlyList<Fraction> genes) =>
                new Fraction(1, 15) * genes[0] - 2 * genes[1] - 15 * genes[2] - new Fraction(4, 5) * genes[3] - 3;

            Fraction E2(IReadOnlyList<Fraction> genes) =>
                -new Fraction(5, 2) * genes[0] - new Fraction(9, 4) * genes[1] + 12 * genes[2] - genes[3] - 17;

            Fraction E3(IReadOnlyList<Fraction> genes) =>
                -13 * genes[0] + new Fraction(3, 10) * genes[1] - 6 * genes[2] - new Fraction(2, 5) * genes[3] - 17;

            Fraction E4(IReadOnlyList<Fraction> genes) =>
                new Fraction(1, 2) * genes[0] + 2 * genes[1] + new Fraction(7, 4) * genes[2] +
                new Fraction(4, 3) * genes[3] + 9;

            var equations = new Equation[] {E1, E2, E3, E4};
            var best = SolveUnknown(4, geneSet, equations);
            Assert.AreEqual(new Fraction(0), best.Fitness.TotalDifference);
            CollectionAssert.AreEqual(
                new[]
                {
                    new Fraction(-3, 2),
                    new Fraction(-7, 2),
                    new Fraction(1, 3),
                    new Fraction(-11, 8)
                }, best.Genes);
        }

        public Chromosome<Fraction, Fitness> SolveUnknown(int numUnknowns, Fraction[] geneSet, Equation[] equations)
        {
            var genetic = new Genetic<Fraction, Fitness>();
            var watch = Stopwatch.StartNew();
            var maxAge = 50;
            var window = new Window(Math.Max(1, geneSet.Length / (2 * maxAge)),
                Math.Max(1, geneSet.Length / 3),
                geneSet.Length / 2);
            var geneIndexes = Enumerable.Range(0, numUnknowns).ToArray();
            var sortedGeneSet = geneSet.OrderBy(gene => gene).ToArray();

            void FnDispaly(Chromosome<Fraction, Fitness> candidate) => Display(candidate, watch);
            Fitness FnFitness(List<Fraction> genes) => Fitness(genes, equations);
            void FnMutate(List<Fraction> genes) => Mutate(genes, sortedGeneSet, window, geneIndexes);

            var optimalFitness = new Fitness(new Fraction(0));
            var best = genetic.GetBest(FnFitness, numUnknowns, optimalFitness, geneSet, FnDispaly, FnMutate,
                null, maxAge);
            Assert.IsTrue(optimalFitness.CompareTo(best.Fitness) <= 0);
            return best;
        }
    }
}