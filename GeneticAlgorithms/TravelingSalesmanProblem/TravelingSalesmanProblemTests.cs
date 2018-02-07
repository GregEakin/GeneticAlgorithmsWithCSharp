/* File: TravelingSalesmanProblemTests.cs
 *     from chapter 12 of _Genetic Algorithms with Python_
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

namespace GeneticAlgorithms.TravelingSalesmanProblem
{
    [TestClass]
    public class TravelingSalesmanProblemTests
    {
        private static Fitness GetFitness(int[] genes, Dictionary<int, double[]> idToLocatonLookup)
        {
            var fitness = GetDistance(idToLocatonLookup[genes[0]], idToLocatonLookup[genes[genes.Length - 1]]);
            for (var i = 0; i < genes.Length - 1; i++)
            {
                var start = idToLocatonLookup[genes[i]];
                var end = idToLocatonLookup[genes[i + 1]];
                fitness += GetDistance(start, end);
            }

            return new Fitness(Math.Round(fitness, 2));
        }

        [TestMethod]
        public void GetFitnessTest()
        {
            var genes = new[] {0, 1, 2};
            var idToLocation = new Dictionary<int, double[]>
            {
                {0, new[] {0.0, 0.0}},
                {1, new[] {0.0, 4.0}},
                {2, new[] {3.0, 0.0}},
            };
            var fitness = GetFitness(genes, idToLocation);
            Assert.AreEqual(12.0, fitness.TotalDistance);
        }

        private static void Display(Chromosome<int, Fitness> candidate, Stopwatch watch)
        {
            Console.WriteLine("{0}\t{1}\t{2}\t{3} ms", string.Join(", ", candidate.Genes), candidate.Fitness,
                candidate.Strategy, watch.ElapsedMilliseconds);
        }

        [TestMethod]
        public void DisplayTest()
        {
            var genes = new[] {0, 1, 2};
            var idToLocation = new Dictionary<int, double[]>
            {
                {0, new[] {0.0, 0.0}},
                {1, new[] {0.0, 4.0}},
                {2, new[] {3.0, 0.0}},
            };
            var fitness = GetFitness(genes, idToLocation);
            var candidate = new Chromosome<int, Fitness>(genes, fitness, Chromosome<int, Fitness>.Strategies.Create);
            var watch = Stopwatch.StartNew();
            Display(candidate, watch);
        }

        private static double GetDistance(IReadOnlyList<double> locationA, IReadOnlyList<double> locationB)
        {
            var sideA = locationA[0] - locationB[0];
            var sideB = locationA[1] - locationB[1];
            var sideC = Math.Sqrt(sideA * sideA + sideB * sideB);
            return sideC;
        }

        [TestMethod]
        public void GetDistanceTest()
        {
            var locationA = new[] {1.0, 0.0};
            var locationB = new[] {0.0, 1.0};
            Assert.AreEqual(Math.Sqrt(2), GetDistance(locationA, locationB));
        }

        private void Mutate(int[] genes, Func<int[], Fitness> fnGetFitness)
        {
            var count = Rand.Random.Next(2, genes.Length);
            var initialFitness = fnGetFitness(genes);
            while (count-- > 0)
            {
                var sample = Rand.RandomSampleArray(Enumerable.Range(0, genes.Length).ToArray(), 2);
                var indexA = sample[0];
                var indexB = sample[1];
                var temp = genes[indexA];
                genes[indexA] = genes[indexB];
                genes[indexB] = temp;
                var fitness = fnGetFitness(genes);
                if (fitness.CompareTo(initialFitness) > 0)
                    return;
            }
        }

        [TestMethod]
        public void MutateTest()
        {
            var genes = new[] {0, 1, 2};
            var idToLocation = new Dictionary<int, double[]>
            {
                {0, new[] {0.0, 0.0}},
                {1, new[] {0.0, 4.0}},
                {2, new[] {3.0, 0.0}},
            };
            Fitness FnGetFitness(int[] genes1) => GetFitness(genes1, idToLocation);

            Mutate(genes, FnGetFitness);
            CollectionAssert.AreNotEqual(new[] {0, 1, 2}, genes);
        }

        private int[] Crossover(int[] parentGenes, int[] donorGenes,
            Func<int[], Fitness> fnGetFitness)
        {
            var pairs = new Dictionary<Pair, int> {{new Pair(donorGenes[0], donorGenes[donorGenes.Length - 1]), 0}};

            for (var i = 0; i < donorGenes.Length - 1; i++)
                pairs[new Pair(donorGenes[i], donorGenes[i + 1])] = 0;

            var tempGenes = parentGenes.ToList();
            if (pairs.ContainsKey(new Pair(parentGenes[0], parentGenes[parentGenes.Length - 1])))
            {
                // find a discontinuity
                var found = false;
                for (var i = 0; i < parentGenes.Length - 1; i++)
                {
                    if (pairs.ContainsKey(new Pair(parentGenes[i], parentGenes[i + 1])))
                        continue;
                    tempGenes = parentGenes.Skip(i + 1).Concat(parentGenes.Take(i + 1)).ToList();
                    found = true;
                    break;
                }

                if (!found)
                    return null;
            }

            var runs = new List<List<int>> {new List<int> {tempGenes[0]}};
            for (var i = 0; i < tempGenes.Count - 1; i++)
            {
                if (pairs.ContainsKey(new Pair(tempGenes[i], tempGenes[i + 1])))
                {
                    runs[runs.Count - 1].Add(tempGenes[i + 1]);
                    continue;
                }

                runs.Add(new List<int> {tempGenes[i + 1]});
            }

            var initialFitness = fnGetFitness(parentGenes);
            var count = Rand.Random.Next(2, 20);
            var runIndexes = Enumerable.Range(0, runs.Count).ToArray();
            while (count-- > 0)
            {
                foreach (var i in runIndexes)
                {
                    if (runs[i].Count == 1)
                        continue;
                    if (Rand.Random.Next(runs.Count) == 0)
                        runs[i].Reverse();
                }

                var randomSample = Rand.RandomSampleArray(runIndexes, 2);
                var indexA = randomSample[0];
                var indexB = randomSample[1];
                var temp = runs[indexA];
                runs[indexA] = runs[indexB];
                runs[indexB] = temp;
                var childGenes = runs.SelectMany(i => i).ToArray();
                if (fnGetFitness(childGenes).CompareTo(initialFitness) > 0)
                    return childGenes;
            }

            return runs.SelectMany(i => i).ToArray();
        }

        [TestMethod]
        public void CrossoverTest()
        {
            var father = new[] {0, 1, 2};
            var mother = new[] {1, 0, 2};
            var idToLocation = new Dictionary<int, double[]>
            {
                {0, new[] {0.0, 0.0}},
                {1, new[] {0.0, 4.0}},
                {2, new[] {3.0, 0.0}},
            };
            Fitness FnGetFitness(int[] genes1) => GetFitness(genes1, idToLocation);

            var child = Crossover(father, mother, FnGetFitness);
            CollectionAssert.AreNotEqual(father, child);
            CollectionAssert.AreNotEqual(mother, child);
        }

        [Ignore]
        [TestMethod]
        public void EightQueensTest()
        {
            var idToLocationLookup = new Dictionary<char, int[]>
            {
                {'A', new[] {4, 7}},
                {'B', new[] {2, 6}},
                {'C', new[] {0, 5}},
                {'D', new[] {1, 3}},
                {'E', new[] {3, 0}},
                {'F', new[] {5, 1}},
                {'G', new[] {7, 2}},
                {'H', new[] {6, 4}},
            };
            var optimalSequence = new[] {'A', 'B', 'C', 'D', 'E', 'F', 'G', 'H'};
            // Solve(idToLocationLookup, optimalSequence);
        }

        [TestMethod]
        public void EightQueensDoubleTest()
        {
            var idToLocationLookup = new Dictionary<int, double[]>
            {
                {1, new[] {4.0, 7.0}},
                {2, new[] {2.0, 6.0}},
                {3, new[] {0.0, 5.0}},
                {4, new[] {1.0, 3.0}},
                {5, new[] {3.0, 0.0}},
                {6, new[] {5.0, 1.0}},
                {7, new[] {7.0, 2.0}},
                {8, new[] {6.0, 4.0}},
            };

            var optimalSequence = new[] {1, 2, 3, 4, 5, 6, 7, 8};
            var fitness = GetFitness(optimalSequence, idToLocationLookup);
            Assert.AreEqual(20.63, fitness.TotalDistance);
            Solve(idToLocationLookup, optimalSequence);
        }

        [TestMethod]
        public void Ulysses16Test()
        {
            var idToLocationLookup = LoadData(@"data\ulysses16.tsp");
            var optimalSequence = new[]
            {
                14, 13, 12, 16, 1, 3, 2, 4,
                8, 15, 5, 11, 9, 10, 7, 6
            };

            var fitness = GetFitness(optimalSequence, idToLocationLookup);
            Assert.AreEqual(73.99, fitness.TotalDistance);
            Solve(idToLocationLookup, optimalSequence);
        }

        [TestMethod]
        public void BenchmarkTest()
        {
            Benchmark.Run(Ulysses16Test);
        }

        private void Solve(Dictionary<int, double[]> idToLocationLookup, int[] optimalSequence)
        {
            var geneSet = idToLocationLookup.Keys.ToArray();
            var watch = Stopwatch.StartNew();

            int[] FnCreate() => Rand.RandomSampleArray(geneSet, geneSet.Length);

            void FnDisplay(Chromosome<int, Fitness> candidate) => Display(candidate, watch);

            Fitness FnGetFitness(int[] genes) => GetFitness(genes, idToLocationLookup);

            void FnMutate(int[] genes) => Mutate(genes, FnGetFitness);

            int[] FnCrossover(int[] parent, int[] donor) => Crossover(parent, donor, FnGetFitness);

            var optimalFitness = FnGetFitness(optimalSequence);
            var best = Genetic<int, Fitness>.GetBest(FnGetFitness, 0, optimalFitness, null, FnDisplay, FnMutate, FnCreate, 50, 25,
                FnCrossover);
            Assert.IsTrue(optimalFitness.CompareTo(best.Fitness) <= 0);
        }

        private Dictionary<int, double[]> LoadData(string localFileName)
        {
            // expects:
            // HEADER section before DATA section, all lines start in column 0
            // DATA section element all have space in column  0
            //     < space > 1 23.45 67.89
            // last line of file is: " EOF"

            var idToLocationLookup = new Dictionary<int, double[]>();
            var content = File.ReadAllLines(localFileName);
            foreach (var row in content)
            {
                if (row[0] != ' ') // HEaders
                    continue;
                if (row == " EOF")
                    break;

                var data = row.Split(' ');
                var id = int.Parse(data[1]);
                var x = double.Parse(data[2]);
                var y = double.Parse(data[3]);
                idToLocationLookup.Add(id, new[] {x, y});
            }

            return idToLocationLookup;
        }
    }
}