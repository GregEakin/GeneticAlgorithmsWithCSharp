using System;

namespace GeneticAlgorithms.RegularExpressions
{
    public class Fitness : IComparable, IComparable<Fitness>
    {
        public int TotalMowed { get; }
        public int TotalInstructions { get; }
        public int StepCount { get; }

        public Fitness(int totalMowed, int totalInstructions, int stepCount)
        {
            TotalMowed = totalMowed;
            TotalInstructions = totalInstructions;
            StepCount = stepCount;
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
            var totalMowedComparison = TotalMowed.CompareTo(that.TotalMowed);
            if (totalMowedComparison != 0) return totalMowedComparison;
            var stepCountComparison = -StepCount.CompareTo(that.StepCount);
            if (stepCountComparison != 0) return stepCountComparison;
            return -TotalInstructions.CompareTo(that.TotalInstructions);
        }

        public override string ToString()
        {
            return $"{TotalMowed} mowed with {TotalInstructions} instructions and {StepCount} steps";
        }
    }
}