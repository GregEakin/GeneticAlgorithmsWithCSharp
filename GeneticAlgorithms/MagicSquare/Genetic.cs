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

using System;
using System.Collections.Generic;
using System.Linq;

namespace GeneticAlgorithms.MagicSquare
{
    public class Genetic<TGene, TFitness>
        where TFitness : IComparable<TFitness>
    {
        public class ReverseComparer<T> : IComparer<T>
        {
            public int Compare(T x, T y)
            {
                return Comparer<T>.Default.Compare(y, x);
            }
        }

        public delegate TFitness FitnessDelegate(List<TGene> gene);

        public delegate void DisplayDelegate(Chromosome<TGene, TFitness> child);

        public delegate void MutateGeneDelegate(List<TGene> genes);

        public delegate Chromosome<TGene, TFitness> MutateChromosomeDelegate(Chromosome<TGene, TFitness> parent);

        public delegate Chromosome<TGene, TFitness> GenerateParentDelegate();

        public delegate List<TGene> CreateDelegate();

        private readonly Random _random = new Random();

        public List<TGene> RandomSample(TGene[] geneSet, int length)
        {
            var genes = new List<TGene>(length);
            while (genes.Count < length)
            {
                var sampleSize = Math.Min(geneSet.Length, length - genes.Count);
                var array = geneSet.OrderBy(x => _random.Next()).Take(sampleSize);
                genes.AddRange(array);
            }

            return genes.ToList();
        }
        private Chromosome<TGene, TFitness> GenerateParent(int length, TGene[] geneSet, FitnessDelegate getFitness)
        {
            var genes = RandomSample(geneSet, length);
            var fitness = getFitness(genes);
            var chromosome = new Chromosome<TGene, TFitness>(genes, fitness);
            return chromosome;
        }

        private Chromosome<TGene, TFitness> Mutate(Chromosome<TGene, TFitness> parent, TGene[] geneSet,
            FitnessDelegate getFitness)
        {
            var childGenes = parent.Genes.ToList();
            var index = _random.Next(childGenes.Count);
            var randomSample = RandomSample(geneSet, 2);
            var newGene = randomSample[0];
            var alternate = randomSample[1];
            childGenes[index] = newGene.Equals(childGenes[index]) ? alternate : newGene;
            var fitness = getFitness(childGenes);
            return new Chromosome<TGene, TFitness>(childGenes, fitness);
        }

        private static Chromosome<TGene, TFitness> MutateCustom(Chromosome<TGene, TFitness> parent,
            MutateGeneDelegate customMutate, FitnessDelegate getFitness)
        {
            var childGenes = parent.Genes.ToList();
            customMutate(childGenes);
            var fitness = getFitness(childGenes);
            return new Chromosome<TGene, TFitness>(childGenes, fitness);
        }

        public Chromosome<TGene, TFitness> GetBest(FitnessDelegate getFitness, int targetLen, TFitness optimalFitness,
            TGene[] geneSet, DisplayDelegate display, MutateGeneDelegate customMutate = null, CreateDelegate customCreate = null,
            int maxAge = 0)
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

            foreach (var improvement in GetImprovement(FnMutate, FnGenerateParent, maxAge))
            {
                display(improvement);
                if (optimalFitness.CompareTo(improvement.Fitness) <= 0)
                    return improvement;
            }

            throw new UnauthorizedAccessException();
        }

        public IEnumerable<Chromosome<TGene, TFitness>> GetImprovement(MutateChromosomeDelegate newChild,
            GenerateParentDelegate generateParent, int maxAge = 0)
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
                    if (maxAge <= 0)
                        continue;

                    parent.Age++;
                    if (maxAge > parent.Age)
                        continue;

                    var index = historicalFitnesses.BinarySearch(child.Fitness, new ReverseComparer<TFitness>());
                    if (index < 0) index = ~index;
                    var difference = historicalFitnesses.Count - index;
                    var proportionSimilar = (double)difference / historicalFitnesses.Count;
                    var exp = Math.Exp(-proportionSimilar);
                    if (_random.NextDouble() < exp)
                    {
                        parent = child;
                        continue;
                    }

                    parent = bestParent;
                    parent.Age = 0;
                    continue;
                }

                if (parent.Fitness.CompareTo(child.Fitness) >= 0)
                {
                    child.Age = parent.Age + 1;
                    parent = child;
                    continue;
                }

                parent = child;
                parent.Age = 0;
                if (child.Fitness.CompareTo(bestParent.Fitness) <= 0)
                    continue;

                bestParent = child;
                historicalFitnesses.Add(child.Fitness);
                yield return child;
            }

            // ReSharper disable once IteratorNeverReturns
        }
    }
}