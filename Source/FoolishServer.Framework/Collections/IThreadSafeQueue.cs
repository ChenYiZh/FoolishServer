﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace FoolishServer.Framework.Collections
{
    public interface IThreadSafeQueue<T> : IEnumerable<T>, IEnumerable, IReadOnlyCollection<T>, ICollection
    {
        T Peek();

        T Dequeue();

        void Enqueue(T item);
    }
}
