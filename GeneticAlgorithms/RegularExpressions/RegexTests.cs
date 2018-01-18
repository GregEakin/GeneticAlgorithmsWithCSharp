using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text.RegularExpressions;
using GeneticAlgorithms.LogicCircuits;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace GeneticAlgorithms.RegularExpressions
{
    [TestClass]
    public class RegexTests
    {
        private static readonly Random Random = new Random();

        private static readonly string[] RepeatMetas = {"?", "*", "+", "{2}", "{2,}"};

        private static readonly string[] StartMetas = {"|", "(", "["};

        private static readonly string[] EndMetas = {")", "]"};

        private static readonly string[] AllMetas = RepeatMetas.Concat(StartMetas).Concat(EndMetas).ToArray();

        private readonly List<ArgumentException> _regexErrorsSeen = new List<ArgumentException>();

        public delegate RepairFunc RepairFunc(string token1, List<string> result1, List<string> finals1);

        [TestMethod]
        public void Test1()
        {
            var pattern = "1*0?10*".ToCharArray().Select(c => c.ToString()).ToArray();
            var repaired = RepairRegex(pattern);
            Assert.AreEqual("1*0?10*", repaired);
        }

        [TestMethod]
        public void Test2()
        {
            var pattern = "1[[2".ToCharArray().Select(c => c.ToString()).ToArray();
            var repaired = RepairRegex(pattern);
            Assert.AreEqual("1[]2", repaired);
        }

        public string RepairRegex(string[] genes)
        {
            var result = new List<string>();
            var finals = new List<string>();
            RepairFunc f = RepairIgnoreRepeatMetas;
            foreach (var token in genes)
                f = f(token, result, finals);

            if (finals.Contains("]") && result[result.Count - 1] == "[")
                result.RemoveAt(result.Count - 1);

            finals.Reverse();
            result.AddRange(finals);
            return string.Join("", result);
        }

        public RepairFunc RepairIgnoreRepeatMetas(string token, List<string> result, List<string> finals)
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

        public RepairFunc RepairIgnoreRepeatMetasFollowingRepeatOrStartMetas(string token, List<string> result,
            List<string> finals)
        {
            var last = result[result.Count - 1];
            if (!RepeatMetas.Contains(token))
            {
                switch (token)
                {
                    case "[":
                        result.Add(token);
                        result.Add("]");
                        return RepairInCharacterSet;
                    case "(":
                        finals.Add(")");
                        break;
                    case ")":
                        var match = string.Join("", finals).IndexOf(")", StringComparison.Ordinal);
                        if (match >= 0)
                            finals.RemoveAt(match);
                        else
                            result.Insert(0, "(");
                        result.Add(token);
                        break;
                }
            }
            else if (StartMetas.Contains(last))
            {
                // pass
            }
            else if (token == "?" && last == "?" && result.Count > 2 && RepeatMetas.Contains(result[result.Count - 2]))
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

        public RepairFunc RepairInCharacterSet(string token, List<string> result, List<string> finals)
        {
            switch (token)
            {
                case "]":
                    if (result[result.Count - 1] == "[")
                        result.RemoveAt(result.Count - 1);
                    result.Add(token);
                    var match = string.Join("", finals).IndexOf(")", StringComparison.Ordinal);
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

        public Fitness GetFitness(string[] genes, List<string> wanted, List<string> unwanted)
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
                _regexErrorsSeen.Add(e);
                return new Fitness(0, wanted.Count, unwanted.Count, length);
            }

            var numWantedMatched = wanted.Sum(i => re.Matches(i).Count);
            var numUnwantedMatched = unwanted.Sum(i => re.Matches(i).Count);
            return new Fitness(numWantedMatched, wanted.Count, numUnwantedMatched, length);
        }

        [TestMethod]
        public void FitnessTest()
        {
            //var pattern = "1*0?10*".ToCharArray().Select(c => $"{c}").ToArray();
            var pattern = new[] {"[[1*0?10*"};
            var wanted = new[] {"01", "11", "10"};
            var unwanted = new[] {"00", ""};

            var fitness = GetFitness(pattern, wanted.ToList(), unwanted.ToList());
            Assert.AreEqual(3, fitness.NumWantedMatched);
            Assert.AreEqual(0, fitness.NumUnwantedMatched);
            Assert.AreEqual(7, fitness.Length);
        }

        public void Display(Chromosome<string, Fitness> candidate, Stopwatch watch)
        {
            Console.WriteLine("{0}\t{1}\t{2} ms", RepairRegex(candidate.Genes), candidate.Fitness,
                watch.ElapsedMilliseconds);
        }

        public bool MutateAdd(string[] genes, string[] geneSet)
        {
            throw new NotImplementedException();
        }

        public bool MutateRemove(string[] genes)
        {
            throw new NotImplementedException();
        }

        public bool MutateReplace(string[] genes, string[] geneSet)
        {
            throw new NotImplementedException();
        }

        public bool MutateSwap(string[] genes)
        {
            throw new NotImplementedException();
        }

        public bool MutateMove(string[] genes)
        {
            throw new NotImplementedException();
        }

        public bool MutateToCharacterSet(string[] genes)
        {
            throw new NotImplementedException();
        }

        public bool MutateToCharacterSetLeft(string[] genes, string[] wanted)
        {
            throw new NotImplementedException();
        }

        public bool MutateAddWanted(string[] genes, string[] wanted)
        {
            throw new NotImplementedException();
        }

        public delegate int FnFitnessDelegate(string[] genes);

        public delegate bool FnMutateDelegate(string[] genes);

        public void Mutate(string[] genes, FnFitnessDelegate fnGetFitness, List<FnMutateDelegate> mutationOperators,
            List<int> mutationRoundCounts)
        {
            var initialFitness = fnGetFitness(genes);
            var count = mutationRoundCounts[Random.Next(mutationRoundCounts.Count)];
            for (var i = 1; i < count + 3; i++)
            {
                var copy = mutationOperators.ToList();
                var func = copy[Random.Next(copy.Count)];
                while (!func(genes))
                {
                    copy.Remove(func);
                    func = copy[Random.Next(copy.Count)];
                }

                if (fnGetFitness(genes).CompareTo(initialFitness) <= 0)
                    continue;

                mutationRoundCounts.Add(i);
                return;
            }
        }

        [TestMethod]
        public void TestTwoDigits()
        {
            var wanted = new[] {"01", "11", "10"};
            var unwanted = new[] {"00", ""};
            FindRegex(wanted, unwanted, 7);
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
//            var customOperators = [
//            partial(mutate_to_character_set_left, wanted = wanted),
//                ];
//            FindRegex(wanted, unwanted, 11, customOperators)
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
//            var customOperators = [
//            mutate_to_character_set,
//                ];
//            FindRegex(wanted, unwanted, 10, customOperators);
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
            var unwanted = (from a in "ABCDEFGHIJKLMNOPQRSTUVWXYZ"
                from b in "ABCDEFGHIJKLMNOPQRSTUVWXYZ"
                where !wanted.Contains($"{a}{b}")
                select $"{a}{b}").Concat(
                from i in "ABCDEFGHIJKLMNOPQRSTUVWXYZ" select $"{i}").ToArray();
//            var customOperators = [
//            partial(mutate_to_character_set_left, wanted = wanted),
//            mutate_to_character_set,
//            partial(mutate_add_wanted, wanted =[i for i in wanted]),
//                ];
//            FindRegex(wanted, unwanted, 120, customOperators);
        }

        public void FindRegex(string[] wanted, string[] unwanted, int expetedLength, string[] customOperators = null)
        {
            var genetic = new Genetic<string, Fitness>();

            var mutationOperators = new List<FnMutateDelegate>();

            throw new NotImplementedException();
        }
    }
}