using System;

namespace GeneticAlgorithms.Cards
{
    public class Fitness : IComparable, IComparable<Fitness>
    {
        public int Sum { get; }
        public int Product { get; }
        public int Duplicate { get; }
        public int Difference { get; }

        public Fitness(int sum, int product, int duplicate)
        {
            Sum = sum;
            Product = product;
            Duplicate = duplicate;

            var sumDifference = Math.Abs(36 - sum);
            var productDifference = Math.Abs(360 - product);
            Difference = sumDifference + productDifference;
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
            var duplicateComparison = -Duplicate.CompareTo(that.Duplicate);
            if (duplicateComparison != 0) return duplicateComparison;
            return -Difference.CompareTo(that.Difference);
        }

        public override string ToString()
        {
            return $"sum: {Sum} prod: {Product} dups: {Duplicate}";
        }
    }
}