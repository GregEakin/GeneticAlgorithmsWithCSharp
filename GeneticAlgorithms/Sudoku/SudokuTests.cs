using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace GeneticAlgorithms.Sudoku
{
    [TestClass]
    public class SudokuTests
    {
        private static readonly Random Random = new Random();

        public static int Fitness(int[] genes, Rule[] validationRules)
        {
//            firstFailingRule = next(rule for rule in validationRules 
//              if genes[rule.Index] == genes[rule.OtherIndex])
                    return 100;
        }

        public static void Display(Chromosome<int, int> candidate, Stopwatch watch)
        {
            for (var row = 0; row < 9; row++)
            {
                var line = string.Join(" | ",
                    new[] {0, 3, 6}.Select(i => string.Join(" ", candidate.Genes.Skip(row * 9 + i).Take(3))));
                Console.WriteLine(" {0}", line);
                if (row < 8 && row % 3 == 2)
                    Console.WriteLine(" ----- + ----- + -----");
            }

            Console.WriteLine(" - = -   - = -   - = - \t{0}\t{1} ms", candidate.Fitness, watch.ElapsedMilliseconds);
            Console.WriteLine();
        }

        public static int[] Mutate(int[] parent, Rule[] validationRules)
        {
            var genes = parent.ToArray();
            foreach (var selectedRule in validationRules)
            {
                if (selectedRule.Index == selectedRule.OtherIndex)
                    continue;

                var row = IndexRow(selectedRule.OtherIndex);
                var start = row * 9;
                var indexA = selectedRule.OtherIndex;
                var indexB = Random.Next(start, genes.Length);

                Console.WriteLine("Swap {0} - {1}", indexA, indexB);
                if (genes[indexA] == genes[indexB])
                    Console.WriteLine("Same items!");

                var temp = genes[indexA];
                genes[indexA] = genes[indexB];
                genes[indexB] = temp;
            }

            return genes;
        }

        public static void ShuffleInPlace(int[] genes, int first, int last)
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

            var candidate = new Chromosome<int, int>(genes.ToArray(), 0);
            var watch = Stopwatch.StartNew();
            Display(candidate, watch);
        }

        [TestMethod]
        public void MutateTest()
        {
            var watch = Stopwatch.StartNew();
            var geneSet = Enumerable.Range(1, 9).ToArray();
            var genes = RandomSample(geneSet, 81);
            var validationRules = BuildValidationRules();
            var fitness = Fitness(genes, validationRules);

            Display(new Chromosome<int, int>(genes, fitness), watch);
            var child = Mutate(genes, validationRules);

            var fitness2 = Fitness(child, validationRules);
            Display(new Chromosome<int, int>(child, fitness2), watch);
            CollectionAssert.AreNotEqual(genes, child);
        }

        [TestMethod]
        public void SudokuTest()
        {
            var generic = new Genetic<int, int>();
            var geneSet = Enumerable.Range(1, 9).ToArray();
            var watch = Stopwatch.StartNew();
            var optimalValue = 100;
            var validationRules = BuildValidationRules();

            void FnDispaly(Chromosome<int, int> candidate) => Display(candidate, watch);
            int FnFitness(int[] genes) => Fitness(genes, validationRules);
            int[] FnCreate() => RandomSample(geneSet, 81);
            int[] FnMutate(int[] genes) => Mutate(genes, validationRules);

            var best = generic.BestFitness(FnFitness, 0, optimalValue, null, FnDispaly, FnMutate, FnCreate, 50);
            Assert.AreEqual(optimalValue, best.Fitness);
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

        public static int[] RandomSample(int[] geneSet, int length)
        {
            var genes = new List<int>(length);
            do
            {
                var sampleSize = Math.Min(geneSet.Length, length - genes.Count);
                var array = geneSet.OrderBy(x => Random.Next()).Take(sampleSize);
                genes.AddRange(array);
            } while (genes.Count < length);

            return genes.ToArray();
        }
    }
}