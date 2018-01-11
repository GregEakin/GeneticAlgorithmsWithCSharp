using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace GeneticAlgorithms.knapsack
{
    [TestClass]
    public class KnapsackTests
    {
        private static readonly Random Random = new Random();

        public static Fitness Fitness(ItemQuantity[] genes)
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

        public static void Display(Chromosome<ItemQuantity, Fitness> candidate, Stopwatch watch)
        {
            var genes = candidate.Genes.OrderByDescending(g => g.Quantity);
            var descriptions = genes.Select(id => $"{id.Quantity} x {id.Item.Name}").ToArray();
            if (descriptions.Length > 0)
                Console.WriteLine("{0}\t{1}\t{2} ms", string.Join(", ", descriptions), candidate.Fitness,
                    watch.ElapsedMilliseconds);
            else
                Console.WriteLine("Empty");
        }

        public static ItemQuantity[] Mutate(ItemQuantity[] input, Resource[] items, double maxWeight, double maxVolume,
            Window window)
        {
            window.Slide();
            var fitness = Fitness(input);
            var genes = input.ToList();
            var remainingWeight = maxWeight - fitness.TotalWeight;
            var remainingVolume = maxVolume - fitness.TotalVolume;

            var removing = genes.Count > 1 && Random.Next(10) == 0;
            if (removing)
            {
                var index1 = Random.Next(genes.Count);
                var iq1 = genes[index1];
                var item1 = iq1.Item;
                remainingWeight += item1.Weight * iq1.Quantity;
                remainingVolume += item1.Volume * iq1.Quantity;
                genes.RemoveAt(index1);
            }

            var adding = (remainingWeight > 0 || remainingVolume > 0) &&
                         (genes.Count == 0 || (genes.Count < items.Length && Random.Next(100) == 0));
            if (adding)
            {
                var newGene = Add(genes.ToArray(), items, remainingWeight, remainingVolume);
                if (newGene != null)
                {
                    genes.Add(newGene);
                    return genes.ToArray();
                }
            }

            var index = Random.Next(genes.Count);
            var iq = genes[index];
            var item = iq.Item;
            remainingWeight += item.Weight * iq.Quantity;
            remainingVolume += item.Volume * iq.Quantity;

            var changeItem = genes.Count < items.Length && Random.Next(4) == 0;
            if (changeItem)
            {
                var itemIndex = Array.IndexOf(items, iq.Item);
                var start = Math.Max(1, itemIndex - window.Size);
                var stop = Math.Min(items.Length - 1, itemIndex + window.Size);
                item = items[Random.Next(start, stop)];
            }

            var maxQuantity = MaxQuantity(item, remainingWeight, remainingVolume);
            if (maxQuantity > 0)
            {
                var newGene = new ItemQuantity(item, window.Size > 1 ? maxQuantity : Random.Next(1, maxQuantity));
                genes[index] = newGene;
            }
            else
                genes.RemoveAt(index);

            return genes.ToArray();
        }

        public static ItemQuantity[] Create(Resource[] items, double maxWeight, double maxVolume)
        {
            var genes = new List<ItemQuantity>();
            var remainingWeight = maxWeight;
            var remainingVolume = maxVolume;
            foreach (var unused in Enumerable.Range(1, Random.Next(items.Length)))
            {
                var newGene = Add(genes.ToArray(), items, remainingWeight, remainingVolume);
                if (newGene == null) continue;
                genes.Add(newGene);
                remainingWeight -= newGene.Quantity * newGene.Item.Weight;
                remainingVolume -= newGene.Quantity * newGene.Item.Volume;
            }

            return genes.ToArray();
        }

        public static ItemQuantity Add(ItemQuantity[] genes, Resource[] items, double maxWeight, double maxVolume)
        {
            var usedItems = genes.Select(iq => iq.Item).ToArray();
            var item = items[Random.Next(items.Length)];
            while (usedItems.Contains(item))
                item = items[Random.Next(items.Length)];

            var maxQuantity = MaxQuantity(item, maxWeight, maxVolume);
            return maxQuantity > 0 ? new ItemQuantity(item, maxQuantity) : null;
        }

        public static int MaxQuantity(Resource item, double maxWeight, double maxVolume)
        {
            var weight = item.Weight > 0 ? (int) (maxWeight / item.Weight) : int.MaxValue;
            var volume = item.Volume > 0 ? (int) (maxVolume / item.Volume) : int.MaxValue;
            return Math.Min(weight, volume);
        }

        public static void FillKnapsack(Resource[] items, double maxWeight, double maxVolume, Fitness optimalFitness)
        {
            var genetic = new Genetic<ItemQuantity, Fitness>();
            var watch = new Stopwatch();
            var window = new Window(1, Math.Max(1, items.Length / 3), items.Length / 2);
            var sortedItems = items.OrderBy(i => i.Value).ToArray();

            Fitness FitnessFun(ItemQuantity[] genes) => Fitness(genes);
            void DisplayFun(Chromosome<ItemQuantity, Fitness> candidate) => Display(candidate, watch);
            ItemQuantity[] MutateFun(ItemQuantity[] genes) => Mutate(genes, sortedItems, maxWeight, maxVolume, window);
            ItemQuantity[] CreateFun() => Create(items, maxWeight, maxVolume);

            watch.Start();
            var best = genetic.BestFitness(FitnessFun, 0, optimalFitness, null, DisplayFun, MutateFun, CreateFun, 50);
            watch.Stop();
            Assert.IsTrue(optimalFitness.CompareTo(best.Fitness) <= 0);
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
            var maxvolume = 4.0;

            var quantities = new[]
            {
                new ItemQuantity(items[0], 1),
                new ItemQuantity(items[1], 14),
                new ItemQuantity(items[2], 6)
            };
            var optimal = Fitness(quantities);
            FillKnapsack(items, maxWeight, maxvolume, optimal);
        }

        [TestMethod]
        public void Exnsd16Test()
        {
            var problemInfo = KnapsackProblemDataParser.LoadData(@"..\..\Data\exnsd16.ukp");
            var items = problemInfo.Resources;
            var maxWeight = problemInfo.MaxWeight;
            var maxVolume = 0.0;
            var optimal = Fitness(problemInfo.Solution);
            FillKnapsack(items, maxWeight, maxVolume, optimal);
        }
    }
}