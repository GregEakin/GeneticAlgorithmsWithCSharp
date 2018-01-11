using System;

namespace GeneticAlgorithms.Knapsack
{
    public class Resource : IComparable
    {
        public string Name { get; }
        public int Value { get; }
        public double Weight { get; }
        public double Volume { get; }

        public Resource(string name, int value, double weight, double volume)
        {
            Name = name;
            Value = value;
            Weight = weight;
            Volume = volume;
        }

        public int CompareTo(object obj)
        {
            switch (obj)
            {
                case null:
                    return 1;
                case Resource that:
                    return string.Compare(Name, that.Name, StringComparison.Ordinal);
                default:
                    throw new ArgumentException("Object is not a Fitness");
            }
        }
    }
}