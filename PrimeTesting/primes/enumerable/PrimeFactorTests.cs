using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace PrimeTesting.enumerable
{
    [TestClass]
    public class PrimeFactorTests
    {
        public static IEnumerable<int> GetFactors(int input)
        {
            var first = PrimeTests.Primes9()
                .TakeWhile(x => x <= Math.Sqrt(input))
                .FirstOrDefault(x => input % x == 0);
            return first == 0
                ? new[] {input}
                : new[] {first}.Concat(GetFactors(input / first));
        }

        [TestMethod]
        public void TestPrimeFactors()
        {
            const int number = 3 * 5 * 5 * 11;
            var factors = GetFactors(number);
            CollectionAssert.AreEqual(new[] {3, 5, 5, 11}, factors.ToList());
        }
    }
}