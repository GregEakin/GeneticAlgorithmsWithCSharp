using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace GeneticAlgorithms.Knights
{
    [TestClass]
    public class KnightTests
    {
        private static readonly Random Random = new Random();

        public delegate Position RandomPosition();

        public static int Fitness(Position[] genes, int width, int height)
        {
            var attacks = from kn in genes
                from pos in Attacks(kn, width, height)
                select pos;
            return attacks.Distinct().Count();
        }

        public static void Display(Chromosome<Position, int> candidate, Stopwatch watch, int width, int height)
        {
            var board = new Board(candidate.Genes, width, height);
            Console.WriteLine(board);

            Console.WriteLine("{0}\n\t{1}\t{2} ms",
                string.Join<Position>(" ", candidate.Genes),
                candidate.Fitness,
                watch.ElapsedMilliseconds);
        }

        public static Position[] Mutate(Position[] input, int width, int height, Position[] allPositions,
            Position[] nonEdgePositions)
        {
            var genes = input.ToArray();
            var loops = Random.Next(10) == 0 ? 2 : 1;
            for (var count = 0; count < loops; count++)
            {
                var positionToKnightIndexes =
                    allPositions.ToDictionary(position => position, position => new List<int>());
                for (var i = 0; i < genes.Length; i++)
                    foreach (var position in Attacks(genes[i], width, height))
                        positionToKnightIndexes[position].Add(i);

                var knightIndexes = new HashSet<int>(Enumerable.Range(0, genes.Length));
                var unattacked = new List<Position>();
                foreach (var kvp in positionToKnightIndexes)
                {
                    if (kvp.Value.Count > 1)
                        continue;
                    if (kvp.Value.Count == 0)
                    {
                        unattacked.Add(kvp.Key);
                        continue;
                    }

                    foreach (var p in kvp.Value)
                        knightIndexes.Remove(p);
                }

                var knightPositions = unattacked.SelectMany(x => Attacks(x, width, height))
                    .Where(nonEdgePositions.Contains).Distinct();
                var potentialKnightPositions = unattacked.Count > 0
                    ? knightPositions.ToArray()
                    : nonEdgePositions;

                var geneIndex = knightIndexes.Count == 0
                    ? Random.Next(genes.Length)
                    : knightIndexes.ElementAt(Random.Next(knightIndexes.Count));

                var position1 = potentialKnightPositions[Random.Next(potentialKnightPositions.Length)];
                genes[geneIndex] = position1;
            }

            return genes;
        }

        public Position[] Create(RandomPosition randomPositionFun, int expectedKnights)
        {
            var genes = Enumerable.Range(0, expectedKnights).Select(k => randomPositionFun());
            return genes.ToArray();
        }

        public static Position[] Attacks(Position location, int width, int height)
        {
            var indexes = new[] {-2, -1, 1, 2};
            var attacks = from x in indexes
                where 0 <= x + location.X && x + location.X < width
                from y in indexes
                where 0 <= y + location.Y && y + location.Y < height
                where Math.Abs(x) != Math.Abs(y)
                select new Position(x + location.X, y + location.Y);
            return attacks.ToArray();
        }

        [TestMethod]
        public void Knight_3x4Test()
        {
            FindKnightPositions(4, 3, 6);
        }

        [TestMethod]
        public void Knight_8x8Test()
        {
            FindKnightPositions(8, 8, 14);
        }

        [TestMethod]
        public void Knight_10x10Test()
        {
            FindKnightPositions(10, 10, 22);
        }

        [TestMethod]
        public void Knight_12x12Test()
        {
            FindKnightPositions(12, 12, 28);
        }

        [TestMethod]
        public void Knight_13x13Test()
        {
            FindKnightPositions(13, 13, 32);
        }

        public void FindKnightPositions(int width, int height, int expectedKnights)
        {
            var generic = new Genetic<Position, int>();
            var watch = Stopwatch.StartNew();

            int FitnessFun(Position[] genes) => Fitness(genes, width, height);
            void DisplayFun(Chromosome<Position, int> candidate) => Display(candidate, watch, width, height);

            var allPositions = (from x in Enumerable.Range(0, width)
                from y in Enumerable.Range(0, height)
                select new Position(x, y)).ToArray();

            var nonEdgePositions = (width < 6 || height < 6
                ? allPositions
                : (from p in allPositions
                    where 0 < p.X && p.X < width - 1 && 0 < p.Y && p.Y < height - 1
                    select p)).ToArray();

            Position RandomPositionFun() => nonEdgePositions[Random.Next(nonEdgePositions.Length)];
            Position[] MutateFun(Position[] genes) => Mutate(genes, width, height, allPositions, nonEdgePositions);
            Position[] CreateFun() => Create(RandomPositionFun, expectedKnights);

            var optimalFitness = width * height;
            var best = generic.BestFitness(FitnessFun, 0, optimalFitness, null, DisplayFun, MutateFun, CreateFun);
            Assert.IsFalse(optimalFitness > best.Fitness);
        }

        [TestMethod]
        public void AttacksTest1()
        {
            var position = new Position(0, 0);
            var attacks = Attacks(position, 8, 8);
            CollectionAssert.AreEquivalent(new[]
            {
                new Position(1, 2),
                new Position(2, 1),
            }, attacks);
        }

        [TestMethod]
        public void AttacksTest2()
        {
            var position = new Position(4, 4);
            var attacks = Attacks(position, 8, 8);
            CollectionAssert.AreEquivalent(new[]
            {
                new Position(6, 5),
                new Position(6, 3),
                new Position(5, 6),
                new Position(5, 2),
                new Position(3, 2),
                new Position(3, 6),
                new Position(2, 3),
                new Position(2, 5),
            }, attacks);
        }

        [TestMethod]
        public void FitnessTest1()
        {
            var genes = new[]
            {
                new Position(1, 0),
                new Position(2, 0),
                new Position(3, 0),
                new Position(0, 2),
                new Position(1, 2),
                new Position(2, 2),
            };
            var fitness = Fitness(genes, 4, 3);
            Assert.AreEqual(12, fitness);
        }

        [TestMethod]
        public void DisplayTest1()
        {
            var genes = new[]
            {
                new Position(1, 0),
                new Position(2, 0),
                new Position(3, 0),
                new Position(0, 2),
                new Position(1, 2),
                new Position(2, 2),
            };

            var candidate = new Chromosome<Position, int>(genes, 86);
            var watch = Stopwatch.StartNew();
            Display(candidate, watch, 4, 3);
        }

        [TestMethod]
        public void BoardTest()
        {
            var genes = new[] {new Position(0, 0), new Position(1, 1), new Position(2, 2), new Position(3, 3)};
            var board = new Board(genes, 8, 8);
            Console.WriteLine(board.ToString());
        }

        [TestMethod]
        public void MutateTest()
        {
            var genes = new[] {new Position(1, 2), new Position(2, 1)};
            int width = 5;
            int height = 5;
            var allPositions = from x in Enumerable.Range(0, width)
                from y in Enumerable.Range(0, height)
                select new Position(x, y);

            var allPositionsArray = allPositions.ToArray();

            var b1 = new Board(genes, width, height);
            Console.WriteLine(b1);

            genes = Mutate(genes, width, height, allPositionsArray, allPositionsArray);
            var b2 = new Board(genes, width, height);
            Console.WriteLine(b2);
        }
    }
}