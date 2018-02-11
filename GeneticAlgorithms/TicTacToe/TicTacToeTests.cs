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

using GeneticAlgorithms.Utilities;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace GeneticAlgorithms.TicTacToe
{
    [TestClass]
    public class TicTacToeTests
    {
        public delegate bool FnMutateDelegate(List<Rule> genes);

        public delegate Fitness FnGetFitness(List<Rule> genes);

        private static Fitness GetFitness(List<Rule> genes)
        {
            var localCopy = genes.ToList();
            var fitness = GetFitnessForAllGames(localCopy);
            return new Fitness(fitness.Wins, fitness.Ties, fitness.Losses, genes.Count);
        }

        private static CompetitionResult PlayOneOnOne(List<Rule> xGenes, List<Rule> oGenes)
        {
            var board = Square.Indexes.ToDictionary(i => i, i => new Square(i));
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

                var move = GetMove(genes, board, empties);
                if (move == null) // could not find a move
                    return lossResult;

                var index = (int)move;
                board[index] = new Square(index, piece);

                var mostRecentMoveOnly = new[] {board[index]};
                if (new RowContentFilter(piece, 3).GetMatches(board, mostRecentMoveOnly).Any() ||
                    new ColumnContentFilter(piece, 3).GetMatches(board, mostRecentMoveOnly).Any() ||
                    new DiagonalContentFilter(piece, 3).GetMatches(board, mostRecentMoveOnly).Any())
                    return winResult;

                empties = board.Values.Where(v => v.Content == ContentType.Empty).ToArray();
            }

            return CompetitionResult.Tie;
        }

        public static Fitness GetFitnessForAllGames(IList<Rule> genes)
        {
            var board = Square.Indexes.ToDictionary(i => i, i => new Square(i));
            var queue = new Queue<Dictionary<int, Square>>();

            // if we go first, we get to pick any square
            queue.Enqueue(board);

            // If the opponet starts, let them start at each square
            foreach (var square in board.Values)
            {
                var candiateCopy = new Dictionary<int, Square>(board)
                {
                    [square.Index] = new Square(square.Index, ContentType.Opponent)
                };
                queue.Enqueue(candiateCopy);
            }

            var wins = 0;
            var ties = 0;
            var losses = 0;
            while (queue.Any())
            {
                board = queue.Dequeue();
                var empties = board.Values.Where(v => v.Content == ContentType.Empty).ToArray();
                if (!empties.Any())
                {
                    // no moves left
                    ties++;
                    continue;
                }

                // It's our turn
                var move = GetMove(genes, board, empties);
                if (move == null) // could not find a move
                {
                    // There are empties, but didn't find a move
                    losses++;
                    continue;
                }

                var index = (int)move;
                board[index] = new Square(index, ContentType.Mine);

                if (DidWeWin(board))
                {
                    wins++;
                    continue;
                }

                if (WillWeLose(board))
                {
                    losses++;
                    continue;
                }

                // queue all remaning OPPONENT responses
                foreach (var square in empties.Where(s => s.Index != index))
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

        private static bool DidWeWin(Dictionary<int, Square> board)
        {
            var mine = board.Values.Where(s => s.Content == ContentType.Mine).ToArray();
            for (var i = 0; i < mine.Length - 2; i++)
            for (var j = i + 1; j < mine.Length - 1; j++)
            for (var k = j + 1; k < mine.Length; k++)
            {
                var sum = 15 - mine[i].Index - mine[j].Index - mine[k].Index;
                if (sum == 0)
                    return true;
            }

            return false;
        }

        private static bool WillWeLose(Dictionary<int, Square> board)
        {
            var theirs = board.Values.Where(s => s.Content == ContentType.Opponent).ToArray();
            for (var i = 0; i < theirs.Length - 1; i++)
            for (var j = i + 1; j < theirs.Length; j++)
            {
                var sum = 15 - theirs[i].Index - theirs[j].Index;
                if (sum > 0 && sum <= 9 && board[sum].Content == ContentType.Empty)
                    return true;
            }

            return false;
        }

        public static int? GetMove(IEnumerable<Rule> ruleSet, Dictionary<int, Square> board, Square[] empties, int startingRuleIndex = 0)
        {
            foreach (var gene in ruleSet.Skip(startingRuleIndex))
            {
                var matches = gene.GetMatches(board, empties);
                if (matches.Count == 0)
                    continue;
                if (matches.Count == 1)
                    return matches.Single();
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

        public static bool MutateAdd(List<Rule> genes, Rule[] geneSet)
        {
            var index = Rand.Random.Next(genes.Count + 1);
            genes.Insert(index, Rand.Select(geneSet));
            return true;
        }

        public static bool MutateRemove(List<Rule> genes)
        {
            if (!genes.Any())
                return false;
            genes.RemoveAt(Rand.Random.Next(genes.Count));
            if (genes.Any() && Rand.PercentChance(50))
                genes.RemoveAt(Rand.Random.Next(genes.Count));
            return true;
        }

        public static bool MutateReplace(List<Rule> genes, Rule[] geneSet)
        {
            if (!genes.Any())
                return false;
            var index = Rand.Random.Next(genes.Count);
            genes[index] = Rand.Select(geneSet);
            return true;
        }

        public static bool MutateSwapAdjacent(List<Rule> genes)
        {
            if (genes.Count < 2)
                return false;
            var index = Rand.Random.Next(genes.Count - 1);
            var temp = genes[index + 1];
            genes[index + 1] = genes[index];
            genes[index] = temp;
            return true;
        }

        public static bool MutateMove(List<Rule> genes)
        {
            if (genes.Count < 3)
                return MutateSwapAdjacent(genes);
            var length = Rand.Random.Next(1, 3);
            var skip = Rand.Random.Next(genes.Count - length + 1);
            var toMove = genes.Skip(skip).Take(length).ToArray();
            genes.RemoveRange(skip, length);
            var index = Rand.Random.Next(genes.Count);
            if (index >= skip)
                index++;
            genes.InsertRange(index, toMove);
            return true;
        }

        private static void Mutate(List<Rule> genes, FnGetFitness fnGetFitness, FnMutateDelegate[] mutationOperators,
            List<int> mutationRoundCounts)
        {
            var initialFitness = fnGetFitness(genes);
            var count = Rand.Select(mutationRoundCounts);
            for (var i = 1; i < count + 2; i++)
            {
                foreach (var func in mutationOperators.OrderBy(o => Rand.Random.Next()))
                {
                    var worked = func(genes);
                    if (worked)
                        break;
                }

                if (fnGetFitness(genes).CompareTo(initialFitness) <= 0)
                    continue;

                mutationRoundCounts.Add(i);
                break;
            }

            // Is it worth it to remove duplicate genes?
            //var seen = new HashSet<string>();
            //genes.RemoveAll(x => !seen.Add(x.ToString()));
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
                new RuleMetadata((expectedContent, count) => new Noop()),
            };

            var genes = geneSet.SelectMany(g => g.CreateRules()).ToArray();
            Console.WriteLine("Created {0} genes", genes.Length);
            return genes;
        }

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
                Rand.RandomSampleList(geneset, Rand.Random.Next(minGenes, maxGenes));

            var optimalFitness = new Fitness(620, 120, 0, 11);

            var best = Genetic<Rule, Fitness>.GetBest(FnGetFitness, minGenes, optimalFitness, null, FnDisplay, FnMutate,
                FnCreate, 500, 20, FnCrossover);
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
                    new Chromosome<Rule, Fitness>(genes, new Fitness(wins, ties, losses, genes.Count), Strategies.None),
                    watch);
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

            var unused =
                Genetic<Rule, Fitness>.Tournament(FnCreate, FnCrossover, PlayOneOnOne, FnDisplay, FnSortKey, 13);
        }
    }
}