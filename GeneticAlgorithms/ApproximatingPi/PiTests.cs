using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace GeneticAlgorithms.ApproximatingPi
{
    [TestClass]
    public class PiTests
    {
        private static readonly Random Random = new Random();

        public static double Fitness(bool[] genes, int[] bitValues)
        {
            var denominator = GetDenominator(genes, bitValues);
            if (denominator == 0)
                return 0.0;

            var ratio = (double) GetNumerator(genes, bitValues) / denominator;
            return Math.PI - Math.Abs(Math.PI - ratio);
        }

        public static void Display(Chromosome<bool, double> candidate, Stopwatch watch, int[] bitValues, bool display)
        {
            if (!display) return;

            var numerator = GetNumerator(candidate.Genes, bitValues);
            var denominator = GetDenominator(candidate.Genes, bitValues);
            Console.WriteLine("{0}/{1}\t{2}\t{3} ms", numerator, denominator, candidate.Fitness,
                watch.ElapsedMilliseconds);
        }

        public static int BitsToInt(bool[] bits, int[] bitValues)
        {
            var result = 0;
            for (var index = 0; index < bits.Length; index++)
            {
                if (!bits[index])
                    continue;

                result += bitValues[index];
            }

            return result;
        }

        public static int GetNumerator(bool[] genes, int[] bitValues) =>
            1 + BitsToInt(genes.Take(bitValues.Length).ToArray(), bitValues);

        public static int GetDenominator(bool[] genes, int[] bitValues) =>
            BitsToInt(genes.Skip(bitValues.Length).ToArray(), bitValues);

        public static bool[] Mutate(bool[] input, int numBits)
        {
            var genes = new bool[input.Length];
            Array.Copy(input, genes, input.Length);
            var numeratorIndex = Random.Next(0, numBits);
            var denominatorIndex = Random.Next(numBits, genes.Length);
            genes[numeratorIndex] = !genes[numeratorIndex];
            genes[denominatorIndex] = !genes[denominatorIndex];
            return genes;
        }

        public bool ApproximatePi(int[] bitValues, int maxSeconds, bool display = true)
        {
            var genetic = new Genetic<bool, double>();
            var geneSet = new[] {false, true};
            var watch = new Stopwatch();
            var optimalFitness = 3.14159; // Math.PI;

            void FnDispaly(Chromosome<bool, double> candidate) => Display(candidate, watch, bitValues, display);
            double FnFitness(bool[] genes) => Fitness(genes, bitValues);
            bool[] FnMutate(bool[] genes) => Mutate(genes, bitValues.Length);

            var length = 2 * bitValues.Length;
            watch.Start();
            var best = genetic.BestFitness(FnFitness, length, optimalFitness, geneSet, FnDispaly, FnMutate, null, 250,
                1, null, maxSeconds);

            return best.Fitness >= optimalFitness;
        }

        [TestMethod]
        public void ApproximatePiTest()
        {
            var found = ApproximatePi(new[] {512, 256, 128, 64, 32, 16, 8, 4, 2, 1}, 5);
            Assert.IsTrue(found);
        }

        [TestMethod]
        public void FastPiSearch()
        {
            var found = ApproximatePi(new[] {211, 84, 134, 193, 142, 159, 274, 209, 161, 33}, 5);
            Assert.IsTrue(found);
        }

        [TestMethod]
        public void OptimizePi()
        {
            var genetic = new Genetic<int, double>();
            var geneSet = Enumerable.Range(1, 512).ToArray();
            var length = 10;
            var maxSeconds = 2;

            double FnGetFitness(int[] genes)
            {
                var watch = new Stopwatch();
                watch.Start();
                var count = 0.0;
                while (watch.Elapsed.Seconds < maxSeconds)
                {
                    var found = ApproximatePi(genes, maxSeconds, false);
                    if (found) count++;
                }

                var distance = Math.Abs(genes.Sum() - 1023);
                var fraction = distance > 0 ? 1.0 / distance : distance;
                count += Math.Round(fraction, 4);
                return count;
            }

            void FnDisplay(Chromosome<int, double> chromosome) => Console.WriteLine("{0}\t{1}",
                string.Join(", ", chromosome.Genes), chromosome.Fitness);

            var initial = new[] {512, 256, 128, 64, 32, 16, 8, 4, 2, 1};
            Console.WriteLine("initial: {0}, {1}", string.Join(", ", initial), FnGetFitness(initial));

            var optimalFitness = 10 * maxSeconds;
            genetic.BestFitness(FnGetFitness, length, optimalFitness, geneSet, FnDisplay, null, null, 0, 1, null, 600);
        }

        [TestMethod]
        public void TestFindTop10Approximations()
        {
            // 355 / 113	3.1415923868256
            var watch = new Stopwatch();
            watch.Start();

            var best = new Dictionary<double, Tuple<int, int>>();
            for (var numerator = 1; numerator < (1 << 10) + 1; numerator++)
            for (var denominator = 1; denominator < (1 << 10) + 1; denominator++)
            {
                var ratio = (double) numerator / denominator;
                var piDist = Math.PI - Math.Abs(Math.PI - ratio);
                if (!best.ContainsKey(piDist) || best[piDist].Item1 > numerator)
                    best[piDist] = new Tuple<int, int>(numerator, denominator);
            }

            var bestApproximations = best.OrderByDescending(d => d.Key).Take(10);
            foreach (var approximation in bestApproximations)
            {
                Console.WriteLine("{0} / {1}\t{2}", approximation.Value.Item1, approximation.Value.Item2,
                    approximation.Key);
            }

            Console.WriteLine("Found best Pi {0}, in {1} ms.", Math.PI, watch.ElapsedMilliseconds);
        }
    }
}