using System;
using System.Linq;

namespace GeneticAlgorithms.LawnmowerProblem
{
    public static class FieldContents
    {
        public static string Grass = " #";
        public static string Mowed = " .";
        public static string Mower = "M";
        public static string None = "  ";
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
        public static Direction East = new Direction(1, 1, 0, '>');
        public static Direction South = new Direction(2, 0, 1, 'v');
        public static Direction West = new Direction(3, -1, 0, '<');

        public static Direction[] Dirs =
        {
            North, South, East, West
        };

        public static Direction GetDirectionAfterTurnLeft90Degrees(Direction direction)
        {
            var newIndex = direction.Index > 0 ? direction.Index - 1 : Dirs.Length - 1;
            var newDirection = Dirs.Single(d => d.Index == newIndex);
            return newDirection;
        }

        public static Direction GetDirectionAfterTurnRight90Degrees(Direction direction)
        {
            var newIndex = direction.Index < Dirs.Length - 1 ? direction.Index + 1 : 0;
            var newDirection = Dirs.Single(d => d.Index == newIndex);
            return newDirection;
        }
    }

    public class Mower
    {
        public Location Location { get; private set; }
        public Direction Direction { get; private set; }
        public int StepCount { get; private set; }

        public Mower(Location location, Direction direction)
        {
            Location = location;
            Direction = direction;
        }

        public void TurnLeft()
        {
            Direction = Directions.GetDirectionAfterTurnLeft90Degrees(Direction);
            StepCount++;
        }

        public void Mow(Field field)
        {
            var newLocation = Direction.MoveFrom(Location);
            var tuple = field.FixLocation(newLocation);
            if (!tuple.Item2) return;
            Location = tuple.Item1;
            StepCount++;
            field[Location] = StepCount.ToString().PadLeft(2);
        }

        public void Jump(Field field, int forward, int right)
        {
            var newLocation = Direction.MoveFrom(Location, forward);
            var rightDirection = Directions.GetDirectionAfterTurnRight90Degrees(Direction);
            newLocation = rightDirection.MoveFrom(newLocation, right);
            var tuple = field.FixLocation(newLocation);
            var valid = tuple.Item2;
            if (!valid) return;
            Location = tuple.Item1;
            StepCount += 1;
            field[Location] = StepCount.ToString().PadLeft(2);
        }
    }

    public abstract class Field
    {
        private readonly string[,] _field;

        public int Width => _field.GetLength(0);

        public int Height => _field.GetLength(1);

        protected Field(int width, int height, string initialContent)
        {
            _field = new string[width, height];

            for (var x = 0; x < width; x++)
            for (var y = 0; y < height; y++)
                _field[x, y] = initialContent;
        }

        public string this[int x, int y]
        {
            get => _field[x, y];
            set => _field[x, y] = value;
        }

        public string this[Location location]
        {
            get => _field[location.X, location.Y];
            set => _field[location.X, location.Y] = value;
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

        public void Display(Mower mower)
        {
            for (var y = 0; y < Height; y++)
            {
                for (var x = 0; x < Width; x++)
                    if (x == mower.Location.X && y == mower.Location.Y)
                        Console.Write("{0}{1} ", FieldContents.Mower, mower.Direction.Symbol);
                    else
                        Console.Write("{0} ", _field[x, y]);

                Console.WriteLine();
            }
        }

        public abstract Tuple<Location, bool> FixLocation(Location location);
    }

    public class ValidatingField : Field
    {
        public ValidatingField(int width, int height, string initialContent) : base(width, height, initialContent)
        {
        }

        public override Tuple<Location, bool> FixLocation(Location location)
        {
            return 0 <= location.X && location.X < Width && 0 <= location.Y && location.Y < Height
                ? new Tuple<Location, bool>(location, true)
                : new Tuple<Location, bool>(null, false);
        }
    }

    public class ToroidField : Field
    {
        public ToroidField(int width, int height, string initialContent) : base(width, height, initialContent)
        {
        }

        public override Tuple<Location, bool> FixLocation(Location location)
        {
            var x = Mod(location.X, Width);
            var y = Mod(location.Y, Height);
            var newLocation = new Location(x, y);
            return new Tuple<Location, bool>(newLocation, true);
        }

        public static int Mod(int x, int m)
        {
            return (x % m + m) % m;
        }
    }
}