using System;

namespace GeneticAlgorithms.Queens
{
    public class Fitness : IComparable
    {
        public int Total { get; }

        public Fitness(int total)
        {
            Total = total;
        }

        public int CompareTo(object obj)
        {
            switch (obj)
            {
                case null:
                    return 1;
                case Fitness that:
                    return -1 * Total.CompareTo(that.Total);
                default:
                    throw new ArgumentException("Object is not a Fitness");
            }
        }

        public override string ToString()
        {
            return Total.ToString();
        }
    }
}