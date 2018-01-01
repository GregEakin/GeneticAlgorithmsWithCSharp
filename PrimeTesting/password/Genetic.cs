using System;
using System.Diagnostics;
using System.Linq;

namespace PrimeTesting.password
{
    public class Genetic
    {
        private readonly Random _random = new Random();

        public string RandomSample(string input, int k)
        {
            var array = input.ToCharArray().OrderBy(x => _random.Next()).Take(k).ToArray();
            return new string(array);
        }

        public string GenerateParent(string geneSet, int length)
        {
            var genes = string.Empty;
            while (genes.Length < length)
            {
                var sampleSize = Math.Min(length - genes.Length, geneSet.Length);
                genes += RandomSample(geneSet, sampleSize);
            }

            return genes;
        }

        public string Mutate(string geneSet, string parent)
        {
            var index = _random.Next(parent.Length);
            var childGenes = parent.ToCharArray();
            var randomSample = RandomSample(geneSet, 2);
            var newGene = randomSample[0];
            var alternate = randomSample[1];
            childGenes[index] = newGene == childGenes[index] ? alternate : newGene;
            return new string(childGenes);
        }

        public delegate int Fitness(string guess);

        public delegate void Display(string geneSet, string guess, Stopwatch stopwatch);


        public string BestFitness(Fitness fitness, int targetLen, int optimalFitness, string geneSet,
            Display display)
        {
            var watch = new Stopwatch();
            watch.Start();

            var bestParent = GenerateParent(geneSet, targetLen);
            var bestFitness = fitness(bestParent);
            display(geneSet, bestParent, watch);

            if (bestFitness >= optimalFitness)
                return bestParent;

            while (true)
            {
                var child = Mutate(geneSet, bestParent);
                var childFitness = fitness(child);
                if (bestFitness >= childFitness)
                    continue;
                display(geneSet, child, watch);
                if (childFitness >= optimalFitness)
                    return child;
                bestFitness = childFitness;
                bestParent = child;
            }
        }
    }
}