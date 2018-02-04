using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace GeneticAlgorithms.Password
{
    public static class Benchmark
    {
        public static void Run(Action function)
        {
            var timings = new List<long>();
            var stdout = Console.Out;
            for (var i = 0; i < 100; i++)
            {
                Console.SetOut(TextWriter.Null);
                var watch = Stopwatch.StartNew();
                function();
                var milliseconds = watch.ElapsedMilliseconds;
                Console.SetOut(stdout);
                timings.Add(milliseconds);
                var mean = timings.Average();
                if (i >= 10 && i % 10 < 9)
                    continue;

                var sd = i > 1 ? Math.Sqrt(timings.Average(v => Math.Pow(v - mean, 2))) : 0.0;
                Console.WriteLine("{0} {1:F2} {2:F2}", i+1, mean, sd);
            }
        }
    }
}