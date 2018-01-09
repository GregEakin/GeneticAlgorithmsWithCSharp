using System;

namespace PrimeTesting.yield
{
    public class LazyList<T> : IMyList<T>
    {
        private readonly Lazy<IMyList<T>> _tail;

        public LazyList(T head, Lazy<IMyList<T>> tail)
        {
            Head = head;
            _tail = tail;
        }

        public T Head { get; }

        public IMyList<T> Tail => _tail.Value;

        public bool Empty => false;

        public IMyList<T> Filter(Predicate<T> p)
        {
            return Empty
                ? this
                : p(Head)
                    ? new LazyList<T>(Head, new Lazy<IMyList<T>>(() => Tail.Filter(p)))
                    : Tail.Filter(p);
        }
    }
}