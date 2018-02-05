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
    public class Prototype
    {
        public static void SetupRules(IDictionary<string, State> states, ISet<Rule> rules)
        {
            var lines = new List<Tuple<string, string>>();
            using (var reader = File.OpenText(@"mapColors\adjacent_states.csv"))
            {
                var index = 0;
                string line;
                while ((line = reader.ReadLine()) != null)
                {
                    var from = line.Split(',');
                    var stateName = from[0];
                    var neighbors = from[1].Split(';');
                    lines.AddRange(neighbors.Select(neighbor => new Tuple<string, string>(stateName, neighbor)));

                    if (states.ContainsKey(stateName))
                        continue;

                    var state = new State(stateName, index++);
                    states[stateName] = state;
                }
            }

            foreach (var rulePair in lines.Where(r => !string.IsNullOrEmpty(r.Item2)))
            {
                var from = states[rulePair.Item1];
                var to = states[rulePair.Item2];
                var rule = new Rule(from.Name, to.Name);
                rules.Add(rule);
            }
        }

        //[TestMethod]
        //public void SetupTest()
        //{
        //    IDictionary<string, State> states = new SortedDictionary<string, State>();
        //    ISet<Rule> rules = new SortedSet<Rule>();
        //    SetupRules(states, rules);
        //    Assert.AreEqual(51, states.Count);
        //    Assert.AreEqual(214, rules.Count);

        //    var genes1 = "OYGBOYGBOYGBOYGBOYGBOYGBOYGBOYGBOYGBOYGBOYGBOYGBOYG".ToCharArray();
        //    Assert.AreEqual(51, genes1.Length);
        //    var count1 = rules.Count(r => r.Valid(genes1));
        //    Assert.AreEqual(160, count1);

        //    var genes2 = "1234567890abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNO".ToCharArray();
        //    Assert.AreEqual(51, genes2.Length);
        //    var count2 = rules.Count(r => r.Valid(genes2));
        //    Assert.AreEqual(214, count2);
        //}
    }
}