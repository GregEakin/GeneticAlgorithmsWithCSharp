/* File: Pair.cs
 *     from chapter 12 of _Genetic Algorithms with Python_
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

namespace GeneticAlgorithms.TravelingSalesmanProblem
{
    public class Pair
    {
        public int Node { get; }

        public int Adjacent { get; }

        public Pair(int node, int adjacent)
        {
            if (node < adjacent)
            {
                var temp = node;
                node = adjacent;
                adjacent = temp;
            }

            Node = node;
            Adjacent = adjacent;
        }

        public override bool Equals(object obj)
        {
            switch (obj)
            {
                case null:
                    return false;
                case Pair that:
                    return Node == that.Node && Adjacent == that.Adjacent;
                default:
                    return false;
            }
        }

        public override int GetHashCode()
        {
            return Node.GetHashCode() * 397 ^ Adjacent.GetHashCode();
        }
    }
}