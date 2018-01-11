using System;
using System.Diagnostics;
using System.Linq;

namespace GeneticAlgorithms.Password
{
    public static class Genetic
    {
        public delegate int FitnessFun(string guess);

        public delegate void DisplayFun(Chromosome child);

        private static readonly Random RandomGen = new Random();

        public static string RandomSample(string input, int k)
        {
            var result = string.Empty;
            while (true)
            {
                var length = Math.Min(input.Length, k - result.Length);
                var array = input.ToCharArray().OrderBy(x => RandomGen.Next()).Take(length).ToArray();
                result += new string(array);

                if (result.Length >= k)
                    return result;
            }
        }

        public static Chromosome Mutate(string geneSet, Chromosome parent, FitnessFun fitnessFun)
        {
            var index = RandomGen.Next(parent.Genes.Length);
            var childGenes = parent.Genes.ToCharArray();
            var randomSample = RandomSample(geneSet, 2);
            var newGene = randomSample[0];
            var alternate = randomSample[1];
            childGenes[index] = newGene == childGenes[index] ? alternate : newGene;
            var genes = new string(childGenes);
            var fit = fitnessFun(genes);
            return new Chromosome(genes, fit);
        }

        public static Chromosome GenerateParent(string geneSet, int length, FitnessFun fitnessFun)
        {
            var genes = string.Empty;
            while (genes.Length < length)
            {
                var sampleSize = Math.Min(length - genes.Length, geneSet.Length);
                genes += RandomSample(geneSet, sampleSize);
            }

            var fit = fitnessFun(geneSet);
            return new Chromosome(genes, fit);
        }

        public static Chromosome BestFitness(FitnessFun fitnessFun, int targetLen, int optimalFitness, string geneSet,
            DisplayFun displayFun)
        {
            var bestParent = GenerateParent(geneSet, targetLen, fitnessFun);
            displayFun(bestParent);

            if (bestParent.Fitness >= optimalFitness)
                return bestParent;

            while (true)
            {
                var child = Mutate(geneSet, bestParent, fitnessFun);
                if (bestParent.Fitness >= child.Fitness)
                    continue;
                displayFun(child);
                if (child.Fitness >= optimalFitness)
                    return child;
                bestParent = child;
            }
        }
    }
}