using System;

namespace GeneticAlgorithms.Knapsack
{
    public class Fitness : IComparable
    {
        public double TotalWeight { get; }

        public double TotalVolume { get; }

        public int TotalValue { get; }

        public Fitness(double totalWeight, double totalVolume, int totalValue)
        {
            TotalWeight = totalWeight;
            TotalVolume = totalVolume;
            TotalValue = totalValue;
        }

        public int CompareTo(object obj)
        {
            switch (obj)
            {
                case null:
                    return 1;
                case Fitness that:
                    return -1 * TotalValue.CompareTo(that.TotalValue);
                default:
                    throw new ArgumentException("Object is not a Fitness");
            }
        }

        public override string ToString()
        {
            return $"wt: {TotalWeight} vol: {TotalVolume} value {TotalValue}";
        }
    }
}