using System;
using System.Collections.Generic;
using System.Diagnostics;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace GeneticAlgorithms.LogicCircuits
{
    [TestClass]
    public class CircuitTests
    {
        private static readonly Random Random = new Random();

        public delegate int FnCreateGene();

        public delegate int FnFitnessDelegate(string[] genes);

        public static int Fitness(string[] genes, string[] rules, string[] inputs)
        {
            throw new NotImplementedException();
        }

        public static void Display(Chromosome<string, int> candidate, Stopwatch watch)
        {
            throw new NotImplementedException();
        }

        public static string[] CreateGene(int index, string[] gates, int[] sources)
        {
            throw new NotImplementedException();
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
            var inputs = new Dictionary<string, string>();
            var gates = new string[0];
            var sources = new string[0];
        }

        [TestMethod]
        public void GenerateOrTest()
        {
        }

        [TestMethod]
        public void GenerateXorTest()
        {
        }

        [TestMethod]
        public void GenerateAxBxCTest()
        {
        }
    }

    public class Node
    {
        public bool CreateGate { get; }
        public int IndexA { get; }
        public int IndexB { get; }
    }
}