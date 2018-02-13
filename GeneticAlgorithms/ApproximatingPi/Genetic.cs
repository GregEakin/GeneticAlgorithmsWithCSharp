/* File: Genetic.cs
 *     from chapter 13 of _Genetic Algorithms with Python_
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
using System.Diagnostics;
using System.Linq;

namespace GeneticAlgorithms.ApproximatingPi
{
    public static partial class Genetic<TGene, TFitness>
        where TFitness : IComparable<TFitness>
    {
        public delegate void MutateGeneDelegate(List<TGene> genes);

        public delegate void DisplayDelegate(Chromosome<TGene, TFitness> child);

        public delegate TFitness GetFitnessDelegate(IReadOnlyList<TGene> gene);

        public delegate List<TGene> CreateDelegate();

        public delegate List<TGene> CrossoverDelegate(List<TGene> genes1, List<TGene> genes2);

        public delegate Chromosome<TGene, TFitness> GenerateParentDelegate();

        public delegate Chromosome<TGene, TFitness> MutateChromosomeDelegate(Chromosome<TGene, TFitness> parent);

        public delegate Chromosome<TGene, TFitness> MutateDelegate(Chromosome<TGene, TFitness> x1, int x2,
            List<Chromosome<TGene, TFitness>> x3);

        private static Chromosome<TGene, TFitness> GenerateParent(int length, IReadOnlyList<TGene> geneSet,
            GetFitnessDelegate getGetFitness)
        {
            var genes = Rand.RandomSample(geneSet, length);
            var fitness = getGetFitness(genes);
            var chromosome =
                new Chromosome<TGene, TFitness>(genes, fitness, Strategies.Create);
            return chromosome;
        }

        private static Chromosome<TGene, TFitness> Mutate(Chromosome<TGene, TFitness> parent, IReadOnlyList<TGene> geneSet,
            GetFitnessDelegate getFitness)
        {
            var childGenes = parent.Genes.ToList();
            var index = Rand.Random.Next(childGenes.Count);
            var randomSample = Rand.RandomSample(geneSet, 2);
            var newGene = randomSample[0];
            var alternate = randomSample[1];
            childGenes[index] = newGene.Equals(childGenes[index]) ? alternate : newGene;
            var fitness = getFitness(childGenes);
            return new Chromosome<TGene, TFitness>(childGenes, fitness, Strategies.Mutate);
        }

        private static Chromosome<TGene, TFitness> MutateCustom(Chromosome<TGene, TFitness> parent,
            MutateGeneDelegate customMutate, GetFitnessDelegate getFitness)
        {
            var childGenes = parent.Genes.ToList();
            customMutate(childGenes);
            var fitness = getFitness(childGenes);
            return new Chromosome<TGene, TFitness>(childGenes, fitness, Strategies.Mutate);
        }

        private static Chromosome<TGene, TFitness> Crossover(List<TGene> parentGenes, int index,
            List<Chromosome<TGene, TFitness>> parents,
            GetFitnessDelegate getFitness, CrossoverDelegate crossover, MutateChromosomeDelegate mutate,
            GenerateParentDelegate generateParent)
        {
            var donorIndex = Rand.Random.Next(0, parents.Count);
            if (donorIndex == index)
                donorIndex = (donorIndex + 1) % parents.Count;
            var childGenes = crossover(parentGenes, parents[donorIndex].Genes);
            if (childGenes == null)
            {
                // parent and donor are indistinguishable
                parents[donorIndex] = generateParent();
                return mutate(parents[index]);
            }

            var fitness = getFitness(childGenes);
            return new Chromosome<TGene, TFitness>(childGenes, fitness,
                Strategies.Crossover);
        }

        public static Chromosome<TGene, TFitness> GetBest(GetFitnessDelegate getFitness, int targetLen,
            TFitness optimalFitness, TGene[] geneSet, DisplayDelegate display, MutateGeneDelegate customMutate = null,
            CreateDelegate customCreate = null, int? maxAge = null, int poolSize = 1, CrossoverDelegate crossover = null,
            int? maxSeconds = null)
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
                    Strategies.Create);
            }

            var strategyLookup =
                new Dictionary<Strategies, MutateDelegate>
                {
                    {Strategies.Create, (p, i, o) => FnGenerateParent()},
                    {Strategies.Mutate, (p, i, o) => FnMutate(p)},
                    {
                        Strategies.Crossover,
                        (p, i, o) => Crossover(p.Genes, i, o, getFitness, crossover, FnMutate, FnGenerateParent)
                    }
                };

            var usedStrategies =
                new List<MutateDelegate> {strategyLookup[Strategies.Mutate]};

            if (crossover != null)
                usedStrategies.Add(strategyLookup[Strategies.Crossover]);

            Chromosome<TGene, TFitness> FnNewChild(Chromosome<TGene, TFitness> parent, int index,
                List<Chromosome<TGene, TFitness>> parents) =>
                crossover != null
                    ? Rand.SelectItem(usedStrategies)(parent, index, parents)
                    : FnMutate(parent);

            try
            {
                foreach (var improvement in GetImprovement(FnNewChild, FnGenerateParent, maxAge, poolSize, maxSeconds))
                {
                    display(improvement);
                    var f = strategyLookup[improvement.Strategy];
                    usedStrategies.Add(f);
                    if (optimalFitness.CompareTo(improvement.Fitness) <= 0)
                        return improvement;
                }
            }
            catch (SearchTimeoutException exception)
            {
                display(exception.Improvement);
                return exception.Improvement;
            }

            throw new UnauthorizedAccessException();
        }

        private static IEnumerable<Chromosome<TGene, TFitness>> GetImprovement(
            MutateDelegate newChild, GenerateParentDelegate generateParent, int? maxAge, int poolSize, int? maxSeconds)
        {
            var watch = Stopwatch.StartNew();
            var bestParent = generateParent();
            if (maxSeconds != null && watch.ElapsedMilliseconds > maxSeconds * 1000)
                throw new SearchTimeoutException(bestParent);

            yield return bestParent;
            var parents = new List<Chromosome<TGene, TFitness>> {bestParent};
            var historicalFitnesses = new List<TFitness> {bestParent.Fitness};
            while (parents.Count < poolSize)
            {
                var parent = generateParent();
                if (maxSeconds != null && watch.ElapsedMilliseconds > maxSeconds * 1000)
                    throw new SearchTimeoutException(parent);

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
                if (maxSeconds > 0 && watch.ElapsedMilliseconds > maxSeconds * 1000)
                    throw new SearchTimeoutException(bestParent);

                pIndex = pIndex > 0 ? pIndex - 1 : lastParentIndex;
                var parent = parents[pIndex];
                var child = newChild(parent, pIndex, parents);
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
                    var proportionSimilar = (double) difference / historicalFitnesses.Count;
                    var exp = Math.Exp(-proportionSimilar);
                    if (Rand.Random.NextDouble() < exp)
                    {
                        parents[pIndex] = child;
                        continue;
                    }

                    bestParent.Age = 0;
                    parents[pIndex] = bestParent;
                    continue;
                }

                if (parent.Fitness.CompareTo(child.Fitness) == 0)
                {
                    child.Age = parent.Age + 1;
                    parents[pIndex] = child;
                    continue;
                }

                child.Age = 0;
                parents[pIndex] = child;
                if (bestParent.Fitness.CompareTo(child.Fitness) >= 0)
                    continue;

                bestParent = child;
                historicalFitnesses.Add(bestParent.Fitness);
                yield return bestParent;
            }

            // ReSharper disable once IteratorNeverReturns
        }
    }
}