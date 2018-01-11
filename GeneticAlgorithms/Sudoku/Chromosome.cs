namespace GeneticAlgorithms.Sudoku
{
    public class Chromosome<TGene, TFitness>
    {
        public TGene[] Genes { get; }

        public TFitness Fitness { get; }

        public int Age { get; set; }

        public Chromosome(TGene[] genes, TFitness fitness)
        {
            Genes = genes;
            Fitness = fitness;
        }
    }
}