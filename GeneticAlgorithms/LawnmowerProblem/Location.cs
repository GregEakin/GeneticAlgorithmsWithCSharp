using System;

namespace GeneticAlgorithms.LawnmowerProblem
{
    public class Location
    {
        public int X { get; }
        public int Y { get; }

        public Location(int x, int y)
        {
            X = x;
            Y = y;
        }

        public Location Move(int xOffset, int yOffset)
        {
            return new Location(X + xOffset, Y + yOffset);
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
                case Location that:
                    return X == that.X && Y == that.Y;
                default:
                    return false;
            }
        }

        public override int GetHashCode()
        {
            return X * 1000 + Y;
        }
    }
}