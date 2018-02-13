/* File: GraphColoringTests.cs
 *     from chapter 5 of _Genetic Algorithms with Python_
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

using GeneticAlgorithms.SortedNumbers;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace GeneticAlgorithms.MapColors
{
    [TestClass]
    public class GraphColoringTests
    {
        /// <summary>
        /// expects: T D1 [D2 ... DN]
        /// where T is the record type
        /// and D1..DN are record-type appropriate data elements
        /// </summary>
        private static Tuple<ISet<Rule>, ISet<string>> LoadData(string localFileName)
        {
            var rules = new HashSet<Rule>();
            var nodes = new HashSet<string>();
            using (var inFile = File.OpenText(localFileName))
                while (!inFile.EndOfStream)
                {
                    var row = inFile.ReadLine();
                    if (string.IsNullOrWhiteSpace(row))
                        continue;

                    if (row[0] == 'e') // e aa bb, aa and bb are node ids
                    {
                        var nodeIds = row.Split(' ');
                        rules.Add(new Rule(nodeIds[1], nodeIds[2]));
                        nodes.Add(nodeIds[1]);
                        nodes.Add(nodeIds[2]);
                    }
                    else if (row[0] == 'n') // n aa ww, aa is a node id, ww is a weight
                    {
                        var nodeIds = row.Split(' ');
                        nodes.Add(nodeIds[1]);
                    }
                }

            return new Tuple<ISet<Rule>, ISet<string>>(rules, nodes);
        }

        [TestMethod]
        public void LoadDataAdjacentStatesTest()
        {
            var data = LoadData(@"Data\adjacent_states.col");
            var rules = data.Item1;
            Assert.AreEqual(107, rules.Count);
            var states = data.Item2;
            Assert.AreEqual(51, states.Count);
        }

        [TestMethod]
        public void LoadDataR100_1gbTest()
        {
            var data = LoadData(@"Data\R100_1gb.col");
            var rules = data.Item1;
            Assert.AreEqual(509, rules.Count);
            var states = data.Item2;
            Assert.AreEqual(100, states.Count);
        }

        private static Rule[] BuildRules(Tuple<string, string[]>[] items)
        {
            var rulesAdded = new Dictionary<Rule, int>();
            foreach (var item in items)
            {
                var state = item.Item1;
                var adjacent = item.Item2;
                foreach (var adjacentState in adjacent)
                {
                    if (string.IsNullOrWhiteSpace(adjacentState))
                        continue;
                    var rule = new Rule(state, adjacentState);
                    if (rulesAdded.ContainsKey(rule))
                        rulesAdded[rule]++;
                    else
                        rulesAdded[rule] = 1;
                }
            }

            foreach (var rule in rulesAdded.Where(r => r.Value != 2))
                Console.WriteLine("Rule {0} is not bidirectional", rule.Key);

            return rulesAdded.Keys.ToArray();
        }

        [TestMethod]
        public void BuildRulesTest()
        {
            var items = new[]
            {
                new Tuple<string, string[]>("AK", new string[0]),
                new Tuple<string, string[]>("CA", new[] {"TX", "FL"}),
                new Tuple<string, string[]>("TX", new[] {"CA", "FL"}),
                new Tuple<string, string[]>("FL", new[] {"CA", "TX"}),
            };
            var rules = BuildRules(items);
            Assert.AreEqual(3, rules.Length);
        }

        private static int GetFitness(IReadOnlyList<char> genes, ISet<Rule> rules, Dictionary<string, int> stateIndexLookup)
        {
            var rulesThatPass = rules.Count(r => r.Valid(genes, stateIndexLookup));
            return rulesThatPass;
        }

        private static void Display(Chromosome<char, int> candidate, Stopwatch watch)
        {
            Console.WriteLine("{0}\t{1}\t{2} ms",
                string.Join("", candidate.Genes),
                candidate.Fitness, watch.ElapsedMilliseconds);
        }

        [TestMethod]
        public void StateTests()
        {
            Color(@"Data\adjacent_states.col", new[] {"Orange", "Yellow", "Green", "Blue"});
        }

        [TestMethod]
        public void R100_1gbTest()
        {
            Color(@"Data\R100_1gb.col", new[] {"Red", "Orange", "Yellow", "Green", "Blue", "Indigo"});
        }

        [TestMethod]
        public void BenchmarkTest()
        {
            Benchmark.Run(R100_1gbTest);
        }

        private static void Color(string file, string[] colors)
        {
            var data = LoadData(file);
            var rules = data.Item1;
            var nodes = data.Item2;
            var optimalValue = rules.Count;
            var colorLookup = colors.ToDictionary(k => k[0], v => v);
            var geneSet = colorLookup.Keys.ToArray();
            var watch = Stopwatch.StartNew();
            var count = 0;
            var nodeIndexLookup = nodes.OrderBy(s => s).ToDictionary(k => k, v => count++);

            void FnDisplay(Chromosome<char, int> candidate) => Display(candidate, watch);

            int FnGetFitness(IReadOnlyList<char> genes) => GetFitness(genes, rules, nodeIndexLookup);

            var best = Genetic<char, int>.GetBest(FnGetFitness, nodes.Count, optimalValue, geneSet, FnDisplay);
            Assert.IsTrue(optimalValue.CompareTo(best.Fitness) <= 0);

            var keys = nodes.OrderBy(c => c).ToArray();
            for (var index = 0; index < nodes.Count; index++)
                Console.WriteLine("{0} is {1}", keys[index], colorLookup[best.Genes[index]]);
        }
    }
}