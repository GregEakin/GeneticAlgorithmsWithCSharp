using System;
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
            var newGene = RandomSample(geneSet, 1)[0];
            var alternate = RandomSample(geneSet, 1)[0];
            childGenes[index] = newGene == childGenes[index] ? alternate : newGene;
            return new string(childGenes);
        }

        public delegate int Fitness(string guess);

        public delegate void Display(string guess);


        public void BestFitness(Fitness fitness, int targetLen, int optimalFitness, string geneSet,
            Display display)
        {
            var bestParent = GenerateParent(geneSet, targetLen);
            var bestFitness = fitness(bestParent);
            display(bestParent);

            while (true)
            {
                var child = Mutate(geneSet, bestParent);
                var childFitness = fitness(child);
                if (bestFitness >= childFitness)
                    continue;
                display(child);
                if (childFitness >= optimalFitness)
                    break;
                bestFitness = childFitness;
                bestParent = child;
            }
        }
    }
}