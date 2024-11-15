﻿/* File: RegexTests.cs
 *     from chapter 17 of _Genetic Algorithms with Python_
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

using GeneticAlgorithms.LogicCircuits;
using GeneticAlgorithms.Utilities;
using System.Diagnostics;
using System.Text.RegularExpressions;

namespace GeneticAlgorithms.RegularExpressions;

[TestClass]
public class RegexTests
{
    private static readonly string[] RepeatMetas = ["?", "*", "+", "{2}", "{2,}"];

    private static readonly string[] StartMetas = ["|", "(", "["];

    private static readonly string[] EndMetas = [")", "]"];

    private static readonly string[] AllMetas = RepeatMetas.Concat(StartMetas).Concat(EndMetas).ToArray();

    private static readonly List<ArgumentException> RegexErrorsSeen = [];

    public delegate bool FnMutateDelegate(List<string> genes);

    public delegate Fitness FnGetFitnessDelegate(List<string> genes);

    public delegate RepairDelegate RepairDelegate(string token1, List<string> result1, List<string> finals1);

    [TestMethod]
    public void AllMetaTest()
    {
        CollectionAssert.AreEquivalent(new[] {"?", "*", "+", "{2}", "{2,}", "|", "(", "[", ")", "]"}, AllMetas);
    }

    [TestMethod]
    public void RepairRegex1Test()
    {
        var pattern = "1*0?10*".ToCharArray().Select(c => c.ToString()).ToList();
        var repaired = RepairRegex(pattern);
        Assert.AreEqual("1*0?10*", repaired);
    }

    [TestMethod]
    public void RepairRegex2Test()
    {
        var pattern = "1[[2".ToCharArray().Select(c => c.ToString()).ToList();
        var repaired = RepairRegex(pattern);
        Assert.AreEqual("1[2]", repaired);
    }

    private static string RepairRegex(IEnumerable<string> genes)
    {
        var result = new List<string>();
        var finals = new List<string>();
        RepairDelegate f = RepairIgnoreRepeatMetas;
        foreach (var token in genes)
            f = f(token, result, finals);
        if (finals.Contains("]") && result[^1] == "[")
            result.RemoveAt(result.Count - 1);
        finals.Reverse();
        result.AddRange(finals);
        return string.Join("", result);
    }

    private static RepairDelegate RepairIgnoreRepeatMetas(string token, List<string> result, List<string> finals)
    {
        if (RepeatMetas.Contains(token) || EndMetas.Contains(token))
            return RepairIgnoreRepeatMetas;
        if (token == "(")
            finals.Add(")");
        result.Add(token);
        if (token == "[")
        {
            finals.Add("]");
            return RepairInCharacterSet;
        }

        return RepairIgnoreRepeatMetasFollowingRepeatOrStartMetas;
    }

    private static RepairDelegate RepairIgnoreRepeatMetasFollowingRepeatOrStartMetas(string token,
        List<string> result,
        List<string> finals)
    {
        var last = result[^1];
        if (!RepeatMetas.Contains(token))
        {
            switch (token)
            {
                case "[":
                    result.Add(token);
                    finals.Add("]");
                    return RepairInCharacterSet;
                case "(":
                    finals.Add(")");
                    break;
                case ")":
                    var match = string.Join("", finals).LastIndexOf(")", StringComparison.Ordinal);
                    if (match >= 0)
                        finals.RemoveAt(match);
                    else
                        result.Insert(0, "(");
                    break;
            }

            result.Add(token);
        }
        else if (StartMetas.Contains(last))
        {
            // pass
        }
        else if (token == "?" && last == "?" && result.Count > 2 && RepeatMetas.Contains(result[^2]))
        {
            // pass
        }
        else if (RepeatMetas.Contains(last))
        {
            // pass
        }
        else
            result.Add(token);

        return RepairIgnoreRepeatMetasFollowingRepeatOrStartMetas;
    }

    private static RepairDelegate RepairInCharacterSet(string token, List<string> result, List<string> finals)
    {
        switch (token)
        {
            case "]":
                if (result[^1] == "[")
                    result.RemoveAt(result.Count - 1);
                result.Add(token);
                var match = string.Join("", finals).LastIndexOf("]", StringComparison.Ordinal);
                if (match >= 0)
                    finals.RemoveAt(match);
                return RepairIgnoreRepeatMetasFollowingRepeatOrStartMetas;
            case "[":
                // pass
                break;
            default:
                result.Add(token);
                break;
        }

        return RepairInCharacterSet;
    }

    private static Fitness GetFitness(List<string> genes, List<string> wanted, List<string> unwanted)
    {
        var pattern = RepairRegex(genes);
        var length = pattern.Length;

        Regex re;
        try
        {
            re = new Regex(pattern);
        }
        catch (ArgumentException e)
        {
            // var key = e.Message;
            RegexErrorsSeen.Add(e);
            return new Fitness(0, wanted.Count, unwanted.Count, length);
        }

        //var numWantedMatched = wanted.Select(i => new Tuple<Match, int>(re.Match(i), i.Length))
        //    .Count(i => i.Item1.Success && i.Item1.Length == i.Item2);
        var numWantedMatched = wanted.Count(i => re.Match(i).Success && re.Match(i).Value.Length == i.Length);
        var numUnwantedMatched = unwanted.Count(i => re.Match(i).Success && re.Match(i).Value.Length == i.Length);
        return new Fitness(numWantedMatched, wanted.Count, numUnwantedMatched, length);
    }

    [TestMethod]
    public void FitnessTest()
    {
        var pattern = "1*0?10*".ToCharArray().Select(c => $"{c}").ToList();
        var wanted = new[] {"01", "11", "10"};
        var unwanted = new[] {"00", ""};

        var fitness = GetFitness(pattern, wanted.ToList(), unwanted.ToList());
        Assert.AreEqual(3, fitness.NumWantedMatched);
        Assert.AreEqual(0, fitness.NumUnwantedMatched);
        Assert.AreEqual(7, fitness.Length);
    }

    private static void Display(Chromosome<string, Fitness> candidate, Stopwatch watch)
    {
        Console.WriteLine("{0}\t{1}\t{2} ms", RepairRegex(candidate.Genes), candidate.Fitness,
            watch.ElapsedMilliseconds);
    }

    private static bool MutateAdd(List<string> genes, string[] geneSet)
    {
        var index = Rand.Random.Next(genes.Count + 1);
        genes.Insert(index, Rand.SelectItem(geneSet));
        return true;
    }

    [TestMethod]
    public void MutateAddTest()
    {
        var genes = "1*0?10*".ToCharArray().Select(c => c.ToString()).ToList();
        var geneSet = new[] {"a"};
        Assert.IsTrue(MutateAdd(genes, geneSet));
        Assert.AreEqual(8, genes.Count);
        Assert.AreEqual(1, genes.Count(g => g == "a"));
    }

    private static bool MutateRemove(List<string> genes)
    {
        if (genes.Count < 1)
            return false;
        genes.RemoveAt(Rand.Random.Next(genes.Count));
        if (genes.Count > 1 && Rand.PercentChance(50))
            genes.RemoveAt(Rand.Random.Next(genes.Count));
        return true;
    }

    [TestMethod]
    public void MutateRemoveTest()
    {
        var genes = "1*0?10*".ToCharArray().Select(c => c.ToString()).ToList();
        Assert.IsTrue(MutateRemove(genes));
        Assert.IsTrue(genes.Count == 6 || genes.Count == 5);
    }

    private static bool MutateReplace(List<string> genes, string[] geneSet)
    {
        if (genes.Count < 1)
            return false;
        var index = Rand.Random.Next(genes.Count);
        genes[index] = Rand.SelectItem(geneSet);
        return true;
    }

    [TestMethod]
    public void MutateReplaceTest()
    {
        var genes = "1*0?10*".ToCharArray().Select(c => c.ToString()).ToList();
        var geneSet = "abcdefg".ToCharArray().Select(c => c.ToString()).Concat(AllMetas).ToArray();
        Assert.IsTrue(MutateReplace(genes, geneSet));
        Assert.AreEqual(7, genes.Count);
        Assert.AreNotEqual("1*0?10*", string.Join(string.Empty, genes.Select(p => p)));
    }

    private static bool MutateSwap(List<string> genes)
    {
        if (genes.Count < 2)
            return false;
        var indexes = Rand.RandomSample(Enumerable.Range(0, genes.Count).ToList(), 2);
        var indexA = indexes[0];
        var indexB = indexes[1];
        (genes[indexA], genes[indexB]) = (genes[indexB], genes[indexA]);
        return true;
    }

    [TestMethod]
    public void MutateSwapTest()
    {
        var genes = "1*3?20%".ToCharArray().Select(c => c.ToString()).ToList();
        var genesCopy = genes.ToList();
        Assert.IsTrue(MutateSwap(genes));
        CollectionAssert.AreEquivalent(genesCopy, genes, string.Join("", genes));
        CollectionAssert.AreNotEqual(genesCopy, genes, string.Join("", genes));
    }

    private static bool MutateMove(List<string> genes)
    {
        if (genes.Count < 3)
            return MutateSwap(genes);
        var length = Rand.Random.Next(1, 3);
        var skip = Rand.Random.Next(genes.Count - length + 1);
        var toMove = genes.Skip(skip).Take(length).ToArray();
        genes.RemoveRange(skip, length);
        var index = Rand.Random.Next(genes.Count);
        if (index >= skip)
            index++;
        genes.InsertRange(index, toMove);
        return true;
    }

    [TestMethod]
    public void MutateMoveTest()
    {
        var genes = "1*3?20%".ToCharArray().Select(c => c.ToString()).ToList();
        var genesCopy = genes.ToList();
        Assert.AreEqual(7, genes.Count);
        CollectionAssert.AreEquivalent(genesCopy, genes, string.Join("", genes));
        CollectionAssert.AreNotEqual(genesCopy, genes, string.Join("", genes));
    }

    private static bool MutateToCharacterSet(List<string> genes)
    {
        if (genes.Count < 3)
            return false;
        var ors = Enumerable.Range(1, genes.Count - 2).Where(i =>
            genes[i] == "|" && !AllMetas.Contains(genes[i - 1]) && !AllMetas.Contains(genes[i + 1])).ToList();
        if (ors.Count <= 0)
            return false;
        var shorter = new List<int>();
        foreach (var i in ors)
        {
            var items = genes.Skip(i - 1).Take(3).Where((_, j) => j % 2 == 0).ToArray();
            var s1 = items.Select(w => w.Length).Sum();
            var s2 = new HashSet<char>(items.SelectMany(mo => mo.ToCharArray())).Count;
            if (s1 > s2)
                shorter.Add(i);
        }

        if (shorter.Count == 0)
            return false;
        var index = Rand.SelectItem(ors);
        var distinct = genes.Skip(index - 1).Take(3).Where((_, j) => j % 2 == 0).SelectMany(s => s.ToCharArray())
            .Select(c => c.ToString()).Distinct();

        //var distinct = new HashSet<char>(genes.SelectMany(mo => mo.Substring(index-1, index+2).ToCharArray()));

        var sequence = new List<string> {"["};
        sequence.AddRange(distinct);
        sequence.Add("]");
        genes.RemoveRange(index - 1, 3);
        genes.InsertRange(index - 1, sequence);
        return true;
    }

    [TestMethod]
    public void MutateToCharacterSetTest()
    {
        var genes = new List<string> {"00", "|", "01"};
        Assert.IsTrue(MutateToCharacterSet(genes));
        CollectionAssert.AreEqual(new[] {"[", "0", "1", "]"}, genes);
    }

    private static bool MutateToCharacterSetLeft(List<string> genes, string[] wanted)
    {
        if (genes.Count < 4)
            return false;
        var ors = Enumerable.Range(-1, genes.Count - 2).Where(i =>
            (i == -1 || StartMetas.Contains(genes[i])) && genes[i + 1].Length == 2 &&
            wanted.Contains(genes[i + 1]) &&
            (genes.Count == i + 1 || genes[i + 2] == "|" || EndMetas.Contains(genes[i + 2]))).ToArray();

        if (ors.Length <= 0)
            return false;
        var lookup = new Dictionary<string, List<int>>();
        foreach (var i in ors)
        {
            var key = genes[i + 1][0].ToString();
            if (lookup.TryGetValue(key, out var value))
                value.Add(i);
            else
                lookup.Add(key, [i]);
        }

        var min2 = lookup.Values.Where(i => i.Count > 1).ToList();
        if (min2.Count <= 0)
            return false;
        var choice = Rand.SelectItem(min2);
        var characterSet = new List<string> {"|", genes[choice[0] + 1][0].ToString(), "["};
        foreach (var i in choice)
            characterSet.Add(genes[i + 1][1].ToString());
        characterSet.Add("]");
        choice.Reverse();
        foreach (var i in choice)
            if (i + 1 >= 0)
                genes.RemoveAt(i + 1);
        genes.AddRange(characterSet);
        return true;
    }

    [TestMethod]
    public void MutateToCharacterSetLeftTest()
    {
        var genes = new List<string> {"MA", "|", "MI", "|", "MS", "|", "MD"};
        var wanted = new[] {"MA", "MI", "MN"};
        var mutated = MutateToCharacterSetLeft(genes, wanted);
        Console.WriteLine(string.Join(", ", genes));
        Assert.IsTrue(mutated);
        CollectionAssert.AreEqual(new List<string> {"|", "|", "MS", "|", "MD", "|", "M", "[", "A", "I", "]"},
            genes);
    }

    private static bool MutateAddWanted(List<string> genes, string[] wanted)
    {
        var index = genes.Count > 0 ? Rand.Random.Next(genes.Count) : 0;
        genes.Insert(index, "|");
        genes.Insert(index + 1, Rand.SelectItem(wanted));
        return true;
    }

    private static void Mutate(List<string> genes, FnGetFitnessDelegate fnGetFitness,
        IReadOnlyCollection<FnMutateDelegate> mutationOperators, List<int> mutationRoundCounts)
    {
        var initialFitness = fnGetFitness(genes);
        var count = Rand.SelectItem(mutationRoundCounts);
        for (var i = 1; i < count + 3; i++)
        {
            var copy = mutationOperators.ToList();
            var func = Rand.SelectItem(copy);
            while (!func(genes))
            {
                copy.Remove(func);
                func = Rand.SelectItem(copy);
            }

            if (fnGetFitness(genes).CompareTo(initialFitness) <= 0)
                continue;

            mutationRoundCounts.Add(i);
            break;
        }
    }

    [TestMethod]
    public void TestTwoDigits()
    {
        var wanted = new[] {"01", "11", "10"};
        var unwanted = new[] {"00", ""};
        var best = FindRegex(wanted, unwanted, 7);
        Assert.AreEqual(3, best.Fitness.NumWantedMatched);
        Assert.AreEqual(0, best.Fitness.NumUnwantedMatched);
        Assert.AreEqual(7, best.Fitness.Length);
    }

    [TestMethod]
    public void TestGrouping()
    {
        var wanted = new[] {"01", "0101", "010101"};
        var unwanted = new[] {"0011", ""};
        FindRegex(wanted, unwanted, 5);
    }

    [TestMethod]
    public void TestStateCodes()
    {
        Fitness.UseRegexLength = true;
        var wanted = new[] {"NE", "NV", "NH", "NJ", "NM", "NY", "NC", "ND"};
        var unwanted = "ABCDEFGHIJKLMNOPQRSTUVWXYZ".Select(v => $"N{v}").Where(st => !wanted.Contains(st))
            .ToArray();
        var customOperators = new FnMutateDelegate[]
        {
            genes => MutateToCharacterSetLeft(genes, wanted)
        };
        FindRegex(wanted, unwanted, 11, customOperators);
    }

    [TestMethod]
    public void TestEvenLength()
    {
        var wanted = new[]
        {
            "00", "01", "10", "11", "0000", "0001", "0010", "0011",
            "0100", "0101", "0110", "0111", "1000", "1001", "1010",
            "1011", "1100", "1101", "1110", "1111"
        };
        var unwanted = new[]
        {
            "0", "1", "000", "001", "010", "011", "100", "101",
            "110", "111", ""
        };
        var customOperators = new FnMutateDelegate[]
        {
            MutateToCharacterSet
        };
        FindRegex(wanted, unwanted, 10, customOperators);
    }

    [TestMethod]
    public void Test50StateCodes()
    {
        Fitness.UseRegexLength = true;
        var wanted = new[]
        {
            "AL", "AK", "AZ", "AR", "CA",
            "CO", "CT", "DE", "FL", "GA",
            "HI", "ID", "IL", "IN", "IA",
            "KS", "KY", "LA", "ME", "MD",
            "MA", "MI", "MN", "MS", "MO",
            "MT", "NE", "NV", "NH", "NJ",
            "NM", "NY", "NC", "ND", "OH",
            "OK", "OR", "PA", "RI", "SC",
            "SD", "TN", "TX", "UT", "VT",
            "VA", "WA", "WV", "WI", "WY"
        };
        const string alphabet = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
        var unwanted = (from a in alphabet
            from b in alphabet
            where !wanted.Contains($"{a}{b}")
            select $"{a}{b}").Concat(
            from i in alphabet select $"{i}").ToArray();
        var customOperators = new FnMutateDelegate[]
        {
            genes => MutateToCharacterSetLeft(genes, wanted),
            MutateToCharacterSet,
            genes => MutateAddWanted(genes, wanted)
        };
        FindRegex(wanted, unwanted, 120, customOperators);
    }

    private static Chromosome<string, Fitness> FindRegex(string[] wanted, string[] unwanted, int expectedLength,
        FnMutateDelegate[] customOperators = null)
    {
        var watch = Stopwatch.StartNew();

        // var set = new HashSet<char>(wanted.SelectMany(mo => mo.ToCharArray()));
        var set = wanted.SelectMany(s => s.ToCharArray()).Distinct().Select(cp => cp.ToString());
        var textGenes = wanted.ToList().Concat(set).ToArray();
        var fullGeneSet = AllMetas.Concat(textGenes).ToArray();

        void FnDisplay(Chromosome<string, Fitness> candidate, int? length) =>
            Display(candidate, watch);

        Fitness FnGetFitness(List<string> genes) =>
            GetFitness(genes, wanted.ToList(), unwanted.ToList());

        var mutationRoundCounts = new List<int> {1};

        var mutationOperators = new List<FnMutateDelegate>
        {
            genes => MutateAdd(genes, fullGeneSet),
            genes => MutateReplace(genes, fullGeneSet),
            MutateRemove,
            MutateSwap,
            MutateMove,
        };

        if (customOperators != null)
            mutationOperators.AddRange(customOperators);

        void FnMutate(List<string> genes) => Mutate(genes, FnGetFitness, mutationOperators, mutationRoundCounts);

        var optimalFitness = new Fitness(wanted.Length, wanted.Length, 0, expectedLength);

        var best = Genetic<string, Fitness>.GetBest(FnGetFitness, textGenes.Max(i => i.Length), optimalFitness,
            fullGeneSet, FnDisplay, FnMutate, null, null, 10);

        Assert.IsTrue(optimalFitness.CompareTo(best.Fitness) <= 0);

        if (!RegexErrorsSeen.Any())
            return best;

        Console.WriteLine();
        Console.WriteLine("Errors:");
        foreach (var error in RegexErrorsSeen)
            Console.WriteLine("  {0}", error.Message);

        return best;
    }

    [TestMethod]
    public void Test33()
    {
        // @@ we should count the '*'s, '?'s, and order the optional terms.
        var pattern = "1*0?10*";
        //var pattern = "0?1?10?";

        var wanted = new[] {"01", "11", "10"};
        var unwanted = new[] {"00", ""};
        var re = new Regex(pattern);

        var numWantedMatched = wanted.Select(i => re.Match(i)).Count(j => j.Success && j.Value.Length == j.Index);
        var numUnwantedMatched =
            unwanted.Select(i => re.Match(i)).Count(i => i.Success && i.Value.Length == i.Length);
        Assert.AreEqual(0, numWantedMatched);
        Assert.AreEqual(0, numUnwantedMatched);
        Assert.AreEqual(7, pattern.Length);

        var x = re.Match("01");
        Assert.IsTrue(x.Success);
        Assert.AreEqual(2, x.Length);
    }
}