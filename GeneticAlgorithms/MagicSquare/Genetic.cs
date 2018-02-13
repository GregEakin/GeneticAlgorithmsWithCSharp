/* File: Genetic.cs
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

using GeneticAlgorithms.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;

namespace GeneticAlgorithms.MagicSquare
{
    public static class Genetic<TGene, TFitness>
        where TFitness : IComparable<TFitness>
    {
        public delegate void MutateGeneDelegate(List<TGene> genes);

        public delegate void DisplayDelegate(Chromosome<TGene, TFitness> child);

        public delegate TFitness GetFitnessDelegate(List<TGene> gene);

        public delegate List<TGene> CreateDelegate();

        public delegate Chromosome<TGene, TFitness> GenerateParentDelegate();

        public delegate Chromosome<TGene, TFitness> MutateChromosomeDelegate(Chromosome<TGene, TFitness> parent);

        private static Chromosome<TGene, TFitness> GenerateParent(int length, TGene[] geneSet, GetFitnessDelegate getFitness)
        {
            var genes = Rand.RandomSample(geneSet, length);
            var fitness = getFitness(genes);
            var chromosome = new Chromosome<TGene, TFitness>(genes, fitness);
            return chromosome;
        }

        private static Chromosome<TGene, TFitness> Mutate(Chromosome<TGene, TFitness> parent, TGene[] geneSet,
            GetFitnessDelegate getFitness)
        {
            var childGenes = parent.Genes.ToList();
            var index = Rand.Random.Next(childGenes.Count);
            var randomSample = Rand.RandomSample(geneSet, 2);
            var newGene = randomSample[0];
            var alternate = randomSample[1];
            childGenes[index] = newGene.Equals(childGenes[index]) ? alternate : newGene;
            var fitness = getFitness(childGenes);
            return new Chromosome<TGene, TFitness>(childGenes, fitness);
        }

        private static Chromosome<TGene, TFitness> MutateCustom(Chromosome<TGene, TFitness> parent,
            MutateGeneDelegate customMutate, GetFitnessDelegate getFitness)
        {
            var childGenes = parent.Genes.ToList();
            customMutate(childGenes);
            var fitness = getFitness(childGenes);
            return new Chromosome<TGene, TFitness>(childGenes, fitness);
        }

        public static Chromosome<TGene, TFitness> GetBest(GetFitnessDelegate getFitness, int targetLen, TFitness optimalFitness,
            TGene[] geneSet, DisplayDelegate display, MutateGeneDelegate customMutate = null, CreateDelegate customCreate = null,
            int? maxAge = null)
        {
            Chromosome<TGene, TFitness> FnMutate(Chromosome<TGene, TFitness> parent) => 
                customMutate == null
                    ? Mutate(parent, geneSet, getFitness)
                    : MutateCustom(parent, customMutate, getFitness);

            Chromosome<TGene, TFitness> FnGenerateParent()
            {
                if (customCreate == null)
                    return GenerateParent(targetLen, geneSet, getFitness);

                var genes = customCreate();
                return new Chromosome<TGene, TFitness>(genes, getFitness(genes));
            }

            foreach (var improvement in GetImprovement(FnMutate, FnGenerateParent, maxAge))
            {
                display(improvement);
                if (optimalFitness.CompareTo(improvement.Fitness) <= 0)
                    return improvement;
            }

            throw new UnauthorizedAccessException();
        }

        private static IEnumerable<Chromosome<TGene, TFitness>> GetImprovement(MutateChromosomeDelegate newChild,
            GenerateParentDelegate generateParent, int? maxAge = null)
        {
            var bestParent = generateParent();
            var parent = bestParent;
            yield return bestParent;

            var historicalFitnesses = new List<TFitness> { bestParent.Fitness };
            while (true)
            {
                var child = newChild(parent);
                if (parent.Fitness.CompareTo(child.Fitness) > 0)
                {
                    if (maxAge == null)
                        continue;

                    parent.Age++;
                    if (parent.Age < maxAge)
                        continue;

                    var index = historicalFitnesses.BinarySearch(child.Fitness);
                    if (index < 0) index = ~index;
                    var difference = historicalFitnesses.Count - index;
                    var proportionSimilar = (double)difference / historicalFitnesses.Count;
                    var exp = Math.Exp(-proportionSimilar);
                    if (Rand.Random.NextDouble() < exp)
                    {
                        parent = child;
                        continue;
                    }

                    bestParent.Age = 0;
                    parent = bestParent;
                    continue;
                }

                if (parent.Fitness.CompareTo(child.Fitness) == 0)
                {
                    child.Age = parent.Age + 1;
                    parent = child;
                    continue;
                }

                child.Age = 0;
                parent = child;
                if (bestParent.Fitness.CompareTo(child.Fitness) > 0)
                    continue;

                bestParent = child;
                historicalFitnesses.Add(bestParent.Fitness);
                yield return bestParent;
            }

            // ReSharper disable once IteratorNeverReturns
        }
    }
}