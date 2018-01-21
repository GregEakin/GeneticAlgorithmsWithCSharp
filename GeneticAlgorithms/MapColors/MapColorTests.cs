using GeneticAlgorithms.SortedNumbers;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace GeneticAlgorithms.MapColors
{
    [TestClass]
    public class MapColorTests
    {
        [TestMethod]
        public void ParseRules()
        {
            var rules = new List<Tuple<string, string>>();
            using (var reader = File.OpenText(@"mapColors\adjacent_states.csv"))
            {
                string line;
                while ((line = reader.ReadLine()) != null)
                {
                    var rule = line.Split(',');
                    var stateName = rule[0];
                    var neighbors = rule[1].Split(';');

                    rules.AddRange(neighbors.Select(neighbor => new Tuple<string, string>(stateName, neighbor)));
                }
            }

            var count1 = rules.Select(r => r.Item1).Distinct().Count();
            var count2 = rules.Count(r => !string.IsNullOrEmpty(r.Item2));
            Console.WriteLine("p edge {0} {1}", count1, count2);
            foreach (var rule in rules.Where(r => !string.IsNullOrEmpty(r.Item2)))
                Console.WriteLine("e {0} {1}", rule.Item1, rule.Item2);
            foreach (var rule in rules.Where(r => string.IsNullOrEmpty(r.Item2)))
                Console.WriteLine("n {0} 0", rule.Item1);
        }

        public static int Fitness(char[] genes, ISet<Rule> rules)
        {
            var rulesThatPass = rules.Count(r => r.Valid(genes));
            return rulesThatPass;
        }

        public static void Display(Chromosome<char, int> candidate, Stopwatch watch, IDictionary<string, State> states)
        {
            foreach (var state in states.Values)
                Console.Write(state.Color(candidate.Genes));
            Console.WriteLine(", {0}, {1} ms", candidate.Fitness, watch.ElapsedMilliseconds);
        }

        [TestMethod]
        public void FourColorTest()
        {
            var states = new SortedDictionary<string, State>();
            var rules = new SortedSet<Rule>();
            Prototype.SetupRules(states, rules);

            var genetic = new Genetic<char, int>();
            var geneSet = "OYGB".ToCharArray();
            var watch = Stopwatch.StartNew();

            int FitnessFun(char[] genes, int size) => Fitness(genes, rules);
            void DisplayFun(Chromosome<char, int> candidate) => Display(candidate, watch, states);

            var best = genetic.BestFitness(FitnessFun, states.Count, rules.Count, geneSet, DisplayFun);
            Assert.IsTrue(best.Fitness >= 214);
        }

        [TestMethod]
        public void SixColorTest()
        {
            var states = new SortedDictionary<string, State>();
            var rules = new SortedSet<Rule>();
            Prototype.SetupRules(states, rules);

            var genetic = new Genetic<char, int>();
            var geneSet = "ROYGBI".ToCharArray();
            var watch = Stopwatch.StartNew();

            int FitnessFun(char[] genes, int size) => Fitness(genes, rules);
            void DisplayFun(Chromosome<char, int> candidate) => Display(candidate, watch, states);

            var best = genetic.BestFitness(FitnessFun, states.Count, rules.Count, geneSet, DisplayFun);
            Assert.IsTrue(best.Fitness >= 214);
        }
    }
}