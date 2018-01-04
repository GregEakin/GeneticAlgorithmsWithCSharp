using System;

namespace PrimeTesting.cards
{
    public class Fitness : IComparable
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
                case Fitness other:
                    return Duplicate != other.Duplicate
                        ? -1 * Duplicate.CompareTo(other.Duplicate)
                        : -1 * Difference.CompareTo(other.Difference);
                default:
                    throw new ArgumentException("Object is not a Fitness");
            }
        }

        public override string ToString()
        {
            return $"sum: {Sum} prod: {Product} dups: {Duplicate}";
        }
    }
}