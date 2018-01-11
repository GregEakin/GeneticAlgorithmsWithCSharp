using System;

namespace GeneticAlgorithms.magicSquare
{
    public class Fitness : IComparable
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
                    return -1 * SumOfDifferences.CompareTo(that.SumOfDifferences);
                default:
                    throw new ArgumentException("Object is not a Fitness");
            }
        }

        public override string ToString()
        {
            return SumOfDifferences.ToString();
        }
    }
}