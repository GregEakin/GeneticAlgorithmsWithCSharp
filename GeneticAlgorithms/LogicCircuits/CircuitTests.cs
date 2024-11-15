﻿/* File: CircuitTests.cs
 *     from chapter 16 of _Genetic Algorithms with Python_
 *     written by Clinton Sheppard
 *
 * Author: Greg Eakin <gregory.eakin@gmail.com>
 * Copyright (c) 2018 Greg Eakin
 * 
 * Licensed under the Apache License, Version 2.0 (the "License").
 * You may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 *   http://www.apache.org/licenses/LICENSE-2.0
 * 
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or
 * implied.  See the License for the specific language governing
 * permissions and limitations under the License.
 */

using GeneticAlgorithms.Utilities;
using System.Diagnostics;

namespace GeneticAlgorithms.LogicCircuits;

[TestClass]
public class CircuitTests
{
    private static Dictionary<char, bool?> _inputs;
    private static List<Tuple<Node.CrateGeneDelegate, int>> _gates;
    private static List<Tuple<Node.CrateGeneDelegate, int>> _sources;

    public delegate int FnGetFitnessDelegate(List<Node> genes);

    public delegate Node CreateGeneDelegate(int index);

    private static int Fitness(List<Node> genes, Tuple<bool[], bool>[] rules, Dictionary<char, bool?> inputs)
    {
        var circuit = NodesToCircuit(genes).Item1;
        var sourceLabels = "ABCD";
        var rulesPassed = 0;
        foreach (var rule in rules)
        {
            inputs.Clear();
            for (var i = 0; i < sourceLabels.Length && i < rule.Item1.Length; i++)
                inputs.Add(sourceLabels[i], rule.Item1[i]);
            if (circuit.Output == rule.Item2)
                rulesPassed++;
        }

        return rulesPassed;
    }

    [TestMethod]
    public void FitnessTest()
    {
        var nodes = new List<Node>();
        nodes.AddRange(_sources.Select(n => new Node(n.Item1)));
        nodes.Add(new Node((a, b) => new Or(a, b), 0, 1));

        var rules = new[]
        {
            new Tuple<bool[], bool>([false, false], false),
            new Tuple<bool[], bool>([false, true], true),
            new Tuple<bool[], bool>([true, false], true),
            new Tuple<bool[], bool>([true, true], true)
        };

        var fitness = Fitness(nodes, rules, _inputs);
        Assert.AreEqual(4, fitness);
        Assert.AreEqual(2, _inputs.Count);
        Assert.AreEqual(rules[^1].Item1[0], _inputs['A']);
        Assert.AreEqual(rules[^1].Item1[1], _inputs['B']);
    }

    private static void Display(Chromosome<Node, int> candidate, Stopwatch watch)
    {
        var circuit = NodesToCircuit(candidate.Genes).Item1;
        Console.WriteLine("{0}\t{1}\t{2} ms", circuit, candidate.Fitness, watch.ElapsedMilliseconds);
    }

    private static Node CreateGene(int index, IReadOnlyList<Tuple<Node.CrateGeneDelegate, int>> sources,
        IReadOnlyList<Tuple<Node.CrateGeneDelegate, int>> gates)
    {
        if (index < sources.Count)
            return new Node(sources[index].Item1);

        var gateType = Rand.SelectItem(gates);
        int? indexA = null;
        int? indexB = null;

        if (gateType.Item2 > 0)
        {
            if (index == 0)
                throw new Exception("Not enough inputs for this gate.");
            indexA = Rand.Random.Next(index);
        }

        if (gateType.Item2 > 1)
        {
            if (index == 1)
                throw new Exception("Not enough inputs for this gate.");
            do
                indexB = Rand.Random.Next(index); while (indexB == indexA);
        }

        return new Node(gateType.Item1, indexA, indexB);
    }

    [TestMethod]
    public void CreateGeneTest()
    {
        _inputs.Add('A', false);
        _inputs.Add('B', true);

        var nodes = new List<Node>(6);
        for (var i = 0; i < 6; i++)
            nodes.Add(CreateGene(i, _sources, _gates));

        var circuit = NodesToCircuit(nodes).Item1;
        Console.WriteLine("{0} = {1}", circuit, circuit.Output);

        // The first set, are the sources 
        for (var i = 0; i < _sources.Count; i++)
        {
            var node = nodes[i];
            Assert.IsNull(node.IndexA);
            Assert.IsNull(node.IndexB);
            var source = node.CreateGate(null, null);
            Assert.IsInstanceOfType(source, typeof(Source));
            Assert.AreEqual("AB"[i].ToString(), source.ToString());
        }

        // the next set are the gates
        for (var i = _sources.Count; i < nodes.Count; i++)
        {
            var node = nodes[i];
            if (node.IndexB == null)
            {
                Assert.IsNotNull(node.IndexA);
                Assert.IsNull(node.IndexB);
                Assert.IsTrue(node.IndexA < i);
                var gate1 = node.CreateGate(null, null);
                Assert.IsInstanceOfType(gate1, typeof(Not));
                continue;
            }

            Assert.IsNotNull(node.IndexA);
            Assert.IsNotNull(node.IndexB);
            Assert.IsTrue(node.IndexA < i);
            Assert.IsTrue(node.IndexB < i);
            Assert.IsTrue(node.IndexA != node.IndexB);
            var gate2 = node.CreateGate(null, null);
            Assert.IsInstanceOfType(gate2, typeof(GateWith2Inputs));
        }
    }

    private static void Mutate(List<Node> childGenes, CreateGeneDelegate fnCreateGene,
        FnGetFitnessDelegate fnGetFitness, int sourceCount)
    {
        if (childGenes.Count <= sourceCount)
            throw new Exception("Not enough genes.");

        var initialFitness = fnGetFitness(childGenes);
        var count = Rand.Random.Next(1, 6);
        while (count-- > 0)
        {
            //var gatesUsed = NodesToCircuit(childGenes).Item2.Where(s => s >= sourceCount).ToArray();
            //if (!gatesUsed.Any())
            //    return;
            //var index = Rand.SelectItem(gatesUsed);
            var index = Rand.Random.Next(childGenes.Count - sourceCount) + sourceCount;
            childGenes[index] = fnCreateGene(index);
            if (fnGetFitness(childGenes).CompareTo(initialFitness) > 0)
                return;
        }
    }

    [TestMethod]
    public void MutateGenesTest()
    {
        _inputs.Add('A', false);
        _inputs.Add('B', true);

        var nodes = new List<Node>(6);
        for (var i = 0; i < 6; i++)
            nodes.Add(CreateGene(i, _sources, _gates));

        var circuit1 = NodesToCircuit(nodes).Item1;
        Console.WriteLine("{0} = {1}", circuit1, circuit1.Output);

        Node FnCreateGene(int index) => CreateGene(index, _sources, _gates);
        Mutate(nodes, FnCreateGene, _ => 0, _sources.Count);

        var circuit2 = NodesToCircuit(nodes).Item1;
        Console.WriteLine("{0} = {1}", circuit2, circuit2.Output);

        // The first set, are the sources 
        for (var i = 0; i < _sources.Count; i++)
        {
            var node = nodes[i];
            Assert.IsNull(node.IndexA);
            Assert.IsNull(node.IndexB);
            var source = node.CreateGate(null, null);
            Assert.IsInstanceOfType(source, typeof(Source));
            Assert.AreEqual("AB"[i].ToString(), source.ToString());
        }

        // the next set are the gates
        for (var i = _sources.Count; i < nodes.Count; i++)
        {
            var node = nodes[i];
            if (node.IndexB == null)
            {
                Assert.IsNotNull(node.IndexA);
                Assert.IsNull(node.IndexB);
                Assert.IsTrue(node.IndexA < i);
                var gate1 = node.CreateGate(null, null);
                Assert.IsInstanceOfType(gate1, typeof(Not));
                continue;
            }

            Assert.IsNotNull(node.IndexA);
            Assert.IsNotNull(node.IndexB);
            Assert.IsTrue(node.IndexA < i);
            Assert.IsTrue(node.IndexB < i);
            Assert.IsTrue(node.IndexA != node.IndexB);
            var gate2 = node.CreateGate(null, null);
            Assert.IsInstanceOfType(gate2, typeof(GateWith2Inputs));
        }
    }

    [TestInitialize]
    public void SetupTest()
    {
        _inputs = new Dictionary<char, bool?>();
        _sources =
        [
            new Tuple<Node.CrateGeneDelegate, int>((_, _) => new Source('A', _inputs), 0),
            new Tuple<Node.CrateGeneDelegate, int>((_, _) => new Source('B', _inputs), 0)
        ];
        _gates =
        [
            new Tuple<Node.CrateGeneDelegate, int>((i1, i2) => new And(i1, i2), 2),
            new Tuple<Node.CrateGeneDelegate, int>((i1, _) => new Not(i1), 1)
        ];
    }

    [TestMethod]
    public void GenerateOrTest()
    {
        var rules = new[]
        {
            new Tuple<bool[], bool>([false, false], false),
            new Tuple<bool[], bool>([false, true], true),
            new Tuple<bool[], bool>([true, false], true),
            new Tuple<bool[], bool>([true, true], true)
        };

        FindCircuit(rules, 6);
    }

    [TestMethod]
    public void GenerateXorTest()
    {
        var rules = new[]
        {
            new Tuple<bool[], bool>([false, false], false),
            new Tuple<bool[], bool>([false, true], true),
            new Tuple<bool[], bool>([true, false], true),
            new Tuple<bool[], bool>([true, true], false)
        };

        FindCircuit(rules, 9);
    }

    [TestMethod]
    public void GenerateAxBxCTest()
    {
        var rules = new[]
        {
            new Tuple<bool[], bool>([false, false, false], false),
            new Tuple<bool[], bool>([false, false, true], true),
            new Tuple<bool[], bool>([false, true, false], true),
            new Tuple<bool[], bool>([false, true, true], false),
            new Tuple<bool[], bool>([true, false, false], true),
            new Tuple<bool[], bool>([true, false, true], false),
            new Tuple<bool[], bool>([true, true, false], false),
            new Tuple<bool[], bool>([true, true, true], true)
        };

        _sources.Add(new Tuple<Node.CrateGeneDelegate, int>((_, _) => new Source('C', _inputs), 0));
        _gates.Add(new Tuple<Node.CrateGeneDelegate, int>((i1, i2) => new Or(i1, i2), 2));

        FindCircuit(rules, 12);
    }

    private static Tuple<bool[], bool>[] Get2BitAdderRulesForBit(int bit)
    {
        var rules = new[]
        {
            new Tuple<int[], int[]>([0, 0, 0, 0], [0, 0, 0]), // 0 + 0 = 0
            new Tuple<int[], int[]>([0, 0, 0, 1], [0, 0, 1]), // 0 + 1 = 1
            new Tuple<int[], int[]>([0, 0, 1, 0], [0, 1, 0]), // 0 + 2 = 2
            new Tuple<int[], int[]>([0, 0, 1, 1], [0, 1, 1]), // 0 + 3 = 3
            new Tuple<int[], int[]>([0, 1, 0, 0], [0, 0, 1]), // 1 + 0 = 1
            new Tuple<int[], int[]>([0, 1, 0, 1], [0, 1, 0]), // 1 + 1 = 2
            new Tuple<int[], int[]>([0, 1, 1, 0], [0, 1, 1]), // 1 + 2 = 3
            new Tuple<int[], int[]>([0, 1, 1, 1], [1, 0, 0]), // 1 + 3 = 4
            new Tuple<int[], int[]>([1, 0, 0, 0], [0, 1, 0]), // 2 + 0 = 2
            new Tuple<int[], int[]>([1, 0, 0, 1], [0, 1, 1]), // 2 + 1 = 3
            new Tuple<int[], int[]>([1, 0, 1, 0], [1, 0, 0]), // 2 + 2 = 4
            new Tuple<int[], int[]>([1, 0, 1, 1], [1, 0, 1]), // 2 + 3 = 5
            new Tuple<int[], int[]>([1, 1, 0, 0], [0, 1, 1]), // 3 + 0 = 3
            new Tuple<int[], int[]>([1, 1, 0, 1], [1, 0, 0]), // 3 + 1 = 4
            new Tuple<int[], int[]>([1, 1, 1, 0], [1, 0, 1]), // 3 + 2 = 5
            new Tuple<int[], int[]>([1, 1, 1, 1], [1, 1, 0]), // 3 + 3 = 6
        };

        var bitNRules = rules.Select(rule =>
            new Tuple<bool[], bool>(rule.Item1.Select(b => b != 0).ToArray(), rule.Item2[2 - bit] != 0));
        _sources.Add(new Tuple<Node.CrateGeneDelegate, int>((_, _) => new Source('C', _inputs), 0));
        _sources.Add(new Tuple<Node.CrateGeneDelegate, int>((_, _) => new Source('D', _inputs), 0));
        _gates.Add(new Tuple<Node.CrateGeneDelegate, int>((i1, i2) => new Or(i1, i2), 2));
        _gates.Add(new Tuple<Node.CrateGeneDelegate, int>((i1, i2) => new Xor(i1, i2), 2));
        return bitNRules.ToArray();
    }

    [TestMethod]
    public void Test2BitAdder1SBit()
    {
        var rules = Get2BitAdderRulesForBit(0);
        FindCircuit(rules, 3);
    }

    [TestMethod]
    public void Test2BitAdder2SBit()
    {
        var rules = Get2BitAdderRulesForBit(1);
        FindCircuit(rules, 7);
    }

    [TestMethod]
    public void Test2BitAdder4SBit()
    {
        var rules = Get2BitAdderRulesForBit(2);
        FindCircuit(rules, 9);
    }

    private static void FindCircuit(Tuple<bool[], bool>[] rules, int expectedLength)
    {
        var watch = Stopwatch.StartNew();

        void FnDisplay(Chromosome<Node, int> candidate, int? length = null)
        {
            if (length != null)
                Console.WriteLine("-- distinct nodes in circuit: {0}", NodesToCircuit(candidate.Genes).Item2.Count);
            else
                Display(candidate, watch);
        }

        int FnGetFitness(List<Node> genes) =>
            Fitness(genes, rules, _inputs);

        Node FnCreateGene(int index) =>
            CreateGene(index, _sources, _gates);

        void FnMutate(List<Node> genes) =>
            Mutate(genes, FnCreateGene, FnGetFitness, _sources.Count);

        var maxLength = 50;

        List<Node> FnCreate() =>
            Enumerable.Range(0, maxLength).Select(FnCreateGene).ToList();

        Chromosome<Node, int> FnOptimizationFunction(int variableLength)
        {
            maxLength = variableLength;
            var chromosome = Genetic<Node, int>.GetBest(FnGetFitness, 0, rules.Length, null, FnDisplay, FnMutate,
                FnCreate, null, 3, null, 30);
            return chromosome;
        }

        bool FnIsImprovement(Chromosome<Node, int> currentBest, Chromosome<Node, int> child) =>
            child.Fitness.CompareTo(rules.Length) == 0 &&
            NodesToCircuit(child.Genes).Item2.Count < NodesToCircuit(currentBest.Genes).Item2.Count;

        bool FnIsOptimal(Chromosome<Node, int> child) =>
            child.Fitness.CompareTo(rules.Length) == 0 &&
            NodesToCircuit(child.Genes).Item2.Count <= expectedLength;

        int FnGetNextFeatureValue(Chromosome<Node, int> currentBest) =>
            NodesToCircuit(currentBest.Genes).Item2.Count;

        var best = Genetic<Node, int>.HillClimbing(FnOptimizationFunction, FnIsImprovement, FnIsOptimal,
            FnGetNextFeatureValue, FnDisplay, maxLength);
        Assert.AreEqual(rules.Length, best.Fitness);
        var circuit = NodesToCircuit(best.Genes).Item2;
        Assert.IsTrue(circuit.Count <= expectedLength);
    }

    private static Tuple<ICircuit, ISet<int>> NodesToCircuit(IReadOnlyList<Node> genes)
    {
        var circuit = new List<ICircuit>();
        var usedIndexes = new List<ISet<int>>();
        for (var i = 0; i < genes.Count; i++)
        {
            var node = genes[i];
            var used = new HashSet<int> {i};
            ICircuit inputA = null;
            ICircuit inputB = null;
            if (node.IndexA != null && i > node.IndexA)
            {
                inputA = circuit[(int) node.IndexA];
                used.UnionWith(usedIndexes[(int) node.IndexA]);
                if (node.IndexB != null && i > node.IndexB)
                {
                    inputB = circuit[(int) node.IndexB];
                    used.UnionWith(usedIndexes[(int) node.IndexB]);
                }
            }

            circuit.Add(node.CreateGate(inputA, inputB));
            usedIndexes.Add(used);
        }

        return new Tuple<ICircuit, ISet<int>>(circuit[^1], usedIndexes[^1]);
    }
}