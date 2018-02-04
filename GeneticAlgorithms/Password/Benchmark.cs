using System;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace GeneticAlgorithms.Password
{
    [TestClass]
    public class Benchmark
    {
        private const string GeneSet = GuessPassordTests.GeneSet;

        private readonly Random _random = new Random();

        //[TestMethod]
        public void BenchmarkTest()
        {
            const int count = 100;
            const int length = 150;

            var timings = new long[count];
            for (var i = 0; i < count; i++)
            {
                var target = string.Join("",
                    Enumerable.Range(0, length).Select(x => GeneSet[_random.Next(GeneSet.Length)]));
                var watch = GuessPassordTests.GuessPassword(target);
                timings[i] = watch.ElapsedMilliseconds;
            }

            var mean = 0.0;
            var sd = 0.0;
            Console.WriteLine("Mean {0}, SD = {1}", mean, sd);
        }
    }
}