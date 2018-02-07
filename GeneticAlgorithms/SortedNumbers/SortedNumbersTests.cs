/* File: Chromosome.cs
 *     from chapter 3 of _Genetic Algorithms with Python_
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

using System;
using System.Diagnostics;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace GeneticAlgorithms.SortedNumbers
{
    [TestClass]
    public class SortedNumbersTests
    {
        private static Fitness GetFitness(int[] genes)
        {
            var count = 1;
            var gap = 0;
            for (var i = 1; i < genes.Length; i++)
            {
                if (genes[i - 1] < genes[i])
                    count++;
                else
                    gap += genes[i - 1] - genes[i];
            }

            return new Fitness(count, gap);
        }

        private static void Display(Chromosome<int, Fitness> candidate, Stopwatch watch)
        {
            Console.WriteLine("{0}\t=> {1}\t{2}",
                string.Join(", ", candidate.Genes),
                candidate.Fitness, watch.ElapsedMilliseconds);
        }

        [TestMethod]
        public void SortTenNumbersTest()
        {
            SortNumbers(10);
        }

        private void SortNumbers(int totalNumbers)
        {
            var geneSet = Enumerable.Range(0, 100).ToArray();
            var watch = Stopwatch.StartNew();

            void FnDisplay(Chromosome<int, Fitness> candidate) => Display(candidate, watch);
            Fitness FnGetFitness(int[] genes) => GetFitness(genes);

            var optimalFitness = new Fitness(totalNumbers, 0);
            var best = Genetic<int, Fitness>.GetBest(FnGetFitness, totalNumbers, optimalFitness, geneSet, FnDisplay);
            Assert.IsTrue(optimalFitness.CompareTo(best.Fitness) <= 0);
        }

        [TestMethod]
        public void BenchmarkTest()
        {
            Benchmark.Run(() => SortNumbers(40));
        }
    }
}