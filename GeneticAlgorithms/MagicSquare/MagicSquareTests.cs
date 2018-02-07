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
        public static Fitness GetFitness(List<int> genes, int diagonalSize, int expectedSum)
        {
            var sums = Sums(genes, diagonalSize);
            var a1 = sums.Item1.Where(r => r != expectedSum).Select(s => Math.Abs(s - expectedSum)).Sum();
            var a2 = sums.Item2.Where(c => c != expectedSum).Select(s => Math.Abs(s - expectedSum)).Sum();
            var a3 = (sums.Item3 != expectedSum) ? Math.Abs(sums.Item3 - expectedSum) : 0;
            var a4 = (sums.Item4 != expectedSum) ? Math.Abs(sums.Item4 - expectedSum) : 0;
            var sumOfDifferences = a1 + a2 + a3 + a4;
            return new Fitness(sumOfDifferences);
        }

        public static void Display(Chromosome<int, Fitness> candidate, Stopwatch watch, int diagonalSize)
        {
            var sums = Sums(candidate.Genes, diagonalSize);
            for (var rowNumber = 0; rowNumber < diagonalSize; rowNumber++)
            {
                var row = candidate.Genes.Skip(rowNumber * diagonalSize).Take(diagonalSize);
                Console.WriteLine("\t{0} = {1}", string.Join(",", row), sums.Item1[rowNumber]);
            }

            Console.WriteLine("{0}\t{1}\t{2}", sums.Item3, string.Join(",", sums.Item2), sums.Item4);
            Console.WriteLine(" - - - - - - - - - - - {0}, {1} ms", candidate.Fitness, watch.ElapsedMilliseconds);
        }

        public static Tuple<int[], int[], int, int> Sums(List<int> genes, int diagonalSize)
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

                northeastDiagonalSum += genes[row * diagonalSize + (diagonalSize - 1 - row)];
                southeastDiagonalSum += genes[row * diagonalSize + row];
            }

            return new Tuple<int[], int[], int, int>(rows, columns, northeastDiagonalSum, southeastDiagonalSum);
        }

        private static void Mutate(List<int> genes, int[] allPositions)
        {
            var randomSample = Rand.RandomSampleArray(allPositions, 2);
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

        private static void Generate(int diagonalSize, int maxAge)
        {
            var watch = Stopwatch.StartNew();

            var nSquared = diagonalSize * diagonalSize;
            var geneSet = Enumerable.Range(1, nSquared).ToArray();
            var expectedSum = diagonalSize * (nSquared + 1) / 2;
            var geneIndexes = Enumerable.Range(0, geneSet.Length).ToArray();

            Fitness FnGetFitness(List<int> genes) => GetFitness(genes, diagonalSize, expectedSum);
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
            var historicalFitnesses = Enumerable.Range(0, 26).Select(n => 50 - 2 * n).ToArray();
            var index = Array.BinarySearch(historicalFitnesses, 13, new Genetic<int, int>.ReverseComparer<int>());
            if (index < 0) index = ~index;
            Assert.AreEqual(19, index);
        }

        [TestMethod]
        public void SimulatedAnnealingSearchTestEven()
        {
            var historicalFitnesses = Enumerable.Range(0, 26).Select(n => 50 - 2 * n).ToArray();
            var index = Array.BinarySearch(historicalFitnesses, 12, new Genetic<int, int>.ReverseComparer<int>());
            if (index < 0) index = ~index;
            Assert.AreEqual(19, index);
        }

        public Tuple<int, double, double> SimulatedAnnealingExp(int value)
        {
            var historicalFitnesses = Enumerable.Range(0, 51).Select(n => 50 - n).ToList();
            var index = historicalFitnesses.BinarySearch(value, new Genetic<int, int>.ReverseComparer<int>());
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
                new Tuple<int, int, double, double>(0, 50, 0.02, 0.98),
                new Tuple<int, int, double, double>(5, 45, 0.12, 0.89),
                new Tuple<int, int, double, double>(10, 40, 0.22, 0.81),
                new Tuple<int, int, double, double>(40, 10, 0.80, 0.45),
                new Tuple<int, int, double, double>(45, 5, 0.90, 0.41),
                new Tuple<int, int, double, double>(50, 0, 1.00, 0.37),
            };

            foreach (var sample in samples)
            {
                var result = SimulatedAnnealingExp(sample.Item1);
                Assert.AreEqual(sample.Item2, result.Item1);
                Assert.AreEqual(sample.Item3, result.Item2, 0.01);
                Assert.AreEqual(sample.Item4, result.Item3, 0.01);
            }
        }
    }
}