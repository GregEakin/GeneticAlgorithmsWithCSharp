using System;

namespace GeneticAlgorithms.LogicCircuits
{
    public partial class Genetic<TGene, TFitness>
        where TFitness : IComparable
    {
        public class SearchTimeoutException : Exception
        {
            public Chromosome<TGene, TFitness> Improvement { get; }

            public SearchTimeoutException(Chromosome<TGene, TFitness> improvement)
            {
                Improvement = improvement;
            }
        }
    }
}