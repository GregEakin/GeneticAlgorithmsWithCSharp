using System;

namespace GeneticAlgorithms.RegularExpressions
{
    public class Fitness : IComparable, IComparable<Fitness>
    {
        public static bool UseRegexLength { get; set; }

        public int NumWantedMatched { get; }
        private readonly int _totalWanted;
        public int NumUnwantedMatched { get; }
        public int Length { get; }

        public Fitness(int numWantedMatched, int totalWanted, int numUnwantedMatched, int length)
        {
            NumWantedMatched = numWantedMatched;
            _totalWanted = totalWanted;
            NumUnwantedMatched = numUnwantedMatched;
            Length = length;
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

            var combined = _totalWanted - NumWantedMatched + NumUnwantedMatched;
            var thatCombined = that._totalWanted - that.NumWantedMatched + that.NumUnwantedMatched;
            if (combined != thatCombined)
                return -combined.CompareTo(thatCombined);

            var success = combined == 0;
            var otherSuccess = thatCombined == 0;
            if (success != otherSuccess)
                return success ? 1 : 0;

            if (!success)
                return UseRegexLength
                    ? -Length.CompareTo(that.Length)
                    : 0;

            return -Length.CompareTo(that.Length);
        }

        public override string ToString()
        {
            var wanted = _totalWanted == NumWantedMatched ? "all" : NumWantedMatched.ToString();
            return $"matches: {wanted} wanted, {NumUnwantedMatched} unwanted, len {Length}";
        }
    }
}