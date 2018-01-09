﻿using System;

namespace PrimeTesting.yield
{
    public interface IMyList<out T>
    {
        T Head { get; }
        IMyList<T> Tail { get; }

        bool Empty { get; }

        IMyList<T> Filter(Predicate<T> p);
    }
}