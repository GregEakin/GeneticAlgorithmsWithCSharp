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
            var geneSet = TicTacToeTests.CreateGeneSet();
            var fitness = new Fitness(1, 2, 3, 4);
            var candidate =
                new Chromosome<Rule, Fitness>(geneSet, fitness, Chromosome<Rule, Fitness>.Strategies.None);
            var watch = Stopwatch.StartNew();
            TicTacToeTests.Display(candidate, watch);
        }

        [TestMethod]
        public void MutateAddTest()
        {
            var ticTacToe = new TicTacToeTests();
            var genes = new List<Rule>();
            var geneSet = TicTacToeTests.CreateGeneSet();
            Assert.IsTrue(ticTacToe.MutateAdd(genes, geneSet));
            Assert.AreEqual(1, genes.Count);
            Assert.IsTrue(ticTacToe.MutateAdd(genes, geneSet));
            Assert.AreEqual(2, genes.Count);
        }

        [TestMethod]
        public void GetMoveTest()
        {
            var geneSet = new[]
            {
                new RuleMetadata((expectedContent, count) => new CenterFilter()),
            };

            var genes = geneSet.SelectMany(g => g.CreateRules()).ToArray();

            var board = Enumerable.Range(1, 9).ToDictionary(i => i, i => new Square(i));
            var empties = board.Values.Where(v => v.Content == ContentType.Empty).ToArray();

            var x = TicTacToeTests.GetMove(genes, board, empties);
            CollectionAssert.AreEqual(new[] {5, 0}, x);
        }

        [TestMethod]
        public void BoardStringTest()
        {
            string GetBoardString(IReadOnlyDictionary<int, Square> b) =>
                string.Join("",
                    new[] {1, 2, 3, 4, 5, 6, 7, 8, 9}.Select(i =>
                        b[i].Content == ContentType.Empty ? "." : b[i].Content == ContentType.Mine ? "x" : "o"));

            var board = Enumerable.Range(1, 9).ToDictionary(i => i, i => new Square(i));
            board[1] = new Square(board[1].Index, ContentType.Mine);
            board[5] = new Square(board[5].Index, ContentType.Opponent);

            Assert.AreEqual("x...o....", GetBoardString(board));
        }

        [TestMethod]
        public void GetFitnessForGameTest()
        {
            var geneSet = new[]
            {
                new RuleMetadata((expectedContent, count) => new CenterFilter()),
            };
            var genes = geneSet.SelectMany(g => g.CreateRules()).ToArray();
            var ticTacToe = new TicTacToeTests();
            var x = ticTacToe.GetFitnessForGames(genes);
            Assert.AreEqual("100.0% Losses (65), 0.0% Ties (0), 0.0% Wins (0), 1 rules", x.ToString());
        }
    }
}