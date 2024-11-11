/* File: KnapsackTests.cs
 *     from chapter 9 of _Genetic Algorithms with Python_
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

using GeneticAlgorithms.MagicSquare;
using GeneticAlgorithms.Utilities;
using System.Diagnostics;

namespace GeneticAlgorithms.Knapsack;

[TestClass]
public class KnapsackTests
{
    private static Fitness GetFitness(IEnumerable<ItemQuantity> genes)
    {
        var totalWeight = 0.0;
        var totalVolume = 0.0;
        var totalValue = 0;
        foreach (var gene in genes)
        {
            var count = gene.Quantity;
            totalWeight += gene.Item.Weight * count;
            totalVolume += gene.Item.Volume * count;
            totalValue += gene.Item.Value * count;
        }

        return new Fitness(totalWeight, totalVolume, totalValue);
    }

    private static void Display(Chromosome<ItemQuantity, Fitness> candidate, Stopwatch watch)
    {
        if (candidate.Genes.Count > 0)
        {
            var descriptions = candidate.Genes.OrderByDescending(g => g.Quantity)
                .Select(iq => $"{iq.Quantity} x {iq.Item.Name}");
            Console.WriteLine("{0}\t{1}\t{2} ms", string.Join(", ", descriptions), candidate.Fitness,
                watch.ElapsedMilliseconds);
        }
        else
            Console.WriteLine("Empty");
    }

    private static int MaxQuantity(Resource item, double maxWeight, double maxVolume)
    {
        var weight = item.Weight > 0 ? (int) (maxWeight / item.Weight) : int.MaxValue;
        var volume = item.Volume > 0 ? (int) (maxVolume / item.Volume) : int.MaxValue;
        return Math.Min(weight, volume);
    }

    private static List<ItemQuantity> Create(IReadOnlyCollection<Resource> items, double maxWeight, double maxVolume)
    {
        var genes = new List<ItemQuantity>();
        var remainingWeight = maxWeight;
        var remainingVolume = maxVolume;
        foreach (var unused in Enumerable.Range(1, Rand.Random.Next(items.Count)))
        {
            var newGene = Add(genes.ToArray(), items, remainingWeight, remainingVolume);
            if (newGene == null) continue;
            genes.Add(newGene);
            remainingWeight -= newGene.Quantity * newGene.Item.Weight;
            remainingVolume -= newGene.Quantity * newGene.Item.Volume;
        }

        return genes;
    }

    private static ItemQuantity Add(IEnumerable<ItemQuantity> genes, IEnumerable<Resource> items, double maxWeight, double maxVolume)
    {
        var usedItems = genes.Select(iq => iq.Item).ToList();
        var item = Rand.SelectItemNotIn(items, usedItems);
        var maxQuantity = MaxQuantity(item, maxWeight, maxVolume);
        return maxQuantity > 0 ? new ItemQuantity(item, maxQuantity) : null;
    }

    private static void Mutate(IList<ItemQuantity> genes, IList<Resource> items, double maxWeight, double maxVolume,
        Window window)
    {
        window.Slide();
        var fitness = GetFitness(genes);
        var remainingWeight = maxWeight - fitness.TotalWeight;
        var remainingVolume = maxVolume - fitness.TotalVolume;

        var removing = genes.Count > 1 && Rand.PercentChance(10);
        if (removing)
        {
            var index1 = Rand.Random.Next(genes.Count);
            var iq1 = genes[index1];
            var item1 = iq1.Item;
            remainingWeight += item1.Weight * iq1.Quantity;
            remainingVolume += item1.Volume * iq1.Quantity;
            genes.RemoveAt(index1);
        }

        var adding = (remainingWeight > 0 || remainingVolume > 0) &&
                     (genes.Count == 0 || (genes.Count < items.Count && Rand.PercentChance(1)));
        if (adding)
        {
            var newGene = Add(genes, items, remainingWeight, remainingVolume);
            if (newGene != null)
            {
                genes.Add(newGene);
                return;
            }
        }

        var index = Rand.Random.Next(genes.Count);
        var iq = genes[index];
        var item = iq.Item;
        remainingWeight += item.Weight * iq.Quantity;
        remainingVolume += item.Volume * iq.Quantity;

        var changeItem = genes.Count < items.Count && Rand.PercentChance(25);
        if (changeItem)
        {
            var itemIndex = items.IndexOf(iq.Item);
            var start = Math.Max(1, itemIndex - window.Size);
            var stop = Math.Min(items.Count - 1, itemIndex + window.Size);
            item = items[Rand.Random.Next(start, stop)];
        }

        var maxQuantity = MaxQuantity(item, remainingWeight, remainingVolume);
        if (maxQuantity > 0)
        {
            var newGene = new ItemQuantity(item, window.Size > 1 ? maxQuantity : Rand.Random.Next(1, maxQuantity));
            genes[index] = newGene;
        }
        else
            genes.RemoveAt(index);
    }

    [TestMethod]
    public void CookieTest()
    {
        var items = new[]
        {
            new Resource("Flour", 1680, 0.265, .41),
            new Resource("Butter", 1440, 0.5, .13),
            new Resource("Sugar", 1840, 0.441, .29),
        };

        var maxWeight = 10.0;
        var maxVolume = 4.0;

        var quantities = new[]
        {
            new ItemQuantity(items[1], 14),
            new ItemQuantity(items[2], 6),
            new ItemQuantity(items[0], 1),
        };
        var optimal = GetFitness(quantities);
        FillKnapsack(items, maxWeight, maxVolume, optimal);
    }

    [TestMethod]
    public void Exnsd16Test()
    {
        var problemInfo = KnapsackProblemDataParser.LoadData(@"Data\exnsd16.ukp");
        var items = problemInfo.Resources;
        var maxWeight = problemInfo.MaxWeight;
        var maxVolume = 0.0;
        var optimal = GetFitness(problemInfo.Solution);
        FillKnapsack(items, maxWeight, maxVolume, optimal);
    }

    [TestMethod]
    public void BenchmarkTest()
    {
        Benchmark.Run(Exnsd16Test);
    }

    private static void FillKnapsack(IReadOnlyCollection<Resource> items, double maxWeight,
        double maxVolume, Fitness optimalFitness)
    {
        var watch = Stopwatch.StartNew();
        var window = new Window(1, Math.Max(1, items.Count / 3), items.Count / 2);
        var sortedItems = items.OrderBy(i => i.Value).ToList();

        void FnDisplay(Chromosome<ItemQuantity, Fitness> candidate) => Display(candidate, watch);
        Fitness FnGetFitness(IReadOnlyList<ItemQuantity> genes) => GetFitness(genes);
        List<ItemQuantity> FnCreate() => Create(items, maxWeight, maxVolume);
        void FnMutate(List<ItemQuantity> genes) => Mutate(genes, sortedItems, maxWeight, maxVolume, window);

        var best = Genetic<ItemQuantity, Fitness>.GetBest(FnGetFitness, 0, optimalFitness, null, FnDisplay, FnMutate, FnCreate, 50);
        Assert.IsTrue(optimalFitness.CompareTo(best.Fitness) <= 0);
    }
}