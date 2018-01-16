namespace GeneticAlgorithms.LogicCircuits
{
    public class Node
    {
        public delegate ICircuit CrateGeneDelegate(ICircuit inputA, ICircuit inputB);

        public CrateGeneDelegate CreateGate { get; }
        public int? IndexA { get; }
        public int? IndexB { get; }

        public Node(CrateGeneDelegate createGate, int? indexA = null, int? indexB = null)
        {
            CreateGate = createGate;
            IndexA = indexA;
            IndexB = indexB;
        }
    }
}