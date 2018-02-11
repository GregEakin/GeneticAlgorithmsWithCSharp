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

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace GeneticAlgorithms.Utilities
{
    public static class Rand
    {
        public static Random Random { get; } = new Random();

        public static bool PercentChance(int value)
        {
            if (value <= 0)
                return false;

            if (value >= 100)
                return true;

            return Rand.Random.NextDouble() < value / 100.0;
        }

        public static List<TGene> RandomSampleList<TGene>(TGene[] geneSet, int length)
        {
            var genes = new List<TGene>(length);
            while (genes.Count < length)
            {
                var sampleSize = Math.Min(geneSet.Length, length - genes.Count);
                var array = geneSet.OrderBy(x => Random.Next()).Take(sampleSize);
                genes.AddRange(array);
            }

            return genes;
        }

        public static TGene[] RandomSampleArray<TGene>(TGene[] geneSet, int length)
        {
            var genes = new List<TGene>(length);
            while (genes.Count < length)
            {
                var sampleSize = Math.Min(geneSet.Length, length - genes.Count);
                var array = geneSet.OrderBy(x => Random.Next()).Take(sampleSize);
                genes.AddRange(array);
            }

            return genes.ToArray();
        }

        public static string RandomSampleString(char[] input, int length)
        {
            var result = string.Empty;
            while (result.Length < length)
            {
                var sampleSize = Math.Min(input.Length, length - result.Length);
                var array = input.OrderBy(x => Random.Next()).Take(sampleSize);
                result += new string(array.ToArray());
            }

            return result;
        }
    }
}