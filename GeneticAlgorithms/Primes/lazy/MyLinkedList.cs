using System;

namespace GeneticAlgorithms.primes.lazy
{
    public class MyLinkedList<T> : IMyList<T>
    {
        public MyLinkedList(T head, IMyList<T> tail)
        {
            Head = head;
            Tail = tail;
        }

        public T Head { get; }

        public IMyList<T> Tail { get; }

        public bool Empty => false;

        public IMyList<T> Filter(Predicate<T> p) => new EmptyList<T>();
    }
}