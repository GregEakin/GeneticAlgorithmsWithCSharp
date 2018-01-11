using System;

namespace GeneticAlgorithms.LinearEquation
{
    public class Fitness : IComparable
    {
        public Fraction TotalDifference { get; }

        public Fitness(Fraction totalDifference)
        {
            TotalDifference = totalDifference;
        }

        public int CompareTo(object obj)
        {
            switch (obj)
            {
                case null:
                    return 1;
                case Fitness that:
                    return -1 * TotalDifference.CompareTo(that.TotalDifference);
                default:
                    throw new ArgumentException("Object is not a Fitness");
            }
        }

        public override string ToString()
        {
            return $"diff: {TotalDifference:F2}";
        }

        public static explicit operator double(Fitness v)
        {
            throw new NotImplementedException();
        }
    }
}