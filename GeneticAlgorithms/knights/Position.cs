using System;

namespace GeneticAlgorithms.Knights
{
    public class Position : IComparable, IComparable<Position>
    {
        public int X { get; }
        public int Y { get; }

        public Position(int x, int y)
        {
            X = x;
            Y = y;
        }

        public override string ToString()
        {
            return $"{X},{Y}";
        }

        public override bool Equals(object obj)
        {
            switch (obj)
            {
                case null:
                    return false;
                case Position that:
                    return CompareTo(that) == 0;
                default:
                    return false;
            }
        }

        public override int GetHashCode()
        {
            return X * 1000 + Y;
        }

        public int CompareTo(object obj)
        {
            switch (obj)
            {
                case null:
                    return 1;
                case Position that:
                    return CompareTo(that);
                default:
                    throw new ArgumentException("Object is not a Fitness");
            }
        }

        public int CompareTo(Position that)
        {
            if (ReferenceEquals(this, that)) return 0;
            if (that is null) return 1;
            var xComparison = X.CompareTo(that.X);
            if (xComparison != 0) return xComparison;
            return Y.CompareTo(that.Y);
        }
    }
}