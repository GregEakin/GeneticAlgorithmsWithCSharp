namespace GeneticAlgorithms.knights
{
    public class Chromosome<TGene, TFitness>
    {
        public TGene[] Genes { get; }

        public TFitness Fitness { get; }

        public Chromosome(TGene[] genes, TFitness fitness)
        {
            Genes = genes;
            Fitness = fitness;
        }
    }
}