using System;
using System.Linq;

namespace GeneticAlgorithms.Password
{
    public static class Genetic
    {
        public delegate int FitnessFun(string guess);

        public delegate void DisplayFun(Chromosome child);

        private static readonly Random Random = new Random();

        private static string RandomSample(string input, int length)
        {
            var result = string.Empty;
            while (result.Length < length)
            {
                var sampleSize = Math.Min(input.Length, length - result.Length);
                var array = input.ToCharArray().OrderBy(x => Random.Next()).Take(sampleSize);
                result += new string(array.ToArray());
            }

            return result;
        }

        private static Chromosome GenerateParent(string geneSet, int length, FitnessFun getFitness)
        {
            var genes = string.Empty;
            while (genes.Length < length)
            {
                var sampleSize = Math.Min(length - genes.Length, geneSet.Length);
                genes += RandomSample(geneSet, sampleSize);
            }

            var fitness = getFitness(genes);
            return new Chromosome(genes, fitness);
        }

        private static Chromosome Mutate(string geneSet, Chromosome parent, FitnessFun getFitness)
        {
            var index = Random.Next(parent.Genes.Length);
            var childGenes = parent.Genes.ToCharArray();
            var randomSample = RandomSample(geneSet, 2);
            var newGene = randomSample[0];
            var alternate = randomSample[1];
            childGenes[index] = newGene == childGenes[index] ? alternate : newGene;
            var genes = new string(childGenes);
            var fitness = getFitness(genes);
            return new Chromosome(genes, fitness);
        }

        public static Chromosome GetBest(FitnessFun fitnessFun, int targetLen, int optimalFitness, string geneSet,
            DisplayFun display)
        {
            var bestParent = GenerateParent(geneSet, targetLen, fitnessFun);
            display(bestParent);

            if (bestParent.Fitness >= optimalFitness)
                return bestParent;

            while (true)
            {
                var child = Mutate(geneSet, bestParent, fitnessFun);
                if (bestParent.Fitness >= child.Fitness)
                    continue;
                display(child);
                if (child.Fitness >= optimalFitness)
                    return child;
                bestParent = child;
            }
        }
    }
}