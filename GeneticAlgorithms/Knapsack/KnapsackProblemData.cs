using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace GeneticAlgorithms.Knapsack
{
    public class KnapsackProblemData
    {
        public Resource[] Resources { get; }

        public int MaxWeight { get; }

        public ItemQuantity[] Solution { get; }

        public KnapsackProblemData(Resource[] resources, int maxWeight, ItemQuantity[] solution)
        {
            Resources = resources;
            MaxWeight = maxWeight;
            Solution = solution;
        }
    }
}