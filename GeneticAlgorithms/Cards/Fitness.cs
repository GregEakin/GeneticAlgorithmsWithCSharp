/* File: Benchmark.cs
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

namespace GeneticAlgorithms.Cards;

public class Fitness : IComparable, IComparable<Fitness>
{
    public int Group1Sum { get; }
    public int Group2Product { get; }
    public int TotalDifference { get; }
    public int DuplicateCount { get; }

    public Fitness(int group1Sum, int group2Product, int duplicateCount)
    {
        Group1Sum = group1Sum;
        Group2Product = group2Product;
        DuplicateCount = duplicateCount;

        var sumDifference = Math.Abs(36 - group1Sum);
        var productDifference = Math.Abs(360 - group2Product);
        TotalDifference = sumDifference + productDifference;
    }

    public int CompareTo(object obj)
    {
        switch (obj)
        {
            case null:
                return 1;
            case Fitness that:
                return CompareTo(that);
            default:
                throw new ArgumentException("Object is not a Fitness");
        }
    }

    public int CompareTo(Fitness that)
    {
        if (ReferenceEquals(this, that)) return 0;
        if (that is null) return 1;
        var duplicateComparison = -DuplicateCount.CompareTo(that.DuplicateCount);
        if (duplicateComparison != 0) return duplicateComparison;
        return -TotalDifference.CompareTo(that.TotalDifference);
    }

    public override string ToString() => $"sum: {Group1Sum} prod: {Group2Product} dups: {DuplicateCount}";
}