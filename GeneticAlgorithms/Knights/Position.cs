/* File: Position.cs
 *     from chapter 6 of _Genetic Algorithms with Python_
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

namespace GeneticAlgorithms.Knights;

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