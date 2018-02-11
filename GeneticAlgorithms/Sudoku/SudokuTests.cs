/* File: SudokuTests.cs
 *     from chapter 11 of _Genetic Algorithms with Python_
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

using GeneticAlgorithms.MagicSquare;
using GeneticAlgorithms.Utilities;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace GeneticAlgorithms.Sudoku
{
    [TestClass]
    public class SudokuTests
    {
        private static int GetFitness(IReadOnlyList<int> genes, IEnumerable<Rule> validationRules)
        {
            foreach (var firstFailingRule in validationRules.Where(rule => genes[rule.Index] == genes[rule.OtherIndex]))
            {
                var fitness = 10 * (IndexRow(firstFailingRule.OtherIndex) + 1) +
                              IndexColumn(firstFailingRule.OtherIndex) + 1;
                return fitness;
            }

            return 100;
        }

        [TestMethod]
        public void GetFitnessTest()
        {
            var genes = new List<int> {0, 1, 0};
            var rules = new[]
            {
                new Rule(0, 1),
                new Rule(0, 2), // This rule fails
                new Rule(1, 2),
            };

            var fitness = GetFitness(genes, rules);
            Assert.AreEqual(13, fitness);
        }

        private static void Display(Chromosome<int, int> candidate, Stopwatch watch)
        {
            for (var row = 0; row < 9; row++)
            {
                var row9 = row * 9;
                var line = string.Join(" | ",
                    new[] {0, 3, 6}.Select(i => string.Join(" ", candidate.Genes.Skip(row9 + i).Take(3))));
                Console.WriteLine(" {0}", line);
                if (row < 8 && row % 3 == 2)
                    Console.WriteLine(" ----- + ----- + -----");
            }

            Console.WriteLine(" - = -   - = -   - = - {0}\t{1} ms\n", candidate.Fitness, watch.ElapsedMilliseconds);
            Console.WriteLine();
        }

        [TestMethod]
        public void DisplayTest()
        {
            var genes = new List<int>(81);
            for (var i = 0; i < 9; i++)
            for (var j = 0; j < 9; j++)
                genes.Add(j);

            var candidate = new Chromosome<int, int>(genes, 0);
            var watch = Stopwatch.StartNew();
            Display(candidate, watch);
        }

        private static void Mutate(List<int> genes, IEnumerable<Rule> validationRules)
        {
            using (var enumerator = validationRules.Where(rule => genes[rule.Index] == genes[rule.OtherIndex])
                .AsEnumerable().GetEnumerator())
            {
                if (!enumerator.MoveNext())
                    return;
                var selectedRule = enumerator.Current;
                if (selectedRule == null)
                    return;

                if (IndexRow(selectedRule.OtherIndex) % 3 == 2 && Rand.PercentChance(10))
                {
                    var sectionStart = SectionStart(selectedRule.Index);
                    var current = selectedRule.OtherIndex;
                    while (selectedRule.OtherIndex == current)
                    {
                        ShuffleInPlace(genes, sectionStart, 80);
                        if (!enumerator.MoveNext())
                            return;
                        selectedRule = enumerator.Current;
                        if (selectedRule == null)
                            return;
                    }

                    return;
                }

                var row = IndexRow(selectedRule.OtherIndex);
                var start = row * 9;
                var indexA = selectedRule.OtherIndex;
                var indexB = Rand.Random.Next(start, genes.Count);
                var temp = genes[indexA];
                genes[indexA] = genes[indexB];
                genes[indexB] = temp;
            }
        }

        [TestMethod]
        public void MutateTest()
        {
            var watch = Stopwatch.StartNew();
            var geneSet = Enumerable.Range(1, 9).ToArray();
            var genes = Rand.RandomSampleList(geneSet, 81);
            var validationRules = BuildValidationRules();

            var fitness1 = GetFitness(genes, validationRules);
            Display(new Chromosome<int, int>(genes, fitness1), watch);

            Mutate(genes, validationRules);
            var fitness2 = GetFitness(genes, validationRules);
            Display(new Chromosome<int, int>(genes, fitness2), watch);
        }

        private static void ShuffleInPlace(List<int> genes, int first, int last)
        {
            while (first < last)
            {
                var index = Rand.Random.Next(first, last + 1);
                var temp = genes[first];
                genes[first] = genes[index];
                genes[index] = temp;
                first++;
            }
        }

        [TestMethod]
        public void ShuffleInPlaceTest()
        {
            var data = new List<int> {0, 1, 2, 3, 4};
            ShuffleInPlace(data, 1, 3);
            Console.WriteLine(string.Join(", ", data));
            Assert.AreEqual(0, data[0]);
            Assert.AreEqual(4, data[4]);
        }

        [TestMethod]
        public void SudokuTest()
        {
            var geneSet = Enumerable.Range(1, 9).ToArray();
            var watch = Stopwatch.StartNew();
            var optimalValue = 100;

            void FnDisplay(Chromosome<int, int> candidate) =>
                Display(candidate, watch);

            var validationRules = BuildValidationRules();

            int FnGetFitness(List<int> genes) =>
                GetFitness(genes, validationRules);

            List<int> FnCreate() =>
                Rand.RandomSampleList(geneSet, 81);

            void FnMutate(List<int> genes) =>
                Mutate(genes, validationRules);

            var best = Genetic<int, int>.GetBest(FnGetFitness, 0, optimalValue, null, FnDisplay, FnMutate, FnCreate,
                50);

            Assert.AreEqual(optimalValue, best.Fitness);
        }

        [TestMethod]
        public void BenchmarkTest()
        {
            Benchmark.Run(SudokuTest);
        }

        private static Rule[] BuildValidationRules()
        {
            var rules = new List<Rule>();
            for (var index = 0; index < 80; index++)
            {
                var itsRow = IndexRow(index);
                var itsColumn = IndexColumn(index);
                var itsSection = RowColumnSection(itsRow, itsColumn);

                for (var index2 = index + 1; index2 < 81; index2++)
                {
                    var otherRow = IndexRow(index2);
                    var otherColumn = IndexColumn(index2);
                    var otherSection = RowColumnSection(otherRow, otherColumn);
                    if (itsRow == otherRow ||
                        itsColumn == otherColumn ||
                        itsSection == otherSection)
                        rules.Add(new Rule(index, index2));
                }
            }

            return rules.OrderBy(r => r.OtherIndex * 100 + r.Index).ToArray();
        }

        [TestMethod]
        public void ValidationRuleTest()
        {
            var rules = BuildValidationRules();
            Assert.AreEqual(810, rules.Length);
        }

        private static int IndexRow(int index) => index / 9;

        private static int IndexColumn(int index) => index % 9;

        private static int RowColumnSection(int row, int column) => (row / 3) * 3 + (column / 3);

        // private static int IndexSelection(int index) => RowColumnSection(IndexRow(index), IndexColumn(index));

        private static int SectionStart(int index) => ((IndexRow(index) % 9) / 3) * 27 + (IndexColumn(index) / 3) * 3;
    }
}