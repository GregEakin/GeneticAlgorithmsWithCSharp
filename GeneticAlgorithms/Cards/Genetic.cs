using System;
using System.Collections.Generic;
using System.Linq;

namespace GeneticAlgorithms.Cards
{
    public class Genetic<TGene, TFitness>
        where TFitness : IComparable
    {
        public delegate TFitness FitnessFun(TGene[] gene, int size);

        public delegate void DisplayFun(Chromosome<TGene, TFitness> child);

        public delegate TGene[] MutateFun(TGene[] genes);
        public delegate TGene[] MutateGeneFun(TGene[] genes);

        public delegate Chromosome<TGene, TFitness> MutateDelegate(Chromosome<TGene, TFitness> parent);

        public delegate Chromosome<TGene, TFitness> GenerateParentDelegate();

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
            var fit = fitnessFun(genes, length);
            return new Chromosome<TGene, TFitness>(genes, fit);
        }

        public TGene[] MutateGene(TGene[] parentGenes, TGene[] geneSet)
        {
            var childGenes = new TGene[parentGenes.Length];
            Array.Copy(parentGenes, childGenes, parentGenes.Length);
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
            var genese = mutateGeneFun != null ? mutateGeneFun(parent.Genes) : MutateGene(parent.Genes, geneSet);
            var fitness = fitnessFun(genese, genese.Length);
            return new Chromosome<TGene, TFitness>(genese, fitness);
        }

        public Chromosome<TGene, TFitness> BestFitness(FitnessFun fitnessFun, int targetLen, TFitness optimalFitness,
            TGene[] geneSet, DisplayFun displayFun, MutateGeneFun mutateGeneFun = null)
        {
            Chromosome<TGene, TFitness> FnMutate(Chromosome<TGene, TFitness> parent) => Mutate(parent, fitnessFun, geneSet, mutateGeneFun);

            Chromosome<TGene, TFitness> FnGenerateParent() => GenerateParent(targetLen, geneSet, fitnessFun);

            foreach (var improvement in GetImprovement(FnMutate, FnGenerateParent))
            {
                displayFun(improvement);
                if (optimalFitness.CompareTo(improvement.Fitness) <= 0)
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
                bestParent = child;
                if (bestParent.Fitness.CompareTo(child.Fitness) <= 0)
                    yield return child;
            }
        }
    }
}