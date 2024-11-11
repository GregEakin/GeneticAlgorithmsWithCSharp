/* File: CardTests.cs
 *     from chapter 6 of _Genetic Algorithms with Python_
 *     written by Clinton Sheppard
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
using System.Diagnostics;

namespace GeneticAlgorithms.Cards;

[TestClass]
public class CardTests
{
    private static Fitness GetFitness(IReadOnlyCollection<int> genes)
    {
        var group1Sum = genes.Take(5).Sum();
        var group2Product = genes.Skip(5).Aggregate(1, (acc, val) => acc * val);
        var duplicateCount = genes.Count - new HashSet<int>(genes).Count;
        return new Fitness(group1Sum, group2Product, duplicateCount);
    }

    private static void Display(Chromosome<int, Fitness> candidate, Stopwatch watch)
    {
        Console.WriteLine("{0} - {1}\t{2},\t{3} ms",
            string.Join(", ", candidate.Genes.Take(5)),
            string.Join(", ", candidate.Genes.Skip(5)),
            candidate.Fitness, watch.ElapsedMilliseconds);
    }

    private static void Mutate(IList<int> genes, IReadOnlyList<int> geneSet)
    {
        if (genes.Count == new HashSet<int>(genes).Count)
        {
            var count = Rand.Random.Next(1, 4);
            while (count-- > 0)
            {
                var randomSample = Rand.RandomSample(Enumerable.Range(0, genes.Count).ToList(), 2);
                var indexA = randomSample[0];
                var indexB = randomSample[1];
                (genes[indexA], genes[indexB]) = (genes[indexB], genes[indexA]);
            }
        }
        else
        {
            var indexA = Rand.Random.Next(genes.Count);
            var indexB = Rand.Random.Next(geneSet.Count);
            genes[indexA] = geneSet[indexB];
        }
    }

    [TestMethod]
    public void FitnessTest()
    {
        var genes = new[] {1, 1, 1, 1, 1, 2, 2, 2, 2, 2};
        var fitness = GetFitness(genes);
        Assert.AreEqual(5, fitness.Group1Sum);
        Assert.AreEqual(32, fitness.Group2Product);
        Assert.AreEqual(8, fitness.DuplicateCount);
    }

    [TestMethod]
    public void DisplayTest()
    {
        var genes = new[] {1, 1, 1, 1, 1, 2, 2, 2, 2, 2};
        var fitness = GetFitness(genes);
        var chromosome = new Chromosome<int, Fitness>(genes, fitness);
        var watch = Stopwatch.StartNew();

        Display(chromosome, watch);
    }

    [TestMethod]
    public void CardTest()
    {
        var geneSet = Enumerable.Range(1, 10).ToArray();
        var watch = Stopwatch.StartNew();

        void FnDisplay(Chromosome<int, Fitness> candidate) => Display(candidate, watch);
        Fitness FnGetFitness(IReadOnlyList<int> genes) => GetFitness(genes);
        void FnMutate(IList<int> genes) => Mutate(genes, geneSet);

        var optimalFitness = new Fitness(36, 360, 0);
        var best = Genetic<int, Fitness>.GetBest(FnGetFitness, 10, optimalFitness, geneSet, FnDisplay, FnMutate);
        Assert.IsTrue(optimalFitness.CompareTo(best.Fitness) <= 0);
    }

    [TestMethod]
    public void BenchmarkTest()
    {
        Benchmark.Run(CardTest);
    }
}