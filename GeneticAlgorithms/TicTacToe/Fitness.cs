using System;

namespace GeneticAlgorithms.TicTacToe
{
    public class Fitness : IComparable, IComparable<Fitness>
    {
        public int Wins { get; }
        public int Ties { get; }
        public int Losses { get; }
        public double PercentWins => 100.0 * Wins / (Wins + Ties + Losses);
        public double PercentLosses => 100.0 * Losses / (Wins + Ties + Losses);
        public double PercentTies => 100.0 * Ties / (Wins + Ties + Losses);
        public int GeneCount { get; }

        public Fitness(int wins, int ties, int losses, int geneCount)
        {
            Wins = wins;
            Ties = ties;
            Losses = losses;
            GeneCount = geneCount;
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

            if (Math.Abs(PercentLosses - that.PercentLosses) > 0.1)
                return -PercentLosses.CompareTo(that.PercentLosses);

            if (Losses > 0)
                return 0;

            if (Ties != that.Ties)
                return -Ties.CompareTo(that.Ties);

            return -GeneCount.CompareTo(that.GeneCount);
        }

        public override string ToString()
        {
            return $"{PercentLosses:F1}% Losses ({Losses}), {PercentTies:F1}% Ties ({Ties}), {PercentWins:F1}% Wins ({Wins}), {GeneCount} rules";
        }
    }
}