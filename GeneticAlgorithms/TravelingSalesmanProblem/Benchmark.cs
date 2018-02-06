/* File: Benchmark.cs
 *     from chapter 12 of _Genetic Algorithms with Python_
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

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace GeneticAlgorithms.TravelingSalesmanProblem
{
    public static class Benchmark
    {
        public static void Run(Action function)
        {
            var timings = new List<double>();
            var stdout = Console.Out;
            for (var i = 0; i < 100; i++)
            {
                Console.SetOut(TextWriter.Null);
                var watch = Stopwatch.StartNew();
                function();
                var seconds = watch.ElapsedMilliseconds / 1000.0;
                timings.Add(seconds);
                Console.SetOut(stdout);
                if (i >= 10 && i % 10 < 9)
                    continue;

                var mean = timings.Average();
                var sd = i > 1 ? Math.Sqrt(timings.Average(v => Math.Pow(v - mean, 2))) : 0.0;
                Console.WriteLine("{0} {1:F2} {2:F2}", i + 1, mean, sd);
            }
        }
    }
}