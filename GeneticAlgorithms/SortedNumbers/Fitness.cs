using System;
using System.Security.Cryptography.X509Certificates;

namespace GeneticAlgorithms.SortedNumbers
{
    public class Fitness : IComparable, IComparable<Fitness>
    {
        public int NumbersInSequenceCount { get; }
        public int TotalGap { get; }

        public Fitness(int numbersInSequenceCount, int totalGap)
        {
            NumbersInSequenceCount = numbersInSequenceCount;
            TotalGap = totalGap;
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
            return NumbersInSequenceCount != that.NumbersInSequenceCount
                ? NumbersInSequenceCount.CompareTo(that.NumbersInSequenceCount)
                : -TotalGap.CompareTo(that.TotalGap);
        }

        public override string ToString() => $"{NumbersInSequenceCount} Sequential, {TotalGap} Total Gap";
    }
}