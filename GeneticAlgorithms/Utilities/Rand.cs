/* File: Random.cs
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

namespace GeneticAlgorithms.Utilities;

public static class Rand
{
    public static Random Random { get; } = new();

    public static T SelectItemNotIn<T>(IEnumerable<T> list, IReadOnlyList<T> except)
    {
        var subList = list.Where(i => !except.Contains(i)).ToList();
        return SelectItem(subList);
    }

    public static T SelectItem<T>(IReadOnlyList<T> list)
    {
        var index = Random.Next(list.Count);
        return list[index];
    }

    public static bool PercentChance(int value)
    {
        if (value <= 0)
            return false;

        if (value >= 100)
            return true;

        return Random.NextDouble() < value / 100.0;
    }

    public static List<TGene> RandomSample<TGene>(IReadOnlyList<TGene> geneSet, int length)
    {
        var genes = new List<TGene>(length);
        while (genes.Count < length)
        {
            var sampleSize = Math.Min(geneSet.Count, length - genes.Count);
            var array = geneSet.OrderBy(_ => Random.Next()).Take(sampleSize);
            genes.AddRange(array);
        }

        return genes;
    }

    public static string RandomSample(IReadOnlyList<char> input, int length)
    {
        var result = string.Empty;
        while (result.Length < length)
        {
            var sampleSize = Math.Min(input.Count, length - result.Length);
            var array = input.OrderBy(_ => Random.Next()).Take(sampleSize);
            result += new string(array.ToArray());
        }

        return result;
    }
}