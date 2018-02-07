/* File: Benchmark.cs
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

using GeneticAlgorithms.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;

namespace GeneticAlgorithms.Knights
{
    public class Genetic<TGene, TFitness>
        where TFitness : IComparable<TFitness>
    {
        public delegate TFitness FitnessDelegate(TGene[] gene);

        public delegate void DisplayDelegate(Chromosome<TGene, TFitness> child);

        public delegate void MutateGeneDelegate(TGene[] genes);

        public delegate Chromosome<TGene, TFitness> MutateChromosomeDelegate(Chromosome<TGene, TFitness> parent);

        public delegate Chromosome<TGene, TFitness> GenerateParentDelegate();

        public delegate TGene[] CreateDelegate();

        private Chromosome<TGene, TFitness> GenerateParent(int length, TGene[] geneSet, FitnessDelegate getFitness)
        {
            var genes = Rand.RandomSampleArray(geneSet, length);
            var fitness = getFitness(genes);
            var chromosome = new Chromosome<TGene, TFitness>(genes, fitness);
            return chromosome;
        }

        private Chromosome<TGene, TFitness> Mutate(Chromosome<TGene, TFitness> parent, TGene[] geneSet,
            FitnessDelegate getFitness)
        {
            var childGenes = parent.Genes.ToArray();
            var index = Rand.Random.Next(childGenes.Length);
            var randomSample = Rand.RandomSampleArray(geneSet, 2);
            var newGene = randomSample[0];
            var alternate = randomSample[1];
            childGenes[index] = newGene.Equals(childGenes[index]) ? alternate : newGene;
            var fitness = getFitness(childGenes);
            return new Chromosome<TGene, TFitness>(childGenes, fitness);
        }

        private static Chromosome<TGene, TFitness> MutateCustom(Chromosome<TGene, TFitness> parent,
            MutateGeneDelegate customMutate, FitnessDelegate getFitness)
        {
            var childGenes = parent.Genes.ToArray();
            customMutate(childGenes);
            var fitness = getFitness(childGenes);
            return new Chromosome<TGene, TFitness>(childGenes, fitness);
        }

        internal Chromosome<TGene, TFitness> GetBest(FitnessDelegate getFitness, int targetLen, TFitness optimalFitness,
            TGene[] geneSet, DisplayDelegate display, MutateGeneDelegate customMutate = null, CreateDelegate customCreate = null)
        {
            Chromosome<TGene, TFitness> FnMutate(Chromosome<TGene, TFitness> parent) => customMutate == null
                ? Mutate(parent, geneSet, getFitness)
                : MutateCustom(parent, customMutate, getFitness);

            Chromosome<TGene, TFitness> FnGenerateParent()
            {
                if (customCreate == null)
                    return GenerateParent(targetLen, geneSet, getFitness);

                var genes = customCreate();
                return new Chromosome<TGene, TFitness>(genes, getFitness(genes));
            }

            foreach (var improvement in GetImprovement(FnMutate, FnGenerateParent))
            {
                display(improvement);
                if (optimalFitness.CompareTo(improvement.Fitness) <= 0)
                    return improvement;
            }

            throw new UnauthorizedAccessException();
        }

        public IEnumerable<Chromosome<TGene, TFitness>> GetImprovement(MutateChromosomeDelegate newChild,
            GenerateParentDelegate generateParent)
        {
            var bestParent = generateParent();
            yield return bestParent;

            while (true)
            {
                var child = newChild(bestParent);
                if (bestParent.Fitness.CompareTo(child.Fitness) > 0)
                    continue;

                bestParent = child;
                if (bestParent.Fitness.CompareTo(child.Fitness) <= 0)
                    yield return child;
            }

            // ReSharper disable once IteratorNeverReturns
        }
    }
}