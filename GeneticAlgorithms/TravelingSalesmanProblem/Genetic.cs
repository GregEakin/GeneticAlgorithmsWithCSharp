using System;
using System.Collections.Generic;
using System.Linq;

namespace GeneticAlgorithms.travelingSalesmanProblem
{
    public class Genetic<TGene, TFitness>
        where TGene : IComparable
        where TFitness : IComparable
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

        public delegate TGene[] CrossoverFun(TGene[] genes1, TGene[] genes2);

        private readonly Random _random = new Random();

        public TGene[] RandomSample(TGene[] geneSet, int length)
        {
            var genes = new List<TGene>(length);
            do
            {
                var sampleSize = Math.Min(geneSet.Length, length - genes.Count);
                var array = geneSet.OrderBy(x => _random.Next()).Take(sampleSize).ToArray();
                genes.AddRange(array);
            } while (genes.Count < length);

            return genes.ToArray();
        }

        public Chromosome<TGene, TFitness> GenerateParent(int length, TGene[] geneSet, FitnessFun fitnessFun,
            CreateFun createFun = null)
        {
            var genes = createFun != null ? createFun() : RandomSample(geneSet, length);
            var fitness = fitnessFun(genes);
            var chromosome =
                new Chromosome<TGene, TFitness>(genes, fitness, Chromosome<TGene, TFitness>.Strategies.Create);
            return chromosome;
        }

        public TGene[] MutateGene(TGene[] parentGenes, TGene[] geneSet)
        {
            var childGenes = new TGene[parentGenes.Length];
            Array.Copy(parentGenes, childGenes, parentGenes.Length);
            var index = _random.Next(parentGenes.Length);
            var randomSample = RandomSample(geneSet, 2);
            var newGene = randomSample[0];
            var alternate = randomSample[1];
            childGenes[index] = newGene.CompareTo(childGenes[index]) == 0 ? alternate : newGene;
            return childGenes;
        }

        public Chromosome<TGene, TFitness> Mutate(Chromosome<TGene, TFitness> parent, FitnessFun fitnessFun,
            TGene[] geneSet, MutateGeneFun mutateGeneFun)
        {
            var genes = (mutateGeneFun != null) ? mutateGeneFun(parent.Genes) : MutateGene(parent.Genes, geneSet);
            var fitness = fitnessFun(genes);
            return new Chromosome<TGene, TFitness>(genes, fitness, Chromosome<TGene, TFitness>.Strategies.Mutate);
        }

        public Chromosome<TGene, TFitness> Crossover(TGene[] parentGenes, int index,
            Chromosome<TGene, TFitness>[] parents,
            FitnessFun fitnessFun, CrossoverFun crossover, MutateChromosomeFun mutateGeneFun,
            GenerateParentFun generateParent)
        {
            var donorIndex = _random.Next(0, parents.Length);
            if (donorIndex == index)
                donorIndex = (donorIndex + 1) % parents.Length;
            var childGenes = crossover(parentGenes, parents[donorIndex].Genes);
            if (childGenes != null)
            {
                var fitness = fitnessFun(parentGenes);
                return new Chromosome<TGene, TFitness>(parentGenes, fitness,
                    Chromosome<TGene, TFitness>.Strategies.Crossover);
            }

            // parent and donor are indistinguishable
            parents[donorIndex] = generateParent();
            return mutateGeneFun(parents[index]);
        }

        internal Chromosome<TGene, TFitness> BestFitness(FitnessFun fitnessFun, int length, TFitness optimalFitness,
            TGene[] geneSet, DisplayFun displayFun, MutateGeneFun mutateGeneFun = null, CreateFun createFun = null,
            int maxAge = 0, int poolSize = 1, CrossoverFun crossoverFun = null, int maxSeconds = 0)
        {
            Chromosome<TGene, TFitness> FnMutate(Chromosome<TGene, TFitness> parent) =>
                Mutate(parent, fitnessFun, geneSet, mutateGeneFun);

            Chromosome<TGene, TFitness> FnGenerateParent() =>
                GenerateParent(length, geneSet, fitnessFun, createFun);

            var strategyLookup =
                new Dictionary<Chromosome<TGene, TFitness>.Strategies, Func<Chromosome<TGene, TFitness>, int,
                    Chromosome<TGene, TFitness>[], Chromosome<TGene, TFitness>>>
                {
                    {Chromosome<TGene, TFitness>.Strategies.Create, (p, i, o) => FnGenerateParent()},
                    {Chromosome<TGene, TFitness>.Strategies.Mutate, (p, i, o) => FnMutate(p)},
                    {
                        Chromosome<TGene, TFitness>.Strategies.Crossover,
                        (p, i, o) => Crossover(p.Genes, i, o, fitnessFun, crossoverFun, FnMutate, FnGenerateParent)
                    }
                };

            var usedStrategies =
                new List<Chromosome<TGene, TFitness>.Strategies> {Chromosome<TGene, TFitness>.Strategies.Mutate};

            if (crossoverFun != null)
                usedStrategies.Add(Chromosome<TGene, TFitness>.Strategies.Crossover);

            Chromosome<TGene, TFitness> NewChildFun(Chromosome<TGene, TFitness> parent, int index,
                Chromosome<TGene, TFitness>[] parents) => crossoverFun != null
                ? strategyLookup[usedStrategies[_random.Next(usedStrategies.Count)]]
                    .Invoke(parent, index, parents)
                : FnMutate(parent);

            foreach (var improvement in GetImprovement(NewChildFun, FnGenerateParent, maxAge, poolSize, maxSeconds))
            {
                displayFun(improvement);
                usedStrategies.Add(improvement.Strategy);
                if (optimalFitness.CompareTo(improvement.Fitness) <= 0)
                    return improvement;
            }

            throw new UnauthorizedAccessException();
        }

        public IEnumerable<Chromosome<TGene, TFitness>> GetImprovement(
            Func<Chromosome<TGene, TFitness>, int, Chromosome<TGene, TFitness>[], Chromosome<TGene, TFitness>>
                newChildFun, GenerateParentFun generateParentFun, int maxAge, int poolSize, int maxSeconds)
        {
            var bestParent = generateParentFun();
            yield return bestParent;
            var parents = new List<Chromosome<TGene, TFitness>> {bestParent};
            var historicalFitnesses = new List<TFitness> {bestParent.Fitness};
            for (var i = 0; i < poolSize - 1; i++)
            {
                var parent = generateParentFun();
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
                var child = newChildFun(parent, pIndex, parents.ToArray());
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
        }
    }
}