using System;
using System.Diagnostics;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace PrimeTesting.linearEquation
{
    [TestClass]
    public class LinearEquationTests
    {
        private static readonly Random Random = new Random();

        public static Fitness Fitness(double[] genes, string equation)
        {
            var fitness = new Fitness(genes.Select(gene => Math.Abs(Math.Exp(gene))).Sum());
            return fitness;
        }

        public static void Display(Chromosome<double, Fitness> candidate, Stopwatch watch)
        {
        }

        public static double[] Mutate(double[] input, double[] items, double maxWeight, double maxVolume,
            Window window)
        {
            return null;
        }
    }
}