using System;

namespace GeneticAlgorithms.LawnmowerProblem
{
    public class Fitness : IComparable
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
                    if (TotalMowed != that.TotalMowed)
                        return TotalMowed.CompareTo(that.TotalMowed);
                    if (StepCount != that.StepCount)
                        return -1 * StepCount.CompareTo(that.StepCount);
                    return -1 * TotalInstructions.CompareTo(that.TotalInstructions);
                default:
                    throw new ArgumentException("Object is not a Fitness");
            }
        }

        public override string ToString()
        {
            return $"{TotalMowed} mowed with {TotalInstructions} instructions and {StepCount} steps";
        }
    }
}