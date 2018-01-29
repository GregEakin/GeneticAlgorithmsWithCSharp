using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace GeneticAlgorithms.TicTacToe
{
    [TestClass]
    public class FitnessTests
    {
        [TestMethod]
        public void FitnessToStringTest()
        {
            var fitness = new Fitness(10, 20, 30, 100);
            Assert.AreEqual("50.0% Losses (30), 33.3% Ties (20), 16.7% Wins (10), 100 rules", fitness.ToString());
        }
    }

    [TestClass]
    public class TicTacToeTests2
    {
        [TestMethod]
        public void DisplayTest()
        {
            var ticTacToe = new TicTacToeTests();
            var geneSet = ticTacToe.CreateGeneSet();
            var fitness = new Fitness(1, 2, 3, 4);
            var candidate =
                new Chromosome<Rule, Fitness>(geneSet, fitness, Chromosome<Rule, Fitness>.Strategies.None);
            var watch = Stopwatch.StartNew();
            ticTacToe.Display(candidate, watch);
        }

        [TestMethod]
        public void MutateAddTest()
        {
            var ticTacToe = new TicTacToeTests();
            var genes = new List<Rule>();
            var geneSet = ticTacToe.CreateGeneSet();
            Assert.IsTrue(ticTacToe.MutateAdd(genes, geneSet));
            Assert.AreEqual(1, genes.Count);
            Assert.IsTrue(ticTacToe.MutateAdd(genes, geneSet));
            Assert.AreEqual(2, genes.Count);
        }

        [TestMethod]
        public void GetMoveTest()
        {
            var ticTacToe = new TicTacToeTests();

            var geneSet = new[]
            {
                new RuleMetadata((expectedContent, count) => new CenterFilter()),
            };

            var genes = geneSet.SelectMany(g => g.CreateRules()).ToArray();

            var board = Enumerable.Range(1, 9).ToDictionary(i => i, i => new Square(i));
            var empties = board.Values.Where(v => v.Content == ContentType.Empty).ToArray();

            var x = ticTacToe.GetMove(genes, board, empties);
            CollectionAssert.AreEqual(new[] {5, 0}, x);
        }
    }
}