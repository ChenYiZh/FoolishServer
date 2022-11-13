﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace FoolishGames.Collections
{
    public interface IThreadSafeStack<T> : IEnumerable<T>, IEnumerable, IReadOnlyCollection<T>
    {
        T Peek();

        T Pop();

        void Push(T item);

        void Clear();
    }
}