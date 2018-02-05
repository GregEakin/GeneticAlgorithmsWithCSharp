/* File: Benchmark.cs
 *     from chapter 8 of _Genetic Algorithms with Python_
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

using System;

namespace GeneticAlgorithms.MagicSquare
{
    public class Fitness : IComparable, IComparable<Fitness>
    {
        private int SumOfDifferences { get; }

        public Fitness(int sumOfDifferences)
        {
            SumOfDifferences = sumOfDifferences;
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
            return -SumOfDifferences.CompareTo(that.SumOfDifferences);
        }

        public override string ToString()
        {
            return SumOfDifferences.ToString();
        }
    }
}