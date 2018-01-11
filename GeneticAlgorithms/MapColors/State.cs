using System;
using System.Collections.Generic;

namespace GeneticAlgorithms.MapColors
{
    public class State : IComparable
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
                    return string.Compare(Name, that.Name, StringComparison.Ordinal);
                default:
                    throw new ArgumentException("Object is not a State");
            }
        }

        //public override int GetHashCode() => Name.GetHashCode();

        //public override bool Equals(object obj) => obj is State item && Name.Equals(item.Name);

        public override string ToString() => Name;
    }
}