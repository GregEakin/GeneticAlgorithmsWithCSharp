/* File: MagicSquareTests.cs
 *     from chapter 8 of _Genetic Algorithms with Python_
 *     writen by Clinton Sheppard
 *
 * Author: Greg Eakin <gregory.eakin@gmail.com>
 * Copyright (c) 2018 Greg Eakin
 * 
 * Licensed under the Apache License, Version 2.0 (the "License").
 * You may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 *   http://www.apache.org/licenses/LICENSE-2.0
 * 
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or
 * implied.  See the License for the specific language governing
 * permissions and limitations under the License.
 */

using GeneticAlgorithms.Utilities;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace GeneticAlgorithms.MagicSquare
{
    [TestClass]
    public class MagicSquareTests
    {
        private static Fitness GetFitness(IReadOnlyList<int> genes, int diagonalSize, int expectedSum)
        {
            var sums = GetSums(genes, diagonalSize);
            var sumOfDifferences = sums.Item1.Concat(sums.Item2).Concat(new[] {sums.Item3, sums.Item4})
                .Where(s => s != expectedSum).Select(s => Math.Abs(s - expectedSum)).Sum();
            return new Fitness(sumOfDifferences);
        }

        private static void Display(Chromosome<int, Fitness> candidate, Stopwatch watch, int diagonalSize)
        {
            var sums = GetSums(candidate.Genes, diagonalSize);
            for (var rowNumber = 0; rowNumber < diagonalSize; rowNumber++)
            {
                var row = candidate.Genes.Skip(rowNumber * diagonalSize).Take(diagonalSize);
                Console.WriteLine("\t{0} = {1}", string.Join(",", row), sums.Item1[rowNumber]);
            }

            Console.WriteLine("{0}\t{1}\t{2}", sums.Item3, string.Join(",", sums.Item2), sums.Item4);
            Console.WriteLine(" - - - - - - - - - - - {0}, {1} ms", candidate.Fitness, watch.ElapsedMilliseconds);
        }

        private static Tuple<int[], int[], int, int> GetSums(IReadOnlyList<int> genes, int diagonalSize)
        {
            var rows = new int[diagonalSize];
            var columns = new int[diagonalSize];
            var northeastDiagonalSum = 0;
            var southeastDiagonalSum = 0;
            for (var row = 0; row < diagonalSize; row++)
            {
                for (var column = 0; column < diagonalSize; column++)
                {
                    var value = genes[row * diagonalSize + column];
                    rows[row] += value;
                    columns[column] += value;
                }

                southeastDiagonalSum += genes[row * diagonalSize + row];
                northeastDiagonalSum += genes[row * diagonalSize + (diagonalSize - 1 - row)];
            }

            return new Tuple<int[], int[], int, int>(rows, columns, northeastDiagonalSum, southeastDiagonalSum);
        }

        private static void Mutate(IList<int> genes, IReadOnlyList<int> indexes)
        {
            var randomSample = Rand.RandomSample(indexes, 2);
            var indexA = randomSample[0];
            var indexB = randomSample[1];
            var temp = genes[indexA];
            genes[indexA] = genes[indexB];
            genes[indexB] = temp;
        }

        [TestMethod]
        public void Size3Test()
        {
            Generate(3, 50);
        }

        [TestMethod]
        public void Size4Test()
        {
            Generate(4, 50);
        }

        [TestMethod]
        public void Size5Test()
        {
            Generate(5, 500);
        }

        [TestMethod]
        public void Size10Test()
        {
            Generate(10, 5000);
        }

        [TestMethod]
        public void BenchmarkTest()
        {
            Benchmark.Run(Size4Test);
        }

        private static void Generate(int diagonalSize, int? maxAge)
        {
            var watch = Stopwatch.StartNew();

            var nSquared = diagonalSize * diagonalSize;
            var geneSet = Enumerable.Range(1, nSquared).ToArray();
            var expectedSum = diagonalSize * (nSquared + 1) / 2;
            var geneIndexes = Enumerable.Range(0, geneSet.Length).ToArray();

            Fitness FnGetFitness(IReadOnlyList<int> genes) => GetFitness(genes, diagonalSize, expectedSum);
            void FnDisplay(Chromosome<int, Fitness> candidate) => Display(candidate, watch, diagonalSize);
            void FnMutate(List<int> genes) => Mutate(genes, geneIndexes);
            List<int> FnCreate() => geneSet.OrderBy(i => Rand.Random.Next(geneSet.Length)).ToList();

            var optimalValue = new Fitness(0);
            var best = Genetic<int, Fitness>.GetBest(FnGetFitness, nSquared, optimalValue, geneSet, FnDisplay, FnMutate,
                FnCreate, maxAge);
            Assert.IsTrue(optimalValue.CompareTo(best.Fitness) <= 0);
        }

        [TestMethod]
        public void SimulatedAnnealingSearchTestOdd()
        {
            var historicalFitnesses = Enumerable.Range(0, 26).Select(n => new Fitness(50 - 2 * n)).ToArray();
            var index = Array.BinarySearch(historicalFitnesses, new Fitness(13));
            if (index < 0) index = ~index;
            Assert.AreEqual(19, index);
        }

        [TestMethod]
        public void SimulatedAnnealingSearchTestEven()
        {
            var historicalFitnesses = Enumerable.Range(0, 26).Select(n => new Fitness(50 - 2 * n)).ToArray();
            var index = Array.BinarySearch(historicalFitnesses, new Fitness(12));
            if (index < 0) index = ~index;
            Assert.AreEqual(19, index);
        }

        private static Tuple<int, double, double> SimulatedAnnealingExp(Fitness value, int count)
        {
            var historicalFitnesses = Enumerable.Range(1, count).Select(f => new Fitness(f)).Reverse().ToList();
            var index = historicalFitnesses.BinarySearch(value);
            if (index < 0) index = ~index;
            var difference = historicalFitnesses.Count - index;
            var proportionSimilar = (double) difference / historicalFitnesses.Count;
            var exp = Math.Exp(-proportionSimilar);
            return new Tuple<int, double, double>(index, proportionSimilar, exp);
        }

        [TestMethod]
        public void SimulatedAnnealingExpTest()
        {
            var samples = new[]
            {
                // index, difference, proportion simular, exp(-proportion)
                new Tuple<Fitness, int, double, double>(new Fitness(0), 50, 0.00, 1.00),
                new Tuple<Fitness, int, double, double>(new Fitness(5), 45, 0.10, 0.90),
                new Tuple<Fitness, int, double, double>(new Fitness(10), 40, 0.20, 0.82),
                new Tuple<Fitness, int, double, double>(new Fitness(40), 10, 0.80, 0.45),
                new Tuple<Fitness, int, double, double>(new Fitness(45), 5, 0.90, 0.41),
                new Tuple<Fitness, int, double, double>(new Fitness(50), 0, 1.00, 0.37),
            };

            foreach (var sample in samples)
            {
                var result = SimulatedAnnealingExp(sample.Item1, 50);
                Console.WriteLine("{0}, {1}, {2}, {3}", sample.Item1, result.Item1, result.Item2, result.Item3);
                Assert.AreEqual(sample.Item2, result.Item1);
                Assert.AreEqual(sample.Item3, result.Item2, 0.01);
                Assert.AreEqual(sample.Item4, result.Item3, 0.01);
            }
        }

        private static int FindIt<TFitness>(List<TFitness> history, TFitness fitness)
            where TFitness : IComparable<TFitness>
        {
            var index = history.BinarySearch(fitness);
            if (index < 0) index = ~index;
            return index;
        }

        [TestMethod]
        public void SearchIntTest()
        {
            // ints are increasung
            Assert.IsTrue(6.CompareTo(2) > 0);

            var history = new List<int> {2, 4, 6};
            for (var fitness = 0; fitness < 8; fitness++)
            {
                var index = FindIt(history, fitness);
                Console.WriteLine("{0}: {1}", fitness, index);
            }
        }

        [TestMethod]
        public void SearchFitnessTest()
        {
            // Fitness are decreasing
            Assert.IsTrue(new Fitness(6).CompareTo(new Fitness(2)) < 0);

            var history = new List<Fitness> {new Fitness(6), new Fitness(4), new Fitness(2)};
            for (var fitness = 0; fitness < 8; fitness++)
            {
                var index = FindIt(history, new Fitness(fitness));
                Console.WriteLine("{0}: {1}", fitness, index);
            }
        }
    }
}