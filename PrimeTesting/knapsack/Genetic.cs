using System;
using System.Collections.Generic;
using System.Linq;

namespace PrimeTesting.knapsack
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

        public delegate TGene[] MutateFun(TGene[] genes);

        public delegate Chromosome<TGene, TFitness> MutateDelegate(Chromosome<TGene, TFitness> parent);

        public delegate Chromosome<TGene, TFitness> GenerateParentDelegate();

        public delegate TGene[] CreateFun();

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

        public Chromosome<TGene, TFitness> GenerateParent(int length, TGene[] geneSet, FitnessFun fitnessFun)
        {
            var genes = RandomSample(geneSet, length);
            var fit = fitnessFun(genes);
            return new Chromosome<TGene, TFitness>(genes, fit);
        }

        public Chromosome<TGene, TFitness> Mutate(Chromosome<TGene, TFitness> parent, TGene[] geneSet,
            FitnessFun fitnessFun)
        {
            var childGenes = new TGene[parent.Genes.Length];
            Array.Copy(parent.Genes, childGenes, parent.Genes.Length);
            var index = _random.Next(parent.Genes.Length);
            var randomSample = RandomSample(geneSet, 2);
            var newGene = randomSample[0];
            var alternate = randomSample[1];
            childGenes[index] = newGene.CompareTo(childGenes[index]) == 0 ? alternate : newGene;
            var fitness = fitnessFun(childGenes);
            return new Chromosome<TGene, TFitness>(childGenes, fitness);
        }

        public Chromosome<TGene, TFitness> MutateCustom(Chromosome<TGene, TFitness> parent, MutateFun mutateFun,
            FitnessFun fitnessFun)
        {
            var childGenes = mutateFun(parent.Genes);
            var fitness = fitnessFun(childGenes);
            return new Chromosome<TGene, TFitness>(childGenes, fitness);
        }

        public Chromosome<TGene, TFitness> BestFitness(FitnessFun fitnessFun, int targetLen, TFitness optimalFitness,
            TGene[] geneSet, DisplayFun displayFun)
        {
            Chromosome<TGene, TFitness> MutateFn1(Chromosome<TGene, TFitness> parent1) =>
                Mutate(parent1, geneSet, fitnessFun);

            Chromosome<TGene, TFitness> GenerateParentFn() => GenerateParent(targetLen, geneSet, fitnessFun);

            foreach (var improvement in GetImprovement(MutateFn1, GenerateParentFn))
            {
                displayFun(improvement);
                if (optimalFitness.CompareTo(improvement.Fitness) <= 0)
                    return improvement;
            }

            throw new UnauthorizedAccessException();
        }

        public Chromosome<TGene, TFitness> BestFitness(FitnessFun fitnessFun, int targetLen, TFitness optimalFitness,
            TGene[] geneSet, DisplayFun displayFun, MutateFun mutateFun)
        {
            Chromosome<TGene, TFitness> MutateFn2(Chromosome<TGene, TFitness> parent1) =>
                MutateCustom(parent1, mutateFun, fitnessFun);

            Chromosome<TGene, TFitness> GenerateParentFn() => GenerateParent(targetLen, geneSet, fitnessFun);

            foreach (var improvement in GetImprovement(MutateFn2, GenerateParentFn))
            {
                displayFun(improvement);
                if (optimalFitness.CompareTo(improvement.Fitness) <= 0)
                    return improvement;
            }

            throw new UnauthorizedAccessException();
        }

        public Chromosome<TGene, TFitness> BestFitness(FitnessFun fitnessFun, int targetLen, TFitness optimalFitness,
            TGene[] geneSet, DisplayFun displayFun, MutateFun mutateFun, CreateFun createFun)
        {
            Chromosome<TGene, TFitness> MutateFn2(Chromosome<TGene, TFitness> parent1) =>
                MutateCustom(parent1, mutateFun, fitnessFun);

            Chromosome<TGene, TFitness> GenerateParentFn()
            {
                var genes = createFun();
                return new Chromosome<TGene, TFitness>(genes, fitnessFun(genes));
            }

            foreach (var improvement in GetImprovement(MutateFn2, GenerateParentFn))
            {
                displayFun(improvement);
                if (optimalFitness.CompareTo(improvement.Fitness) <= 0)
                    return improvement;
            }

            throw new UnauthorizedAccessException();
        }

        internal Chromosome<TGene, TFitness> BestFitness(FitnessFun fitnessFun, int nSquared, TFitness optimalValue,
            TGene[] geneSet, DisplayFun displayFun, MutateFun mutateFun, CreateFun createFun, int maxAge)
        {
            Chromosome<TGene, TFitness> FnMutate(Chromosome<TGene, TFitness> parent) =>
                MutateCustom(parent, mutateFun, fitnessFun);

            Chromosome<TGene, TFitness> FnGenerateParent()
            {
                var genes = createFun();
                var fitness = fitnessFun(genes);
                var chromosome = new Chromosome<TGene, TFitness>(genes, fitness);
                return chromosome;
            }

            foreach (var improvement in GetImprovement(FnMutate, FnGenerateParent, maxAge))
            {
                displayFun(improvement);
                if (optimalValue.CompareTo(improvement.Fitness) <= 0)
                    return improvement;
            }

            throw new UnauthorizedAccessException();
        }

        public IEnumerable<Chromosome<TGene, TFitness>> GetImprovement(MutateDelegate mutate,
            GenerateParentDelegate generateParent)
        {
            var bestParent = generateParent();
            yield return bestParent;
            while (true)
            {
                var child = mutate(bestParent);
                if (bestParent.Fitness.CompareTo(child.Fitness) > 0)
                    continue;
                if (child.Fitness.CompareTo(bestParent.Fitness) > 0)
                    yield return child;
                bestParent = child;
            }
        }

        public IEnumerable<Chromosome<TGene, TFitness>> GetImprovement(MutateDelegate newChild,
            GenerateParentDelegate generateParent, int maxAge)
        {
            var bestParent = generateParent();
            var parent = bestParent;
            yield return bestParent;

            var historicalFitnesses = new List<TFitness> {bestParent.Fitness};
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
                    var proportionSimilar = (double) difference / historicalFitnesses.Count;
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

                yield return child;
                bestParent = child;
                historicalFitnesses.Add(child.Fitness);
            }
        }
    }
}