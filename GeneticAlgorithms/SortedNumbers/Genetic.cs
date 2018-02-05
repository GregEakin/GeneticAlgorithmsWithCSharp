/* File: Chromosome.cs
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

using System;
using System.Collections.Generic;
using System.Linq;

namespace GeneticAlgorithms.SortedNumbers
{
    public class Genetic<TGene, TFitness>
        where TFitness : IComparable<TFitness>
    {
        public delegate TFitness GetFitnessFun(TGene[] gene);

        public delegate void DisplayFun(Chromosome<TGene, TFitness> child);

        private readonly Random _random = new Random();

        public TGene[] RandomSample(TGene[] geneSet, int length)
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

        public Chromosome<TGene, TFitness> GenerateParent(int length, TGene[] geneSet, GetFitnessFun getFitness)
        {
            var genes = RandomSample(geneSet, length);
            var fit = getFitness(genes);
            return new Chromosome<TGene, TFitness>(genes, fit);
        }

        public Chromosome<TGene, TFitness> Mutate(Chromosome<TGene, TFitness> parent, TGene[] geneSet,
            GetFitnessFun getFitness)
        {
            var childGenes = parent.Genes.ToArray();
            var index = _random.Next(parent.Genes.Length);
            var randomSample = RandomSample(geneSet, 2);
            var newGene = randomSample[0];
            var alternate = randomSample[1];
            childGenes[index] = newGene.Equals(childGenes[index]) ? alternate : newGene;
            var fitness = getFitness(childGenes);
            return new Chromosome<TGene, TFitness>(childGenes, fitness);
        }

        public Chromosome<TGene, TFitness> GetBest(GetFitnessFun getFitness, int targetLen,
            TFitness optimalFitness, TGene[] geneSet, DisplayFun display)
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

        private IEnumerable<Chromosome<TGene, TFitness>> GetImprovment(
            Func<Chromosome<TGene, TFitness>, Chromosome<TGene, TFitness>> newChild,
            Func<Chromosome<TGene, TFitness>> generateParent)
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