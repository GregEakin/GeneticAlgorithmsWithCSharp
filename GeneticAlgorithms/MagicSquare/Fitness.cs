using System;

namespace GeneticAlgorithms.MagicSquare
{
    public class Fitness : IComparable, IComparable<Fitness>
    {
        private int SumOfDifferences { get; }

        public Fitness(int sumOfDifferences)
        {
            SumOfDifferences = sumOfDifferences;
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
            return -SumOfDifferences.CompareTo(that.SumOfDifferences);
        }

        public override string ToString()
        {
            return SumOfDifferences.ToString();
        }
    }
}