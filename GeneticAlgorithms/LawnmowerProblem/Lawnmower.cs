/* File: Lawnmower.cs
 *     from chapter 15 of _Genetic Algorithms with Python_
 *     written by Clinton Sheppard
 *
 * Author: Greg Eakin <gregory.eakin@gmail.com>
 * Copyright (c) 2018 Greg Eakin
 * 
 * Licensed under the Apache License, Version 2.0 (the "License").
 * You may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 *   http://www.apache.org/licenses/LICENSE-2.0
 * 
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or
 * implied.  See the License for the specific language governing
 * permissions and limitations under the License.
 */

namespace GeneticAlgorithms.LawnmowerProblem;

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

public static class Directions
{
    public static readonly Direction North = new(0, 0, -1, '^');
    public static readonly Direction East = new(1, 1, 0, '>');
    public static readonly Direction South = new(2, 0, 1, 'v');
    public static readonly Direction West = new(3, -1, 0, '<');

    private static readonly Direction[] Dirs =
    [
        North, East, South, West
    ];

    public static Direction GetDirectionAfterTurnLeft90Degrees(Direction direction)
    {
        var newIndex = direction.Index > 0 ? direction.Index - 1 : Dirs.Length - 1;
        var newDirection = Dirs[newIndex];
        return newDirection;
    }

    public static Direction GetDirectionAfterTurnRight90Degrees(Direction direction)
    {
        var newIndex = direction.Index < Dirs.Length - 1 ? direction.Index + 1 : 0;
        var newDirection = Dirs[newIndex];
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
        if (!tuple.Item2) return;
        Location = tuple.Item1;
        StepCount++;
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
        //var sum = Enumerable.Range(0, Height)
        //    .SelectMany(row => Enumerable.Range(0, Width), (row, colum) => new {row, colum})
        //    .Count(t => _field[t.row, t.colum] != FieldContents.Grass);

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

    public abstract (Location, bool) FixLocation(Location location);
}

public class ValidatingField : Field
{
    public ValidatingField(int width, int height, string initialContent)
        : base(width, height, initialContent)
    {
    }

    public override (Location, bool) FixLocation(Location location)
    {
        return 0 <= location.X && location.X < Width &&
               0 <= location.Y && location.Y < Height
            ? new (location, true)
            : new (null, false);
    }
}

public class ToroidField : Field
{
    public ToroidField(int width, int height, string initialContent)
        : base(width, height, initialContent)
    {
    }

    public override (Location, bool) FixLocation(Location location)
    {
        var x = Mod(location.X, Width);
        var y = Mod(location.Y, Height);
        var newLocation = new Location(x, y);
        return new (newLocation, true);
    }

    public static int Mod(int x, int m)
    {
        return (x % m + m) % m;
    }
}