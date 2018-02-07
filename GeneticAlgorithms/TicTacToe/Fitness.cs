/* File: Fitness.cs
 *     from chapter 18 of _Genetic Algorithms with Python_
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

namespace GeneticAlgorithms.TicTacToe
{
    public class Fitness : IComparable, IComparable<Fitness>
    {
        public int Wins { get; }
        public int Ties { get; }
        public int Losses { get; }
        public double PercentWins => 100.0 * Wins / (Wins + Ties + Losses);
        public double PercentLosses => 100.0 * Losses / (Wins + Ties + Losses);
        public double PercentTies => 100.0 * Ties / (Wins + Ties + Losses);
        public int GeneCount { get; }

        public Fitness(int wins, int ties, int losses, int geneCount)
        {
            Wins = wins;
            Ties = ties;
            Losses = losses;
            GeneCount = geneCount;
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

            if (Math.Abs(PercentLosses - that.PercentLosses) > 0.1)
                return -PercentLosses.CompareTo(that.PercentLosses);

            if (Losses > 0)
                return 0;

            if (Ties != that.Ties)
                return -Ties.CompareTo(that.Ties);

            return -GeneCount.CompareTo(that.GeneCount);
        }

        public override string ToString()
        {
            return $"{PercentLosses:F1}% Losses ({Losses}), {PercentTies:F1}% Ties ({Ties}), {PercentWins:F1}% Wins ({Wins}), {GeneCount} rules";
        }
    }
}