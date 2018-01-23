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

        public delegate TFitness FitnessFun(TGene[] gene);

        public delegate void DisplayFun(Chromosome<TGene, TFitness> child);

        public delegate TGene[] MutateGeneFun(TGene[] genes);

        public delegate Chromosome<TGene, TFitness> MutateChromosomeFun(Chromosome<TGene, TFitness> parent);

        public delegate Chromosome<TGene, TFitness> GenerateParentFun();

        public delegate TGene[] CreateFun();

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

        public Chromosome<TGene, TFitness> GenerateParent(int length, TGene[] geneSet, FitnessFun fitnessFun,
            CreateFun createFun = null)
        {
            var genes = createFun != null ? createFun() : RandomSample(geneSet, length);
            var fitness = fitnessFun(genes);
            var chromosome = new Chromosome<TGene, TFitness>(genes, fitness);
            return chromosome;
        }

        public TGene[] MutateGene(TGene[] parentGenes, TGene[] geneSet)
        {
            var childGenes = parentGenes.ToArray();
            var index = _random.Next(parentGenes.Length);
            var randomSample = RandomSample(geneSet, 2);
            var newGene = randomSample[0];
            var alternate = randomSample[1];
            childGenes[index] = newGene.Equals(childGenes[index]) ? alternate : newGene;
            return childGenes;
        }

        public Chromosome<TGene, TFitness> Mutate(Chromosome<TGene, TFitness> parent, FitnessFun fitnessFun,
            TGene[] geneSet, MutateGeneFun mutateGeneFun)
        {
            var genese = (mutateGeneFun != null) ? mutateGeneFun(parent.Genes) : MutateGene(parent.Genes, geneSet);
            var fitness = fitnessFun(genese);
            return new Chromosome<TGene, TFitness>(genese, fitness);
        }

        internal Chromosome<TGene, TFitness> BestFitness(FitnessFun fitnessFun, int length, TFitness optimalFitness,
            TGene[] geneSet, DisplayFun displayFun, MutateGeneFun mutateGeneFun = null, CreateFun createFun = null,
            int maxAge = 0)
        {
            Chromosome<TGene, TFitness> FnMutate(Chromosome<TGene, TFitness> parent) =>
                Mutate(parent, fitnessFun, geneSet, mutateGeneFun);

            Chromosome<TGene, TFitness> FnGenerateParent() =>
                GenerateParent(length, geneSet, fitnessFun, createFun);

            foreach (var improvement in GetImprovement(FnMutate, FnGenerateParent, maxAge))
            {
                displayFun(improvement);
                if (optimalFitness.CompareTo(improvement.Fitness) <= 0)
                    return improvement;
            }

            throw new UnauthorizedAccessException();
        }

        public IEnumerable<Chromosome<TGene, TFitness>> GetImprovement(MutateChromosomeFun mutateChromosomeFun,
            GenerateParentFun generateParentFun, int maxAge = 0)
        {
            var bestParent = generateParentFun();
            var parent = bestParent;
            yield return bestParent;

            var historicalFitnesses = new List<TFitness> { bestParent.Fitness };
            while (true)
            {
                var child = mutateChromosomeFun(parent);
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
        }
    }
}