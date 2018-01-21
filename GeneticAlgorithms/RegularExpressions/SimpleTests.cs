using System;
using System.Text.RegularExpressions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using static GeneticAlgorithms.Utilities.ExpectedException;

namespace GeneticAlgorithms.RegularExpressions
{
    [TestClass]
    public class SimpleTests
    {
        [TestMethod]
        public void Test1()
        {
            var pattern = "1*0?10*";

            var wanted = new[] { "01", "11", "10" };
            var unwanted = new[] { "00", "" };

            var rgx = new Regex(pattern);
            foreach (var want in wanted)
            {
                var matches = rgx.Matches(want);
                Assert.AreEqual(1, matches.Count);
            }

            foreach (var unwant in unwanted)
            {
                var matches = rgx.Matches(unwant);
                Assert.AreEqual(0, matches.Count);
            }
        }

        [TestMethod]
        public void Test2()
        {
            // Define a regular expression for repeated words.
            var rx = new Regex(@"\b(?<word>\w+)\s+(\k<word>)\b", RegexOptions.Compiled | RegexOptions.IgnoreCase);

            // Define a test string.        
            var text = "The the quick brown fox  fox jumped over the lazy dog dog.";

            // Find matches.
            var matches = rx.Matches(text);
            Assert.AreEqual(3, matches.Count);

            var match1 = rx.Match(text);
            Assert.IsTrue(match1.Success);

            // Report the number of matches found.
            Console.WriteLine("{0} matches found in:\n   {1}",
                matches.Count,
                text);

            // Report on each match.
            foreach (Match match in matches)
            {
                var groups = match.Groups;
                Console.WriteLine("'{0}' repeated at positions {1} and {2}",
                    groups["word"].Value,
                    groups[0].Index,
                    groups[1].Index);
            }
        }

        [TestMethod]
        public void Test3()
        {
            AssertThrows<ArgumentException>(() => new Regex("[[[**(("));
        }
    }
}