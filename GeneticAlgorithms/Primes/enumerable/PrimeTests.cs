using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace GeneticAlgorithms.primes.enumerable
{
    // https://handcraftsman.wordpress.com/2010/09/02/ienumerable-of-prime-numbers-in-csharp/
    [TestClass]
    public class PrimeTests
    {
        public static readonly int[] Primes = {2, 3, 5, 7, 11, 13, 17, 19, 23, 29, 31, 37};

        public static IEnumerable<int> Primes1()
        {
            var ints = Enumerable.Range(2, int.MaxValue - 10);
            return ints.Where(x => ints.TakeWhile(y => y < x).All(y => x % y != 0));
        }

        [TestMethod]
        public void Test1()
        {
            CollectionAssert.AreEqual(Primes, Primes1().Take(Primes.Length).ToList());
        }


        public static IEnumerable<int> Primes2()
        {
            var ints = Enumerable.Range(2, int.MaxValue - 10);
            return ints.Where(x => ints.TakeWhile(y => y <= Math.Sqrt(x)).All(y => x % y != 0));
        }

        [TestMethod]
        public void Test2()
        {
            CollectionAssert.AreEqual(Primes, Primes2().Take(Primes.Length).ToList());
        }


        public static IEnumerable<int> Primes3()
        {
            var ints = Enumerable.Range(2, int.MaxValue - 10);
            return ints.Where(x =>
            {
                var sqrt = Math.Sqrt(x);
                return ints.TakeWhile(y => y <= sqrt).All(y => x % y != 0);
            });
        }

        [TestMethod]
        public void Test3()
        {
            CollectionAssert.AreEqual(Primes, Primes3().Take(Primes.Length).ToList());
        }


        public static IEnumerable<int> Primes4()
        {
            return new[] {2}.Concat(OddInts5().Where(x =>
            {
                var sqrt = Math.Sqrt(x);
                return OddInts5()
                    .TakeWhile(y => y <= sqrt)
                    .All(y => x % y != 0);
            }));
        }

        [TestMethod]
        public void Test4()
        {
            CollectionAssert.AreEqual(Primes, Primes4().Take(Primes.Length).ToList());
        }

        public static IEnumerable<int> OddInts5()
        {
            var start = 1;
            while (start > 0)
            {
                yield return start += 2;
            }
        }

        public static IEnumerable<int> Primes5()
        {
            return new[] {2}.Concat(OddInts5().Where(x =>
            {
                var sqrt = Math.Sqrt(x);
                return Primes5()
                    .TakeWhile(y => y <= sqrt)
                    .All(y => x % y != 0);
            }));
        }

        [TestMethod]
        public void Test5()
        {
            CollectionAssert.AreEqual(Primes, Primes5().Take(Primes.Length).ToList());
        }

        public static IEnumerable<int> PotentialPrimes6()
        {
            yield return 3;
            int k = 1;
            while (k > 0)
            {
                yield return 6 * k - 1;
                yield return 6 * k + 1;
                k++;
            }
        }

        public static IEnumerable<int> Primes6()
        {
            return new[] {2}.Concat(PotentialPrimes6().Where(x =>
            {
                var sqrt = Math.Sqrt(x);
                return PotentialPrimes6()
                    .TakeWhile(y => y <= sqrt)
                    .All(y => x % y != 0);
            }));
        }

        [TestMethod]
        public void Test6()
        {
            CollectionAssert.AreEqual(Primes, Primes6().Take(Primes.Length).ToList());
        }

        public static IEnumerable<int> Primes7()
        {
            return PotentialPrimes7().Where(x =>
            {
                var sqrt = Math.Sqrt(x);
                return PotentialPrimes7()
                    .TakeWhile(y => y <= sqrt)
                    .All(y => x % y != 0);
            });
        }

        public static IEnumerable<int> PotentialPrimes7()
        {
            yield return 2;
            yield return 3;
            int k = 1;
            while (k > 0)
            {
                yield return 6 * k - 1;
                yield return 6 * k + 1;
                k++;
            }
        }

        [TestMethod]
        public void Test7()
        {
            CollectionAssert.AreEqual(Primes, Primes7().Take(Primes.Length).ToList());
        }

        public static IEnumerable<int> Primes8()
        {
            var memoized = new List<int>();
            var primes = PotentialPrimes8().Where(x =>
            {
                var sqrt = Math.Sqrt(x);
                return memoized.TakeWhile(y => y <= sqrt).All(y => x % y != 0);
            });
            foreach (var prime in primes)
            {
                yield return prime;
                memoized.Add(prime);
            }
        }

        private static IEnumerable<int> PotentialPrimes8()
        {
            yield return 2;
            yield return 3;
            var k = 1;
            loop:
            yield return k * 6 - 1;
            yield return k * 6 + 1;
            k++;
            goto loop;
        }

        [TestMethod]
        public void Test8()
        {
            CollectionAssert.AreEqual(Primes, Primes8().Take(Primes.Length).ToList());
        }

        public static IEnumerable<int> Primes9()
        {
            var memoized = new List<int>();

            var sqrt = 1;
            var primes = PotentialPrimes8().Where(x =>
            {
                sqrt = GetSqrtCeiling(x, sqrt);
                return memoized.TakeWhile(y => y <= sqrt).All(y => x % y != 0);
            });

            foreach (var prime in primes)
            {
                yield return prime;
                memoized.Add(prime);
            }
        }

        private static int GetSqrtCeiling(int value, int start)
        {
            while (start * start < value)
                start++;
            return start;
        }

        [TestMethod]
        public void Test9()
        {
            CollectionAssert.AreEqual(Primes, Primes9().Take(Primes.Length).ToList());
        }

        [TestMethod]
        public void TimingTest()
        {
            const int limit = 100000;
            var watch = Stopwatch.StartNew();
            var list = Primes9().Take(limit);
            Assert.AreEqual(limit, list.Count());
            Console.WriteLine("Time took {0} ms", watch.ElapsedMilliseconds);
        }
    }
}