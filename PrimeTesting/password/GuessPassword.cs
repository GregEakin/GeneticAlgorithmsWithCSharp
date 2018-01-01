using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Diagnostics;

namespace PrimeTesting.password
{
    [TestClass]
    public class GuessPassword
    {
        public const string GeneSet = " abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ!.";
        public const string Target = "Hello World!";

        private readonly Stopwatch _stopwatch = new Stopwatch();

        public static int Fitness(string guess)
        {
            var lenght = Math.Min(Target.Length, guess.Length);
            var sum = 0;
            for (var i = 0; i < lenght; i++)
            {
                if (Target[i] == guess[i])
                    sum++;
            }

            return sum;
        }

        public void Display(string guess)
        {
            var fitness = Fitness(guess);
            Console.WriteLine("{0}\t{1}\t{2} ms", guess, fitness, _stopwatch.ElapsedMilliseconds);
        }

        [TestMethod]
        public void Test1()
        {
            var genetic = new Genetic();

            _stopwatch.Reset();
            _stopwatch.Start();
            genetic.BestFitness(Fitness, Target.Length, Target.Length, GeneSet, Display);

            _stopwatch.Stop();
        }

        [TestMethod]
        public void RandomSampleTest()
        {
            var genetic = new Genetic();
            for (var i = 0; i < 10; i++)
            {
                var x = genetic.RandomSample(GeneSet, Target.Length);
                Console.WriteLine(x);
            }
        }

        [TestMethod]
        public void GenerateParentTest()
        {
            var genetic = new Genetic();

            var parent = genetic.GenerateParent(GeneSet, Target.Length);
            Console.WriteLine(parent);
        }
    }
}