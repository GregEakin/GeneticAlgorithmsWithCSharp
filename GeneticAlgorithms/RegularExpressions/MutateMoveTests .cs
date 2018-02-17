/* File: RegexTests.cs
 *     from chapter 17 of _Genetic Algorithms with Python_
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
using System.Collections.Concurrent;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace GeneticAlgorithms.RegularExpressions
{
    [TestClass]
    public class MutateMoveTests
    {
        [TestMethod]
        public void DiversityTest()
        {
            // line numbers are from: ch17/regexTests.py

            var initialInput = "abcdefghij".ToCharArray().ToList();
            for (var i = 3; i <= initialInput.Count - 1; i++)
            {
                Console.WriteLine("with " + i + " genes");
                var eqResults = new ConcurrentDictionary<string, int>();
                var geResults = new ConcurrentDictionary<string, int>();

                // 168: stop = start + random.randint(1, 2)
                var length = 1;

                // 167: start = random.choice(range(len(genes)))
                for (var start = 0; start < i - length + 1; start++)
                {
                    // 169: toMove = genes[start:stop]
                    var toMove = initialInput.Skip(start).Take(length).ToList();

                    // 170: genes[start:stop] = []
                    var genes = initialInput.Take(start)
                        .Concat(initialInput.Skip(start + length).Take(i - start - length)).ToList();

                    for (var k = 0; k < genes.Count; k++)
                    {
                        {
                            // 171: index = random.choice(range(len(genes)))
                            var index = k;
                            // 172: if index == start:
                            if (index == start)
                                // 173: index += 1
                                index += 1;

                            // Console.WriteLine("eq {0}", index);

                            // 174: genes[index:index] = toMove
                            var genesCopy = genes.ToList();
                            genesCopy.InsertRange(index, toMove);

                            var result = string.Join("", genesCopy);
                            eqResults.AddOrUpdate(result, 1, (id, count) => count + 1);
                            // Console.WriteLine(string.Join("", genes) + "->" + result + " eq");
                        }

                        {
                            // 171: index = random.choice(range(len(genes)))
                            var index = k;
                            // 172: if index >= start:
                            if (index >= start)
                                // 173: index += 1
                                index += 1;

                            // Console.WriteLine("ge {0}", index);

                            // 174: genes[index:index] = toMove
                            var genesCopy = genes.ToList();
                            genesCopy.InsertRange(index, toMove);

                            var result = string.Join("", genesCopy);
                            geResults.AddOrUpdate(result, 1, (id, count) => count + 1);
                            // Console.WriteLine(string.Join("", genes) + "->" + result + " ge");
                        }
                    }
                }

                Console.WriteLine("{0} eq variations: {1}", eqResults.Count,
                    string.Join(", ",
                        eqResults.OrderBy(x => x.Key).Select(x => string.Format("{0}({1})", x.Key, x.Value))));
                Console.WriteLine("{0} ge variations: {1}", geResults.Count,
                    string.Join(", ",
                        geResults.OrderBy(x => x.Key).Select(x => string.Format("{0}({1})", x.Key, x.Value))));
            }
        }
    }
}