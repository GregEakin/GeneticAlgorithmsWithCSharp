using System;
using System.Collections.Generic;
using System.Linq;

namespace GeneticAlgorithms.MapColors
{
    public class Genetic<TGene, TFitness>
        where TFitness : IComparable<TFitness>
    {
        public delegate TFitness FitnessFun(TGene[] gene, int size);

        public delegate void DisplayFun(Chromosome<TGene, TFitness> child);

        private readonly Random _random = new Random();

        public TGene[] RandomSample(TGene[] geneSet, int length)
        {
            var genes = new List<TGene>(length);
            while (true)
            {
                var sampleSize = Math.Min(geneSet.Length, length - genes.Count);
                var array = geneSet.OrderBy(x => _random.Next()).Take(sampleSize).ToArray();
                genes.AddRange(array);

                if (genes.Count >= length)
                    return genes.ToArray();
            }
        }

        public Chromosome<TGene, TFitness> GenerateParent(TGene[] geneSet, int length, FitnessFun fitnessFun)
        {
            var genes = RandomSample(geneSet, 2 * length);
            var fit = fitnessFun(genes, length);
            return new Chromosome<TGene, TFitness>(genes, fit);
        }

        public Chromosome<TGene, TFitness> Mutate(TGene[] geneSet, Chromosome<TGene, TFitness> parent,
            FitnessFun fitnessFun)
        {
            var childGenes = parent.Genes.ToArray();
            var index = _random.Next(parent.Genes.Length);
            var randomSample = RandomSample(geneSet, 2);
            var newGene = randomSample[0];
            var alternate = randomSample[1];
            childGenes[index] = newGene.Equals(childGenes[index]) ? alternate : newGene;
            var fitness = fitnessFun(childGenes, childGenes.Length / 2);
            return new Chromosome<TGene, TFitness>(childGenes, fitness);
        }

        public Chromosome<TGene, TFitness> BestFitness(FitnessFun fitnessFun, int targetLen, TFitness optimalFitness,
            TGene[] geneSet, DisplayFun displayFun)
        {
            var bestParent = GenerateParent(geneSet, targetLen, fitnessFun);
            displayFun(bestParent);

            if (bestParent.Fitness.CompareTo(optimalFitness) >= 0)
                return bestParent;

            while (true)
            {
                var child = Mutate(geneSet, bestParent, fitnessFun);
                if (bestParent.Fitness.CompareTo(child.Fitness) > 0)
                    continue;
                displayFun(child);
                if (child.Fitness.CompareTo(optimalFitness) >= 0)
                    return child;
                bestParent = child;
            }
        }
    }
}