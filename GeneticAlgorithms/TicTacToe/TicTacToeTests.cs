using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace GeneticAlgorithms.TicTacToe
{
    [TestClass]
    public class TicTacToeTests
    {
        private readonly Random _random = new Random();

        public Fitness GetFitness(Rule[] genes)
        {
            var localCopy = genes.ToArray();
            var fitness = GetFitnessForGames(localCopy);
            return new Fitness(fitness.Wins, fitness.Ties, fitness.Losses, genes.Length);
        }

        private readonly int[] _squareIndexes = {1, 2, 3, 4, 5, 6, 7, 8, 9};

        public CompetitionResult PlayOneOnOne(Rule[] xGenes, Rule[] oGenes)
        {
            var board = Enumerable.Range(1, 9 + 1).ToDictionary(i => i, i => new Square(i));
            var empties = board.Values.Where(v => v.Content == ContentType.Empty).ToArray();
            var roundData = new[]
            {
                new Tuple<Rule[], ContentType, CompetitionResult, CompetitionResult>(xGenes, ContentType.Mine,
                    CompetitionResult.Loss, CompetitionResult.Win),
                new Tuple<Rule[], ContentType, CompetitionResult, CompetitionResult>(oGenes, ContentType.Opponent,
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
                if (moveAndRuleIndex == null)
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

        public Fitness GetFitnessForGames(Rule[] genes)
        {
            string GetBoardString(Dictionary<int, Square> b) =>
                string.Join("",
                    _squareIndexes.Select(i =>
                        b[i].Content == ContentType.Empty ? "." : b[i].Content == ContentType.Mine ? "x" : "o"));

            var board = Enumerable.Range(1, 9).ToDictionary(i => i, i => new Square(i));
            var queue = new Queue<Dictionary<int, Square>>();
            queue.Enqueue(board);
            foreach (var square in board.Values)
            {
                var candiateCopy = new Dictionary<int, Square>(board);
                candiateCopy[square.Index] = new Square(square.Index, ContentType.Opponent);
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

                if (candidateIndexAndRuleIndex == null)
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
                    var candiateCopy = new Dictionary<int, Square>(board);
                    candiateCopy[square.Index] = new Square(square.Index, ContentType.Opponent);
                    queue.Enqueue(candiateCopy);
                }
            }

            return new Fitness(wins, ties, losses, genes.Length);
        }

        public int[] GetMove(Rule[] ruleSet, Dictionary<int, Square> board, Square[] empties, int startingRuleIndex = 0)
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

        public void Display(Chromosome<Rule, Fitness> candidate, Stopwatch watch)
        {
            var localCopy = candidate.Genes.Reverse().Select(g => g.ToString());
            Console.WriteLine("\t{0}\n{1}\n{2} ms", string.Join("\n\t", localCopy), candidate.Fitness,
                watch.ElapsedMilliseconds);
        }

        [TestMethod]
        public void DisplayTest()
        {
            var geneSet = CreateGeneSet();
            var fitness = new Fitness(1, 2, 3, 4);
            var candidate =
                new Chromosome<Rule, Fitness>(geneSet, fitness, Chromosome<Rule, Fitness>.Strategies.None);
            var watch = Stopwatch.StartNew();
            Display(candidate, watch);
        }

        public bool MutateAdd(List<Rule> genes, Rule[] geneSet)
        {
            var index = _random.Next(genes.Count);
            genes.Insert(index, geneSet[_random.Next(geneSet.Length)]);
            return true;
        }

        [TestMethod]
        public void MutateAddTest()
        {
            var genes = new List<Rule>();
            var geneSet = CreateGeneSet();
            Assert.IsTrue(MutateAdd(genes, geneSet));
            Assert.AreEqual(1, genes.Count);
            Assert.IsTrue(MutateAdd(genes, geneSet));
            Assert.AreEqual(2, genes.Count);
        }

        public bool MutateRemove(List<Rule> genes)
        {
            if (!genes.Any())
                return false;
            genes.RemoveAt(_random.Next(genes.Count));
            if (genes.Any() && _random.Next(2) == 1)
                genes.RemoveAt(_random.Next(genes.Count));
            return true;
        }

        public bool MutateReplace(List<Rule> genes, Rule[] geneSet)
        {
            if (!genes.Any())
                return false;
            var index = _random.Next(genes.Count);
            genes[index] = geneSet[_random.Next(geneSet.Length)];
            return false;
        }

        public bool MutateSwapAdjacent(List<Rule> genes)
        {
            if (genes.Count < 2)
                return false;
            var index = _random.Next(genes.Count - 1);
            var temp = genes[index + 1];
            genes[index + 1] = genes[index];
            genes[index] = temp;
            return true;
        }

        public bool MutateMove(List<Rule> genes)
        {
            if (genes.Count < 3)
                return false;
            var skip = _random.Next(genes.Count - 1);
            var length = _random.Next(1, 3);
            var toMove = genes.Skip(skip).Take(length).ToArray();
            genes.RemoveRange(skip, length);
            var index = _random.Next(genes.Count);
            genes.InsertRange(index, toMove);
            return true;
        }

        public Rule[] Mutate(Rule[] input, Func<Rule[], Fitness> fnGetFitness, FnMutateDelegate[] mutationOperators,
            List<int> mutationRoundCounts)
        {
            var genes = input.ToList();
            var initialFitness = fnGetFitness(genes.ToArray());
            var count = _random.Next(mutationRoundCounts.Count);
            for (var i = 1; i < count + 2; i++)
            {
                var copy = mutationOperators.ToList();
                var func = copy[_random.Next(copy.Count)];
                while (!func(genes))
                {
                    copy.Remove(func);
                    func = copy[_random.Next(copy.Count)];
                }

                if (fnGetFitness(genes.ToArray()).CompareTo(initialFitness) > 0)
                {
                    mutationRoundCounts.Add(i);
                    return genes.ToArray();
                }
            }

            return genes.ToArray();
        }

        public Rule[] CreateGeneSet()
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
            var genetic = new Genetic<Rule, Fitness>();

            var minGenes = 10;
            var maxGenes = 20;
            var geneset = CreateGeneSet();
            var watch = Stopwatch.StartNew();

            void FnDisplay(Chromosome<Rule, Fitness> candidate, int? length) => Display(candidate, watch);

            Fitness FnGetFitness(Rule[] genes) => GetFitness(genes);

            var mutationRoundCounts = new List<int> {1};

            var mutationOperators = new FnMutateDelegate[]
            {
                genes => MutateAdd(genes, geneset),
                genes => MutateReplace(genes, geneset),
                MutateRemove,
                MutateSwapAdjacent,
                MutateMove,
            };

            Rule[] FnMutate(Rule[] genes) => Mutate(genes, FnGetFitness, mutationOperators, mutationRoundCounts);

            Rule[] FnCrossover(Rule[] parent, Rule[] doner)
            {
                var child = new List<Rule>();
                child.AddRange(parent.Take(parent.Length / 2));
                child.AddRange(doner.Skip(doner.Length / 2));
                var retval = FnMutate(child.ToArray());
                return retval;
            }

            Rule[] FnCreate() => genetic.RandomSample(geneset, _random.Next(minGenes, maxGenes));

            var optimalFitness = new Fitness(620, 120, 0, 11);

            var best = genetic.BestFitness(FnGetFitness, minGenes, optimalFitness, null, FnDisplay, FnMutate, FnCreate,
                500, 20, FnCrossover);
            Assert.IsTrue(optimalFitness.CompareTo(best.Fitness) <= 0);
        }

        [TestMethod]
        public void TornamentTest()
        {
            var genetic = new Genetic<Rule, Fitness>();

            var minGenes = 10;
            var maxGenes = 20;
            var geneset = CreateGeneSet();
            var watch = Stopwatch.StartNew();

            void FnDisplay(Rule[] genes, int wins, int ties, int losses, int generation)
            {
                Console.WriteLine("-- generation {0} --", generation);
                Display(
                    new Chromosome<Rule, Fitness>(genes, new Fitness(wins, ties, losses, genes.Length),
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

            Rule[] FnMutate(Rule[] genes) => Mutate(genes, x => null, mutationOperators, mutationRoundCounts);

            Rule[] FnCrossover(Rule[] parent, Rule[] doner)
            {
                var child = new List<Rule>();
                child.AddRange(parent.Take(parent.Length / 2));
                child.AddRange(doner.Skip(doner.Length / 2));
                var retval = FnMutate(child.ToArray());
                return retval;
            }

            Rule[] FnCreate() => genetic.RandomSample(geneset, _random.Next(minGenes, maxGenes));

            int FnSortKey(Rule[] genes, int wins, int ties, int losses) => -1000 * losses - ties + 1 / genes.Length;

            var best = genetic.Tournament(FnCreate, FnCrossover, PlayOneOnOne, FnDisplay, FnSortKey, 13);
        }

        static Rule HaveThreeInRow => new RowContentFilter(ContentType.Mine, 3);
        static Rule HaveThreeInColumn => new ColumnContentFilter(ContentType.Mine, 3);
        static Rule HaveThreeInDiagonal => new DiagonalContentFilter(ContentType.Mine, 3);
        static Rule OpponentHasTwoInARow => new WinFilter(ContentType.Opponent);

        [TestMethod]
        public void FitnessTest1()
        {
            var fitness = new Fitness(10, 10, 10, 100);
            Assert.AreEqual("33.3% Losses (10), 33.3% Ties (10), 33.3% Wins (10), 100 rules", fitness.ToString());
        }
    }
}