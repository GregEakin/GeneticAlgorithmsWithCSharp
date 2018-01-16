using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace GeneticAlgorithms.LawnmowerProblem
{
    [TestClass]
    public class LawnmowerTests
    {
        private static readonly Random Random = new Random();

        public delegate Tuple<Field, Mower, Program> FnEvaluateDelegate(INode[] genes);

        public delegate Fitness FnFitnessDelegate(INode[] genes);

        public delegate Field CreateFieldFun();

        public static Fitness Fitness(INode[] genes, FnEvaluateDelegate fnEvaluate)
        {
            var tuple = fnEvaluate(genes);
            return new Fitness(tuple.Item1.CountMowed(), genes.Length, tuple.Item2.StepCount);
        }

        public static void Display(Chromosome<INode, Fitness> candidate, Stopwatch watch, FnEvaluateDelegate fnEvaluate)
        {
            var tuple = fnEvaluate(candidate.Genes);
            tuple.Item1.Display(tuple.Item2);
            Console.WriteLine("{0}\t{1} ms", candidate.Fitness, watch.ElapsedMilliseconds);
            tuple.Item3.Print();
            Console.WriteLine();
        }

        public static INode[] Mutate(INode[] input, Func<INode>[] geneSet, int minGenes, int maxGenes,
            FnFitnessDelegate fnFitness, int maxRounds)
        {
            var genes = new List<INode>(input);
            var count = Random.Next(1, maxRounds + 1);
            var initialFitness = fnFitness(input);
            while (count-- > 0)
            {
                var fitness = fnFitness(genes.ToArray());
                if (fitness.CompareTo(initialFitness) > 0)
                    return genes.ToArray();

                var adding = genes.Count == 0 || genes.Count <= maxGenes && Random.Next(0, 6) == 0;
                if (adding)
                {
                    genes.Add(geneSet[Random.Next(geneSet.Length)]());
                    continue;
                }

                var removing = genes.Count > minGenes && Random.Next(0, 51) == 0;
                if (removing)
                {
                    genes.RemoveAt(Random.Next(genes.Count));
                    continue;
                }

                var index = Random.Next(genes.Count);
                genes[index] = geneSet[Random.Next(geneSet.Length)]();
            }

            return genes.ToArray();
        }

        public static INode[] Create(Func<INode>[] geneSet, int minGenes, int maxGenes)
        {
            var numGenes = Random.Next(minGenes, maxGenes + 1);
            var genes = new INode[numGenes];
            for (var i = 0; i < numGenes; i++)
                genes[i] = geneSet[Random.Next(geneSet.Length)]();
            return genes;
        }

        public static INode[] Crossover(INode[] mother, INode[] father)
        {
            if (mother.Length <= 2)
                return mother;
            var length = Random.Next(1, mother.Length - 1);
            var start = Random.Next(0, mother.Length - length + 1);
            if (father.Length < start + length)
                return mother;

            var child = new INode[mother.Length];
            Array.Copy(mother, child, start);
            Array.Copy(father, start, child, start, length);
            Array.Copy(mother, start + length, child, start + length, mother.Length - start - length);
            return child;
        }

        [TestMethod]
        public void MutateTest()
        {
            var parent = new INode[] {new Mow(), new Mow(), new Mow(), new Mow(),};
            var geneSet = new Func<INode>[] {() => new Turn()};

            Tuple<Field, Mower, Program> FnEvaluate(INode[] instructions)
            {
                var field = new ToroidField(10, 10, FieldContents.Grass);
                var location = new Location(5, 5);
                var dir = Directions.North;
                var mower = new Mower(location, dir);
                var program = new Program(instructions);

                program.Evaluate(mower, field);
                return new Tuple<Field, Mower, Program>(field, mower, program);
            }

            Fitness FnFitness(INode[] genes) => Fitness(genes, FnEvaluate);

            var child = Mutate(parent, geneSet, 2, 5, FnFitness, 1);
            Assert.IsTrue(child.Any(c => c.GetType() == typeof(Mow)));
            FnEvaluate(child);
        }

        [TestMethod]
        public void CreateTest()
        {
            var geneSet = new Func<INode>[] {() => new Mow(), () => new Turn()};
            var child = Create(geneSet, 2, 5);
            Assert.IsTrue(child.Length >= 2);
            Assert.IsTrue(child.Length <= 5);
        }

        [TestMethod]
        public void CrossoverTest()
        {
            var mother = new INode[] {new Mow(), new Mow(), new Mow(),};
            var father = new INode[] {new Turn(), new Turn(), new Turn(), new Turn(),};
            var child = Crossover(mother, father);
            Assert.AreEqual(mother.Length, child.Length);
            Assert.AreEqual(1, child.Count(c => c.GetType() == typeof(Turn)));
        }

        [TestMethod]
        public void MowTurnTest()
        {
            var width = 8;
            var height = 8;
            var geneSet = new Func<INode>[] {() => new Mow(), () => new Turn(),};
            var minGenes = width * height;
            var maxGenes = 3 * minGenes / 2;
            var maxMutationRounds = 3;
            var expectedNumberOfInstructions = 78;

            Field FnCreateField() => new ToroidField(width, height, FieldContents.Grass);

            RunWith(geneSet, width, height, minGenes, maxGenes,
                expectedNumberOfInstructions, maxMutationRounds,
                FnCreateField, expectedNumberOfInstructions);
        }

        [TestMethod]
        public void MowTurnJumpTest()
        {
            var width = 8;
            var height = 8;
            var geneSet = new Func<INode>[]
            {
                () => new Mow(),
                () => new Turn(),
                () => new Jump(Random.Next(0, Math.Min(width, height)), Random.Next(0, Math.Min(width, height)))
            };
            var minGenes = width * height;
            var maxGenes = 3 * minGenes / 2;
            var maxMutationRounds = 1;
            var expectedNumberOfInstructions = 64;

            Field FnCreateField() => new ToroidField(width, height, FieldContents.Grass);

            RunWith(geneSet, width, height, minGenes, maxGenes,
                expectedNumberOfInstructions, maxMutationRounds,
                FnCreateField, expectedNumberOfInstructions);
        }

        [TestMethod]
        public void MowTurnJumpValidatingTest()
        {
            var width = 8;
            var height = 8;
            var geneSet = new Func<INode>[]
            {
                () => new Mow(),
                () => new Turn(),
                () => new Jump(Random.Next(0, Math.Min(width, height)), Random.Next(0, Math.Min(width, height)))
            };
            var minGenes = width * height;
            var maxGenes = 3 * minGenes / 2;
            var maxMutationRounds = 3;
            var expectedNumberOfInstructions = 79;

            Field FnCreateField() => new ValidatingField(width, height, FieldContents.Grass);

            RunWith(geneSet, width, height, minGenes, maxGenes,
                expectedNumberOfInstructions, maxMutationRounds,
                FnCreateField, expectedNumberOfInstructions);
        }

        [TestMethod]
        public void MowTurnRepeatTest()
        {
            var width = 8;
            var height = 8;
            var geneSet = new Func<INode>[]
            {
                () => new Mow(),
                () => new Turn(),
                () => new Repeat(Random.Next(0, Math.Min(width, height)), Random.Next(0, Math.Min(width, height)))
            };
            var minGenes = 3;
            var maxGenes = 20;
            var maxMutationRounds = 3;
            var expectedNumberOfInstructions = 9;
            var expectedNumberOfSteps = 88;

            Field FnCreateField() => new ToroidField(width, height, FieldContents.Grass);

            RunWith(geneSet, width, height, minGenes, maxGenes,
                expectedNumberOfInstructions, maxMutationRounds,
                FnCreateField, expectedNumberOfSteps);
        }

        [TestMethod]
        public void MowTurnJumpFuncTest()
        {
            var width = 8;
            var height = 8;
            var geneSet = new Func<INode>[]
            {
                () => new Mow(),
                () => new Turn(),
                () => new Jump(Random.Next(0, Math.Min(width, height)), Random.Next(0, Math.Min(width, height))),
                () => new Func()
            };
            var minGenes = 3;
            var maxGenes = 20;
            var maxMutationRounds = 3;
            var expectedNumberOfInstructions = 18;
            var expectedNumberOfSteps = 65;

            Field FnCreateField() => new ToroidField(width, height, FieldContents.Grass);

            RunWith(geneSet, width, height, minGenes, maxGenes,
                expectedNumberOfInstructions, maxMutationRounds,
                FnCreateField, expectedNumberOfSteps);
        }

        [TestMethod]
        public void MowTurnJumpCallTest()
        {
            var width = 8;
            var height = 8;
            var geneSet = new Func<INode>[]
            {
                () => new Mow(),
                () => new Turn(),
                () => new Jump(Random.Next(0, Math.Min(width, height)), Random.Next(0, Math.Min(width, height))),
                () => new Func(true),
                () => new Call(Random.Next(0, 6)),
            };
            var minGenes = 3;
            var maxGenes = 20;
            var maxMutationRounds = 3;
            var expectedNumberOfInstructions = 18;
            var expectedNumberOfSteps = 65;

            Field FnCreateField() => new ToroidField(width, height, FieldContents.Grass);

            RunWith(geneSet, width, height, minGenes, maxGenes,
                expectedNumberOfInstructions, maxMutationRounds,
                FnCreateField, expectedNumberOfSteps);
        }

        private void RunWith(Func<INode>[] geneSet, int width, int height, int minGenes, int maxGenes,
            int expectedNumberOfInstructions, int maxMutationRounds, CreateFieldFun fnCreateField,
            int expectedNumberOfSteps)
        {
            var genetic = new Genetic<INode, Fitness>();
            var mowerStartLocation = new Location(width / 2, height / 2);
            var mowerStartDirection = Directions.South;

            INode[] FnCreate() => Create(geneSet, 1, height);

            Tuple<Field, Mower, Program> FnEvaluate(INode[] instructions)
            {
                var program = new Program(instructions);
                var mower = new Mower(mowerStartLocation, mowerStartDirection);
                var field = fnCreateField();

                program.Evaluate(mower, field);
                return new Tuple<Field, Mower, Program>(field, mower, program);
            }

            Fitness FnFitness(INode[] genes) => Fitness(genes, FnEvaluate);

            var watch = Stopwatch.StartNew();

            void FnDisplay(Chromosome<INode, Fitness> candidate) => Display(candidate, watch, FnEvaluate);

            INode[] FnMutate(INode[] child) => Mutate(child, geneSet, minGenes, maxGenes, FnFitness, maxMutationRounds);

            var optimalFitness = new Fitness(width * height, expectedNumberOfInstructions, expectedNumberOfSteps);

            var best = genetic.BestFitness(FnFitness, 0, optimalFitness, null, FnDisplay, FnMutate, FnCreate, 0, 10,
                Crossover, 30);

            Assert.IsTrue(best.Fitness.CompareTo(optimalFitness) <= 0);
        }
    }
}