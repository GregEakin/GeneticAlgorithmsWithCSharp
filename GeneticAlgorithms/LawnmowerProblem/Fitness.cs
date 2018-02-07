/* File: Fitnes.cs
 *     from chapter 15 of _Genetic Algorithms with Python_
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

namespace GeneticAlgorithms.LawnmowerProblem
{
    public class Fitness : IComparable, IComparable<Fitness>
    {
        public int TotalMowed { get; }
        public int TotalInstructions { get; }
        public int StepCount { get; }

        public Fitness(int totalMowed, int totalInstructions, int stepCount)
        {
            TotalMowed = totalMowed;
            TotalInstructions = totalInstructions;
            StepCount = stepCount;
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
            var totalMowedComparison = TotalMowed.CompareTo(that.TotalMowed);
            if (totalMowedComparison != 0) return totalMowedComparison;
            var stepCountComparison = -StepCount.CompareTo(that.StepCount);
            if (stepCountComparison != 0) return stepCountComparison;
            return -TotalInstructions.CompareTo(that.TotalInstructions);
        }

        public override string ToString()
        {
            return $"{TotalMowed} mowed with {TotalInstructions} instructions and {StepCount} steps";
        }
    }
}