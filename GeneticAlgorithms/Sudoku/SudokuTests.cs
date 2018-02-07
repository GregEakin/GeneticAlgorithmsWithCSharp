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
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using GeneticAlgorithms.Utilities;

namespace GeneticAlgorithms.Sudoku
{
    [TestClass]
    public class SudokuTests
    {
        private static readonly Random Random = new Random();

        public static int GetFitness(List<int> genes, Rule[] validationRules)
        {
//            firstFailingRule = next(rule for rule in validationRules 
//              if genes[rule.Index] == genes[rule.OtherIndex])
                    return 100;
        }

        public static void Display(Chromosome<int, int> candidate, Stopwatch watch)
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

        public static void Mutate(List<int> genes, Rule[] validationRules)
        {
            foreach (var selectedRule in validationRules)
            {
                if (selectedRule.Index == selectedRule.OtherIndex)
                    continue;

                var row = IndexRow(selectedRule.OtherIndex);
                var start = row * 9;
                var indexA = selectedRule.OtherIndex;
                var indexB = Random.Next(start, genes.Count);

                Console.WriteLine("Swap {0} - {1}", indexA, indexB);
                if (genes[indexA] == genes[indexB])
                    Console.WriteLine("Same items!");

                var temp = genes[indexA];
                genes[indexA] = genes[indexB];
                genes[indexB] = temp;
            }
        }

        public static void ShuffleInPlace(List<int> genes, int first, int last)
        {
            while (first < last)
            {
                var index = Random.Next(first, last);
                var temp = genes[index];
                genes[index] = genes[first];
                genes[first] = temp;
                first++;
            }
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

        [TestMethod]
        public void MutateTest()
        {
            var watch = Stopwatch.StartNew();
            var geneSet = Enumerable.Range(1, 9).ToArray();
            var genes = RandomFn.RandomSampleList(geneSet, 81);
            var validationRules = BuildValidationRules();
            var fitness = GetFitness(genes, validationRules);

            Display(new Chromosome<int, int>(genes, fitness), watch);
            Mutate(genes, validationRules);

            var fitness2 = GetFitness(genes, validationRules);
            Display(new Chromosome<int, int>(genes, fitness2), watch);
        }

        [TestMethod]
        public void SudokuTest()
        {
            var genetic = new Genetic<int, int>();
            var geneSet = Enumerable.Range(1, 9).ToArray();
            var watch = Stopwatch.StartNew();
            var optimalValue = 100;

            void FnDisplay(Chromosome<int, int> candidate) =>
                Display(candidate, watch);

            var validationRules = BuildValidationRules();

            int FnGetFitness(List<int> genes) => 
                GetFitness(genes, validationRules);

            List<int> FnCreate() => 
                RandomFn.RandomSampleList(geneSet, 81);

            void FnMutate(List<int> genes) => 
                Mutate(genes, validationRules);

            var best = genetic.GetBest(FnGetFitness, 0, optimalValue, null, FnDisplay, FnMutate, FnCreate, 50);

            Assert.AreEqual(optimalValue, best.Fitness);
        }

        [TestMethod]
        public void BenchmarkTest()
        {
            Benchmark.Run(SudokuTest);
        }

        public static Rule[] BuildValidationRules()
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

                    if (itsRow == otherRow || itsColumn == otherColumn || itsSection == otherSection)
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

        public static int IndexRow(int index) => index / 9;

        public static int IndexColumn(int index) => index % 9;

        public static int RowColumnSection(int row, int column) => (row / 3) * 3 + (column / 3);

        public static int IndexSelection(int index) => RowColumnSection(IndexRow(index), IndexColumn(index));

        public static int SectionStart(int index) => ((IndexRow(index) % 9) / 3) * 27 + (IndexColumn(index) / 3) * 3;
    }
}