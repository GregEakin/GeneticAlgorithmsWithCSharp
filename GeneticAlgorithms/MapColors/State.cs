/* File: Chromosome.cs
 *     from chapter 5 of _Genetic Algorithms with Python_
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
using System.Collections.Generic;

namespace GeneticAlgorithms.MapColors
{
    public class State : IComparable, IComparable<State>
    {
        public string Name { get; }
        public int Index { get; }
        public ISet<State> Neighbors { get; } = new SortedSet<State>();

        public State(string name, int index)
        {
            Name = name;
            Index = index;
        }

        public char Color(char[] genes) => genes[Index];

        public int CompareTo(object obj)
        {
            switch (obj)
            {
                case null:
                    return 1;
                case State that:
                    return CompareTo(that);
                default:
                    throw new ArgumentException("Object is not a State");
            }
        }

        public int CompareTo(State that)
        {
            if (ReferenceEquals(this, that)) return 0;
            if (that is null) return 1;
            return string.Compare(Name, that.Name, StringComparison.Ordinal);
        }

        public override int GetHashCode() => Name.GetHashCode() * 10 + Index;

        public override bool Equals(object obj) => obj is State that && CompareTo(that) == 0;

        public override string ToString() => Name;
    }
}