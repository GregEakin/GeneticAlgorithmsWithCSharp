/* File: Chromosome.cs
 *     from chapter 6 of _Genetic Algorithms with Python_
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

namespace GeneticAlgorithms.Knights
{
    public class Chromosome<TGene, TFitness>
    {
        public IReadOnlyList<TGene> Genes { get; }

        public TFitness Fitness { get; }

        public Chromosome(IReadOnlyList<TGene> genes, TFitness fitness)
        {
            Genes = genes;
            Fitness = fitness;
        }
    }
}