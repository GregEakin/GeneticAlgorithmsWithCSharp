/* File: genetic.cs
 *     from chapter 1 of _Genetic Algorithms with Python_
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

using GeneticAlgorithms.Utilities;
using System;

namespace GeneticAlgorithms.Password
{
    public static class Genetic
    {
        public delegate void DisplayDelegate(Chromosome child);

        public delegate int GetFitnessDelegate(string guess);

        private static Chromosome GenerateParent(char[] geneSet, int length, GetFitnessDelegate getFitness)
        {
            var genes = string.Empty;
            while (genes.Length < length)
            {
                var sampleSize = Math.Min(length - genes.Length, geneSet.Length);
                genes += Rand.RandomSample(geneSet, sampleSize);
            }

            var fitness = getFitness(genes);
            return new Chromosome(genes, fitness);
        }

        private static Chromosome Mutate(Chromosome parent, char[] geneSet, GetFitnessDelegate getFitness)
        {
            var childGenes = parent.Genes.ToCharArray();
            var index = Rand.Random.Next(parent.Genes.Length);
            var randomSample = Rand.RandomSample(geneSet, 2);
            var newGene = randomSample[0];
            var alternate = randomSample[1];
            childGenes[index] = newGene == childGenes[index] ? alternate : newGene;
            var genes = new string(childGenes);
            var fitness = getFitness(genes);
            return new Chromosome(genes, fitness);
        }

        public static Chromosome GetBest(GetFitnessDelegate getFitness, int targetLen, int optimalFitness, char[] geneSet,
            DisplayDelegate display)
        {
            var bestParent = GenerateParent(geneSet, targetLen, getFitness);
            display(bestParent);

            if (bestParent.Fitness >= optimalFitness)
                return bestParent;

            while (true)
            {
                var child = Mutate(bestParent, geneSet, getFitness);
                if (bestParent.Fitness >= child.Fitness)
                    continue;
                display(child);
                if (child.Fitness >= optimalFitness)
                    return child;
                bestParent = child;
            }
        }
    }
}