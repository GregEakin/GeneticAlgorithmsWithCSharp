using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace GeneticAlgorithms.primes
{
    [TestClass]
    public class UnitTest1
    {
        [TestMethod]
        public void Test2()
        {
            var rnd = new Random();
            var teams = new List<HockeyTeam>
            {
                new HockeyTeam("Detroit Red Wings", 1926),
                new HockeyTeam("Chicago Blackhawks", 1926),
                new HockeyTeam("San Jose Sharks", 1991),
                new HockeyTeam("Montreal Canadiens", 1909),
                new HockeyTeam("St. Louis Blues", 1967)
            };

            int[] years = {1920, 1930, 1980, 2000};
            var foundedBeforeYear = years[rnd.Next(0, years.Length)];
            Console.WriteLine("Teams founded before {0}:", foundedBeforeYear);
            foreach (var team in teams.FindAll(team => team.Founded <= foundedBeforeYear).OrderBy(team => team.Name))
                Console.WriteLine("    {0}: {1}", team.Name, team.Founded);
        }
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