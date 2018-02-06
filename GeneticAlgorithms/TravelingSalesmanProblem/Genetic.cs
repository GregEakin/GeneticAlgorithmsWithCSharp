/* File: Genetic.cs
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

using System;
using System.Collections.Generic;
using System.Linq;

namespace GeneticAlgorithms.TravelingSalesmanProblem
{
    public class Genetic<TGene, TFitness>
        where TFitness : IComparable<TFitness>
    {
        private class ReverseComparer<T> : IComparer<T>
        {
            public int Compare(T x, T y)
            {
                return Comparer<T>.Default.Compare(y, x);
            }
        }

        public delegate TFitness GetFitnessDelegate(TGene[] gene);

        public delegate void DisplayDelegate(Chromosome<TGene, TFitness> child);

        public delegate void MutateGeneDelegate(TGene[] genes);

        public delegate Chromosome<TGene, TFitness> MutateChromosomeDelegate(Chromosome<TGene, TFitness> parent);

        public delegate Chromosome<TGene, TFitness> GenerateParentDelegate();

        public delegate TGene[] CreateDelegate();

        public delegate TGene[] CrossoverFun(TGene[] genes1, TGene[] genes2);

        private readonly Random _random = new Random();

        private TGene[] RandomSample(TGene[] geneSet, int length)
        {
            var genes = new List<TGene>(length);
            while (genes.Count < length)
            {
                var sampleSize = Math.Min(geneSet.Length, length - genes.Count);
                var array = geneSet.OrderBy(x => _random.Next()).Take(sampleSize);
                genes.AddRange(array);
            }

            return genes.ToArray();
        }

        private Chromosome<TGene, TFitness> GenerateParent(int length, TGene[] geneSet,
            GetFitnessDelegate getGetFitness)
        {
            var genes = RandomSample(geneSet, length);
            var fitness = getGetFitness(genes);
            var chromosome =
                new Chromosome<TGene, TFitness>(genes, fitness, Chromosome<TGene, TFitness>.Strategies.Create);
            return chromosome;
        }

        private Chromosome<TGene, TFitness> Mutate(Chromosome<TGene, TFitness> parent, TGene[] geneSet,
            GetFitnessDelegate getFitness)
        {
            var childGenes = parent.Genes.ToArray();
            var index = _random.Next(childGenes.Length);
            var randomSample = RandomSample(geneSet, 2);
            var newGene = randomSample[0];
            var alternate = randomSample[1];
            childGenes[index] = newGene.Equals(childGenes[index]) ? alternate : newGene;
            var fitness = getFitness(childGenes);
            return new Chromosome<TGene, TFitness>(childGenes, fitness, Chromosome<TGene, TFitness>.Strategies.Mutate);
        }

        private Chromosome<TGene, TFitness> MutateCustom(Chromosome<TGene, TFitness> parent,
            MutateGeneDelegate customMutate, GetFitnessDelegate getFitness)
        {
            var childGenes = parent.Genes.ToArray();
            customMutate(childGenes);
            var fitness = getFitness(childGenes);
            return new Chromosome<TGene, TFitness>(childGenes, fitness, Chromosome<TGene, TFitness>.Strategies.Mutate);
        }

        private Chromosome<TGene, TFitness> Crossover(TGene[] parentGenes, int index,
            List<Chromosome<TGene, TFitness>> parents,
            GetFitnessDelegate getFitness, CrossoverFun crossover, MutateChromosomeDelegate mutate,
            GenerateParentDelegate generateParent)
        {
            var donorIndex = _random.Next(0, parents.Count);
            if (donorIndex == index)
                donorIndex = (donorIndex + 1) % parents.Count;
            var childGenes = crossover(parentGenes, parents[donorIndex].Genes);
            if (childGenes == null)
            {
                // parent and donor are indistinguishable
                parents[donorIndex] = generateParent();
                return mutate(parents[index]);
            }

            var fitness = getFitness(parentGenes);
            return new Chromosome<TGene, TFitness>(childGenes, fitness,
                Chromosome<TGene, TFitness>.Strategies.Crossover);
        }

        internal Chromosome<TGene, TFitness> GetBest(GetFitnessDelegate getFitness, int targetLen,
            TFitness optimalFitness, TGene[] geneSet, DisplayDelegate display, MutateGeneDelegate customMutate = null,
            CreateDelegate customCreate = null,
            int maxAge = 0, int poolSize = 1, CrossoverFun crossover = null)
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
                return new Chromosome<TGene, TFitness>(genes, getFitness(genes),
                    Chromosome<TGene, TFitness>.Strategies.Create);
            }

            var strategyLookup =
                new Dictionary<Chromosome<TGene, TFitness>.Strategies, Func<Chromosome<TGene, TFitness>, int,
                    List<Chromosome<TGene, TFitness>>, Chromosome<TGene, TFitness>>>
                {
                    {Chromosome<TGene, TFitness>.Strategies.Create, (p, i, o) => FnGenerateParent()},
                    {Chromosome<TGene, TFitness>.Strategies.Mutate, (p, i, o) => FnMutate(p)},
                    {
                        Chromosome<TGene, TFitness>.Strategies.Crossover,
                        (p, i, o) => Crossover(p.Genes, i, o, getFitness, crossover, FnMutate, FnGenerateParent)
                    }
                };

            var usedStrategies =
                new List<Func<Chromosome<TGene, TFitness>, int, List<Chromosome<TGene, TFitness>>,
                    Chromosome<TGene, TFitness>>> {strategyLookup[Chromosome<TGene, TFitness>.Strategies.Mutate]};

            Chromosome<TGene, TFitness> FnNewChild(Chromosome<TGene, TFitness> parent, int index,
                List<Chromosome<TGene, TFitness>> parents)
            {
                if (crossover != null)
                {
                    usedStrategies.Add(strategyLookup[Chromosome<TGene, TFitness>.Strategies.Crossover]);
                    return usedStrategies[_random.Next(usedStrategies.Count)](parent, index, parents);
                }

                return FnMutate(parent);
            }

            foreach (var improvement in GetImprovement(FnNewChild, FnGenerateParent, maxAge, poolSize))
            {
                display(improvement);
                var f = strategyLookup[improvement.Strategy];
                usedStrategies.Add(f);
                if (optimalFitness.CompareTo(improvement.Fitness) <= 0)
                    return improvement;
            }

            throw new UnauthorizedAccessException();
        }

        private IEnumerable<Chromosome<TGene, TFitness>> GetImprovement(
            Func<Chromosome<TGene, TFitness>, int, List<Chromosome<TGene, TFitness>>, Chromosome<TGene, TFitness>>
                newChild, GenerateParentDelegate generateParent, int maxAge, int poolSize)
        {
            var bestParent = generateParent();
            yield return bestParent;
            var parents = new List<Chromosome<TGene, TFitness>> {bestParent};
            var historicalFitnesses = new List<TFitness> {bestParent.Fitness};
            for (var i = 0; i < poolSize - 1; i++)
            {
                var parent = generateParent();
                if (parent.Fitness.CompareTo(bestParent.Fitness) > 0)
                {
                    yield return parent;
                    bestParent = parent;
                    historicalFitnesses.Add(parent.Fitness);
                }

                parents.Add(parent);
            }

            var lastParentIndex = poolSize - 1;
            var pIndex = 1;
            while (true)
            {
                pIndex = pIndex > 0 ? pIndex - 1 : lastParentIndex;
                var parent = parents[pIndex];
                var child = newChild(parent, pIndex, parents);
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
                    var proportionSimilar = (double) difference / historicalFitnesses.Count;
                    var exp = Math.Exp(-proportionSimilar);
                    if (_random.NextDouble() < exp)
                    {
                        parents[pIndex] = child;
                        continue;
                    }

                    parents[pIndex] = bestParent;
                    parent.Age = 0;
                    continue;
                }

                if (parent.Fitness.CompareTo(child.Fitness) >= 0)
                {
                    child.Age = parent.Age + 1;
                    parents[pIndex] = child;
                    continue;
                }

                parents[pIndex] = child;
                parent.Age = 0;
                if (child.Fitness.CompareTo(bestParent.Fitness) <= 0)
                    continue;

                yield return child;
                bestParent = child;
                historicalFitnesses.Add(child.Fitness);
            }

            // ReSharper disable once IteratorNeverReturns
        }
    }
}