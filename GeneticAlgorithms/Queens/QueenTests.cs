/* File: QueenTests.cs
 *     from chapter 4 of _Genetic Algorithms with Python_
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

using GeneticAlgorithms.SortedNumbers;
using GeneticAlgorithms.Utilities;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace GeneticAlgorithms.Queens
{
    [TestClass]
    public class QueenTests
    {
        private static Fitness Fitness(int[] genes, int size)
        {
            var board = new Board(genes, size);
            var rowsWithQueens = new HashSet<int>();
            var colsWithQueens = new HashSet<int>();
            var northEastDiagonalsWithQueens = new HashSet<int>();
            var southEastDiagonalsWithQueens = new HashSet<int>();
            for (var row = 0; row < size; row++)
            for (var col = 0; col < size; col++)
                if (board[row, col] == 'Q')
                {
                    rowsWithQueens.Add(row);
                    colsWithQueens.Add(col);
                    northEastDiagonalsWithQueens.Add(row + col);
                    southEastDiagonalsWithQueens.Add(size - 1 - row + col);
                }

            var total = size - rowsWithQueens.Count
                        + size - colsWithQueens.Count
                        + size - northEastDiagonalsWithQueens.Count
                        + size - southEastDiagonalsWithQueens.Count;
            return new Fitness(total);
        }

        private static void Display(Chromosome<int, Fitness> candidate, Stopwatch watch, int size)
        {
            var board = new Board(candidate.Genes, size);
            Console.WriteLine(board.ToString());
            Console.WriteLine("{0}\t- {1}\t{2} ms", string.Join(",", candidate.Genes), candidate.Fitness,
                watch.ElapsedMilliseconds);
        }

        [TestMethod]
        public void BoardTest1()
        {
            var gene = new[] {0, 0, 1, 1, 2, 2, 3, 3};
            var board = new Board(gene, gene.Length / 2);
            var output = board.ToString();
            Console.WriteLine(output);
        }

        [TestMethod]
        public void BoardTest2()
        {
            var gene = new[] {0, 0, 1, 1, 2, 2, 3, 3, 4, 4, 5, 5, 6, 6, 7, 7};
            var board = new Board(gene, gene.Length / 2);
            var output = board.ToString();
            Console.WriteLine(output);
        }

        [TestMethod]
        public void FitnessFunTest1()
        {
            var gene = new[] {0, 0, 1, 1, 2, 2, 3, 3, 4, 4, 5, 5, 6, 6, 7, 7};
            var fitness = Fitness(gene, gene.Length / 2);
            Assert.AreEqual(7, fitness.Total);
        }

        [TestMethod]
        public void FitnessFunTest2()
        {
            var gene = new[] {4, 0, 2, 4, 0, 1, 7, 2, 6, 5, 3, 7, 1, 6, 5, 3};
            var fitness = Fitness(gene, gene.Length / 2);
            Assert.AreEqual(0, fitness.Total);
        }

        [TestMethod]
        public void RandomSampelTest1()
        {
            var geneSet = "ABCabc".ToCharArray();
            var gene = Rand.RandomSample(geneSet, geneSet.Length).ToArray();
            Assert.IsInstanceOfType(gene, typeof(char[]));
            Assert.AreEqual(geneSet.Length, gene.Length);
            Assert.IsTrue(geneSet.All(c => gene.Contains(c)));
        }

        [TestMethod]
        public void RandomSampelTest2()
        {
            var geneSet = "ABCabc".ToCharArray();
            var gene = Rand.RandomSample(geneSet, 2 * geneSet.Length).ToArray();
            Assert.IsInstanceOfType(gene, typeof(char[]));
            CollectionAssert.AreEquivalent("AABBCCaabbcc".ToCharArray(), gene);
        }

        [TestMethod]
        public void RandomSampelTest3()
        {
            var geneSet = new[] {1, 2, 3};
            int[] gene;
            do
                gene = Rand.RandomSampleArray(geneSet, geneSet.Length); while (gene.SequenceEqual(geneSet));

            CollectionAssert.AreEqual(new[] {1, 2, 3}, geneSet);
            CollectionAssert.AreNotEqual(new[] {1, 2, 3}, gene);
        }

        [TestMethod]
        public void GenerateParentTest()
        {
            int FitnessFun(int[] guess) => 86;

            var geneSet = new[] {1, 3, 5, 7, 9};
            var parent = Genetic<int, int>.GenerateParent(10, geneSet, FitnessFun);
            Assert.IsInstanceOfType(parent, typeof(Chromosome<int, int>));
            Assert.AreEqual(86, parent.Fitness);
            Assert.IsTrue(geneSet.All(c => parent.Genes.Contains(c)));
        }

        [TestMethod]
        public void MutateTest()
        {
            int FitnessFun(int[] guess) => 86;
            var geneSet = new[] {0, 1, 2, 3, 4, 5, 6, 7};
            var genes = new[] {0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0};
            var parent = new Chromosome<int, int>(genes, 0);
            var child = Genetic<int, int>.Mutate(parent, geneSet, FitnessFun);
            Assert.IsTrue(parent.Genes.All(x => x == 0));
            Assert.IsFalse(child.Genes.All(x => x == 0));
        }

        [TestMethod]
        public void FourQueensTest()
        {
            Test(4);
        }

        [TestMethod]
        public void EightQueensTest()
        {
            Test(8);
        }

        private static void Test(int size)
        {
            var geneSet = Enumerable.Range(0, size).ToArray();
            var watch = Stopwatch.StartNew();

            void DisplayFun(Chromosome<int, Fitness> candidate) => Display(candidate, watch, geneSet.Length);
            Fitness FitnessFun(int[] genes) => Fitness(genes, size);

            var optimalFitness = new Fitness(0);
            var best = Genetic<int, Fitness>.GetBest(FitnessFun, 2 * size, optimalFitness, geneSet, DisplayFun);
            Assert.IsTrue(optimalFitness.CompareTo(best.Fitness) <= 0);
        }

        [TestMethod]
        public void BenchmarkTest()
        {
            Benchmark.Run(() => Test(20));
        }
    }
}