using System;

namespace PrimeTesting.mapColors
{
    public class Rule : IComparable
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
                    var thisRule = From.Name + To.Name;
                    var thatRule = that.From.Name + that.To.Name;
                    return string.Compare(thisRule, thatRule, StringComparison.Ordinal);
                default:
                    throw new ArgumentException("Object is not a Rule");
            }
        }
    }
}