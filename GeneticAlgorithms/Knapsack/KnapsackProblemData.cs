﻿/* File: KnapsackProblemData.cs
 *     from chapter 9 of _Genetic Algorithms with Python_
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

namespace GeneticAlgorithms.Knapsack;

public class KnapsackProblemData
{
    public Resource[] Resources { get; }

    public int MaxWeight { get; }

    public ItemQuantity[] Solution { get; }

    public KnapsackProblemData(Resource[] resources, int maxWeight, ItemQuantity[] solution)
    {
        Resources = resources;
        MaxWeight = maxWeight;
        Solution = solution;
    }
}