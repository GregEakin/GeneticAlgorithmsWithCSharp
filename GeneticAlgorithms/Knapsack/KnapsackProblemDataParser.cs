/* File: KnapsackProblemDataParser.cs
 *     from chapter 9 of _Genetic Algorithms with Python_
 *     writen by Clinton Sheppard
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

using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace GeneticAlgorithms.Knapsack
{
    public class KnapsackProblemDataParser
    {
        public List<Resource> Resources { get; } = new List<Resource>();

        public int MaxWeight { get; private set; }

        public List<ItemQuantity> Solution { get; } = new List<ItemQuantity>();

        public delegate FindMethod FindMethod(string line, KnapsackProblemDataParser data);

        public static KnapsackProblemData LoadData(string filename)
        {
            var lines = File.ReadAllLines(filename);
            var data = new KnapsackProblemDataParser();
            FindMethod f = FindConstraint;
            foreach (var line in lines)
            {
                f = f(line.Trim(), data);
                if (f == null)
                    break;
            }

            return new KnapsackProblemData(data.Resources.ToArray(), data.MaxWeight, data.Solution.ToArray());
        }

        public static FindMethod FindConstraint(string line, KnapsackProblemDataParser data)
        {
            var parts = line.Split(' ');
            if (parts[0] != "c:")
                return FindConstraint;

            data.MaxWeight = int.Parse(parts[1]);
            return FindDataStart;
        }

        public static FindMethod FindDataStart(string line, KnapsackProblemDataParser data)
        {
            if (line != "begin data")
                return FindDataStart;

            return ReadResourceOrFindDataEnd;
        }

        public static FindMethod ReadResourceOrFindDataEnd(string line, KnapsackProblemDataParser data)
        {
            if (line == "end data")
                return FindSolutionStart;
            var parts = line.Split('\t');
            var resouce = new Resource("R" + (data.Resources.Count + 1), int.Parse(parts[1]), int.Parse(parts[0]), 0);
            data.Resources.Add(resouce);
            return ReadResourceOrFindDataEnd;
        }

        public static FindMethod FindSolutionStart(string line, KnapsackProblemDataParser data)
        {
            if (line == "sol:")
                return ReadSolutionResourceOrFindSolutionEnd;
            return FindSolutionStart;
        }

        public static FindMethod ReadSolutionResourceOrFindSolutionEnd(string line, KnapsackProblemDataParser data)
        {
            if (string.IsNullOrWhiteSpace(line))
                return null;

            var parts = line.Split('\t').Where(s => !string.IsNullOrWhiteSpace(s)).Select(p => p).ToArray();
            var resourceIndex = int.Parse(parts[0]) - 1;
            var resourceQuantity = int.Parse(parts[1]);
            var item = new ItemQuantity(data.Resources[resourceIndex], resourceQuantity);
            data.Solution.Add(item);
            return ReadSolutionResourceOrFindSolutionEnd;
        }
    }
}