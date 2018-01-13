using System;
using System.Linq;
using System.Text;

namespace GeneticAlgorithms.LawnmowerProblem
{
    public static class FieldContents
    {
        public static char Grass = '#';
        public static char Mowed = '.';
        public static char Mower = 'M';
        public static char None = ' ';
    }

    public class Direction
    {
        public int Index { get; }
        public int XOffset { get; }
        public int YOffset { get; }
        public char Symbol { get; }

        public Direction(int index, int xOffset, int yOffset, char symbol)
        {
            Index = index;
            XOffset = xOffset;
            YOffset = yOffset;
            Symbol = symbol;
        }

        public Location MoveFrom(Location location, int distance = 1)
        {
            return new Location(location.X + distance * XOffset, location.Y + distance * YOffset);
        }
    }

    public class Directions
    {
        public static Direction North = new Direction(0, 0, -1, '^');
        public static Direction South = new Direction(1, 1, 0, 'v');
        public static Direction East = new Direction(2, 0, 1, '<');
        public static Direction West = new Direction(3, -1, 0, '>');

        public static Direction[] Dirs =
        {
            North, South, East, West
        };

        public static Direction GetDirectionAfterTurnLeft90Degrees(Direction direction)
        {
            var newIndex = direction.Index > 0 ? direction.Index - 1 : Directions.Dirs.Length - 1;
            var newDirection = Dirs.Single(d => d.Index == newIndex);
            return newDirection;
        }

        public static Direction GetDirectionAfterTurnRight90Degrees(Direction direction)
        {
            var newIndex = direction.Index < Directions.Dirs.Length ? direction.Index + 1 : 0;
            var newDirection = Dirs.Single(d => d.Index == newIndex);
            return newDirection;
        }
    }

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
    }

    public class Mower
    {
        public Location Location { get; }
        public Direction Direction { get; }
        public int StepCount { get; }

        public Mower(Location location, Direction direction, int stepCount)
        {
            Location = location;
            Direction = direction;
            StepCount = stepCount;
        }

        public Mower TurnLeft()
        {
            return new Mower(Location, Directions.GetDirectionAfterTurnLeft90Degrees(Direction), StepCount + 1);
        }

        public Mower Mow(Field field)
        {
            return null;
        }
    }

    public class Field
    {
        private readonly char[,] _field;

        public int Width => _field.GetLength(0);

        public int Height => _field.GetLength(1);

        public Field(int width, int height)
        {
            _field = new char[width, height];
        }

        public char this[int x, int y]
        {
            get => _field[x, y];
            set => _field[x, y] = value;
        }

        public int CountMowed()
        {
            var sum = 0;
            for (var x = 0; x < Width; x++)
            for (var y = 0; y < Height; y++)
                if (_field[x, y] != FieldContents.Grass)
                    sum++;
            return sum;
        }

        public void Dispaly(Mower mower)
        {
            {
                for (var y = 0; y < Height; y++)
                {
                    for (var x = 0; x < Width; x++)
                        if (y != mower.Location.Y)
                        {
                            Console.Write(" {0} ", _field[x, y]);
                        }
                        else
                        {
                            {
                                if (x != mower.Location.X)
                                    Console.Write(" {0} ", _field[x, y]);
                                else
                                    Console.Write("{0}{1} ", FieldContents.Mower, mower.Direction.Symbol);
                            }
                        }

                    Console.WriteLine();
                }
            }
        }
    }

    public class ValidatingField : Field
    {
        public ValidatingField(int width, int height) : base(width, height)
        {
        }
    }

    public class ToroidField : Field
    {
        public ToroidField(int width, int height) : base(width, height)
        {
        }
    }
}