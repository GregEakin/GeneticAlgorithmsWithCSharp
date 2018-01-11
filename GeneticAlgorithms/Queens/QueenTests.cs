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
        public static Fitness Fitness(int[] genes, int size)
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

            var total = size - rowsWithQueens.Count +
                        size - colsWithQueens.Count +
                        size - northEastDiagonalsWithQueens.Count +
                        size - southEastDiagonalsWithQueens.Count;
            return new Fitness(total);
        }

        public static void Display(Chromosome<int, Fitness> candidate, Stopwatch watch, int size)
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
            var genetic = new Genetic<char, int>();
            var gene = genetic.RandomSample(geneSet, geneSet.Length);
            Assert.IsInstanceOfType(gene, typeof(char[]));
            Assert.AreEqual(geneSet.Length, gene.Length);
            Assert.IsTrue(geneSet.All(c => gene.Contains(c)));
        }

        [TestMethod]
        public void RandomSampelTest2()
        {
            var geneSet = "ABCabc".ToCharArray();
            var genetic = new Genetic<char, char>();
            var gene = genetic.RandomSample(geneSet, 2 * geneSet.Length);
            Assert.IsInstanceOfType(gene, typeof(char[]));
            CollectionAssert.AreEquivalent("AABBCCaabbcc".ToCharArray(), gene);
        }

        [TestMethod]
        public void RandomSampelTest3()
        {
            var geneSet = new[] {1, 2, 3};
            var genetic = new Genetic<int, char>();
            int[] gene;
            do
                gene = genetic.RandomSample(geneSet, geneSet.Length);
            while (gene.SequenceEqual(geneSet));
            CollectionAssert.AreEqual(new[] {1, 2, 3}, geneSet);
            CollectionAssert.AreNotEqual(new[] {1, 2, 3}, gene);
        }


        [TestMethod]
        public void GenerateParentTest()
        {
            int FitnessFun(int[] guess, int size) => 86;
            var genetic = new Genetic<int, int>();

            var geneSet = new[] {1, 3, 5, 7, 9};
            var parent = genetic.GenerateParent(geneSet, 10, FitnessFun);
            Assert.IsInstanceOfType(parent, typeof(Chromosome<int, int>));
            Assert.AreEqual(86, parent.Fitness);
            Assert.IsTrue(geneSet.All(c => parent.Genes.Contains(c)));
        }

        [TestMethod]
        public void MutateTest()
        {
            int FitnessFun(int[] guess, int size) => 86;
            var genetic = new Genetic<int, int>();
            var geneSet = new[] {0, 1, 2, 3, 4, 5, 6, 7};
            var genes = new[] {0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0};
            var parent = new Chromosome<int, int>(genes, 0);
            var child = genetic.Mutate(geneSet, parent, FitnessFun);
            Assert.IsTrue(parent.Genes.All(x => x == 0));
            Assert.IsFalse(child.Genes.All(x => x == 0));
        }

        [TestMethod]
        public void FourQueensTest()
        {
            var genetic = new Genetic<int, Fitness>();
            var geneSet = new[] {0, 1, 2, 3};
            var watch = new Stopwatch();
            void DisplayFun(Chromosome<int, Fitness> candidate) => Display(candidate, watch, geneSet.Length);

            watch.Start();
            var best = genetic.BestFitness(Fitness, geneSet.Length, new Fitness(0), geneSet, DisplayFun);
            watch.Stop();
            Assert.IsTrue(best.Fitness.Total <= 0);
        }

        [TestMethod]
        public void EightQueensTest()
        {
            var genetic = new Genetic<int, Fitness>();
            var geneSet = new[] {0, 1, 2, 3, 4, 5, 6, 7};
            var watch = new Stopwatch();
            void DisplayFun(Chromosome<int, Fitness> candidate) => Display(candidate, watch, geneSet.Length);

            watch.Start();
            var best = genetic.BestFitness(Fitness, geneSet.Length, new Fitness(0), geneSet, DisplayFun);
            watch.Stop();
            Assert.IsTrue(best.Fitness.Total <= 0);
        }
    }
}