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
using GeneticAlgorithms.Utilities;
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
        private delegate int FnFitnessDelegate(IReadOnlyList<string> genes);

        private delegate int OperationDelegate(int lhs, int rhs);

        private static int Evaluate(IReadOnlyList<string> genes, Dictionary<string, OperationDelegate> prioritizedOperations)
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

        private static int GetFitness(IReadOnlyList<string> genes, int expectedTotal, FnFitnessDelegate fnEvaluate)
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

        private static List<string> Create(IReadOnlyCollection<string> numbers, IReadOnlyList<string> operators, int minNumbers, int maxNumbers)
        {
            var genes = numbers.OrderBy(n => Rand.Random.Next()).Take(1).ToList();
            var count = Rand.Random.Next(minNumbers, 1 + maxNumbers);
            while (count > 1)
            {
                count--;
                genes.Add(operators.OrderBy(n => Rand.Random.Next()).First());
                genes.Add(numbers.OrderBy(n => Rand.Random.Next()).First());
            }

            return genes;
        }

        private static void Mutate(List<string> genes, List<string> numbers, string[] operations, int minNumbers,
            int maxNumbers, FnFitnessDelegate fnGetFitness)
        {
            var count = Rand.Random.Next(1, 10);
            var initialFitness = fnGetFitness(genes);
            while (count > 0)
            {
                count--;
                if (fnGetFitness(genes) > initialFitness)
                    return;

                var numberCount = (1 + genes.Count) / 2;
                var adding = numberCount < maxNumbers && Rand.PercentChance(1);
                if (adding)
                {
                    genes.Add(Rand.SelectItem(operations));
                    genes.Add(Rand.SelectItem(numbers));
                    continue;
                }

                var removing = numberCount > minNumbers && Rand.PercentChance(5);
                if (removing)
                {
                    var index = Rand.Random.Next(0, genes.Count - 1);
                    genes.RemoveAt(index);
                    genes.RemoveAt(index);
                    continue;
                }

                var index2 = Rand.Random.Next(0, genes.Count);
                genes[index2] = (index2 & 1) == 1
                    ? Rand.SelectItem(operations)
                    : Rand.SelectItem(numbers);
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
            var numbers = new List<string> {"1", "2", "3", "4", "5", "6", "7"};
            var expectedTotal = Evaluate(optimalLengthSolution, prioritizedOperations);
            var minNumbers = (1 + optimalLengthSolution.Count) / 2;
            var maxNumbers = 6 * minNumbers;
            var watch = Stopwatch.StartNew();

            void FnDisplay(Chromosome<string, int> candidate) =>
                Display(candidate, watch);

            int FnEvaluate(IReadOnlyList<string> genes) =>
                Evaluate(genes, prioritizedOperations);

            int FnGetFitness(IReadOnlyList<string> genes) =>
                GetFitness(genes, expectedTotal, FnEvaluate);

            List<string> FnCreate() =>
                Create(numbers, operations, minNumbers, maxNumbers);

            void FnMutate(List<string> child) =>
                Mutate(child, numbers, operations, minNumbers, maxNumbers, FnGetFitness);

            var optimalFitness = FnGetFitness(optimalLengthSolution);
            var best = Genetic<string, int>.GetBest(FnGetFitness, 0, optimalFitness, null, FnDisplay, FnMutate, FnCreate, 50);
            Assert.IsTrue(optimalFitness.CompareTo(best.Fitness) <= 0);
        }

        [TestMethod]
        public void BenchmarkTest()
        {
            Benchmark.Run(ExponentTest);
        }
    }
}