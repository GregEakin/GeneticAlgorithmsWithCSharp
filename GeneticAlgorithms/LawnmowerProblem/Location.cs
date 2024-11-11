/* File: Location.cs
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