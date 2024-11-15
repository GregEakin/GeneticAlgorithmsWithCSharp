﻿/* File: LinearEquationTests.cs
 *     from chapter 10 of _Genetic Algorithms with Python_
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

using GeneticAlgorithms.MagicSquare;
using GeneticAlgorithms.Utilities;
using System.Diagnostics;

namespace GeneticAlgorithms.LinearEquation;

[TestClass]
public class LinearEquationTests
{
    public delegate Fraction FnEquationDelegate(IReadOnlyList<Fraction> genes);

    public delegate IReadOnlyList<Fraction> FnGenesToInputsDelegate(IReadOnlyList<Fraction> genes);

    private static Fitness GetFitness(IReadOnlyList<Fraction> genes, FnEquationDelegate[] equations)
    {
        try
        {
            var fitness = equations.Aggregate(new Fraction(0),
                (current, equation) => current + Fraction.Abs(equation(genes)));
            return new Fitness(fitness);
        }
        catch (ArgumentException)
        {
            // divide by zero?
            return new Fitness(new Fraction(int.MaxValue));
        }
    }

    [TestMethod]
    public void FitnessTest()
    {
        var child = new List<Fraction> {new(2), new(-1)};
        Fraction E1(IReadOnlyList<Fraction> genes) => genes[0] + 2 * genes[1] - 4;
        Fraction E2(IReadOnlyList<Fraction> genes) => 4 * genes[0] + 4 * genes[1] - 12;
        var equations = new FnEquationDelegate[] {E1, E2};
        var fitness = GetFitness(child, equations);
        Assert.AreEqual(new Fraction(12), fitness.TotalDifference);
    }

    private static void Display(Chromosome<Fraction, Fitness> candidate, Stopwatch watch,
        FnGenesToInputsDelegate fnGenesToInputs)
    {
        var symbols = "xyza".ToCharArray();
        var result = string.Join(", ", symbols.Zip(fnGenesToInputs(candidate.Genes), (s, v) => $"{s} = {v}"));
        Console.WriteLine("{0}\t{1}\t{2} ms", result, candidate.Fitness, watch.ElapsedMilliseconds);
    }

    [TestMethod]
    public void DisplayTest()
    {
        var range = Enumerable.Range(-5, 11).ToArray();
        var geneSet =
            (from n in range from d in range.Where(i => i != 0) select new Fraction(n, d))
            .Distinct().OrderBy(g => g);

        IReadOnlyList<Fraction> FnGenesToInputs(IReadOnlyList<Fraction> genes1) => genes1;
        var genes = geneSet.OrderBy(_ => Rand.Random.Next()).Take(4).ToList();
        var fitness = new Fitness(new Fraction(42));
        var chromosome = new Chromosome<Fraction, Fitness>(genes, fitness);
        var watch = Stopwatch.StartNew();
        Display(chromosome, watch, FnGenesToInputs);
    }

    [TestMethod]
    public void GeneSetTest()
    {
        var range = Enumerable.Range(-5, 11).ToArray();
        var geneSet =
            (from n in range from d in range.Where(i => i != 0) select new Fraction(n, d))
            .Distinct().OrderBy(g => g).ToArray();
        foreach (var gene in geneSet)
            Console.WriteLine("{0}, {1}", gene, (double) gene);
    }

    private static void Mutate(IList<Fraction> genes, IList<Fraction> sortedGeneSet, Window window, IReadOnlyList<int> geneIndexes)
    {
        var indexes = Rand.PercentChance(10)
            ? geneIndexes.OrderBy(_ => Rand.Random.Next()).Take(1 + Rand.Random.Next(geneIndexes.Count - 1)).ToArray()
            : [Rand.SelectItem(geneIndexes)];
        window.Slide();
        foreach (var index in indexes)
        {
            var geneSetIndex = sortedGeneSet.IndexOf(genes[index]);
            var start = Math.Max(0, geneSetIndex - window.Size);
            var stop = Math.Min(sortedGeneSet.Count - 1, geneSetIndex + window.Size);
            var geneSetIndex2 = Rand.Random.Next(start, stop);
            genes[index] = sortedGeneSet[geneSetIndex2];
        }
    }

    [TestMethod]
    public void MutateTest()
    {
        var watch = Stopwatch.StartNew();
        var fitness = new Fitness(new Fraction(0));

        var range = Enumerable.Range(-5, 11).ToArray();
        var geneSet =
            (from n in range from d in range.Where(i => i != 0) select new Fraction(n, d))
            .Distinct().OrderBy(g => g);

        IReadOnlyList<Fraction> FnGenesToInputs(IReadOnlyList<Fraction> genes1) => genes1;

        var genes = geneSet.OrderBy(_ => Rand.Random.Next()).Take(4).ToList();
        Display(new Chromosome<Fraction, Fitness>(genes, fitness), watch, FnGenesToInputs);

        var sortedGeneSet = genes.OrderBy(g => g).ToList();
        Display(new Chromosome<Fraction, Fitness>(sortedGeneSet, fitness), watch, FnGenesToInputs);

        var geneIndexes = Enumerable.Range(0, genes.Count).ToArray();
        var window = new Window(1, genes.Count, genes.Count);
        Mutate(genes, sortedGeneSet, window, geneIndexes);
        Display(new Chromosome<Fraction, Fitness>(genes, fitness), watch, FnGenesToInputs);
    }

    [TestMethod]
    public void TwoUnknownsTest()
    {
        var range = Enumerable.Range(-5, 11).ToArray();
        var geneSet =
            (from n in range from d in range.Where(i => i != 0) select new Fraction(n, d))
            .Distinct().OrderBy(g => g).ToArray();

        IReadOnlyList<Fraction> FnGenesToInputs(IReadOnlyList<Fraction> genes) => genes;

        Fraction E1(IReadOnlyList<Fraction> genes) =>
            genes[0] + 2 * genes[1] - 4;

        Fraction E2(IReadOnlyList<Fraction> genes) =>
            4 * genes[0] + 4 * genes[1] - 12;

        var equations = new FnEquationDelegate[] {E1, E2};
        SolveUnknown(2, geneSet, equations, FnGenesToInputs);
        //Assert.AreEqual(new Fraction(0), best.Fitness.TotalDifference);
        //CollectionAssert.AreEqual(
        //    new[]
        //    {
        //        new Fraction(2),
        //        new Fraction(1)
        //    }, best.Genes);
    }

    [TestMethod]
    public void ThreeUnknownsTest()
    {
        var geneRange = Enumerable.Range(-5, 11).ToArray();
        var geneSet =
            (from n in geneRange from d in geneRange.Where(i => i != 0) select new Fraction(n, d))
            .Distinct().OrderBy(g => g).ToArray();

        IReadOnlyList<Fraction> FnGenesToInputs(IReadOnlyList<Fraction> genes) => genes;

        Fraction E1(IReadOnlyList<Fraction> genes) =>
            6 * genes[0] - 2 * genes[1] + 8 * genes[2] - 20;

        Fraction E2(IReadOnlyList<Fraction> genes) =>
            genes[1] + 8 * genes[0] * genes[2] + 1;

        Fraction E3(IReadOnlyList<Fraction> genes) =>
            2 * genes[2] * 6 / genes[0] + 3 * genes[1] / 2 - 6;

        var equations = new FnEquationDelegate[] {E1, E2, E3};
        SolveUnknown(3, geneSet, equations, FnGenesToInputs);
        //Assert.AreEqual(new Fraction(0), best.Fitness.TotalDifference);
        //CollectionAssert.AreEqual(
        //    new[]
        //    {
        //        new Fraction(2, 3),
        //        new Fraction(-5),
        //        new Fraction(3, 4)
        //    }, best.Genes);
    }

    [TestMethod]
    public void FourUnknownsTest()
    {
        var geneRange = Enumerable.Range(-13, 27).Where(i => i != 0).ToArray();
        var geneSet =
            (from n in geneRange from d in geneRange.Where(i => i != 0) select new Fraction(n, d))
            .Distinct().OrderBy(g => g).ToArray();

        IReadOnlyList<Fraction> FnGenesToInputs(IReadOnlyList<Fraction> genes) => genes;

        Fraction E1(IReadOnlyList<Fraction> genes) =>
            new Fraction(1, 15) * genes[0] - 2 * genes[1] - 15 * genes[2] - new Fraction(4, 5) * genes[3] - 3;

        Fraction E2(IReadOnlyList<Fraction> genes) =>
            -new Fraction(5, 2) * genes[0] - new Fraction(9, 4) * genes[1] + 12 * genes[2] - genes[3] - 17;

        Fraction E3(IReadOnlyList<Fraction> genes) =>
            -13 * genes[0] + new Fraction(3, 10) * genes[1] - 6 * genes[2] - new Fraction(2, 5) * genes[3] - 17;

        Fraction E4(IReadOnlyList<Fraction> genes) =>
            new Fraction(1, 2) * genes[0] + 2 * genes[1] + new Fraction(7, 4) * genes[2] +
            new Fraction(4, 3) * genes[3] + 9;

        var equations = new FnEquationDelegate[] {E1, E2, E3, E4};
        SolveUnknown(4, geneSet, equations, FnGenesToInputs);
        //Assert.AreEqual(new Fraction(0), best.Fitness.TotalDifference);
        //CollectionAssert.AreEqual(
        //    new[]
        //    {
        //        new Fraction(-3, 2),
        //        new Fraction(-7, 2),
        //        new Fraction(1, 3),
        //        new Fraction(-11, 8)
        //    }, best.Genes);
    }

    [TestMethod]
    public void BenchmarkTest()
    {
        Benchmark.Run(FourUnknownsTest);
    }

    private static void SolveUnknown(int numUnknowns, Fraction[] geneSet, FnEquationDelegate[] equations,
        FnGenesToInputsDelegate fnGenesToInputs)
    {
        var watch = Stopwatch.StartNew();
        var maxAge = 50;
        var window = new Window(Math.Max(1, geneSet.Length / (2 * maxAge)),
            Math.Max(1, geneSet.Length / 3),
            geneSet.Length / 2);
        var geneIndexes = Enumerable.Range(0, numUnknowns).ToArray();
        var sortedGeneSet = geneSet.OrderBy(gene => gene).ToList();

        void FnDisplay(Chromosome<Fraction, Fitness> candidate) => Display(candidate, watch, fnGenesToInputs);
        Fitness FnGetFitness(IReadOnlyList<Fraction> genes) => GetFitness(genes, equations);
        void FnMutate(List<Fraction> genes) => Mutate(genes, sortedGeneSet, window, geneIndexes);

        var optimalFitness = new Fitness(new Fraction(0));
        var best = Genetic<Fraction, Fitness>.GetBest(FnGetFitness, numUnknowns, optimalFitness, geneSet, FnDisplay, FnMutate,
            null, maxAge);
        Assert.IsTrue(optimalFitness.CompareTo(best.Fitness) <= 0);
        Assert.AreEqual(0.0, (float) best.Fitness.TotalDifference, 0.0001);
    }
}