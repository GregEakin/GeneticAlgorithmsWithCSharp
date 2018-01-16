using System;

namespace GeneticAlgorithms.Knapsack
{
    public class Fitness : IComparable, IComparable<Fitness>
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
                    return CompareTo(that);
                default:
                    throw new ArgumentException("Object is not a Fitness");
            }
        }

        public int CompareTo(Fitness that)
        {
            if (ReferenceEquals(this, that)) return 0;
            if (that is null) return 1;
            return TotalValue.CompareTo(that.TotalValue);
        }

        public override string ToString()
        {
            return $"wt: {TotalWeight} vol: {TotalVolume} value {TotalValue}";
        }
    }
}