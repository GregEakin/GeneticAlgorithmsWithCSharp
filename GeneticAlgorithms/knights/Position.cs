using System;

namespace GeneticAlgorithms.knights
{
    public class Position : IComparable
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
                    return X == that.X && Y == that.Y;
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
                    return X != that.X
                        ? X.CompareTo(that.X)
                        : Y.CompareTo(that.Y);
                default:
                    throw new ArgumentException("Object is not a Fitness");
            }
        }
    }
}