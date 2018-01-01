using System;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace PrimeTesting.lazy
{
    [TestClass]
    public class PrimeTests
    {
        public static LazyList<int> From(int n)
        {
            return new LazyList<int>(n, new Lazy<IMyList<int>>(() => From(n + 1)));
        }

        public static IMyList<int> Primes(IMyList<int> numbers)
        {
            return new LazyList<int>(
                numbers.Head, new Lazy<IMyList<int>>(() =>
                    Primes(
                        numbers.Tail.Filter(n => n % numbers.Head != 0)
                    )));
        }

        [TestMethod]
        public void LazyListTest()
        {
            var numbers = From(2);
            Assert.AreEqual(2, numbers.Head);
            Assert.AreEqual(3, numbers.Tail.Head);
            Assert.AreEqual(4, numbers.Tail.Tail.Head);
        }

        [TestMethod]
        public void PrimeTest()
        {
            var numbers = Primes(From(2));
            Assert.AreEqual(2, numbers.Head);
            Assert.AreEqual(3, numbers.Tail.Head);
            Assert.AreEqual(5, numbers.Tail.Tail.Head);
        }

        [TestMethod]
        public void PrintPrimes()
        {
            var numbers = Primes(From(2));
            for (var i = 0; i < 20; i++)
            {
                Console.WriteLine(numbers.Head);
                numbers = numbers.Tail;
            }
        }

        [TestMethod]
        public void Test2()
        {
            var rnd = new Random();
            var teams = new List<HockeyTeam>();
            teams.AddRange(new[] { new HockeyTeam("Detroit Red Wings", 1926),
                new HockeyTeam("Chicago Blackhawks", 1926),
                new HockeyTeam("San Jose Sharks", 1991),
                new HockeyTeam("Montreal Canadiens", 1909),
                new HockeyTeam("St. Louis Blues", 1967) });
            int[] years = { 1920, 1930, 1980, 2000 };
            var foundedBeforeYear = years[rnd.Next(0, years.Length)];
            Console.WriteLine("Teams founded before {0}:", foundedBeforeYear);
            foreach (var team in teams.FindAll(x => x.Founded <= foundedBeforeYear))
                Console.WriteLine("{0}: {1}", team.Name, team.Founded);
        }
    }

    public class HockeyTeam
    {
        public HockeyTeam(string name, int year)
        {
            Name = name;
            Founded = year;
        }

        public string Name { get; }

        public int Founded { get; }
    }
}