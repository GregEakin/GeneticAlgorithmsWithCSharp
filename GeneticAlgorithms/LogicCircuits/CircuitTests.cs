using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace GeneticAlgorithms.LogicCircuits
{
    [TestClass]
    public class CircuitTests
    {
        private static readonly Random Random = new Random();

        public delegate int FnCreateGene();

        public delegate int FnFitnessDelegate(string[] genes);

        public static int Fitness(Node[] genes, Tuple<bool[], bool>[] rules, ICircuit[] inputs)
        {
            var circuit = NodesToCircuit(genes).Item1;
            var sourceLabels = "ABCD".ToCharArray();
            var rulesPassed = 0;
            foreach (var rule in rules)
            {
                // inputs.Clear();
                // inputs.update(zip(sourceLabels, rule.Item1))
                if (circuit.Output == rule.Item2)
                    rulesPassed++;
            }

            return rulesPassed;
        }

        public static void Display(Chromosome<Node, int> candidate, Stopwatch watch)
        {
            var circuit = NodesToCircuit(candidate.Genes).Item1;
            Console.WriteLine("{0}\t{1}\t{2} ms", circuit, candidate.Fitness, watch.ElapsedMilliseconds);
        }

        public static Node CreateGene(int index, Tuple<Node.CrateGeneDelegate, ICircuit>[] gates,
            Tuple<Node.CrateGeneDelegate, ICircuit>[] sources)
        {
            var gateType = index < sources.Length
                ? sources[index]
                : gates[Random.Next(gates.Length)];

            int? indexA = null;
            int? indexB = null;
            if (gateType.Item2.InputCount > 0)
                indexA = Random.Next(index);
            if (gateType.Item2.InputCount > 1)
            {
                indexB = index > 1 && index >= sources.Length
                    ? Random.Next(index)
                    : 0;
                if (indexB == indexA)
                    indexB = Random.Next(index);
            }

            return new Node(gateType.Item1, indexA, indexB);
        }

        public static string[] Mutate(string[] childGenes, FnCreateGene fnCreateGene, FnFitnessDelegate fnFitness,
            int sourceCount)
        {
            throw new NotImplementedException();
        }

        public static string[] Crossover(string[] mother, string[] father)
        {
            throw new NotImplementedException();
        }

        public void SetupClass()
        {
            var inputs = new HashSet<string>();
            var gates = new ICircuit[0];
            // [[circuits.And, circuits.And], [lambda i1, i2: circuits.Not(i1), circuits.Not]]
            // { {new And(), new And() }, {(i1, i2)=> new Not(i1), new Not()} }
            var sources = new bool?[0];
            // [ [lambda i1, i2: circuits.Source('A', cls.inputs), circuits.Source],
            //   [lambda i1, i2: circuits.Source('B', cls.inputs), circuits.Source] ]
            // { {(i1, i2) => circuits.Souce('A', inputs), circuits.Source },
            //   {(i1, i2) => circuits.Souce('B', inputs), circuits.Source } }
        }

        [TestMethod]
        public void CreateGeneTest()
        {
            var sourceContainer = new Dictionary<char, bool?> {{'A', false}, {'B', true}};

            var source = new[]
            {
                new Tuple<Node.CrateGeneDelegate, ICircuit>((a, b) => new Source('A', sourceContainer),
                    new Source('A', sourceContainer)),
                new Tuple<Node.CrateGeneDelegate, ICircuit>((a, b) => new Source('B', sourceContainer),
                    new Source('B', sourceContainer))
            };

            var gates = new[]
            {
                new Tuple<Node.CrateGeneDelegate, ICircuit>((a, b) => new And(a, b), new And(null, null)),
                new Tuple<Node.CrateGeneDelegate, ICircuit>((a, b) => new Or(a, b), new Or(null, null))
            };

            var nodes = new Node[6];
            for (var i = 0; i < nodes.Length; i++)
                nodes[i] = CreateGene(i, gates, source);

            var tuple = NodesToCircuit(nodes);
            var circuit = tuple.Item1;
            Console.Write("{0} = {1}", circuit, circuit.Output);
        }

        [TestMethod]
        public void GenerateOrTest()
        {
            var rules = new[]
            {
                new Tuple<bool[], bool>(new[] {false, false}, false),
                new Tuple<bool[], bool>(new[] {false, true}, true),
                new Tuple<bool[], bool>(new[] {true, false}, true),
                new Tuple<bool[], bool>(new[] {true, true}, true)
            };

            var optimalLength = 6;
            FindCircuit(rules, optimalLength);
        }

        [TestMethod]
        public void GenerateXorTest()
        {
        }

        [TestMethod]
        public void GenerateAxBxCTest()
        {
        }

        public void FindCircuit(Tuple<bool[], bool>[] rules, int optimalLength)
        {
        }

        public static Tuple<ICircuit, int> NodesToCircuit(Node[] genes)
        {
            var circuit = new List<ICircuit>();
            var usedIndexes = new List<int>();
            for (var i = 0; i < genes.Length; i++)
            {
                var node = genes[i];
                var used = new HashSet<int> {i};
                ICircuit inputA = null;
                ICircuit inputB = null;
                if (node.IndexA != null && i > node.IndexA)
                {
                    inputA = circuit[(int) node.IndexA];
                    used.Add(usedIndexes[(int) node.IndexA]);
                    if (node.IndexB != null && i > node.IndexB)
                    {
                        inputB = circuit[(int) node.IndexB];
                        used.Add(usedIndexes[(int) node.IndexB]);
                    }
                }

                circuit.Add(node.CreateGate(inputA, inputB));
                usedIndexes.AddRange(used);
            }

            return new Tuple<ICircuit, int>(circuit[circuit.Count - 1], usedIndexes[usedIndexes.Count - 1]);
        }
    }

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