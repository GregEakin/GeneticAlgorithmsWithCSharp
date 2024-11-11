/* File: OneMaxTests.cs
 *     from chapter 2 of _Genetic Algorithms with Python_
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

using System.Diagnostics;

namespace GeneticAlgorithms.Password;

[TestClass]
public class OneMaxTests
{
    private static int GetFitness(string genes)
    {
        var sum = genes.Count(c => c == '1');
        return sum;
    }

    private static void Display(Chromosome candidate, Stopwatch stopwatch)
    {
        Console.WriteLine("{0}...{1}\t{2}\t{3} ms", candidate.Genes.Substring(0, 15),
            candidate.Genes.Substring(candidate.Genes.Length - 15),
            candidate.Fitness, stopwatch.ElapsedMilliseconds);
    }

    private static void Test(int length = 100)
    {
        var geneSet = new[] {'0', '1'};
        var watch = Stopwatch.StartNew();
        void FnDisplay(Chromosome candidate) => Display(candidate, watch);
        int FnGetFitness(string genes) => GetFitness(genes);

        var optimalFitness = length;
        var best = Genetic.GetBest(FnGetFitness, length, optimalFitness, geneSet, FnDisplay);
        Assert.AreEqual(optimalFitness, best.Fitness);
    }

    [TestMethod]
    public void SingleTest()
    {
        Test(50);
    }

    [TestMethod]
    public void BenchmarkTest()
    {
        Benchmark.Run(() => Test(4000));
    }
}