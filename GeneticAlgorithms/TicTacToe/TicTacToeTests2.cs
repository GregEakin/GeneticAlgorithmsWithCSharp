/* File: TicTacToeTests2.cs
 *     from chapter 18 of _Genetic Algorithms with Python_
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

using System;
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
            var geneSet = TicTacToeTests.CreateGeneSet().ToList();
            var fitness = new Fitness(1, 2, 3, 4);
            var candidate =
                new Chromosome<Rule, Fitness>(geneSet, fitness, Strategies.None);
            var watch = Stopwatch.StartNew();
            TicTacToeTests.Display(candidate, watch);
        }

        [TestMethod]
        public void MutateAddTest()
        {
            var genes = new List<Rule>();
            var geneSet = TicTacToeTests.CreateGeneSet();
            Assert.IsTrue(TicTacToeTests.MutateAdd(genes, geneSet));
            Assert.AreEqual(1, genes.Count);
            Assert.IsTrue(TicTacToeTests.MutateAdd(genes, geneSet));
            Assert.AreEqual(2, genes.Count);
        }

        [TestMethod]
        public void MutateRemove()
        {
            var genes = new List<Rule> {new DiagonalContentFilter(null, null)};
            Assert.IsTrue(TicTacToeTests.MutateRemove(genes));
            Assert.IsFalse(genes.Any());
        }

        [TestMethod]
        public void MutateReplace()
        {
            var genes = new List<Rule> {new DiagonalContentFilter(null, null)};
            var geneSet = new Rule[] {new DiagonalOppositeFilter(null)};
            Assert.IsTrue(TicTacToeTests.MutateReplace(genes, geneSet));
            Assert.AreEqual(1, genes.Count);
            Assert.IsInstanceOfType(genes[0], typeof(DiagonalOppositeFilter));
        }

        [TestMethod]
        public void MutateSwapAdjacent()
        {
            var genes = new List<Rule> {new DiagonalContentFilter(null, null), new DiagonalOppositeFilter(null)};
            Assert.IsTrue(TicTacToeTests.MutateSwapAdjacent(genes));
            Assert.IsInstanceOfType(genes[0], typeof(DiagonalOppositeFilter));
            Assert.IsInstanceOfType(genes[1], typeof(DiagonalContentFilter));
        }

        [TestMethod]
        public void MutateMoveTest()
        {
            var save = new List<Rule>
            {
                new DiagonalContentFilter(null, null),
                new DiagonalOppositeFilter(null),
                new RowOppositeFilter(null)
            };

            var genes = save.ToList();
            Assert.IsTrue(TicTacToeTests.MutateMove(genes));
            CollectionAssert.AreNotEqual(save, genes);
        }

        [TestMethod]
        public void GetMoveTest()
        {
            var geneSet = new[]
            {
                new RuleMetadata((expectedContent, count) => new CenterFilter()),
            };

            var genes = geneSet.SelectMany(g => g.CreateRules()).ToList();

            var board = Enumerable.Range(1, 9).ToDictionary(i => i, i => new Square(i));
            var empties = board.Values.Where(v => v.Content == ContentType.Empty).ToArray();

            var move = TicTacToeTests.GetMove(genes, board, empties);
            Assert.IsNotNull(move);
            Assert.AreEqual(5, (int)move);
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
        public void GetFitnessForAllGamesTest()
        {
            var geneSet = new[]
            {
                new RuleMetadata((expectedContent, count) => new CenterFilter()),
            };
            var genes = geneSet.SelectMany(g => g.CreateRules()).ToList();
            var fitness = TicTacToeTests.GetFitnessForAllGames(genes);
            Assert.AreEqual("100.0% Losses (65), 0.0% Ties (0), 0.0% Wins (0), 1 rules", fitness.ToString());
        }
    }
}