/* File: PiTest.cs
 *     from chapter 13 of _Genetic Algorithms with Python_
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
using System.IO;
using System.Linq;

namespace GeneticAlgorithms.ApproximatingPi
{
    [TestClass]
    public class PiTests
    {
        private static double GetFitness(IReadOnlyCollection<bool> genes, IReadOnlyCollection<int> bitValues)
        {
            var denominator = GetDenominator(genes, bitValues);
            if (denominator == 0)
                return 0.0;

            var numerator = GetNumerator(genes, bitValues);
            var ratio = (double) numerator / denominator;
            return Math.PI - Math.Abs(Math.PI - ratio);
        }

        [TestMethod]
        public void GetFitnessTest()
        {
            // 355 / 113
            var genes = new List<bool>
            {
                false, true, false, true, true, false, false, false, true, false,
                false, false, false, true, true, true, false, false, false, true,
            };
            var bitValues = new List<int> {512, 256, 128, 64, 32, 16, 8, 4, 2, 1};
            var fitness = GetFitness(genes, bitValues);
            Assert.AreEqual(3.1415923868256, fitness, 0.00000001);
        }

        private static void Display(Chromosome<bool, double> candidate, Stopwatch watch, IReadOnlyCollection<int> bitValues)
        {
            var numerator = GetNumerator(candidate.Genes, bitValues);
            var denominator = GetDenominator(candidate.Genes, bitValues);
            Console.WriteLine("{0}/{1}\t{2}\t{3} ms", numerator, denominator, candidate.Fitness,
                watch.ElapsedMilliseconds);
        }

        [TestMethod]
        public void DisplayTest()
        {
            var genes = new List<bool>
            {
                false, true, false, true, true, false, false, false, true, false,
                false, false, false, true, true, true, false, false, false, true,
            };
            var bitValues = new List<int> {512, 256, 128, 64, 32, 16, 8, 4, 2, 1};
            var candidate =
                new Chromosome<bool, double>(genes, 3.1415923868256, Strategies.Create);
            var watch = Stopwatch.StartNew();
            Display(candidate, watch, bitValues);
        }

        private static int BitsToInt(IEnumerable<bool> bits, IEnumerable<int> bitValues)
        {
            return bits.Zip(bitValues, (b, v) => new {Bit = b, Value = v}).Where(bv => bv.Bit).Sum(bv => bv.Value);
        }

        [TestMethod]
        public void BitsToIntTest()
        {
            var bits = new[]
            {
                true, true, false, false
            };
            var bitValues = new[] {8, 4, 2, 1};
            var value = BitsToInt(bits, bitValues);
            Assert.AreEqual(0x0C, value);
        }

        private static int GetNumerator(IEnumerable<bool> genes, IReadOnlyCollection<int> bitValues) =>
            1 + BitsToInt(genes.Take(bitValues.Count), bitValues);

        [TestMethod]
        public void GetNumeratorTest()
        {
            var bits = new List<bool>
            {
                true, true, false, false,
                false, false, false, false
            };
            var bitValues = new List<int> {8, 4, 2, 1};
            var value = GetNumerator(bits, bitValues);
            Assert.AreEqual(0x0D, value);
        }

        private static int GetDenominator(IEnumerable<bool> genes, IReadOnlyCollection<int> bitValues) =>
            BitsToInt(genes.Skip(bitValues.Count), bitValues);

        [TestMethod]
        public void GetDenominatorTest()
        {
            var bits = new List<bool>
            {
                false, false, false, false,
                true, true, false, false
            };
            var bitValues = new List<int> {8, 4, 2, 1};
            var value = GetDenominator(bits, bitValues);
            Assert.AreEqual(0x0C, value);
        }

        private static void Mutate(IList<bool> genes, int numBits)
        {
            var index = Rand.Random.Next(genes.Count);
            genes[index] = !genes[index];
        }

        [TestMethod]
        public void MutateTest()
        {
            var bits = new List<bool>
            {
                false, false, false, false,
                true, true, false, false
            };
            var save = bits.ToArray();
            Mutate(bits, bits.Count / 2);
            CollectionAssert.AreNotEqual(save, bits);
        }

        private static bool ApproximatePi(IReadOnlyCollection<int> bitValues, int? maxSeconds = null)
        {
            var geneSet = new[] {false, true};
            var watch = Stopwatch.StartNew();

            void FnDispaly(Chromosome<bool, double> candidate) =>
                Display(candidate, watch, bitValues);

            double FnGetFitness(IReadOnlyList<bool> genes) =>
                GetFitness(genes, bitValues);

            var optimalFitness = Math.Round(355.0 / 113.0, 5); // = 3.14159;

            void FnMutate(List<bool> genes) =>
                Mutate(genes, bitValues.Count);

            var length = 2 * bitValues.Count;
            var best = Genetic<bool, double>.GetBest(FnGetFitness, length, optimalFitness, geneSet, FnDispaly, FnMutate,
                null, 250, 1, null, maxSeconds);

            return optimalFitness <= best.Fitness;
        }

        [TestMethod]
        public void OptimizeTest()
        {
            var geneSet = Enumerable.Range(1, 512).ToArray();
            var length = 10;
            var maxSeconds = 2;

            double FnGetFitness(IReadOnlyList<int> genes)
            {
                var watch = Stopwatch.StartNew();
                var count = 0.0;
                var stdout = Console.Out;
                Console.SetOut(TextWriter.Null);
                while (watch.ElapsedMilliseconds < maxSeconds * 1000)
                {
                    var found = ApproximatePi(genes, maxSeconds);
                    if (found) count += 1.0;
                }

                Console.SetOut(stdout);
                var distance = Math.Abs(genes.Sum() - 1023);
                var fraction = distance > 0 ? 1.0 / distance : distance;
                count += Math.Round(fraction, 4);
                return count;
            }

            void FnDisplay(Chromosome<int, double> chromosome) => Console.WriteLine("{0}\t{1}",
                string.Join(", ", chromosome.Genes), chromosome.Fitness);

            var initial = new List<int> {512, 256, 128, 64, 32, 16, 8, 4, 2, 1};
            Console.WriteLine("initial: {0} {1}", initial, FnGetFitness(initial));

            var optimalFitness = 10 * maxSeconds;
            var unused = Genetic<int, double>.GetBest(FnGetFitness, length, optimalFitness, geneSet, FnDisplay, null,
                null, null, 1, null, 600);
        }

        [TestMethod]
        public void BenchmarkTest()
        {
            Benchmark.Run(() => ApproximatePi(new List<int>
            {
                98, 334, 38, 339, 117, 39, 145, 123, 40, 129
            }));
        }

        [TestMethod]
        public void TestFindTop10Approximations()
        {
            // 355 / 113	3.1415923868256
            var watch = Stopwatch.StartNew();

            var best = new Dictionary<double, Tuple<int, int>>();
            for (var numerator = 1; numerator < 1 << 10; numerator++)
            for (var denominator = 1; denominator < 1 << 10; denominator++)
            {
                var ratio = (double) numerator / denominator;
                var piDist = Math.PI - Math.Abs(Math.PI - ratio);
                if (!best.ContainsKey(piDist) || best[piDist].Item1 > numerator)
                    best[piDist] = new Tuple<int, int>(numerator, denominator);
            }

            var bestApproximations = best.OrderByDescending(d => d.Key).Take(10);
            foreach (var approximation in bestApproximations)
            {
                Console.WriteLine("{0} / {1}\t{2}", approximation.Value.Item1, approximation.Value.Item2,
                    approximation.Key);
            }

            Console.WriteLine("Found best Pi {0}, in {1} ms.", Math.PI, watch.ElapsedMilliseconds);
        }

        [TestMethod]
        public void ApproximatePiTest()
        {
            var found = ApproximatePi(new List<int> {512, 256, 128, 64, 32, 16, 8, 4, 2, 1}, 10);
            Assert.IsTrue(found);
        }

        [TestMethod]
        public void FastPiSearch()
        {
            var found = ApproximatePi(new List<int> {211, 84, 134, 193, 142, 159, 274, 209, 161, 33}, 5);
            Assert.IsTrue(found);
        }
    }
}