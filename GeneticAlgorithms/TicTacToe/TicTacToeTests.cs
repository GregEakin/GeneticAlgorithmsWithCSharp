/* File: TicTacToeTests.cs
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

using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using GeneticAlgorithms.Utilities;

namespace GeneticAlgorithms.TicTacToe
{
    [TestClass]
    public class TicTacToeTests
    {
        private readonly int[] _squareIndexes = {1, 2, 3, 4, 5, 6, 7, 8, 9};

        private Fitness GetFitness(List<Rule> genes)
        {
            var localCopy = genes.ToList();
            var fitness = GetFitnessForGames(localCopy);
            return new Fitness(fitness.Wins, fitness.Ties, fitness.Losses, genes.Count);
        }

        private CompetitionResult PlayOneOnOne(List<Rule> xGenes, List<Rule> oGenes)
        {
            var board = Enumerable.Range(1, 9).ToDictionary(i => i, i => new Square(i));
            var empties = board.Values.Where(v => v.Content == ContentType.Empty).ToArray();
            var roundData = new[]
            {
                new Tuple<List<Rule>, ContentType, CompetitionResult, CompetitionResult>(xGenes, ContentType.Mine,
                    CompetitionResult.Loss, CompetitionResult.Win),
                new Tuple<List<Rule>, ContentType, CompetitionResult, CompetitionResult>(oGenes, ContentType.Opponent,
                    CompetitionResult.Win, CompetitionResult.Loss),
            };

            var playerIndex = 0;
            while (empties.Any())
            {
                var playerData = roundData[playerIndex];
                playerIndex = 1 - playerIndex;
                var genes = playerData.Item1;
                var piece = playerData.Item2;
                var lossResult = playerData.Item3;
                var winResult = playerData.Item4;

                var moveAndRuleIndex = GetMove(genes, board, empties);
                if (moveAndRuleIndex == null) // could not find a move
                    return lossResult;

                var index = moveAndRuleIndex[0];
                board[index] = new Square(index, piece);

                var mostRecentMoveOnly = new[] {board[index]};
                if (new RowContentFilter(piece, 3).Matches(board, mostRecentMoveOnly).Any() ||
                    new ColumnContentFilter(piece, 3).Matches(board, mostRecentMoveOnly).Any() ||
                    new DiagonalContentFilter(piece, 3).Matches(board, mostRecentMoveOnly).Any())
                    return winResult;

                empties = board.Values.Where(v => v.Content == ContentType.Empty).ToArray();
            }

            return CompetitionResult.Tie;
        }

        public Fitness GetFitnessForGames(List<Rule> genes)
        {
            string GetBoardString(IReadOnlyDictionary<int, Square> b) =>
                string.Join("",
                    _squareIndexes.Select(i =>
                        b[i].Content == ContentType.Empty ? "." : b[i].Content == ContentType.Mine ? "x" : "o"));

            var board = Enumerable.Range(1, 9).ToDictionary(i => i, i => new Square(i));
            var queue = new Queue<Dictionary<int, Square>>();
            queue.Enqueue(board);
            foreach (var square in board.Values)
            {
                var candiateCopy = new Dictionary<int, Square>(board)
                {
                    [square.Index] = new Square(square.Index, ContentType.Opponent)
                };
                queue.Enqueue(candiateCopy);
            }

            var winningRules = new Dictionary<int, List<string>>();
            var wins = 0;
            var ties = 0;
            var losses = 0;
            while (queue.Any())
            {
                board = queue.Dequeue();
                var boardString = GetBoardString(board);
                var empties = board.Values.Where(v => v.Content == ContentType.Empty).ToArray();

                if (!empties.Any())
                {
                    ties++;
                    continue;
                }

                var candidateIndexAndRuleIndex = GetMove(genes, board, empties);

                if (candidateIndexAndRuleIndex == null) // could not find a move
                {
                    // There are empties, but didn't find a move
                    losses++;
                    // Go to next board
                    continue;
                }

                // found at least one move
                var index = candidateIndexAndRuleIndex[0];
                board[index] = new Square(index, ContentType.Mine);
                // newBoardString = getBoardString(board)

                // if we now have three MINE in any ROW, COLUMN or DIAGONAL, we won
                var mostRecentMoveOnly = new[] {board[index]};
                if (HaveThreeInRow.Matches(board, mostRecentMoveOnly).Any() ||
                    HaveThreeInColumn.Matches(board, mostRecentMoveOnly).Any() ||
                    HaveThreeInDiagonal.Matches(board, mostRecentMoveOnly).Any())
                {
                    var ruleId = candidateIndexAndRuleIndex[1];
                    if (!winningRules.ContainsKey(ruleId))
                        winningRules[ruleId] = new List<string>();
                    winningRules[ruleId].Add(boardString);
                    wins++;
                    // Go to next board
                    continue;
                }

                // we lose if any empties have two OPPONENT pieces in ROW, COL or DIAG
                empties = board.Values.Where(v => v.Content == ContentType.Empty).ToArray();
                if (OpponentHasTwoInARow.Matches(board, empties).Any())
                {
                    losses++;
                    // Go to next board
                    continue;
                }

                // queue all possible OPPONENT responses
                foreach (var square in empties)
                {
                    var candiateCopy = new Dictionary<int, Square>(board)
                    {
                        [square.Index] = new Square(square.Index, ContentType.Opponent)
                    };
                    queue.Enqueue(candiateCopy);
                }
            }

            return new Fitness(wins, ties, losses, genes.Count);
        }

        public static int[] GetMove(List<Rule> ruleSet, Dictionary<int, Square> board, Square[] empties,
            int startingRuleIndex = 0)
        {
            var ruleSetCopy = ruleSet.ToArray();
            for (var ruleIndex = startingRuleIndex; ruleIndex < ruleSetCopy.Length; ruleIndex++)
            {
                var gene = ruleSetCopy[ruleIndex];
                var matches = gene.Matches(board, empties);
                if (matches.Count == 0)
                    continue;
                if (matches.Count == 1)
                    return new[] {matches.First(), ruleIndex};
                if (empties.Length > matches.Count)
                    empties = empties.Where(e => matches.Contains(e.Index)).ToArray();
            }

            return null;
        }

        public static void Display(Chromosome<Rule, Fitness> candidate, Stopwatch watch)
        {
            var localCopy = candidate.Genes;
            localCopy.Reverse();
            Console.WriteLine("\t{0}\n{1}\n{2} ms", string.Join("\n\t", localCopy.Select(g => g.ToString())),
                candidate.Fitness, watch.ElapsedMilliseconds);
        }

        public bool MutateAdd(List<Rule> genes, Rule[] geneSet)
        {
            var index = Rand.Random.Next(genes.Count);
            genes.Insert(index, geneSet[Rand.Random.Next(geneSet.Length)]);
            return true;
        }

        public bool MutateRemove(List<Rule> genes)
        {
            if (!genes.Any())
                return false;
            genes.RemoveAt(Rand.Random.Next(genes.Count));
            if (genes.Any() && Rand.Random.Next(2) == 1)
                genes.RemoveAt(Rand.Random.Next(genes.Count));
            return true;
        }

        public bool MutateReplace(List<Rule> genes, Rule[] geneSet)
        {
            if (!genes.Any())
                return false;
            var index = Rand.Random.Next(genes.Count);
            genes[index] = geneSet[Rand.Random.Next(geneSet.Length)];
            return false;
        }

        public bool MutateSwapAdjacent(List<Rule> genes)
        {
            if (genes.Count < 2)
                return false;
            var index = Rand.Random.Next(genes.Count - 1);
            var temp = genes[index + 1];
            genes[index + 1] = genes[index];
            genes[index] = temp;
            return true;
        }

        public bool MutateMove(List<Rule> genes)
        {
            if (genes.Count < 3)
                return false;
            var skip = Rand.Random.Next(genes.Count - 1);
            var length = Rand.Random.Next(1, 3);
            var toMove = genes.Skip(skip).Take(length).ToArray();
            genes.RemoveRange(skip, length);
            var index = Rand.Random.Next(genes.Count);
            genes.InsertRange(index, toMove);
            return true;
        }

        private void Mutate(List<Rule> genes, Func<List<Rule>, Fitness> fnGetFitness,
            FnMutateDelegate[] mutationOperators,
            List<int> mutationRoundCounts)
        {
            var initialFitness = fnGetFitness(genes);
            var count = Rand.Random.Next(mutationRoundCounts.Count);
            for (var i = 1; i < count + 2; i++)
            {
                var copy = mutationOperators.ToList();
                var func = copy[Rand.Random.Next(copy.Count)];
                while (!func(genes))
                {
                    copy.Remove(func);
                    func = copy[Rand.Random.Next(copy.Count)];
                }

                if (fnGetFitness(genes).CompareTo(initialFitness) > 0)
                {
                    mutationRoundCounts.Add(i);
                    return;
                }
            }
        }

        public static Rule[] CreateGeneSet()
        {
            var options = new[]
            {
                new Tuple<ContentType?, int[]>(ContentType.Opponent, new[] {0, 1, 2}),
                new Tuple<ContentType?, int[]>(ContentType.Mine, new[] {0, 1, 2}),
            };

            var geneSet = new[]
            {
                new RuleMetadata((expectedContent, count) => new RowContentFilter(expectedContent, count), options),
                new RuleMetadata((expectedContent, count) => new TopRowFilter(), options),
                new RuleMetadata((expectedContent, count) => new MiddleRowFilter(), options),
                new RuleMetadata((expectedContent, count) => new BottomRowFilter(), options),
                new RuleMetadata((expectedContent, count) => new ColumnContentFilter(expectedContent, count), options),
                new RuleMetadata((expectedContent, count) => new LeftColumnFilter(), options),
                new RuleMetadata((expectedContent, count) => new MiddleColumnFilter(), options),
                new RuleMetadata((expectedContent, count) => new RightColumnFilter(), options),
                new RuleMetadata((expectedContent, count) => new DiagonalContentFilter(expectedContent, count),
                    options),
                new RuleMetadata((expectedContent, count) => new DiagonalLocationFilter(), options),
                new RuleMetadata((expectedContent, count) => new CornerFilter()),
                new RuleMetadata((expectedContent, count) => new SideFilter()),
                new RuleMetadata((expectedContent, count) => new CenterFilter()),
                new RuleMetadata((expectedContent, count) => new RowOppositeFilter(expectedContent), options),
                new RuleMetadata((expectedContent, count) => new ColumnOppositeFilter(expectedContent), options),
                new RuleMetadata((expectedContent, count) => new DiagonalOppositeFilter(expectedContent), options),
            };

            var genes = geneSet.SelectMany(g => g.CreateRules()).ToArray();
            Console.WriteLine("Created {0} genes", genes.Length);
            return genes;
        }

        public delegate bool FnMutateDelegate(List<Rule> genes);

        [TestMethod]
        public void PerfectKnowledgeTest()
        {
            var minGenes = 10;
            var maxGenes = 20;
            var geneset = CreateGeneSet();
            var watch = Stopwatch.StartNew();

            void FnDisplay(Chromosome<Rule, Fitness> candidate, int? length) =>
                Display(candidate, watch);

            Fitness FnGetFitness(List<Rule> genes) =>
                GetFitness(genes);

            var mutationRoundCounts = new List<int> {1};

            var mutationOperators = new FnMutateDelegate[]
            {
                genes => MutateAdd(genes, geneset),
                genes => MutateReplace(genes, geneset),
                MutateRemove,
                MutateSwapAdjacent,
                MutateMove,
            };

            void FnMutate(List<Rule> genes) =>
                Mutate(genes, FnGetFitness, mutationOperators, mutationRoundCounts);

            List<Rule> FnCrossover(List<Rule> parent, List<Rule> doner)
            {
                var child = parent.Take(parent.Count / 2).Concat(doner.Skip(doner.Count / 2)).ToList();
                FnMutate(child);
                return child;
            }

            List<Rule> FnCreate() =>
                Rand.RandomSampleList(geneset.ToArray(), Rand.Random.Next(minGenes, maxGenes));

            var optimalFitness = new Fitness(620, 120, 0, 11);

            var best = Genetic<Rule, Fitness>.GetBest(FnGetFitness, minGenes, optimalFitness, null, FnDisplay, FnMutate, FnCreate,
                500, 20, FnCrossover);
            Assert.IsTrue(optimalFitness.CompareTo(best.Fitness) <= 0);
        }

        [TestMethod]
        public void TornamentTest()
        {
            var minGenes = 10;
            var maxGenes = 20;
            var geneset = CreateGeneSet();
            var watch = Stopwatch.StartNew();

            void FnDisplay(List<Rule> genes, int wins, int ties, int losses, int generation)
            {
                Console.WriteLine("-- generation {0} --", generation);
                Display(
                    new Chromosome<Rule, Fitness>(genes, new Fitness(wins, ties, losses, genes.Count),
                        Chromosome<Rule, Fitness>.Strategies.None), watch);
            }

            var mutationRoundCounts = new List<int> {1};

            var mutationOperators = new FnMutateDelegate[]
            {
                genes => MutateAdd(genes, geneset),
                genes => MutateReplace(genes, geneset),
                MutateRemove,
                MutateSwapAdjacent,
                MutateMove,
            };

            void FnMutate(List<Rule> genes) =>
                Mutate(genes, x => new Fitness(0, 0, 0, 0), mutationOperators, mutationRoundCounts);

            List<Rule> FnCrossover(List<Rule> parent, List<Rule> doner)
            {
                var child = parent.Take(parent.Count / 2).Concat(doner.Skip(doner.Count / 2)).ToList();
                FnMutate(child);
                return child;
            }

            List<Rule> FnCreate() =>
                Rand.RandomSampleList(geneset, Rand.Random.Next(minGenes, maxGenes));

            int FnSortKey(List<Rule> genes, int wins, int ties, int losses) => -1000 * losses - ties + 1 / genes.Count;

            var unused = Genetic<Rule, Fitness>.Tournament(FnCreate, FnCrossover, PlayOneOnOne, FnDisplay, FnSortKey, 13);
        }

        static Rule HaveThreeInRow => new RowContentFilter(ContentType.Mine, 3);
        static Rule HaveThreeInColumn => new ColumnContentFilter(ContentType.Mine, 3);
        static Rule HaveThreeInDiagonal => new DiagonalContentFilter(ContentType.Mine, 3);
        static Rule OpponentHasTwoInARow => new WinFilter(ContentType.Opponent);
    }
}