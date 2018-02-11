/* File: GuessPassordTests.cs
 *     from chapter 1 of _Genetic Algorithms with Python_
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
using GeneticAlgorithms.Utilities;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace GeneticAlgorithms.Password
{
    [TestClass]
    public class GuessPassordTests
    {
        private static readonly char[] GeneSet = " abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ!.,".ToCharArray();

        private static int GetFitness(string guess, string target)
        {
            var sum = target.Zip(guess, (expected, actual) => expected == actual).Count(c => c);
            return sum;
        }

        private static void Display(Chromosome candidate, Stopwatch stopwatch)
        {
            Console.WriteLine("{0}\t{1}\t{2} ms", candidate.Genes, candidate.Fitness, stopwatch.ElapsedMilliseconds);
        }

        private static void GuessPassword(string target)
        {
            var watch = Stopwatch.StartNew();
            int FnGetFitness(string guess) => GetFitness(target, guess);
            void FnDisplay(Chromosome candidate) => Display(candidate, watch);

            var optimalFitness = target.Length;
            var best = Genetic.GetBest(FnGetFitness, target.Length, optimalFitness, GeneSet, FnDisplay);
            Assert.AreEqual(target.Length, best.Fitness);
            Assert.AreEqual(target, best.Genes);
        }

        [TestMethod]
        public void HelloWorldTest()
        {
            const string target = "Hello World!";
            GuessPassword(target);
        }

        [TestMethod]
        public void LongPasswordTest()
        {
            const string target = "For I am fearfully and wonderfully made.";
            GuessPassword(target);
        }

        [TestMethod]
        public void RandomPasswordTest()
        {
            const int length = 150;
            var target = string.Join("",
                Enumerable.Range(0, length).Select(x => Rand.SelectItem(GeneSet)));
            GuessPassword(target);
        }

        [TestMethod]
        public void BenchmarkTest()
        {
            Benchmark.Run(RandomPasswordTest);
        }
    }
}
