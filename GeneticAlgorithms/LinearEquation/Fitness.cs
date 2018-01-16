using System;
using System.Collections.Generic;

namespace GeneticAlgorithms.LinearEquation
{
    public class Fitness : IComparable, IComparable<Fitness>
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
                    return CompareTo(that);
                default:
                    throw new ArgumentException("Object is not a Fitness");
            }
        }

        public int CompareTo(Fitness that)
        {
            if (ReferenceEquals(this, that)) return 0;
            if (that is null) return 1;
            return -Comparer<Fraction>.Default.Compare(TotalDifference, that.TotalDifference);
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