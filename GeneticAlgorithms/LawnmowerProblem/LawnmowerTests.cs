/* File: LawnmowerTests.cs
 *     from chapter 15 of _Genetic Algorithms with Python_
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

using GeneticAlgorithms.ApproximatingPi;
using GeneticAlgorithms.Utilities;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace GeneticAlgorithms.LawnmowerProblem
{
    [TestClass]
    public class LawnmowerTests
    {
        public delegate Field FnCreateFieldDelegate();

        public delegate INode FnCreateGeneDelegate();

        public delegate Fitness FnGetFitnessDelegate(IReadOnlyList<INode> genes);

        public delegate Tuple<Field, Mower, Program> FnEvaluateDelegate(IReadOnlyList<INode> genes);

        private static Fitness GetFitness(IReadOnlyList<INode> genes, FnEvaluateDelegate fnEvaluate)
        {
            var tuple = fnEvaluate(genes);
            return new Fitness(tuple.Item1.CountMowed(), genes.Count, tuple.Item2.StepCount);
        }

        private static void Display(Chromosome<INode, Fitness> candidate, Stopwatch watch,
            FnEvaluateDelegate fnEvaluate)
        {
            var tuple = fnEvaluate(candidate.Genes);
            tuple.Item1.Display(tuple.Item2);
            Console.WriteLine("{0}\t{1} ms", candidate.Fitness, watch.ElapsedMilliseconds);
            tuple.Item3.Print();
            Console.WriteLine();
        }

        private static void Mutate(List<INode> genes, IReadOnlyList<FnCreateGeneDelegate> geneSet, int minGenes, int maxGenes,
            FnGetFitnessDelegate fnGetFitness, int maxRounds)
        {
            var count = Rand.Random.Next(1, maxRounds + 1);
            var initialFitness = fnGetFitness(genes);
            while (count-- > 0)
            {
                if (fnGetFitness(genes).CompareTo(initialFitness) > 0)
                    return;

                var adding = genes.Count == 0 || genes.Count < maxGenes - 1 && Rand.PercentChance(20);
                if (adding)
                {
                    var gene1 = Rand.SelectItem(geneSet)();
                    genes.Add(gene1);
                    continue;
                }

                var removing = genes.Count > minGenes && Rand.PercentChance(2);
                if (removing)
                {
                    var index1 = Rand.Random.Next(genes.Count);
                    genes.RemoveAt(index1);
                    continue;
                }

                // Replace gene
                var gene = Rand.SelectItem(geneSet)();
                var index = Rand.Random.Next(genes.Count);
                genes[index] = gene;
            }
        }

        private static List<INode> Create(IReadOnlyList<FnCreateGeneDelegate> geneSet, int minGenes, int maxGenes)
        {
            var numGenes = Rand.Random.Next(minGenes, maxGenes + 1);
            var genes = new List<INode>(numGenes);
            for (var i = 0; i < numGenes; i++)
                genes.Add(Rand.SelectItem(geneSet)());
            return genes;
        }

        private static List<INode> Crossover(IReadOnlyList<INode> mother, IReadOnlyList<INode> father)
        {
            if (mother.Count <= 2)
                return mother.ToList();
            var length = Rand.Random.Next(1, mother.Count - 1);
            var start = Rand.Random.Next(0, mother.Count - length + 1);
            if (father.Count < start + length)
                return mother.ToList();

            var child = mother.Take(start)
                .Concat(father.Skip(start).Take(length))
                .Concat(mother.Skip(start + length))
                .ToList();
            return child;
        }

        [TestMethod]
        public void MutateTest()
        {
            var parent = new List<INode> {new Mow(), new Mow(), new Mow(), new Mow()};
            var geneSet = new FnCreateGeneDelegate[] {() => new Turn()};

            Tuple<Field, Mower, Program> FnEvaluate(IReadOnlyList<INode> instructions)
            {
                var field = new ToroidField(10, 10, FieldContents.Grass);
                var location = new Location(5, 5);
                var dir = Directions.North;
                var mower = new Mower(location, dir);
                var program = new Program(instructions);

                program.Evaluate(mower, field, 0);
                return new Tuple<Field, Mower, Program>(field, mower, program);
            }

            Fitness FnFitness(IReadOnlyList<INode> genes) =>
                GetFitness(genes, FnEvaluate);

            var copy = parent.ToList();
            Mutate(parent, geneSet, 2, 5, FnFitness, 1);
            CollectionAssert.AreNotEqual(copy, parent);
            Assert.IsTrue(parent.Any(c => c.GetType() != typeof(Mow)));
        }

        [TestMethod]
        public void CreateTest()
        {
            var geneSet = new FnCreateGeneDelegate[] {() => new Mow(), () => new Turn()};
            var child = Create(geneSet, 2, 5);
            Assert.IsTrue(child.Count >= 2);
            Assert.IsTrue(child.Count <= 5);
        }

        [TestMethod]
        public void CrossoverTest()
        {
            var mother = new List<INode> {new Mow(), new Mow(), new Mow(),};
            var father = new List<INode> {new Turn(), new Turn(), new Turn(), new Turn(),};
            var child = Crossover(mother, father);
            Assert.AreEqual(mother.Count, child.Count);
            Assert.AreEqual(1, child.Count(c => c.GetType() == typeof(Turn)));
        }

        [TestMethod]
        public void MowTurnTest()
        {
            var width = 8;
            var height = 8;
            var geneSet = new FnCreateGeneDelegate[] {() => new Mow(), () => new Turn(),};
            var minGenes = width * height;
            var maxGenes = 3 * minGenes / 2;
            var maxMutationRounds = 3;
            var expectedNumberOfInstructions = 78;

            Field FnCreateField() =>
                new ToroidField(width, height, FieldContents.Grass);

            RunWith(geneSet, width, height, minGenes, maxGenes,
                expectedNumberOfInstructions, maxMutationRounds,
                FnCreateField, expectedNumberOfInstructions);
        }

        [TestMethod]
        public void MowTurnJumpTest()
        {
            var width = 8;
            var height = 8;
            var geneSet = new FnCreateGeneDelegate[]
            {
                () => new Mow(),
                () => new Turn(),
                () => new Jump(Rand.Random.Next(0, Math.Min(width, height)),
                    Rand.Random.Next(0, Math.Min(width, height)))
            };
            var minGenes = width * height;
            var maxGenes = 3 * minGenes / 2;
            var maxMutationRounds = 1;
            var expectedNumberOfInstructions = 64;

            Field FnCreateField() =>
                new ToroidField(width, height, FieldContents.Grass);

            RunWith(geneSet, width, height, minGenes, maxGenes,
                expectedNumberOfInstructions, maxMutationRounds,
                FnCreateField, expectedNumberOfInstructions);
        }

        [TestMethod]
        public void MowTurnJumpValidatingTest()
        {
            var width = 8;
            var height = 8;
            var geneSet = new FnCreateGeneDelegate[]
            {
                () => new Mow(),
                () => new Turn(),
                () => new Jump(Rand.Random.Next(0, Math.Min(width, height)),
                    Rand.Random.Next(0, Math.Min(width, height)))
            };
            var minGenes = width * height;
            var maxGenes = 3 * minGenes / 2;
            var maxMutationRounds = 3;
            var expectedNumberOfInstructions = 79;

            Field FnCreateField() =>
                new ValidatingField(width, height, FieldContents.Grass);

            RunWith(geneSet, width, height, minGenes, maxGenes,
                expectedNumberOfInstructions, maxMutationRounds,
                FnCreateField, expectedNumberOfInstructions);
        }

        [TestMethod]
        public void MowTurnRepeatTest()
        {
            var width = 8;
            var height = 8;
            var geneSet = new FnCreateGeneDelegate[]
            {
                () => new Mow(),
                () => new Turn(),
                () => new Repeat(Rand.Random.Next(0, Math.Min(width, height)),
                    Rand.Random.Next(0, Math.Min(width, height)))
            };
            var minGenes = 3;
            var maxGenes = 20;
            var maxMutationRounds = 3;
            var expectedNumberOfInstructions = 9;
            var expectedNumberOfSteps = 88;

            Field FnCreateField() =>
                new ToroidField(width, height, FieldContents.Grass);

            RunWith(geneSet, width, height, minGenes, maxGenes,
                expectedNumberOfInstructions, maxMutationRounds,
                FnCreateField, expectedNumberOfSteps);
        }

        [TestMethod]
        public void MowTurnJumpFuncTest()
        {
            var width = 8;
            var height = 8;
            var geneSet = new FnCreateGeneDelegate[]
            {
                () => new Mow(),
                () => new Turn(),
                () => new Jump(Rand.Random.Next(0, Math.Min(width, height)),
                    Rand.Random.Next(0, Math.Min(width, height))),
                () => new Func()
            };
            var minGenes = 3;
            var maxGenes = 20;
            var maxMutationRounds = 3;
            var expectedNumberOfInstructions = 18;
            var expectedNumberOfSteps = 65;

            Field FnCreateField() =>
                new ToroidField(width, height, FieldContents.Grass);

            RunWith(geneSet, width, height, minGenes, maxGenes,
                expectedNumberOfInstructions, maxMutationRounds,
                FnCreateField, expectedNumberOfSteps);
        }

        [TestMethod]
        public void MowTurnJumpCallTest()
        {
            var width = 8;
            var height = 8;
            var geneSet = new FnCreateGeneDelegate[]
            {
                () => new Mow(),
                () => new Turn(),
                () => new Jump(Rand.Random.Next(0, Math.Min(width, height)),
                    Rand.Random.Next(0, Math.Min(width, height))),
                () => new Func(true),
                () => new Call(Rand.Random.Next(0, 6)),
            };
            var minGenes = 3;
            var maxGenes = 20;
            var maxMutationRounds = 3;
            var expectedNumberOfInstructions = 18;
            var expectedNumberOfSteps = 65;

            Field FnCreateField() =>
                new ToroidField(width, height, FieldContents.Grass);

            RunWith(geneSet, width, height, minGenes, maxGenes,
                expectedNumberOfInstructions, maxMutationRounds,
                FnCreateField, expectedNumberOfSteps);
        }

        private static void RunWith(FnCreateGeneDelegate[] geneSet, int width, int height, int minGenes, int maxGenes,
            int expectedNumberOfInstructions, int maxMutationRounds, FnCreateFieldDelegate fnCreateField,
            int expectedNumberOfSteps)
        {
            var mowerStartLocation = new Location(width / 2, height / 2);
            var mowerStartDirection = Directions.South;

            List<INode> FnCreate() =>
                Create(geneSet, 1, height);

            Tuple<Field, Mower, Program> FnEvaluate(IReadOnlyList<INode> instructions)
            {
                var program = new Program(instructions);
                var mower = new Mower(mowerStartLocation, mowerStartDirection);
                var field = fnCreateField();

                try
                {
                    program.Evaluate(mower, field, 0);
                }
                catch (Exception)
                {
                    // pass
                }
                return new Tuple<Field, Mower, Program>(field, mower, program);
            }

            Fitness FnGetFitness(IReadOnlyList<INode> genes) =>
                GetFitness(genes, FnEvaluate);

            var watch = Stopwatch.StartNew();

            void FnDisplay(Chromosome<INode, Fitness> candidate) =>
                Display(candidate, watch, FnEvaluate);

            void FnMutate(List<INode> child) =>
                Mutate(child, geneSet, minGenes, maxGenes, FnGetFitness, maxMutationRounds);

            var optimalFitness = new Fitness(width * height, expectedNumberOfInstructions, expectedNumberOfSteps);

            var best = Genetic<INode, Fitness>.GetBest(FnGetFitness, 0, optimalFitness, null, FnDisplay, FnMutate,
                FnCreate, null, 10, Crossover);

            Assert.IsTrue(optimalFitness.CompareTo(best.Fitness) <= 0);
        }
    }
}