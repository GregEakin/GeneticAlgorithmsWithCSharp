/* File: Chromosome.cs
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

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace GeneticAlgorithms.MapColors
{
    [TestClass]
    public class MapColorTests
    {


        [TestMethod]
        public void ParseRules()
        {
            var rules = new List<Tuple<string, string>>();
            using (var reader = File.OpenText(@"mapColors\adjacent_states.csv"))
            {
                string line;
                while ((line = reader.ReadLine()) != null)
                {
                    var rule = line.Split(',');
                    var stateName = rule[0];
                    var neighbors = rule[1].Split(';');

                    rules.AddRange(neighbors.Select(neighbor => new Tuple<string, string>(stateName, neighbor)));
                }
            }

            var count1 = rules.Select(r => r.Item1).Distinct().Count();
            var count2 = rules.Count(r => !string.IsNullOrEmpty(r.Item2));
            Console.WriteLine("p edge {0} {1}", count1, count2);
            foreach (var rule in rules.Where(r => !string.IsNullOrEmpty(r.Item2)))
                Console.WriteLine("e {0} {1}", rule.Item1, rule.Item2);
            foreach (var rule in rules.Where(r => string.IsNullOrEmpty(r.Item2)))
                Console.WriteLine("n {0} 0", rule.Item1);
        }

        [TestMethod]
        public void FourColorTest()
        {
            //var states = new SortedDictionary<string, State>();
            //var rules = new SortedSet<Rule>();
            //Prototype.SetupRules(states, rules);

            //var genetic = new Genetic<char, int>();
            //var geneSet = "OYGB".ToCharArray();
            //var watch = Stopwatch.StartNew();

            //int FitnessFun(char[] genes) => Fitness(genes, rules);
            //void DisplayFun(Chromosome<char, int> candidate) => Display(candidate, watch, states);

            //var best = genetic.GetBest(FitnessFun, states.Count, rules.Count, geneSet, DisplayFun);
            //Assert.IsTrue(best.Fitness >= 214);
        }

        [TestMethod]
        public void SixColorTest()
        {
            //var states = new SortedDictionary<string, State>();
            //var rules = new SortedSet<Rule>();
            //Prototype.SetupRules(states, rules);

            //var genetic = new Genetic<char, int>();
            //var geneSet = "ROYGBI".ToCharArray();
            //var watch = Stopwatch.StartNew();

            //int FitnessFun(char[] genes) => Fitness(genes, rules);
            //void DisplayFun(Chromosome<char, int> candidate) => Display(candidate, watch, states);

            //var best = genetic.GetBest(FitnessFun, states.Count, rules.Count, geneSet, DisplayFun);
            //Assert.IsTrue(best.Fitness >= 214);
        }
    }
}