using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace GeneticAlgorithms.EquationGeneration
{
    [TestClass]
    public class EquationGenerationTests
    {
        private static readonly Random Random = new Random();

        public delegate int FnEvaluateDelegate(string[] genes);

        public delegate int FnFitnessDelegate(string[] genes);

        public static int Evaluate(string[] genes, Dictionary<string, Func<int, int, int>> prioritizedOperations)
        {
            var equation = new List<string>(genes);

            foreach (var operationSet in prioritizedOperations)
            {
                for (var i = 1; i < equation.Count; i += 2)
                {
                    var opToken = equation[i];
                    if (opToken == operationSet.Key)
                    {
                        var leftOperand = int.Parse(equation[i - 1]);
                        var rightOperand = int.Parse(equation[i + 1]);
                        equation[i - 1] = Convert.ToString(operationSet.Value(leftOperand, rightOperand));

                        equation.RemoveAt(i + 1);
                        equation.RemoveAt(i);
                        i += -2;
                    }
                }
            }

            return int.Parse(equation[0]);
        }

        [TestMethod]
        public void EvaluateTest()
        {
            var genes = new[] {"20", "+", "2", "-", "-1"};
            var prioritizedOperations = new Dictionary<string, Func<int, int, int>>
            {
                {"+", Add},
                {"-", Subratct},
            };
            var total = Evaluate(genes, prioritizedOperations);
            Assert.AreEqual(23, total);
        }


        public int Add(int a, int b) => a + b;

        public int Subratct(int a, int b) => a - b;

        public int Multiply(int a, int b) => a * b;

        public static int Fitness(string[] genes, int expectedTotal, FnEvaluateDelegate fnEvaluate)
        {
            var result = fnEvaluate(genes);
            var fitness = result != expectedTotal
                ? expectedTotal - Math.Abs(result - expectedTotal)
                : 1000 - genes.Length;
            return fitness;
        }

        public static void Display(Chromosome<string, int> candidate, Stopwatch watch)
        {
            Console.WriteLine("{0}\t{1}\t{2} ms",
                string.Join(" ", candidate.Genes),
                candidate.Fitness,
                watch.ElapsedMilliseconds);
        }

        public static string[] Create(string[] numbers, string[] operators, int minNumbers, int maxNumbers)
        {
            var genes = numbers.OrderBy(n => Random.Next()).Take(1).ToList();
            var count = Random.Next(minNumbers, 1 + maxNumbers);
            while (count > 1)
            {
                count--;
                genes.Add(operators.OrderBy(n => Random.Next()).First());
                genes.Add(numbers.OrderBy(n => Random.Next()).First());
            }

            return genes.ToArray();
        }

        public static string[] Mutate(string[] input, string[] numbers, string[] operations, int minNumbers,
            int maxNumbers,
            FnFitnessDelegate fnGetFitness)
        {
            var genes = new List<string>(input);

            var count = Random.Next(1, 10);
            var initialFitness = fnGetFitness(genes.ToArray());
            while (count > 0)
            {
                count--;
                if (fnGetFitness(genes.ToArray()) > initialFitness)
                    return genes.ToArray();

                var numberCount = (1 + genes.Count) / 2;
                var adding = numberCount < maxNumbers && Random.Next(0, 100) == 0;
                if (adding)
                {
                    genes.Add(operations[Random.Next(operations.Length)]);
                    genes.Add(numbers[Random.Next(numbers.Length)]);
                    continue;
                }

                var removing = numberCount > minNumbers && Random.Next(0, 20) == 0;
                if (removing)
                {
                    var index = Random.Next(0, genes.Count - 1);
                    genes.RemoveAt(index);
                    genes.RemoveAt(index);
                    continue;
                }

                var index2 = Random.Next(0, genes.Count);
                genes[index2] = (index2 & 1) == 1
                    ? operations[Random.Next(operations.Length)]
                    : numbers[Random.Next(numbers.Length)];
            }

            return genes.ToArray();
        }

        [TestMethod]
        public void AddTest()
        {
            var operations = new[] {"+", "-"};
            var prioritizedOperations = new Dictionary<string, Func<int, int, int>>
            {
                {"+", Add},
                {"-", Subratct},
            };
            var optimalLengthSolution = new[] {"7", "+", "7", "+", "7", "+", "7", "+", "7", "-", "6"};
            Solve(operations, prioritizedOperations, optimalLengthSolution);
        }

        [TestMethod]
        public void MultiplicationTest()
        {
            var operations = new[] { "+", "-", "*" };
            var prioritizedOperations = new Dictionary<string, Func<int, int, int>>
            {
                {"*", Multiply },
                {"+", Add},
                {"-", Subratct},
            };
            var optimalLengthSolution = new[] { "6", "*", "3", "*", "3", "*", "6", "-", "7" };
            Solve(operations, prioritizedOperations, optimalLengthSolution);
        }

        [TestMethod]
        public void ExponentTest()
        {
            var operations = new[] { "^", "+", "-", "*" };
            var prioritizedOperations = new Dictionary<string, Func<int, int, int>>
            {
                {"^", (a, b) => (int)Math.Pow(a, b)},
                {"*", Multiply },
                {"+", Add},
                {"-", Subratct},
            };
            var optimalLengthSolution = new[] { "6", "^", "3", "*", "2", "-", "5" };
            Solve(operations, prioritizedOperations, optimalLengthSolution);
        }

        public void Solve(string[] operations, Dictionary<string, Func<int, int, int>> prioritizedOperations,
            string[] optimalLengthSolution)
        {
            var genetic = new Genetic<string, int>();

            var numbers = new[] {"1", "2", "3", "4", "5", "6", "7"};
            var expectedTotal = Evaluate(optimalLengthSolution, prioritizedOperations);
            var minNumbers = (1 + optimalLengthSolution.Length) / 2;
            var maxNumbers = 6 * minNumbers;
            var watch = Stopwatch.StartNew();

            void FnDisplay(Chromosome<string, int> candidate) => Display(candidate, watch);
            int FnEvaluate(string[] genes) => Evaluate(genes, prioritizedOperations);
            int FnGetFitness(string[] genes) => Fitness(genes, expectedTotal, FnEvaluate);
            string[] FnCreate() => Create(numbers, operations, minNumbers, maxNumbers);

            string[] FnMutate(string[] genes) =>
                Mutate(genes, numbers, operations, minNumbers, maxNumbers, FnGetFitness);

            var optimalFitness = FnGetFitness(optimalLengthSolution);
            var best = genetic.BestFitness(FnGetFitness, 0, optimalFitness, null, FnDisplay, FnMutate, FnCreate, 50);
            Assert.IsTrue(optimalFitness.CompareTo(best.Fitness) <= 0);
        }
    }
}