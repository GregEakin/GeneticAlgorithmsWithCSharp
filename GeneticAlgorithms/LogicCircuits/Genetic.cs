using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace GeneticAlgorithms.LogicCircuits
{
    public partial class Genetic<TGene, TFitness>
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

        public delegate void DisplayFun(Chromosome<TGene, TFitness> child, int? length = null);

        public delegate TGene[] MutateGeneFun(TGene[] genes);

        public delegate Chromosome<TGene, TFitness> MutateChromosomeFun(Chromosome<TGene, TFitness> parent);

        public delegate Chromosome<TGene, TFitness> GenerateParentFun();

        public delegate TGene[] CreateFun();

        public delegate TGene[] CrossoverFun(TGene[] genes1, TGene[] genes2);

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
            var chromosome =
                new Chromosome<TGene, TFitness>(genes, fitness, Chromosome<TGene, TFitness>.Strategies.Create);
            return chromosome;
        }

        public TGene[] MutateGene(TGene[] parentGenes, TGene[] geneSet)
        {
            var childGenes = parentGenes.ToArray();
            var index = _random.Next(childGenes.Length);
            var randomSample = RandomSample(geneSet, 2);
            var newGene = randomSample[0];
            var alternate = randomSample[1];
            childGenes[index] = newGene.Equals(childGenes[index]) ? alternate : newGene;
            return childGenes;
        }

        public Chromosome<TGene, TFitness> Mutate(Chromosome<TGene, TFitness> parent, FitnessFun fitnessFun,
            TGene[] geneSet, MutateGeneFun mutateGeneFun)
        {
            var genes = mutateGeneFun != null ? mutateGeneFun(parent.Genes) : MutateGene(parent.Genes, geneSet);
            var fitness = fitnessFun(genes);
            return new Chromosome<TGene, TFitness>(genes, fitness, Chromosome<TGene, TFitness>.Strategies.Mutate);
        }

        public Chromosome<TGene, TFitness> Crossover(TGene[] parentGenes, int index,
            List<Chromosome<TGene, TFitness>> parents,
            FitnessFun fitnessFun, CrossoverFun crossover, MutateChromosomeFun mutateGeneFun,
            GenerateParentFun generateParent)
        {
            var donorIndex = _random.Next(0, parents.Count);
            if (donorIndex == index)
                donorIndex = (donorIndex + 1) % parents.Count;
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
                    List<Chromosome<TGene, TFitness>>, Chromosome<TGene, TFitness>>>
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
                List<Chromosome<TGene, TFitness>> parents) => crossoverFun != null
                ? strategyLookup[usedStrategies[_random.Next(usedStrategies.Count)]]
                    .Invoke(parent, index, parents)
                : FnMutate(parent);

            try
            {
                foreach (var improvement in GetImprovement(NewChildFun, FnGenerateParent, maxAge, poolSize, maxSeconds))
                {
                    displayFun(improvement);
                    usedStrategies.Add(improvement.Strategy);
                    if (optimalFitness.CompareTo(improvement.Fitness) <= 0)
                        return improvement;
                }
            }
            catch (SearchTimeoutException exception)
            {
                displayFun(exception.Improvement);
                return exception.Improvement;
            }

            throw new UnauthorizedAccessException();
        }

        public IEnumerable<Chromosome<TGene, TFitness>> GetImprovement(
            Func<Chromosome<TGene, TFitness>, int, List<Chromosome<TGene, TFitness>>, Chromosome<TGene, TFitness>>
                newChildFun, GenerateParentFun generateParentFun, int maxAge, int poolSize, int maxSeconds)
        {
            var watch = Stopwatch.StartNew();
            var bestParent = generateParentFun();
            if (maxSeconds > 0 && watch.ElapsedMilliseconds > maxSeconds * 1000)
                throw new SearchTimeoutException(bestParent);
            //yield return bestParent;

            var parents = new List<Chromosome<TGene, TFitness>> {bestParent};
            var historicalFitnesses = new List<TFitness> {bestParent.Fitness};
            for (var i = 0; i < poolSize - 1; i++)
            {
                var parent = generateParentFun();
                if (maxSeconds > 0 && watch.ElapsedMilliseconds > maxSeconds * 1000)
                    throw new SearchTimeoutException(parent);
                //yield return parent;

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
                //yield return bestParent;

                pIndex = pIndex > 0 ? pIndex - 1 : lastParentIndex;
                var parent = parents[pIndex];
                var child = newChildFun(parent, pIndex, parents);
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
                    if (_random.NextDouble() < Math.Exp(-proportionSimilar))
                    {
                        parents[pIndex] = child;
                        continue;
                    }

                    parents[pIndex] = bestParent;
                    parent.Age = 0;
                    continue;
                }

                if (child.Fitness.CompareTo(parent.Fitness) <= 0)
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

        public Chromosome<TGene, TFitness> HillClimbing(Func<int, Chromosome<TGene, TFitness>> optimizationFunction,
            Func<Chromosome<TGene, TFitness>, Chromosome<TGene, TFitness>, bool> isImprovement,
            Func<Chromosome<TGene, TFitness>, bool> isOptimal,
            Func<Chromosome<TGene, TFitness>, int> getNextFeatureValue,
            Action<Chromosome<TGene, TFitness>, int?> display,
            int initialFeatureValue)
        {
            var best = optimizationFunction(initialFeatureValue);
            while (!isOptimal(best))
            {
                var featureValue = getNextFeatureValue(best);
                var child = optimizationFunction(featureValue);
                if (!isImprovement(best, child))
                    continue;

                best = child;
                display(best, featureValue);
            }

            return best;
        }
    }
}