using System;
using System.Collections.Generic;

namespace GeneticAlgorithms.MapColors
{
    public class Rule : IComparable, IComparable<Rule>
    {
        public State From { get; }
        public State To { get; }

        public Rule(State from, State to)
        {
            From = from;
            To = to;

            From.Neighbors.Add(To);
            To.Neighbors.Add(From);
        }

        public bool Valid(char[] genes) => genes[From.Index] != genes[To.Index];

        public int CompareTo(object obj)
        {
            switch (obj)
            {
                case null:
                    return 1;
                case Rule that:
                    return CompareTo(that);
                default:
                    throw new ArgumentException("Object is not a Rule");
            }
        }

        public int CompareTo(Rule that)
        {
            if (ReferenceEquals(this, that)) return 0;
            if (that is null) return 1;
            var fromComparison = Comparer<State>.Default.Compare(From, that.From);
            if (fromComparison != 0) return fromComparison;
            return Comparer<State>.Default.Compare(To, that.To);
        }
    }
}