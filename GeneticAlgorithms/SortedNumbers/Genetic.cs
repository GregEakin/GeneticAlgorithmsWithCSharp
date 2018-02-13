/* File: Genetic.cs
 *     from chapter 3 of _Genetic Algorithms with Python_
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
using System.Collections.Generic;
using System.Linq;

namespace GeneticAlgorithms.SortedNumbers
{
    public static class Genetic<TGene, TFitness>
        where TFitness : IComparable<TFitness>
    {
        public delegate void DisplayDelegate(Chromosome<TGene, TFitness> child);

        public delegate TFitness GetFitnessDelegate(IReadOnlyList<TGene> gene);

        public delegate Chromosome<TGene, TFitness> MutateChromosomeDelegate(Chromosome<TGene, TFitness> parent);

        public delegate Chromosome<TGene, TFitness> GenerateParentDelegate();

        public static Chromosome<TGene, TFitness> GenerateParent(int length, IReadOnlyList<TGene> geneSet, GetFitnessDelegate getFitness)
        {
            var genes = Rand.RandomSample(geneSet, length).ToArray();
            var fit = getFitness(genes);
            return new Chromosome<TGene, TFitness>(genes, fit);
        }

        public static Chromosome<TGene, TFitness> Mutate(Chromosome<TGene, TFitness> parent, IReadOnlyList<TGene> geneSet,
            GetFitnessDelegate getFitness)
        {
            var childGenes = parent.Genes.ToArray();
            var index = Rand.Random.Next(parent.Genes.Count);
            var randomSample = Rand.RandomSample(geneSet, 2);
            var newGene = randomSample[0];
            var alternate = randomSample[1];
            childGenes[index] = newGene.Equals(childGenes[index]) ? alternate : newGene;
            var fitness = getFitness(childGenes);
            return new Chromosome<TGene, TFitness>(childGenes, fitness);
        }

        public static Chromosome<TGene, TFitness> GetBest(GetFitnessDelegate getFitness, int targetLen,
            TFitness optimalFitness, TGene[] geneSet, DisplayDelegate display)
        {
            Chromosome<TGene, TFitness> FnMutate(Chromosome<TGene, TFitness> parent) =>
                Mutate(parent, geneSet, getFitness);

            Chromosome<TGene, TFitness> FnGenerateParent() => GenerateParent(targetLen, geneSet, getFitness);

            foreach (var improvment in GetImprovment(FnMutate, FnGenerateParent))
            {
                display(improvment);
                if (optimalFitness.CompareTo(improvment.Fitness) <= 0)
                    return improvment;
            }

            throw new Exception();
        }

        private static IEnumerable<Chromosome<TGene, TFitness>> GetImprovment(MutateChromosomeDelegate newChild,
            GenerateParentDelegate generateParent)
        {
            var bestParent = generateParent();
            yield return bestParent;
            while (true)
            {
                var child = newChild(bestParent);
                if (bestParent.Fitness.CompareTo(child.Fitness) > 0)
                    continue;
                if (child.Fitness.CompareTo(bestParent.Fitness) <= 0)
                {
                    bestParent = child;
                    continue;
                }

                yield return child;
                bestParent = child;
            }

            // ReSharper disable once IteratorNeverReturns
        }
    }
}