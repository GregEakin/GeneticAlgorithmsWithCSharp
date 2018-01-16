using System;

namespace GeneticAlgorithms.Queens
{
    public class Fitness : IComparable, IComparable<Fitness>
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
                    return CompareTo(that);
                default:
                    throw new ArgumentException("Object is not a Fitness");
            }
        }

        public int CompareTo(Fitness that)
        {
            if (ReferenceEquals(this, that)) return 0;
            if (that is null) return 1;
            return -Total.CompareTo(that.Total);
        }

        public override string ToString()
        {
            return Total.ToString();
        }
    }
}