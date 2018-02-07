/* File: EquationGenerationTests.cs
 *     from chapter 14 of _Genetic Algorithms with Python_
 *     writen by Clinton Sheppard
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

using GeneticAlgorithms.ApproximatingPi;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace GeneticAlgorithms.EquationGeneration
{
    [TestClass]
    public class EquationGenerationTests
    {
        private static readonly Random Random = new Random();

        private delegate int FnEvaluateDelegate(List<string> genes);

        private delegate int FnFitnessDelegate(List<string> genes);

        private delegate int OperationDelegate(int lhs, int rhs);

        private static int Evaluate(List<string> genes, Dictionary<string, OperationDelegate> prioritizedOperations)
        {
            var equation = genes.ToList();
            foreach (var operationSet in prioritizedOperations)
            {
                for (var i = 1; i < equation.Count; i += 2)
                {
                    var opToken = equation[i];
                    if (opToken != operationSet.Key)
                        continue;

                    var leftOperand = int.Parse(equation[i - 1]);
                    var rightOperand = int.Parse(equation[i + 1]);
                    equation[i - 1] = Convert.ToString(operationSet.Value(leftOperand, rightOperand));

                    equation.RemoveAt(i + 1);
                    equation.RemoveAt(i);
                    i -= 2;
                }
            }

            return int.Parse(equation[0]);
        }

        [TestMethod]
        public void EvaluateTest()
        {
            var genes = new List<string> {"20", "+", "2", "-", "-1"};
            var prioritizedOperations = new Dictionary<string, OperationDelegate>
            {
                {"+", Add},
                {"-", Subratct},
            };
            var total = Evaluate(genes, prioritizedOperations);
            Assert.AreEqual(23, total);
        }

        private static int Add(int a, int b) =>
            a + b;

        private static int Subratct(int a, int b) =>
            a - b;

        private static int Multiply(int a, int b) =>
            a * b;

        private static int GetFitness(List<string> genes, int expectedTotal, FnEvaluateDelegate fnEvaluate)
        {
            try
            {
                var result = fnEvaluate(genes);
                var fitness = result != expectedTotal
                    ? expectedTotal - Math.Abs(result - expectedTotal)
                    : 1000 - genes.Count;
                return fitness;
            }
            catch (OverflowException)
            {
                return 1000;
            }
        }

        private static void Display(Chromosome<string, int> candidate, Stopwatch watch)
        {
            Console.WriteLine("{0}\t{1}\t{2} ms",
                string.Join(" ", candidate.Genes),
                candidate.Fitness,
                watch.ElapsedMilliseconds);
        }

        private static List<string> Create(List<string> numbers, string[] operators, int minNumbers, int maxNumbers)
        {
            var genes = numbers.OrderBy(n => Random.Next()).Take(1).ToList();
            var count = Random.Next(minNumbers, 1 + maxNumbers);
            while (count > 1)
            {
                count--;
                genes.Add(operators.OrderBy(n => Random.Next()).First());
                genes.Add(numbers.OrderBy(n => Random.Next()).First());
            }

            return genes;
        }

        private static void Mutate(List<string> genes, List<string> numbers, string[] operations, int minNumbers,
            int maxNumbers, FnFitnessDelegate fnGetFitness)
        {
            var count = Random.Next(1, 10);
            var initialFitness = fnGetFitness(genes);
            while (count > 0)
            {
                count--;
                if (fnGetFitness(genes) > initialFitness)
                    return;

                var numberCount = (1 + genes.Count) / 2;
                var adding = numberCount < maxNumbers && Random.Next(0, 100) == 0;
                if (adding)
                {
                    genes.Add(operations[Random.Next(operations.Length)]);
                    genes.Add(numbers[Random.Next(numbers.Count)]);
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
                    : numbers[Random.Next(numbers.Count)];
            }
        }

        [TestMethod]
        public void AddTest()
        {
            var operations = new[] {"+", "-"};
            var prioritizedOperations = new Dictionary<string, OperationDelegate>
            {
                {"+", Add},
                {"-", Subratct},
            };
            var optimalLengthSolution = new List<string> {"7", "+", "7", "+", "7", "+", "7", "+", "7", "-", "6"};
            Solve(operations, prioritizedOperations, optimalLengthSolution);
        }

        [TestMethod]
        public void MultiplicationTest()
        {
            var operations = new[] {"+", "-", "*"};
            var prioritizedOperations = new Dictionary<string, OperationDelegate>
            {
                {"*", Multiply},
                {"+", Add},
                {"-", Subratct},
            };
            var optimalLengthSolution = new List<string> {"6", "*", "3", "*", "3", "*", "6", "-", "7"};
            Solve(operations, prioritizedOperations, optimalLengthSolution);
        }

        [TestMethod]
        public void ExponentTest()
        {
            var operations = new[] {"^", "+", "-", "*"};
            var prioritizedOperations = new Dictionary<string, OperationDelegate>
            {
                {"^", (a, b) => (int) Math.Pow(a, b)},
                {"*", Multiply},
                {"+", Add},
                {"-", Subratct},
            };
            var optimalLengthSolution = new List<string> {"6", "^", "3", "*", "2", "-", "5"};
            Solve(operations, prioritizedOperations, optimalLengthSolution);
        }

        private static void Solve(string[] operations, Dictionary<string, OperationDelegate> prioritizedOperations,
            List<string> optimalLengthSolution)
        {
            var genetic = new Genetic<string, int>();

            var numbers = new List<string> {"1", "2", "3", "4", "5", "6", "7"};
            var expectedTotal = Evaluate(optimalLengthSolution, prioritizedOperations);
            var minNumbers = (1 + optimalLengthSolution.Count) / 2;
            var maxNumbers = 6 * minNumbers;
            var watch = Stopwatch.StartNew();

            void FnDisplay(Chromosome<string, int> candidate) =>
                Display(candidate, watch);

            int FnEvaluate(List<string> genes) =>
                Evaluate(genes, prioritizedOperations);

            int FnGetFitness(List<string> genes) =>
                GetFitness(genes, expectedTotal, FnEvaluate);

            List<string> FnCreate() =>
                Create(numbers, operations, minNumbers, maxNumbers);

            void FnMutate(List<string> child) =>
                Mutate(child, numbers, operations, minNumbers, maxNumbers, FnGetFitness);

            var optimalFitness = FnGetFitness(optimalLengthSolution);
            var best = genetic.GetBest(FnGetFitness, 0, optimalFitness, null, FnDisplay, FnMutate, FnCreate, 50);
            Assert.IsTrue(optimalFitness.CompareTo(best.Fitness) <= 0);
        }

        [TestMethod]
        public void BenchmarkTest()
        {
            Benchmark.Run(ExponentTest);
        }
    }
}