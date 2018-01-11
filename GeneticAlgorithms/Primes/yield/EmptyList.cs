using System;

namespace GeneticAlgorithms.primes.yield
{
    public class EmptyList<T> : IMyList<T>
    {
        public T Head => throw new Exception();

        public IMyList<T> Tail => throw new Exception();

        public bool Empty => true;

        public IMyList<T> Filter(Predicate<T> p) => new EmptyList<T>();
    }
}