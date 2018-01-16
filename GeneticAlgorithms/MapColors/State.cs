using System;
using System.Collections.Generic;

namespace GeneticAlgorithms.MapColors
{
    public class State : IComparable, IComparable<State>
    {
        public string Name { get; }
        public int Index { get; }
        public ISet<State> Neighbors { get; } = new SortedSet<State>();

        public State(string name, int index)
        {
            Name = name;
            Index = index;
        }

        public char Color(char[] genes) => genes[Index];

        public int CompareTo(object obj)
        {
            switch (obj)
            {
                case null:
                    return 1;
                case State that:
                    return CompareTo(that);
                default:
                    throw new ArgumentException("Object is not a State");
            }
        }

        public int CompareTo(State that)
        {
            if (ReferenceEquals(this, that)) return 0;
            if (that is null) return 1;
            return string.Compare(Name, that.Name, StringComparison.Ordinal);
        }

        public override int GetHashCode() => Name.GetHashCode() * 10 + Index;

        public override bool Equals(object obj) => obj is State that && CompareTo(that) == 0;

        public override string ToString() => Name;
    }
}